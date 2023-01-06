using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    
    public class Userlogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
       // public int orgId { get; set; }

    }
}
