using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;

namespace SmartRecycle.Models
{
    [Table("Detections")]
    public class Detection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public int GlassBottles { get; set; }

        [Required]
        public int PlasticBottles { get; set; }

        [Required]
        public int Cans { get; set; }

        [Required]
        public int TotalItems { get; set; }



    }
}