using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Services
{
    public class CalculateAge
    {
       public  Dictionary<string, string> CalculateYourAge(DateTime Dob, DateTime CurrentDate)
        {
            Dictionary<string, string> age = new Dictionary<string, string>();
            DateTime dateOfBirth;
            DateTime.TryParse(Dob.ToString(), out dateOfBirth);
            DateTime currentDate = CurrentDate;
            TimeSpan difference = currentDate.Subtract(dateOfBirth);
            DateTime Age = DateTime.MinValue + difference;
            int ageInYears = Age.Year - 1;
            int ageInMonths = Age.Month - 1;
            int ageInDays = Age.Day - 1;
            age.Add("Years", ageInYears.ToString());
            age.Add("Months", ageInMonths.ToString());
            age.Add("Days", ageInDays.ToString());
            return age;
        }

     

    }
}
