using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class RegisterFamilyMember
    {
        [Required]
        public string FullName { get; set; }
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
        public string Relation { get; set; }
        [Required]
        public int Pincode { get; set; }
        public string ChannelPartnerCode { get; set; }
    }
}
