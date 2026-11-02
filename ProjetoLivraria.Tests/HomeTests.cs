using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjetoLivraria.Controllers;
using Xunit;

namespace ProjetoLivraria.Tests;

public class HomeTests
{
    [Fact]
    public async Task HomeSemAutenticacaoRedirecionaParaLogin()
    {
        await using var app = new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services => services.AddDataProtection().UseEphemeralDataProtectionProvider());
        });
        using var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/Home/Index");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("http://localhost/Conta/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public void HomeEnviaNomeDoUsuarioAutenticadoParaView()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Maria Leitora")], "Cookies"))
                }
            }
        };
        Assert.IsType<ViewResult>(controller.Index());
        Assert.Equal("Maria Leitora", (string?)controller.ViewData["NomeUsuario"]);
    }

    [Fact]
    public async Task HomePersonalizadaExibeNomeAposLoginComCookieReal()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("Sistema Livraria", html);
        Assert.Contains("Bem-vindo(a), Maria Leitora!", html);
    }

    [Fact]
    public async Task HomeOfereceLinkParaCadastroDeLivros()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var html = await client.GetStringAsync("/");
        Assert.Contains("href=\"/Livro/Cadastrar\"", html);
        Assert.Contains("Cadastrar Livros", html);
    }
}
