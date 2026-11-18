using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
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

        [HttpGet]
        public IActionResult Cadastrar()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new CadastroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(CadastroViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            model.Nome = model.Nome?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (_loginRepository.BuscarPorEmail(model.Email) != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
                    return View(model);
                }

                var usuario = new LoginModel
                {
                    Nome = model.Nome,
                    Email = model.Email
                };
                usuario.SenhaHash = _passwordHasher.HashPassword(usuario, model.Senha);
                _loginRepository.Cadastrar(usuario);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
                return View(model);
            }
            catch (MySqlException)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível concluir o cadastro. Verifique a conexão com o banco e tente novamente.");
                return View(model);
            }

            TempData["MensagemSucesso"] = "Cadastro realizado com sucesso! Faça login para continuar.";
            return RedirectToAction(nameof(Login));
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
            LoginModel? usuario;
            try
            {
                usuario = _loginRepository.BuscarPorEmail(model.Email.Trim().ToLowerInvariant());
            }
            catch (MySqlException)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível acessar o banco de dados. Tente novamente.");
                return View(model);
            }

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
