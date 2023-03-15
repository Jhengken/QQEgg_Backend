using System.Text.Json.Serialization;

namespace QQEgg_Backend.DTO
{
    public class LoginPostDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
