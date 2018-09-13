using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AudioToolNew.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "音频工具";

            return View();
        }
        [HttpPost]
        public ActionResult Test(string path)
        {
            TimeSpan time = TimeSpan.Parse("00:00:00.09");
            string folder = System.IO.Path.GetDirectoryName(path);
            Util.ReadLrc(path);
            return new JsonResult() {Data=true};
        }
    }
}
