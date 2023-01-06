using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class PrescriptionAppoinmentBooking
    {
        public string LabId { get; set; }
        public string TimeSlot { get; set; }
        public string TestDate { get; set; }
        public string Testprices { get; set; }
        public string AppointmentType { get; set; }
        public string PrescriptionImg { get; set; }
        public string SampleCollectionAddress { get; set; }
        public string SlotId { get; set; }

    }
}
