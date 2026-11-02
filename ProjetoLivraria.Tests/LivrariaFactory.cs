using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProjetoLivraria.Controllers;
using ProjetoLivraria.Models;
using ProjetoLivraria.Repositories;
using Xunit;

namespace ProjetoLivraria.Tests;

public sealed class LivrariaFactory : WebApplicationFactory<HomeController>
{
    public const string SenhaDeTeste = "SenhaFicticiaParaTestes!2026";
    public LivrosEmMemoria Livros { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.RemoveAll<ILoginRepository>();
            services.RemoveAll<ILivroRepository>();
            services.AddSingleton<ILoginRepository, LoginDeTeste>();
            services.AddSingleton<ILivroRepository>(Livros);
        });
    }

    public HttpClient CriarCliente() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public static async Task<string> TokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "O formulário deve emitir um token antifalsificação.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    public static async Task LoginAsync(HttpClient client)
    {
        var token = await TokenAsync(client, "/Conta/Login");
        var response = await client.PostAsync("/Conta/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "leitora@example.test", ["Senha"] = SenhaDeTeste, ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
    }
}

internal sealed class LoginDeTeste : ILoginRepository
{
    private readonly LoginModel usuario;
    public LoginDeTeste()
    {
        usuario = new LoginModel { Id = 1, Nome = "Maria Leitora", Email = "leitora@example.test", SenhaHash = "" };
        usuario.SenhaHash = new PasswordHasher<LoginModel>().HashPassword(usuario, LivrariaFactory.SenhaDeTeste);
    }
    public LoginModel BuscarPorEmail(string email) => email == usuario.Email ? usuario : null!;
}

public sealed class LivrosEmMemoria : ILivroRepository
{
    public List<LivroModel> Cadastrados { get; } = [];
    public Exception? ErroAoCadastrar { get; set; }
    public void Cadastrar(LivroModel livro)
    {
        if (ErroAoCadastrar is not null) throw ErroAoCadastrar;
        Cadastrados.Add(livro);
    }
    public List<LivroModel> Listar() => Cadastrados.ToList();
    public bool IsbnExiste(string isbn) => Cadastrados.Any(livro => livro.Isbn == isbn);
}
