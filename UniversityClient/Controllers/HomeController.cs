using Microsoft.AspNetCore.Mvc;

namespace UniversityClient.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Courses");

    public IActionResult Error() => Content("Произошла непредвиденная ошибка.");
}
