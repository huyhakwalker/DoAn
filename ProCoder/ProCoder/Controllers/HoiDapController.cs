using Microsoft.AspNetCore.Mvc;

namespace ProCoder.Controllers
{
    public class HoiDapController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
