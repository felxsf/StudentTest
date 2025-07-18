using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime TimeStamp { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;
        
        [MaxLength(4000)]
        public string? Exception { get; set; }
        
        [MaxLength(200)]
        public string? MachineName { get; set; }
        
        public int? ThreadId { get; set; }
        
        public int? ProcessId { get; set; }
        
        [MaxLength(100)]
        public string? CorrelationId { get; set; }
        
        [MaxLength(100)]
        public string? UserId { get; set; }
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        [MaxLength(100)]
        public string? Properties { get; set; }
    }
} 