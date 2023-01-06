using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class SetNewPassword
    {
        [Required]
        public string Mobile { get; set; }

        [Required]
        public string  Password { get; set; }
    }
}
