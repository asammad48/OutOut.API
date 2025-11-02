using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOut.Models;
using OutOut.Models.Domains;
using RestSharp;
using System.Threading.Tasks;

namespace OutOut.Infrastructure.Services
{
    public class FacebookAuthenticator
    {
        private readonly AppSettings _appSettings;
        public FacebookAuthenticator(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public async Task<bool> ValidateAccessToken(string accessToken)
        {
            var client = new RestClient("https://graph.facebook.com/");
            var request = new RestRequest("debug_token", Method.Get);
            request.AddQueryParameter("input_token", accessToken);
            request.AddQueryParameter("access_token", $"{_appSettings.AppSecrets.FacebookAppId}|{_appSettings.AppSecrets.FacebookAppSecret}");
            request.RequestFormat = DataFormat.Json;

            var restResponse = await client.ExecuteAsync<ValidateFacebookAccessTokenResponse>(request);
            if (restResponse.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

            return restResponse.Data.Data.IsValid;
        }

        public async Task<ExternalUserInfo> GetAccessTokenInfo(string accessToken)
        {
            bool isValid = await ValidateAccessToken(accessToken);
            if (!isValid)
                return null;

            var client = new RestClient("https://graph.facebook.com/");
            var request = new RestRequest("me", Method.Get);
            request.AddQueryParameter("access_token", accessToken);
            request.AddQueryParameter("fields", "email,name");
            request.RequestFormat = DataFormat.Json;

            var restResponse = await client.ExecuteAsync<GetFacebookAccessTokenInfoResponse>(request);
            if (restResponse.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var request2 = new RestRequest("me/picture", Method.Get);
            request2.AddQueryParameter("access_token", accessToken);
            request2.AddQueryParameter("height", "660");
            request2.AddQueryParameter("width", "660");
            request2.RequestFormat = DataFormat.Json;
            var picRestResponse = await client.ExecuteAsync(request2);
            if (picRestResponse.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            return new ExternalUserInfo
            {
                Name = restResponse.Data.Name,
                Email = restResponse.Data.Email,
                ImageUrl = picRestResponse.RawBytes
            };
        }
    }
    class ValidateFacebookAccessTokenResponseData
    {
        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }
    }
    class ValidateFacebookAccessTokenResponse
    {
        public ValidateFacebookAccessTokenResponseData Data { get; set; }
    }
    class GetFacebookAccessTokenInfoResponse
    {
        public string Name { get; set; }
        public FacebookPicture Picture { get; set; }
        public string Email { get; set; }
    }
    class FacebookPicture
    {
        [JsonProperty("data")]
        public FacebookPictureData Data { get; set; }
    }
    class FacebookPictureData
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
