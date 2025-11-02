using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using OutOut.Models.Domains;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Identity;
using OutOut.Persistence.Identity.Interfaces;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.Auth;
using OutOut.ViewModels.Responses.Auth;
using OutOut.ViewModels.Responses.Customers;
using OutOut.ViewModels.Responses.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace OutOut.Core.Services
{
    public class AuthService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly OTPService _otpService;
        private readonly EmailComposerService _emailComposer;
        private readonly AppSettings _appSettings;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly GoogleAuthenticator _googleAuthenticator;
        private readonly FacebookAuthenticator _facebookAuthenticator;
        private readonly AppleAuthenticator _appleAuthenticator;
        private readonly RefreshTokensFactory<ApplicationUser> _refreshTokenFactory;
        private readonly IUserRepository _userRepo;
        private readonly IUserRefreshTokenStore<ApplicationUser> _store;
        private readonly ILogger<AuthService> _logger;
        private readonly FileUploaderService _fileUploader;

        public AuthService(IOptions<AppSettings> appSettings,
                           IMapper mapper,
                           ILogger<AuthService> logger,
                           UserManager<ApplicationUser> userManager,
                           OTPService otpService,
                           EmailComposerService emailComposer,
                           IUserDetailsProvider userDetailsProvider,
                           RefreshTokensFactory<ApplicationUser> refreshTokenFactory,
                           IUserRepository userRepo,
                           IUserStore<ApplicationUser> store,
                           GoogleAuthenticator googleAuthenticator,
                           FileUploaderService fileUploader,
                           FacebookAuthenticator facebookAuthenticator,
                           AppleAuthenticator appleAuthenticator,
                           SignInManager<ApplicationUser> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _mapper = mapper;
            _otpService = otpService;
            _emailComposer = emailComposer;
            _appSettings = appSettings.Value;
            _store = store as IUserRefreshTokenStore<ApplicationUser>;
            _userDetailsProvider = userDetailsProvider;
            _refreshTokenFactory = refreshTokenFactory;
            _userRepo = userRepo;
            _logger = logger;
            _googleAuthenticator = googleAuthenticator;
            _fileUploader = fileUploader;
            _facebookAuthenticator = facebookAuthenticator;
            _appleAuthenticator = appleAuthenticator;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUserResponse> Register(CustomerRegistrationRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new OutOutException(ErrorCodes.DuplicateEmail);

            var newUser = _mapper.Map<ApplicationUser>(request);
            
            newUser.Location = new UserLocation(_appSettings.DefaultUserLocation.Longitude, _appSettings.DefaultUserLocation.Latitude, _appSettings.DefaultUserLocation.Description);
            newUser.Gender = Gender.Unspecified;

            var otp = _otpService.GenerateOTPResult();
            newUser.VerificationOTP = _mapper.Map<UserOTP>(otp);

            var identityResult = await _userManager.CreateAsync(newUser, request.Password);

            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            await _emailComposer.SendSuccessRegistrationMail(newUser.Email, newUser.FullName, otp.OTP);

            return _mapper.Map<ApplicationUserResponse>(newUser);
        }

        public async Task<LoginResponse> VerifyAccount(VerifyAccountRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            if (existingUser.EmailConfirmed == true)
                throw new OutOutException(ErrorCodes.EmailAlreadyConfirmed);

            if (!_otpService.ValidateOTPHash(request.OTP, existingUser.VerificationOTP.HashedOTP))
                throw new OutOutException(ErrorCodes.InvalidOTP);

            if (HasOTPExpired(existingUser.VerificationOTP.RequestHistory))
                throw new OutOutException(ErrorCodes.OTPExpired);


            existingUser.VerificationOTP.HashedOTP = null;
            existingUser.EmailConfirmed = true;

            var identityResult = await _userManager.UpdateAsync(existingUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return await LoginAsync(new LoginRequest { Email = existingUser.Email, FirebaseMessagingToken = request.FirebaseMessagingToken }, false);
        }

        public async Task<bool> ResendVerificationEmail(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            if (existingUser.EmailConfirmed == true)
                throw new OutOutException(ErrorCodes.EmailAlreadyConfirmed);

            if (HasReachedLimit(existingUser.VerificationOTP.RequestHistory))
                throw new OutOutException(ErrorCodes.VerificationRequestsLimitReached);

            var generatedOtp = _otpService.GenerateOTPResult();

            existingUser.VerificationOTP.HashedOTP = generatedOtp.HashedOTP;
            existingUser.VerificationOTP.RequestHistory = UpdateHistory(existingUser.VerificationOTP.RequestHistory);

            var identityResult = await _userManager.UpdateAsync(existingUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            await _emailComposer.SendSuccessRegistrationMail(existingUser.Email, existingUser.FullName, generatedOtp.OTP);

            return identityResult.Succeeded;
        }

        public async Task<OTPVerificationTimeLeftResponse> GetTimeLeftVerification(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            if (existingUser.EmailConfirmed == true)
                throw new OutOutException(ErrorCodes.EmailAlreadyConfirmed);

            if (existingUser.VerificationOTP.RequestHistory == null || existingUser.VerificationOTP.RequestHistory.Count() == 0)
                throw new OutOutException(ErrorCodes.OTPNotGenerated);

            var otpExpiredOn = existingUser.VerificationOTP.RequestHistory.Last().AddMinutes(_appSettings.OTPConfigurations.ValidForMinutes);
            var timeLeft = _mapper.Map<OTPVerificationTimeLeftResponse>(otpExpiredOn.Subtract(DateTime.UtcNow));
            return timeLeft;
        }

        public async Task<LoginResponse> ExternalAuthenticationAsync(ExternalAuthenticationRequest externalLoginRequest)
        {
            ExternalProvider provider = ExternalProvider.None;
            ExternalUserInfo externalUserInfo = null;
            switch (externalLoginRequest.ExternalLoginProvider)
            {
                case ExternalProvider.Facebook:
                    provider = ExternalProvider.Facebook;
                    externalUserInfo = await _facebookAuthenticator.GetAccessTokenInfo(externalLoginRequest.AccessToken);
                    break;
                case ExternalProvider.Google:
                    provider = ExternalProvider.Google;
                    externalUserInfo = await _googleAuthenticator.GetAccessTokenInfo(externalLoginRequest.AccessToken);
                    break;
                case ExternalProvider.Apple:
                    provider = ExternalProvider.Apple;
                    externalUserInfo = await _appleAuthenticator.GetAccessTokenInfo(externalLoginRequest.AccessToken);
                    externalUserInfo.Name = externalLoginRequest.FullName;
                    if (string.IsNullOrEmpty(externalLoginRequest.FullName) || externalLoginRequest.FullName.Contains("null"))
                        externalUserInfo.Name = RemoveDigits(externalUserInfo.Email.Split('@')[0].Humanize(LetterCasing.Title)).TrimStart().TrimEnd();
                    break;
                default:
                    break;
            }

            if (externalUserInfo == null)
                throw new OutOutException(ErrorCodes.InvalidToken);

            var existingUser = await _userManager.FindByEmailAsync(externalUserInfo.Email);
            if (existingUser == null)
            {
                var profileImage = await _fileUploader.UploadFile(_appSettings.Directories.ProfileImages, externalUserInfo.ImageUrl, ".jpeg");
                var applicationUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = Guid.NewGuid().ToString(),
                    Email = externalUserInfo.Email,
                    PasswordHash = null,
                    ProfileImage = profileImage,
                    FullName = externalUserInfo.Name,
                    ExternalProvider = provider,
                    EmailConfirmed = true,
                    Gender = Gender.Unspecified,
                    Location = new UserLocation(_appSettings.DefaultUserLocation.Longitude, _appSettings.DefaultUserLocation.Latitude, _appSettings.DefaultUserLocation.Description)
                };

                var result = await _userManager.CreateAsync(applicationUser);
                if (!result.Succeeded)
                    throw new OutOutException(result);

                await _emailComposer.SendExternalSuccessRegistrationMail(applicationUser.Email, applicationUser.FullName);

                return await LoginAsync(new LoginRequest { Email = externalUserInfo.Email, FirebaseMessagingToken = externalLoginRequest.FirebaseMessagingToken }, false);
            }
            else
            {
                var fileName = await _fileUploader.UploadFile(_appSettings.Directories.ProfileImages, externalUserInfo.ImageUrl, ".jpeg");
                _fileUploader.DeleteFile(_appSettings.Directories.ProfileImages, existingUser.ProfileImage);
                existingUser.ProfileImage = fileName;
                await _userManager.UpdateAsync(existingUser);

                return await LoginAsync(new LoginRequest { Email = externalUserInfo.Email, FirebaseMessagingToken = externalLoginRequest.FirebaseMessagingToken }, false);
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest, bool checkPassword = true)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null)
                throw new OutOutException(ErrorCodes.InvalidLogin);

            if (!user.EmailConfirmed)
                throw new OutOutException(ErrorCodes.UnverifiedEmail);

            //check password
            if (checkPassword)
            {
                var passwordResult = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
                if (!passwordResult)
                    throw new OutOutException(ErrorCodes.InvalidLogin);
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            //set claims for the token
            var authClaims = new List<Claim>()
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddDays(_appSettings.JWTTokenDuration.Days)
                                     .AddHours(_appSettings.JWTTokenDuration.Hours)
                                     .AddMinutes(_appSettings.JWTTokenDuration.Minutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.AppSecrets.JWTSecretKey)),
                                                           SecurityAlgorithms.HmacSha256)
            );

            //add fcm token 
            if (!string.IsNullOrEmpty(loginRequest.FirebaseMessagingToken))
            {
                await _userRepo.AssignFirebaseMessagingTokenToUser(user.Id, loginRequest.FirebaseMessagingToken);
            }

            //refresh token
            var refreshToken = await _refreshTokenFactory.GenerateRefreshToken(user, token.Id);
            await _refreshTokenFactory.RemoveAllExipredTokens(user);

            return new LoginResponse()
            {
                User = _mapper.Map<ApplicationUserResponse>(user),
                UserRoles = userRoles.ToList(),
                IsVerifiedEmail = true,

                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,

                RefreshToken = refreshToken.RefreshToken,
                RefreshTokenExpiration = refreshToken.ExpirationDate
            };
        }

        public async Task<LoginResponse> RefreshAccessToken(RefreshTokenRequest refreshTokenRequest)
        {
            //verify token
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            string userEmail = "";

            SecurityToken validatedToken;
            ClaimsPrincipal claimsPrincipal;
            try
            {
                //validate the token's format
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.AppSecrets.JWTSecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };
                claimsPrincipal = jwtTokenHandler.ValidateToken(refreshTokenRequest.AccessToken, validationParameters, out validatedToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception while verifying the token : {e.Message}");
                throw new OutOutException(ErrorCodes.InvalidToken, HttpStatusCode.Locked);
            }

            //validate the token was being encrypted using same encryption algorithm & check its expiration date
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                if (!result)
                    throw new OutOutException(ErrorCodes.InvalidToken, HttpStatusCode.Locked);
            }

            //get data from token claims
            var userId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            userEmail = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var accessTokenId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new OutOutException(ErrorCodes.InvalidToken, HttpStatusCode.Locked);

            CancellationToken cancelation = new CancellationToken();
            var storedTokens = await _store.GetRefreshTokensAsync(userId, cancelation);

            //validate refresh tokens
            if (storedTokens == null || !storedTokens.Any(x => x.RefreshToken == refreshTokenRequest.RefreshToken && x.AccessTokenUniqeId == accessTokenId && x.ExpirationDate >= DateTime.UtcNow))
                throw new OutOutException(ErrorCodes.InvalidRefreshToken, HttpStatusCode.Locked);

            //remove refresh token from the user
            await _refreshTokenFactory.RevokeRefreshToken(user, refreshTokenRequest.RefreshToken);

            //re-login the user without password and generate a new refresh token
            return await LoginAsync(new LoginRequest { Email = userEmail }, false);
        }

        public async Task<bool> LogoutAsync(LogoutRequest viewModel)
        {
            var userId = _userDetailsProvider.UserId;
            await _userRepo.UnassignFirebaseMessagingTokenFromUser(userId, viewModel.FirebaseMessagingToken);
            return true;
        }

        public async Task WebLogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            IdentityResult identityResult = IdentityResult.Failed();

            if (request.OldPassword == null && currentUser.PasswordHash == null)
                identityResult = await _userManager.AddPasswordAsync(currentUser, request.NewPassword);
            else if (request.OldPassword == null && currentUser.PasswordHash != null)
                throw new OutOutException(ErrorCodes.InvalidNullParameters);
            else
                identityResult = await _userManager.ChangePasswordAsync(currentUser, request.OldPassword, request.NewPassword);

            if (!identityResult.Succeeded)
                throw new OutOutException(ErrorCodes.WrongOldPassword);

            return true;
        }

        public async Task<bool> ForgetPassword(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            if (HasReachedLimit(existingUser.ResetPasswordOTP.RequestHistory))
                throw new OutOutException(ErrorCodes.ResetOTPRequestsLimitReached);

            var generatedOtp = _otpService.GenerateOTPResult();
            existingUser.ResetPasswordOTP.HashedOTP = generatedOtp.HashedOTP;
            existingUser.ResetPasswordOTP.RequestHistory = UpdateHistory(existingUser.ResetPasswordOTP.RequestHistory);

            var identityResult = await _userManager.UpdateAsync(existingUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            _ = Task.Run(() => _emailComposer.SendResetPasswordEmail(existingUser.Email, existingUser.FullName, generatedOtp.OTP));

            return identityResult.Succeeded;
        }

        public async Task<string> VerifyResetPassword(VerifyResetPasswordRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            if (!_otpService.ValidateOTPHash(request.OTP, existingUser.ResetPasswordOTP.HashedOTP))
                throw new OutOutException(ErrorCodes.InvalidOTP);

            if (HasOTPExpired(existingUser.ResetPasswordOTP.RequestHistory))
                throw new OutOutException(ErrorCodes.OTPExpired);

            return existingUser.ResetPasswordOTP.HashedOTP;
        }

        public async Task<bool> ResetPassword(ResetPasswordRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            if (!existingUser.ResetPasswordOTP.HashedOTP.Equals(request.HashedOTP))
                throw new OutOutException(ErrorCodes.InvalidOTP);

            IdentityResult identityResult;

            identityResult = await _userManager.RemovePasswordAsync(existingUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            identityResult = await _userManager.AddPasswordAsync(existingUser, request.NewPassword);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            existingUser.ResetPasswordOTP.HashedOTP = null;
            identityResult = await _userManager.UpdateAsync(existingUser);
            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);

            return identityResult.Succeeded;
        }

        internal bool HasReachedLimit(IEnumerable<DateTime> resendHistory)
        {
            var currentDate = DateTime.UtcNow;

            if (resendHistory == null || resendHistory.Count() == 0)
                return false;

            if (resendHistory.Any(request => currentDate.Subtract(request).TotalSeconds < 60))
                return true;

            if (currentDate.Subtract(resendHistory.OrderBy(a => a).TakeLast(10).FirstOrDefault()).TotalHours < 1 && resendHistory?.Count() >= 10)
                return true;

            return false;
        }

        internal bool HasOTPExpired(IEnumerable<DateTime> resendHistory)
        {
            var currentDate = DateTime.UtcNow;

            if (resendHistory == null || resendHistory.Count() == 0)
                return true;

            if (currentDate.Subtract(resendHistory.Last()).TotalMinutes > _appSettings.OTPConfigurations.ValidForMinutes)
                return true;

            return false;
        }

        internal List<DateTime> UpdateHistory(List<DateTime> requestHistory)
        {
            requestHistory.Sort();

            while (requestHistory.Count >= 10)
            {
                requestHistory.Remove(requestHistory.First());
            }

            requestHistory.RemoveAll(request => DateTime.UtcNow.Subtract(request).TotalHours > 1);

            requestHistory.Add(DateTime.UtcNow);

            return requestHistory;
        }

        public async Task VerifyAndAssignSuperAdminRole(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
                throw new OutOutException(ErrorCodes.EmailNotFound);

            existingUser.EmailConfirmed = true;
            var identityResult = await _userManager.UpdateAsync(existingUser);

            var rolesList = await _userManager.GetRolesAsync(existingUser);
            if (!rolesList.Contains(Roles.SuperAdmin))
                await _userManager.AddToRoleAsync(existingUser, Roles.SuperAdmin);

            if (!identityResult.Succeeded)
                throw new OutOutException(identityResult);
        }

        public List<string> GetAllowedMobileVersions()
        {
            var versions = _appSettings.AllowedMobileVersions;
            if (versions != null && versions.Any())
            {
                return new List<string>(versions);
            }
            throw new OutOutException(ErrorCodes.NoAvailableVersions, "No available versions");
        }

        private string RemoveDigits(string value) => Regex.Replace(value, @"\d", "");
    }
}
