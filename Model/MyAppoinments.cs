using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class MyAppoinments
    {
        public string BookingId { get; set; }
        public string  TestCode { get; set; }
        public string TestName { get; set; }
        public string LabName { get; set; }
        public string LabId { get; set; }
        public string BookingDate { get; set; }
        public string TestProfileName { get; set; }
        public string BookStatus { get; set; }
    }
}
