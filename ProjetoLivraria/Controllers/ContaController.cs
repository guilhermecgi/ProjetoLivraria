using Microsoft.AspNetCore.Mvc;

namespace ProjetoLivraria.Controllers
{
    public class ContaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
