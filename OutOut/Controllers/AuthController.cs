using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOut.Core.Services;
using OutOut.ViewModels.Requests.Auth;
using OutOut.ViewModels.Responses.Auth;
using OutOut.ViewModels.Responses.Customers;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [Produces(typeof(OperationResult<ApplicationUserResponse>))]
        [HttpPost]
        public async Task<IActionResult> Register(CustomerRegistrationRequest request)
        {
            var result = await _authService.Register(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoginResponse>))]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginViewModel)
        {
            var result = await _authService.LoginAsync(loginViewModel);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoginResponse>))]
        [HttpPost]
        public async Task<IActionResult> ExternalAuthentication([FromBody] ExternalAuthenticationRequest viewModel)
        {
            var result = await _authService.ExternalAuthenticationAsync(viewModel);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoginResponse>))]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenRequest refreshTokenViewModel)
        {
            var result = await _authService.RefreshAccessToken(refreshTokenViewModel);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var result = await _authService.ChangePassword(changePasswordRequest);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword([FromQuery] string email)
        {
            var result = await _authService.ForgetPassword(email);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<string>))]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetPassword([FromBody] VerifyResetPasswordRequest request)
        {
            var result = await _authService.VerifyResetPassword(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<LoginResponse>))]
        [HttpPost]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequest request)
        {
            var result = await _authService.VerifyAccount(request);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [HttpGet]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            var result = await _authService.ResendVerificationEmail(email);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<OTPVerificationTimeLeftResponse>))]
        [HttpGet]
        public async Task<IActionResult> TimeLeftVerification(string email)
        {
            var result = await _authService.GetTimeLeftVerification(email);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Produces(typeof(OperationResult<bool>))]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest viewModel)
        {
            var result = await _authService.LogoutAsync(viewModel);
            return Ok(SuccessHelper.Wrap(result));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> WebLogout()
        {
            await _authService.WebLogoutAsync();
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllowedVersions()
        {
            var result = _authService.GetAllowedMobileVersions();
            return Ok(SuccessHelper.Wrap(result));
        }
    }
}
