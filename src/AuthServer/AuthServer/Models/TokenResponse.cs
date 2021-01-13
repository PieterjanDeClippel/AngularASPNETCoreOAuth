using Newtonsoft.Json;

namespace AuthServer.Models
{
    public class TokenResponse
    {
        public TokenResponse()
        {
        }

        public TokenResponse(IdentityModel.Client.TokenResponse tokenResponse)
        {
            AccessToken = tokenResponse.AccessToken;
            RefreshToken = tokenResponse.RefreshToken;
            TokenType = tokenResponse.TokenType;
            Error = tokenResponse.Error;
            ExpiresIn = tokenResponse.ExpiresIn;
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
