﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class SpeakerController : MultiTenantController
    {
        public ActionResult Index()
        {
            return View("Index", Tenant);
        }
        
    }
}