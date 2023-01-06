using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class VerifyFamilyMemberRequest
    {
        [Required]
        public string FamilyMemberId { get; set; }

        [Required]
        public int OTP { get; set; }
    }
}
