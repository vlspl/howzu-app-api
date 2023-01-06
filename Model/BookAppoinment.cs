using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class BookAppoinment
    {
        public string LabId { get; set; }
        public string DoctorId { get; set; }
        public string TimeSlot { get; set; }
        public string TestDate { get; set; }
        public string TotalAmount { get; set; }
        public string AppointmentType { get; set; }

        public string SampleCollectionAddress { get; set; }
        public string TestCount { get; set; }
        public string TestId { get; set; }
        public string TestPrice { get; set; }
        public string SlotId { get; set; }
        public string PaymentMethod { get; set; }
    }
}
