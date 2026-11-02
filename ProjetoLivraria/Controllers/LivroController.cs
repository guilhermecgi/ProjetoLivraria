using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoLivraria.Models;
using ProjetoLivraria.Repositories;

namespace ProjetoLivraria.Controllers
{
    [Authorize]
    public class LivroController : Controller
    {
        private readonly ILivroRepository _livroRepository;

        public LivroController(ILivroRepository livroRepository)
        {
            _livroRepository = livroRepository;
        }

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

            try
            {
                if (_livroRepository.IsbnExiste(livro.Isbn))
                {
                    ModelState.AddModelError(nameof(livro.Isbn), "Este ISBN já está cadastrado");
                    return View(livro);
                }

                _livroRepository.Cadastrar(livro);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(nameof(livro.Isbn), "Este ISBN já está cadastrado");
                return View(livro);
            }

            TempData["MensagemSucesso"] = "Livro cadastrado com sucesso!";
            return RedirectToAction(nameof(Cadastrar));
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return View(_livroRepository.Listar());
        }
    }
}
