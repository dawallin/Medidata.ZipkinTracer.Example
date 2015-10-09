using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Medidata.ZipkinTracer.Core;

namespace Medidata.ZipkinTracer.Example.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int method = 0)
        {
            ViewBag.Title = "Home Page";

            string result;

            switch (method)
            {
                case 1:
                    result = A();
                    break;
                case 2:
                    result = B();
                    break;
                case 3:
                    result = C();
                    break;
                default:
                    result = "";
                    break;
            }

            ViewBag.Result = result;

            return View();
        }

        public string A()
        {
            return "a";
        }

        public string B()
        {
            var url =  Url.Action("A", "Home", null, protocol: Request.Url.Scheme);
            var tracer = (ITracerClient)System.Web.HttpContext.Current.Items["zipkinClient"];
            var uri = new Uri(url);
            var span = tracer.StartClientTrace(uri, "B->A");
            var result = new HttpClient().GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            tracer.EndClientTrace(span);
            return "b -> " + result;
        }

        public string C()
        {
            var url1 = Url.Action("B", "Home", null, protocol: Request.Url.Scheme);
            var tracer = (ITracerClient)System.Web.HttpContext.Current.Items["zipkinClient"];
            var uri1 = new Uri(url1);
            var span1 = tracer.StartClientTrace(uri1, "C->B");
            var result1 = new HttpClient().GetAsync(url1).Result.Content.ReadAsStringAsync().Result;
            tracer.EndClientTrace(span1);

            var url2 = Url.Action("A", "Home", null, protocol: Request.Url.Scheme);
            var uri2 = new Uri(url2);
            var span2 = tracer.StartClientTrace(uri2, "C->A");
            var result2 = new HttpClient().GetAsync(url2).Result.Content.ReadAsStringAsync().Result;
            tracer.EndClientTrace(span2);
            return "c -> (" + result1 + ") | (" + result2 + ")";
        }
    }
}
