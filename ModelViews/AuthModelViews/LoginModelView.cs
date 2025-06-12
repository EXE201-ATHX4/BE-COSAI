using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AuthModelViews
{
    public class LoginModelView
    {
       
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
