using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoLivraria.Models;

namespace ProjetoLivraria.Controllers
{
    [Authorize]
    public class LivroController : Controller
    {
        [HttpGet]
        public IActionResult Cadastrar()
        {
            return View(new LivroModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar([Bind("Isbn,Nome,Descricao,Valor")] LivroModel livro)
        {
            if (!ModelState.IsValid)
            {
                return View(livro);
            }

            return View(livro);
        }
    }
}
