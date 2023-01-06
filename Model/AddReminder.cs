using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class AddReminder
    {

     
         

        public string MedName { get; set; }

        public string Medfor { get; set; }
        public string MedSdate { get; set; }
       
        
        public int MedDoseDuration { get; set; }
        public int DoseInterval { get; set; }
        public string DoseTime { get; set; }
        public string MedicationStatus { get; set; }
        public string NotificationStatus { get; set; }

        public string ForWhome { get; set; }
        public string ColorCode { get; set; }

    }
    
}
