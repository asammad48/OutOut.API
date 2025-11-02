using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOut.Constants.Errors;
using OutOut.Models;
using OutOut.Models.Domains;
using OutOut.Models.Exceptions;
using RestSharp;

namespace OutOut.Infrastructure.Services
{
    public class GoogleAuthenticator
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<GoogleAuthenticator> _logger;

        public GoogleAuthenticator(IOptions<AppSettings> appSettings, ILogger<GoogleAuthenticator> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<ExternalUserInfo> GetAccessTokenInfo(string accessToken)
        {
            var client = new RestClient("https://oauth2.googleapis.com");
            var request = new RestRequest("tokeninfo", Method.Get);
            request.AddQueryParameter("id_token", accessToken);
            request.RequestFormat = DataFormat.Json;
            var restResponse = await client.ExecuteAsync<GoogleAccessTokenInfoResponse>(request);

            if (restResponse?.ErrorException != null || restResponse?.ErrorMessage != null)
                _logger.LogError($"Gmail token error : {restResponse?.ErrorException?.Message}, error message: {restResponse?.ErrorMessage}");

            _logger.LogInformation($"Gmail token response, email: {restResponse?.Data?.Email}, aud: {restResponse?.Data?.Aud}");

            if (restResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError($"Gmail token error: status code is {restResponse.StatusCode}");
                return null;
            }

            bool validAudience = _appSettings.AppSecrets.GoogleClientIds.Contains(restResponse.Data.Aud);
            if (!validAudience)
            {
                _logger.LogError($"Gmail token error: invalid audience");
                return null;
            }

            if (!restResponse.Data.EmailVerified)
                throw new OutOutException(ErrorCodes.UnverifiedEmail);

            var client2 = new RestClient();
            var request2 = new RestRequest(restResponse.Data.Picture, Method.Get);
            var picResponse = await client2.ExecuteAsync(request2);

            if (picResponse?.ErrorException != null || picResponse?.ErrorMessage != null)
                _logger.LogError($"Gmail token error, picture error : {picResponse?.ErrorException?.Message}, error message: {picResponse?.ErrorMessage}");

            return new ExternalUserInfo
            {
                Name = restResponse.Data.Name,
                Email = restResponse.Data.Email,
                ImageUrl = picResponse?.RawBytes
            };
        }

        
    }
    class GoogleAccessTokenInfoResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Aud { get; set; }
        public string Picture { get; set; }

        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }
    }
}
