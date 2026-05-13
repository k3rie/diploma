using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Diploma.Models.DTOs
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public UserAuthDto User { get; set; }
    }
}