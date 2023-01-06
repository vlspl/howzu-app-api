using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class MedicationList
    {
        const int maxPageSize = 20;

        
       // public String FromDate { get; set; }
       // public String MedName { get; set; }
        // String EndDate { get; set; }
        public string ForWhome { get; set; }

        public int pageNumber { get; set; } = 1;

        private int _pageSize { get; set; } = 10;
        public int pageSize
        {

            get { return _pageSize; }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
        public string Searching { get; set; }

    }
}
