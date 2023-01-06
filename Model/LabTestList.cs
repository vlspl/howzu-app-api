using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class LabTestList
    {
        public int LabId { get; set; }

        const int maxPageSize = 20;

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
