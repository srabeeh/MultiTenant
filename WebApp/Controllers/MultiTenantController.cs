using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class MultiTenantController: Controller
    {
        public Tenant Tenant
        {
            get
            {
                object multitenant;
                if (!Request.GetOwinContext().Environment.TryGetValue("MultiTenant", out multitenant))
                {
                    throw new ApplicationException("Could not find tenant");
                }

                return (Tenant) multitenant;
            }
       
        }
    }
}