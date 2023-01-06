using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class WaterInTake
    {
        [Required]
        public string Water_cosumtion { get; set; }
        [Required]
        public string time { get; set; }
        [Required]
        public string date { get; set; }

        //[Required]
        //public string Intake_Goal { get; set; }
    }
}
