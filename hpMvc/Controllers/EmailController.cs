using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace hpMvc.Controllers
{
    public class EmailController : Controller
    {
        //
        // GET: /Email/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SendTestEmail()
        {
            MailMessage mm = new MailMessage();
            mm.IsBodyHtml = true;
            mm.Subject = "Test email";
            mm.From = new MailAddress("jojo@jjj.com");
            mm.To.Add("j.rezuke@verizon.net");

            AlternateView av = AlternateView.CreateAlternateViewFromString("<img alt='' hspace=0 src='cid:mailLogoID' align=baseline /> <h2>Hello</h2><br /> <div>How about this!</div>", null, "text/html");

            string path = Path.Combine(Server.MapPath("~/Content/Images"), "mailLogo.gif");

            LinkedResource mailLogo = new LinkedResource(path);
            mailLogo.ContentId = "mailLogoID";
            av.LinkedResources.Add(mailLogo);
                        
            //AlternateView av3 = new AlternateView(@"C:\0\Work\Halfpint\hpMvc\hpMvc\Content\Images\mailLogo.gif", MediaTypeNames.Image.Gif);
            //av3.ContentId = "mailLogoID";
            //av3.TransferEncoding = TransferEncoding.Base64;
            
            mm.AlternateViews.Add(av);
            //mm.AlternateViews.Add(av3);
            

            SmtpClient smtp = new SmtpClient();
            smtp.Send(mm);
            return RedirectToAction("Index");
        }

    }
}
