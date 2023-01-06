using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class PendingAppointmentList
    {
        public string  BookLabId { get; set; }
        public string LabId { get; set; }
        public string LabName { get; set; }
        public string LabLogo { get; set; }
        public string BookDate { get; set; }
        public string TimeSlot { get; set; }
        public string TestDate { get; set; }
        public string BookStatus { get; set; }
        public string BookMode { get; set; }
        public string AppointmentType { get; set; }
        public string TestId { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }

    }
}
