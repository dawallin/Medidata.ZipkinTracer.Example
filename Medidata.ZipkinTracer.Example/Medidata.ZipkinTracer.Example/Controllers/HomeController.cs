using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
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
            var a = System.Web.HttpContext.Current.Request.Headers;
            
            return "a";
        }

        public string B()
        {
            var url =  Url.Action("A", "Home", null, protocol: Request.Url.Scheme);
            var result = httpCall(url, "B->A");
            return "b -> " + result;
        }

        public string C()
        {
            var url1 = Url.Action("B", "Home", null, protocol: Request.Url.Scheme);
            var result1 = httpCall(url1, "C->B");

            var url2 = Url.Action("A", "Home", null, protocol: Request.Url.Scheme);
            var result2 = httpCall(url2, "C->A");

            return "c -> (" + result1 + ") | (" + result2 + ")";
        }

        private string httpCall(string url, string methodName)
        {
            using (var client = new HttpClient())
            {
                var tracer = (ITracerClient)System.Web.HttpContext.Current.Items["zipkinClient"];
                var span = tracer.StartClientTrace(new Uri(url), methodName);

                client.DefaultRequestHeaders.Add("X-B3-TraceId", toBitString(span.Trace_id));
                client.DefaultRequestHeaders.Add("X-B3-ParentSpanId", toBitString(span.Parent_id));
                client.DefaultRequestHeaders.Add("X-B3-SpanId", toBitString(span.Id));

                tracer.EndClientTrace(span);

                return client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            }
        }

        private string toBitString(long id)
        {
            return Convert.ToString(id, 16);
        }
    }
}
