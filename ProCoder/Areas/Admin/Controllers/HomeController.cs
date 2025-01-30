using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public HomeController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
