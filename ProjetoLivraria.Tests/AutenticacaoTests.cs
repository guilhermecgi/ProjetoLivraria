using System.Net;
using Xunit;

namespace ProjetoLivraria.Tests;

public class AutenticacaoTests
{
    [Theory]
    [InlineData("/Home/Index")]
    [InlineData("/Livro/Cadastrar")]
    [InlineData("/Livro/Listar")]
    public async Task PaginasProtegidasExigemCookie(string path)
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Conta/Login", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("leitora@example.test", "senha-incorreta")]
    [InlineData("inexistente@example.test", LivrariaFactory.SenhaDeTeste)]
    public async Task CredenciaisIncorretasExibemErroESeguemSemAutenticacao(string email, string senha)
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        var token = await LivrariaFactory.TokenAsync(client, "/Conta/Login");
        var response = await client.PostAsync("/Conta/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = senha, ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("E-mail ou senha incorretos.", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/")).StatusCode);
    }

    [Theory]
    [InlineData("/Conta/Login")]
    [InlineData("/Livro/Cadastrar")]
    public async Task PostSemAntifalsificacaoERejeitado(string path)
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        if (path.StartsWith("/Livro")) await LivrariaFactory.LoginAsync(client);
        var response = await client.PostAsync(path, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "leitora@example.test", ["Senha"] = LivrariaFactory.SenhaDeTeste,
            ["Isbn"] = "123", ["Nome"] = "Livro", ["Valor"] = "10"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(app.Livros.Cadastrados);
    }

    [Fact]
    public async Task LogoutInvalidaAcessoAHome()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var token = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar");
        var response = await client.PostAsync("/Conta/Logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Conta/Login", response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/")).StatusCode);
    }
}
