using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VLS_API.Model
{
   
    public class UserRegister
    {

       [Required]
        public string FullName { get; set; }

        [Required]
        public string Mobile { get; set; }
       
        public string EmailId { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string BirthDate { get; set; }

        public string HealthId { get; set; }

        public string Aadharnumber { get; set; }

        public int Pincode { get; set; }

        public string ChannelPartnerCode { get; set; }


    }
}
