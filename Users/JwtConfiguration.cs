using System.ComponentModel.DataAnnotations;

namespace OpenAlprWebhookProcessor.Users
{
    public class JwtConfiguration
    {
        [Required]
        public string SecretKey { get; set; }
    }
}
