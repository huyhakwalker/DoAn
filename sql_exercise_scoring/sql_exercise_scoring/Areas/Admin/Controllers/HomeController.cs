using Microsoft.AspNetCore.Mvc;
using sql_exercise_scoring.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace sql_exercise_scoring.Areas.Admin.Controllers
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
