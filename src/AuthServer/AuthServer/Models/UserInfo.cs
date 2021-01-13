using Newtonsoft.Json;
using System;

namespace AuthServer.Models
{
    public class UserInfo
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("userName")]
        public string Username { get; set; }
    
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
