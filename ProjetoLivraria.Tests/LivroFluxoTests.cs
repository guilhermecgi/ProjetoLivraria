using System.Net;
using Xunit;

namespace ProjetoLivraria.Tests;

public class LivroFluxoTests
{
    [Fact]
    public async Task DescricaoAcimaDaCapacidadeDaTabelaNaoSalva()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var response = await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Isbn"] = "123", ["Nome"] = "Livro", ["Valor"] = "10", ["Descricao"] = new string('a', 65536),
            ["__RequestVerificationToken"] = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar")
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field-validation-error", await response.Content.ReadAsStringAsync());
        Assert.Empty(app.Livros.Cadastrados);
    }

    [Theory]
    [InlineData("Isbn")]
    [InlineData("Nome")]
    [InlineData("Valor")]
    public async Task CampoObrigatorioAusenteNaoSalva(string campo)
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var dados = new Dictionary<string, string>
        {
            ["Isbn"] = "123", ["Nome"] = "Livro", ["Valor"] = "10",
            ["__RequestVerificationToken"] = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar")
        };
        dados.Remove(campo);
        var response = await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(dados));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field-validation-error", await response.Content.ReadAsStringAsync());
        Assert.Empty(app.Livros.Cadastrados);
    }

    [Fact]
    public async Task FluxoDeCadastroApareceNaListagemEBloqueiaSegundoEnvio()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var dados = new Dictionary<string, string>
        {
            ["Isbn"] = "9788535914849", ["Nome"] = "Acervo de teste", ["Valor"] = "49.90",
            ["__RequestVerificationToken"] = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar")
        };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(dados))).StatusCode);
        Assert.Null(Assert.Single(app.Livros.Cadastrados).Descricao);
        var lista = WebUtility.HtmlDecode(await client.GetStringAsync("/Livro/Listar")).Replace('\u00a0', ' ');
        Assert.Contains("Acervo de teste", lista);
        Assert.Contains("R$ 49,90", lista);
        Assert.Contains("Sem descrição", lista);
        dados["__RequestVerificationToken"] = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar");
        var repetido = await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(dados));
        Assert.Equal(HttpStatusCode.OK, repetido.StatusCode);
        Assert.Contains("Este ISBN já está cadastrado", WebUtility.HtmlDecode(await repetido.Content.ReadAsStringAsync()));
        Assert.Single(app.Livros.Cadastrados);
    }

    [Theory]
    [InlineData("29.90")]
    [InlineData("29,90")]
    public async Task CadastroValidoSalvaRedirecionaEAtualizarNaoReenvia(string valor)
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var token = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar");
        var response = await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Isbn"] = "9788535914849", ["Nome"] = "Livro de teste", ["Descricao"] = "Descrição de teste", ["Valor"] = valor,
            ["Id"] = "999", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Livro/Cadastrar", response.Headers.Location?.ToString());
        var salvo = Assert.Single(app.Livros.Cadastrados);
        Assert.Equal("9788535914849", salvo.Isbn);
        Assert.Equal("Livro de teste", salvo.Nome);
        Assert.Equal("Descrição de teste", salvo.Descricao);
        Assert.Equal(29.90m, salvo.Valor);
        Assert.Equal(0, salvo.Id);
        var html = WebUtility.HtmlDecode(await client.GetStringAsync(response.Headers.Location));
        Assert.Contains("Livro cadastrado com sucesso!", html);
        await client.GetAsync("/Livro/Cadastrar");
        Assert.Single(app.Livros.Cadastrados);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("100000000")]
    [InlineData("1.234")]
    public async Task ValorInvalidoRetornaFormularioSemSalvar(string valor)
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var token = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar");
        var response = await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Isbn"] = "123", ["Nome"] = "Livro preservado", ["Valor"] = valor, ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("field-validation-error", html);
        Assert.Contains("Livro preservado", html);
        Assert.Empty(app.Livros.Cadastrados);
    }

    [Fact]
    public async Task CadastroExibeFormularioComTokenECampos()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var response = await client.GetAsync("/Livro/Cadastrar");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        foreach (var campo in new[] { "Isbn", "Nome", "Descricao", "Valor", "__RequestVerificationToken" })
            Assert.Contains($"name=\"{campo}\"", html);
        Assert.Contains("action=\"/Livro/Cadastrar\"", html);
        Assert.Contains("method=\"post\"", html);
        Assert.Contains("Salvar Livro", html);
        Assert.Contains("href=\"/\"", html);
    }
}
