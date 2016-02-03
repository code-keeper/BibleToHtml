using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace bible.Controllers
{
    public class DownloadBibleController : Controller
    {
        //
        // GET: /DownloadBible/

        public ActionResult DownloadBible()
        {
			string url = "http://piibel.net/.xml?q=1Ms%202";
			WebClient client = new WebClient();
			client.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
			client.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			string data = client.DownloadString(url);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(data);
            return Content("");
        }

    }
}
