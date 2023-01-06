using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class UpdatePatientToDoctor
    {
              public string Degree { get; set; }

        public string SpecialistIn { get; set; }

        public string Clinic { get; set; }
        public string Mobile { get; set; }
    }
}
