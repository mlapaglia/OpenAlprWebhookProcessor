using System;
using System.ComponentModel.DataAnnotations;

namespace OpenAlprWebhookProcessor.Data
{
    public class MachineLearningConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Key { get; set; }

        [Required]
        [MaxLength(500)]
        public string Value { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string ValueType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string UpdatedBy { get; set; }
    }
}
