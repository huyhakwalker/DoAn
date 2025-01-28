using Microsoft.AspNetCore.Mvc;
using sql_exercise_scoring.Models;
using System.Diagnostics;

namespace sql_exercise_scoring.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlExerciseScoringContext _context;

        public HomeController(ILogger<HomeController> logger, SqlExerciseScoringContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
