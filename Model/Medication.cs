using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class Medication
    {
        public string TabletName { get; set; }

        public string  Reason { get; set; }

        public string  DailyDose  { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
    }
}
