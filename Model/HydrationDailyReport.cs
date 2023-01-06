using System.ComponentModel.DataAnnotations;

namespace Howzu_API.Model
{
    public class HydrationDailyReport
    {
        [Required]
        public int HydrationId { get; set; }
        [Required]
        public int Water_ml { get; set; }
        [Required]
        public string Consumetime { get; set; }
    }
}
