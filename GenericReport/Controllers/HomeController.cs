﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenericReport.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            //return View();
            return RedirectToAction("TestReport", "Report");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}