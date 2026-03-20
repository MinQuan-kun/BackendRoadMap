//using System.Diagnostics;
//using BackendService.Models;
//using Microsoft.AspNetCore.Mvc;

//namespace BackendService.Controllers
//{
//    public class HomeController : Controller
//    {
//        [HttpGet]
//        public IActionResult Get()
//        {
//            return Ok(new { message = "Backend is running!" });
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}
