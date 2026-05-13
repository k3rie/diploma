using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackOffice.Web.Models
{
    public class BreadCrumb
    {
        public BreadCrumb(string Url,string Name)
        {
            this.Url = Url;
            this.Name = Name;
        }
        public string Url;
        public string Name;
    }
}