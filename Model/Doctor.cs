using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class Doctor
    {
        public string UserId { get; set; }
        public string FullName { get; set; }

        public string Mobile { get; set; }

        public string EmailId { get; set; }

        public string Gender { get; set; }

        public string BirthDate { get; set; }

        public string Address { get; set; }

        public string Country { get; set; }

        public string Pincode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ProfileIamge { get; set; }

        public string Degree { get; set; }

        public string SpecialistIn { get; set; }

        public string Clinic { get; set; }

    }
}
