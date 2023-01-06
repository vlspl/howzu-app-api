using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class BMIReport
    {
       
        [Required]
        public string Height { get; set; }
        [Required]
        public string Weight { get; set; }
        [Required]
        public string Result { get; set; }
        [Required]
        public string BMIValue { get; set; }

    }
}
