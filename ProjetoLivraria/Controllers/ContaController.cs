using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoLivraria.Models;
using ProjetoLivraria.Repositories;
using System.Security.Claims;

namespace ProjetoLivraria.Controllers
{
    public class ContaController : Controller
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IPasswordHasher<LoginModel> _passwordHasher;

        public ContaController(
            ILoginRepository loginRepository,
            IPasswordHasher<LoginModel> passwordHasher)
        {
            _loginRepository = loginRepository;
            _passwordHasher = passwordHasher;
        }

        // Exibir tela de login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Receber os dados do formulário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Verificar os campos obrigatórios
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar usuário pelo e-mail
            var usuario = _loginRepository.BuscarPorEmail(model.Email);

            // Verificar se o usuário existe
            if (usuario == null)
            {
                ModelState.AddModelError("", "E-mail ou senha incorretos.");
                return View(model);
            }

            // Verificar a senha utilizando o hash
            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.SenhaHash,
                model.Senha
            );

            // Verificar se a senha está correta
            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "E-mail ou senha incorretos.");
                return View(model);
            }

            // Criar as informações do usuário autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var identidade = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identidade);

            // Criar o cookie de autenticação
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // Redirecionar para a página inicial
            return RedirectToAction("Index", "Home");
        }

        // Realizar logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "Conta");
        }
    }
}
