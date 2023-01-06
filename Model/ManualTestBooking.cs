using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class ManualTestBooking
    {
        public string  LabName { get; set; }
        public int DoctorId { get; set; }
        public int TestId { get; set; }
        public string Testdate { get; set; }
    }
}
