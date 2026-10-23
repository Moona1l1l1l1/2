using Microsoft.AspNetCore.Mvc;
using UniversityClient.Services;

namespace UniversityClient.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseApiClient _api;

    public CoursesController(ICourseApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await _api.GetCoursesAsync(ct);
        if (!result.IsSuccess)
            ViewBag.Error = result.Error;

        return View(result.Data ?? new List<Models.CourseViewModel>());
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await _api.GetCourseAsync(id, ct);
        if (result.IsNotFound)
            return NotFound();

        if (!result.IsSuccess)
        {
            ViewBag.Error = result.Error;
            return View(model: null);
        }

        return View(result.Data);
    }
}
