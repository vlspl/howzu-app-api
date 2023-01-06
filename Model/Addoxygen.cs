using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class Addoxygen
    {
        public int Oxygen { get; set; }

        public int PulseRate { get; set; }
        public string Notes { get; set; }
        public string Date { get; set; }
        public string Result { get; set; }
    }
}
