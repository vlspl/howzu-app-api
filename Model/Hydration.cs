using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class Hydration
    {
        [Required]
        public int Height_cm { get; set; }
        [Required]
        public float Weight_Kg { get; set; }
        [Required]
        public string Wakeuptime { get; set; }
        [Required]
        public string Bedtime { get; set; }
        [Required]
        public string Intake_ml { get; set; }
        [Required]
        public string Cupsize_ml { get; set; }
    }
}
