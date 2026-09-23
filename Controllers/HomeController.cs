using Microsoft.AspNetCore.Authorization;
using TaskManagementWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new HomeIndexViewModel();
            viewModel.CurrentUserName = User.Identity.Name;

            return View(viewModel);
        }
    }
}