using System.Net;
using ProjetoLivraria.Models;
using Xunit;

namespace ProjetoLivraria.Tests;

public class LivroListagemTests
{
    [Fact]
    public async Task ListaVaziaExibeMensagemERetornoParaHome()
    {
        await using var app = new LivrariaFactory();
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var response = await client.GetAsync("/Livro/Listar");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("Nenhum livro cadastrado", html);
        Assert.Contains("href=\"/\"", html);
        var home = await client.GetStringAsync("/");
        Assert.Contains("href=\"/Livro/Listar\"", home);
    }

    [Fact]
    public async Task ListaExibeTodosOsCamposComMoedaBrasileiraEHtmlEscapado()
    {
        await using var app = new LivrariaFactory();
        app.Livros.Cadastrados.Add(new LivroModel { Isbn = "9788535914849", Nome = "Livro <script>alert(1)</script>", Descricao = "Descrição & detalhes", Valor = 1234.56m });
        app.Livros.Cadastrados.Add(new LivroModel { Isbn = "123", Nome = "Sem descrição", Descricao = null, Valor = 0.01m });
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var response = await client.GetAsync("/Livro/Listar");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("9788535914849", html);
        Assert.Contains("Livro &lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
        var decoded = WebUtility.HtmlDecode(html).Replace('\u00a0', ' ');
        Assert.Contains("Descrição & detalhes", decoded);
        Assert.Contains("Sem descrição", decoded);
        Assert.Contains("R$ 1.234,56", decoded);
        Assert.Contains("R$ 0,01", decoded);
        Assert.Contains("table-responsive", html);
    }
}
