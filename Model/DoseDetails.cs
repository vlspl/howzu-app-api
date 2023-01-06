using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class DoseDetails
    {
        public int MedicationId { get; set; }
        public string DaySlot { get; set; }
        public string Dosetime { get; set; }
        public string Mealtime { get; set; }
        public string Quantity { get; set; }
    }
}
