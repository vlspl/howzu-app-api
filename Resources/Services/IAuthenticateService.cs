using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLS_API.Model;

namespace VLS_API.Services
{
   public interface IAuthenticateService
    {
        User Authenticate(string userName, string password);
    }
}
