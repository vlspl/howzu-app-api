using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class CreateReport
    {
        public int ReportId { get; set; }
        public int TestId { get; set; }
        public string AnalyteName { get; set; }
        public string SubAnalyteName { get; set; }
        public string Specimen { get; set; }
        public string MethodName { get; set; }
        public string ResultType { get; set; }
        public string ReferenceType { get; set; }
        public string AgeGroup { get; set; }
        public string MaleRange { get; set; }
        public string FemaleRange { get; set; }
        public string Grade { get; set; }
        public string Unit { get; set; }
        public string Interpretation { get; set; }
        public string LowerLimit { get; set; }
        public string UpperLimit { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }
        public string ReportPath { get; set; }
        public int BookingId { get; set; }

        public string MinRange { get; set; }
        public string MaxRange { get; set; }


    }
}
