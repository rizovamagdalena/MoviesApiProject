using Newtonsoft.Json;

namespace MoviesAPI.Models.System
{
    public class LoginResponse
    {
        [JsonProperty("accessToken")]
        public string Token { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
