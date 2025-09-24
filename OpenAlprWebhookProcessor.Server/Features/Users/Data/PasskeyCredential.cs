using System;
using System.ComponentModel.DataAnnotations;

namespace OpenAlprWebhookProcessor.Features.Users.Data
{
    public class PasskeyCredential
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string CredentialId { get; set; }

        [Required]
        public byte[] PublicKey { get; set; }

        [Required]
        public byte[] UserHandle { get; set; }

        [Required]
        public uint SignatureCounter { get; set; }

        [Required]
        [MaxLength(255)]
        public string CredType { get; set; }

        [Required]
        public DateTime RegDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string AaGuid { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
