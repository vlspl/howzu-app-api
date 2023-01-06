using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class SuggestTestBookAppoinment
    {
        public int RcomId { get; set; }
        public int LabId { get; set; }
        public int DoctorId { get; set; }
        public string TimeSlot { get; set; }
        public string TestDate { get; set; }
        public string TotalAmount { get; set; }
        public string AppointmentType { get; set; }
        public string SampleCollectionAddress { get; set; }
        public string TestCount { get; set; }
        public string TestId { get; set; }
        public string TestPrice { get; set; }
        public string PaymentMethod { get; set; }
    }
}
