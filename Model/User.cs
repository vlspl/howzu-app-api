using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class User
    {
        public int UserId { get; set; }
        public string OrgId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public string EmailId { get; set; }
        public Boolean MobileVerified { get; set; }
        public Boolean RegistrationStatus { get; set; }
    }
}
