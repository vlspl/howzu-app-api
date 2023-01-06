using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class ResheduleAppointment
    {
        public string BookingId { get; set; }
        public string TestDate { get; set; }
        public string TimeSlot { get; set; }
        public string AppinmentType { get; set; }
        public string SampleCollectionAddress { get; set; }

    }
}
