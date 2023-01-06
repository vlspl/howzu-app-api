using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class AddManualReport
    {
        public List<CreateReport> AnalyteDetails { set; get; }
        public string Notes { set; get; }
    }
}
