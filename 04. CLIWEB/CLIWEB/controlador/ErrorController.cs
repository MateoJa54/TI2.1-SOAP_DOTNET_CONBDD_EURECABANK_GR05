using System.Web.Mvc;

namespace ec.edu.monster.web.controlador
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = 500;
            return View("Error");
        }
    }
}
