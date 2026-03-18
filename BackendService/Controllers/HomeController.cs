using System.Diagnostics;
using BackendService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackendService.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Ok(new
            {
                status = "ok",
                message = "BackendService is running",
                timestamp = DateTime.UtcNow
            });
        }

        public IActionResult Privacy()
        {
            return Ok(new { message = "Privacy endpoint" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return Problem(detail: "An unexpected error occurred.", title: "Server Error");
        }
    }
}
