using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class AddOldReport
    {
        public string TestName { set; get; }
        public string RefDoctor { set; get; }
        public string TestDate { set; get; }
        public string ReportPath { set; get; }
        public string Notes { set; get; }
        public string LabName { get; set; }
        public int ReturnData { get; set; }  

        public List<OldReportParameterList> ParameterDetails { set; get; }
      
    }
}
