using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ProCoder.Controllers
{
    [Authorize]
    public class FavouritesController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<FavouritesController> _logger;

        public FavouritesController(SqlExerciseScoringContext context, ILogger<FavouritesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Favourites
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Favourites/Index action called");
            
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            
            try
            {
                // Tìm CoderId từ claims - cách đáng tin cậy nhất
                var coderIdClaim = User.FindFirst("CoderId")?.Value;
                if (!string.IsNullOrEmpty(coderIdClaim) && int.TryParse(coderIdClaim, out int coderId))
                {
                    _logger.LogInformation("Found CoderId from claims: {CoderId}", coderId);
                    
                    // Tìm coder theo ID
                    var coder = await _context.Coders.FindAsync(coderId);
                    
                    if (coder != null)
                    {
                        _logger.LogInformation("Coder found: {CoderName} (ID: {CoderId})", coder.CoderName, coder.CoderId);
                        
                        // Lấy danh sách bài tập yêu thích
                        var favourites = await _context.Favourites
                            .Include(f => f.Coder)
                            .Include(f => f.Problem)
                                .ThenInclude(p => p.Theme)
                            .Where(f => f.CoderId == coder.CoderId)
                            .ToListAsync();
                        
                        _logger.LogInformation("Found {Count} favourites for coder", favourites.Count);
                        
                        return View(favourites);
                    }
                }
                
                // Nếu không tìm thấy CoderId từ claims, thử cách cũ
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.Identity.Name; // Thử Username
                }
                
                var coderByUsername = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == userId || c.CreatedBy == userId);
                
                if (coderByUsername == null)
                {
                    _logger.LogWarning("Coder not found for user {UserId}", userId);
                    return View(Enumerable.Empty<Favourite>());
                }
                
                var favouritesByUsername = await _context.Favourites
                    .Include(f => f.Coder)
                    .Include(f => f.Problem)
                        .ThenInclude(p => p.Theme)
                    .Where(f => f.CoderId == coderByUsername.CoderId)
                    .ToListAsync();
                
                return View(favouritesByUsername);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favourite problems: {ErrorMessage}", ex.Message);
                return View(Enumerable.Empty<Favourite>());
            }
        }

        // GET: Favourites/Details/5
        public async Task<IActionResult> Details(int? coderId, int? problemId)
        {
            if (coderId == null || problemId == null)
            {
                return NotFound();
            }

            var favourite = await _context.Favourites
                .Include(f => f.Coder)
                .Include(f => f.Problem)
                .FirstOrDefaultAsync(m => m.CoderId == coderId && m.ProblemId == problemId);
            
            if (favourite == null)
            {
                return NotFound();
            }

            return View(favourite);
        }

        // POST: Favourites/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int problemId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CreatedBy == userId);
            
            if (coder == null)
            {
                return NotFound();
            }

            // Check if favourite already exists
            var existingFavourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.CoderId == coder.CoderId && f.ProblemId == problemId);
            
            if (existingFavourite != null)
            {
                // Already favourited, so remove it
                _context.Favourites.Remove(existingFavourite);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Problems", new { id = problemId });
            }

            // Create new favourite
            var favourite = new Favourite
            {
                CoderId = coder.CoderId,
                ProblemId = problemId
            };

            _context.Add(favourite);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", "Problems", new { id = problemId });
        }

        // POST: Favourites/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int coderId, int problemId)
        {
            var favourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.CoderId == coderId && f.ProblemId == problemId);
            
            if (favourite != null)
            {
                _context.Favourites.Remove(favourite);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoint to toggle favourite status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavourite(int problemId)
        {
            _logger.LogInformation("ToggleFavourite action called for problem ID: {ProblemId}", problemId);
            
            // Log whether the user is authenticated
            _logger.LogInformation("User authenticated: {IsAuthenticated}", User.Identity.IsAuthenticated);
            
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated trying to toggle favorite");
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng yêu thích" });
            }
            
            try
            {
                // Log all available claims for debugging
                _logger.LogInformation("Available claims:");
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("Claim Type: {ClaimType}, Value: {ClaimValue}", claim.Type, claim.Value);
                }
                
                // Try multiple ways to get user ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.FindFirst("sub")?.Value; // Try "sub" claim
                }
                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.Identity.Name; // Try Username
                }
                
                // Try to get CoderId directly from claims - this is the most reliable way
                var coderIdClaim = User.FindFirst("CoderId")?.Value;
                if (!string.IsNullOrEmpty(coderIdClaim) && int.TryParse(coderIdClaim, out int coderId))
                {
                    _logger.LogInformation("Found CoderId directly from claims: {CoderId}", coderId);
                    
                    // Find the coder by ID
                    var coder = await _context.Coders.FindAsync(coderId);
                    
                    if (coder != null)
                    {
                        _logger.LogInformation("Coder found by ID: {CoderId}, {CoderName}", coder.CoderId, coder.CoderName);
                        
                        // Continue with checking if problem exists
                        var problem = await _context.Problems.FindAsync(problemId);
                        if (problem == null)
                        {
                            _logger.LogWarning("Problem not found: {ProblemId}", problemId);
                            return Json(new { success = false, message = "Không tìm thấy bài tập" });
                        }

                        _logger.LogInformation("Problem found: {ProblemId}, {ProblemName}", problem.ProblemId, problem.ProblemName);

                        var existingFavourite = await _context.Favourites
                            .FirstOrDefaultAsync(f => f.CoderId == coder.CoderId && f.ProblemId == problemId);

                        bool isFavourited;
                        
                        if (existingFavourite != null)
                        {
                            // Remove favourite
                            _logger.LogInformation("Removing existing favorite");
                            _context.Favourites.Remove(existingFavourite);
                            isFavourited = false;
                        }
                        else
                        {
                            // Add favourite
                            _logger.LogInformation("Adding new favorite");
                            var favourite = new Favourite
                            {
                                CoderId = coder.CoderId,
                                ProblemId = problemId
                            };
                            _context.Add(favourite);
                            isFavourited = true;
                        }
                        
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Favorite toggled successfully. Is favorited: {IsFavourited}", isFavourited);
                        
                        return Json(new { success = true, isFavourited });
                    }
                }
                
                // If we get here, we couldn't find a valid coder
                _logger.LogWarning("No valid coder found for the authenticated user");
                return Json(new { success = false, message = "Không thể xác định người dùng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite: {ErrorMessage}", ex.Message);
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // GET API: Trả về danh sách ID của các bài tập yêu thích
        [HttpGet]
        public async Task<IActionResult> GetFavourites()
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated trying to get favorites");
                return Json(new { success = false, message = "User not authenticated" });
            }
            
            try
            {
                // Try to get CoderId directly from claims
                var coderIdClaim = User.FindFirst("CoderId")?.Value;
                if (!string.IsNullOrEmpty(coderIdClaim) && int.TryParse(coderIdClaim, out int coderId))
                {
                    _logger.LogInformation("Found CoderId from claims: {CoderId}", coderId);
                    
                    // Find the coder by ID
                    var coder = await _context.Coders.FindAsync(coderId);
                    
                    if (coder != null)
                    {
                        _logger.LogInformation("Getting favorites for coder: {CoderName} (ID: {CoderId})", coder.CoderName, coder.CoderId);
                        
                        var favouriteProblemIds = await _context.Favourites
                            .Where(f => f.CoderId == coder.CoderId)
                            .Select(f => f.ProblemId)
                            .ToListAsync();
                        
                        _logger.LogInformation("Found {Count} favorites", favouriteProblemIds.Count);
                        return Json(new { success = true, favourites = favouriteProblemIds });
                    }
                }
                
                // If we couldn't get CoderId from claims, fall back to the old way
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.Identity.Name; // Try Username
                }
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found");
                    return Json(new { success = false, message = "User not found" });
                }
                
                var fallbackCoder = await _context.Coders.FirstOrDefaultAsync(c => c.CreatedBy == userId);
                
                if (fallbackCoder == null)
                {
                    _logger.LogWarning("Coder not found with any ID method");
                    return Json(new { success = false, message = "User not found" });
                }

                var fallbackFavourites = await _context.Favourites
                    .Where(f => f.CoderId == fallbackCoder.CoderId)
                    .Select(f => f.ProblemId)
                    .ToListAsync();
                
                _logger.LogInformation("Found {Count} favorites using fallback method", fallbackFavourites.Count);
                return Json(new { success = true, favourites = fallbackFavourites });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites: {ErrorMessage}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 