using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class DoseHistory
    {
        public int MedicationId { get; set; }
        public int DoseId { get; set; }
        public bool Dosetaken { get; set; }
        public string  Skipreason { get; set; }
        public string Takentime { get; set; }
    }
}
