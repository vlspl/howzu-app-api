using System.ComponentModel.DataAnnotations;

namespace Howzu_API.Model
{
    public class FamilyMemberreportlist
    {
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
        [Required]
        public int Familymemberid { get; set; }
    }
}
