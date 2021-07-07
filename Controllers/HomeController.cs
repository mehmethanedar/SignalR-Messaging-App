using ChatApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatApplication.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext db = new AppDbContext();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult MessagesSent(string receiver, string Sender)
        {
            try
            {
                var messages = db.Messages.Where(x => (x.ReceiverName == receiver && x.SenderName == Sender) || (x.ReceiverName == Sender && x.SenderName == receiver)).ToList();
                List<SelectListItem> model = new List<SelectListItem>();

                foreach (var message in messages)
                {
                    model.Add(new SelectListItem()
                    {
                        Value = message.ReceiverName,
                        Text = message.MessageContent,

                    }); ;
                }
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }            
        }
    }
}