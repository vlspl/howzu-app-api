using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class UserHydration
    {
        [Required]
        public float Weight_Kg { get; set; }
        [Required]
        public string Wakeuptime { get; set; }
        [Required]
        public string Bedtime { get; set; }

        public string Gender { get; set; }

        public string Intakegoal { get; set; }
        public string ActionStatus { get; set; }
    }
}
