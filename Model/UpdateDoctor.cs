using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class UpdateDoctor
    {
        public string Address { get; set; }
        public Int64 Pincode { get; set; }
        public string City { get; set; }
        public string ProfileIamge { get; set; }
        public string Aadharcard { get; set; }
        public string Degree { get; set; }
        public string SpecialistIn { get; set; }
        public string Clinic { get; set; }
        public string EmailId { get; set; }
    }
}
