using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class LabSlot
    {
        public int LabId { get; set; }

        public string Weekday { get; set; }

        public string AppointmentType { get; set; }
    }
}
