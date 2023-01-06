using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Controllers
{
    public class UserDataModel
    {
        //[Required]
        //public int Id { get; set; }
        //[Required]
        //public string Name { get; set; }
        //[Required]
        //public string About { get; set; }

        [Required]
        public IFormFile ProfileImage { get; set; }
        
    }
}
