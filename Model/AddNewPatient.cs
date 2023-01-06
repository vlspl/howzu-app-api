using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class AddNewPatient
    {

        public string FullName { get; set; }

       
        public string Mobile { get; set; }

        public string EmailId { get; set; }

        //[Required]
        //public string Password { get; set; }

       
        public string Gender { get; set; }

       
        public string BirthDate { get; set; }

       
        public string Age { get; set; }


      
        public string Address { get; set; }

        public string HealthId { get; set; }

        public string Aadharnumber { get; set; }

        public string Pincode { get; set; }

        //public string ChannelPartnerCode { get; set; }




    }
}
