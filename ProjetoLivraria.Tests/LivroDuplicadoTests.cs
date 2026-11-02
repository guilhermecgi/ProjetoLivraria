using System.Net;
using System.Reflection;
using MySql.Data.MySqlClient;
using ProjetoLivraria.Controllers;
using ProjetoLivraria.Models;
using Xunit;

namespace ProjetoLivraria.Tests;

public class LivroDuplicadoTests
{
    [Theory]
    [InlineData(false, "29.90")]
    [InlineData(true, "29.90")]
    [InlineData(false, "29,90")]
    [InlineData(true, "29,90")]
    public async Task IsbnDuplicadoRetornaErroEPreservaDados(bool concorrente, string valor)
    {
        await using var app = new LivrariaFactory();
        if (concorrente)
            app.Livros.ErroAoCadastrar = ErroMySql(1062);
        else
            app.Livros.Cadastrados.Add(new LivroModel { Isbn = "9788535914849", Nome = "Existente", Valor = 10 });
        using var client = app.CriarCliente();
        await LivrariaFactory.LoginAsync(client);
        var token = await LivrariaFactory.TokenAsync(client, "/Livro/Cadastrar");
        var response = await client.PostAsync("/Livro/Cadastrar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Isbn"] = "9788535914849", ["Nome"] = "Dados preservados", ["Descricao"] = "Texto preservado", ["Valor"] = valor,
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("Este ISBN já está cadastrado", html);
        Assert.Contains("9788535914849", html);
        Assert.Contains("Dados preservados", html);
        Assert.Contains("Texto preservado", html);
        Assert.Matches("name=\"Valor\"[^>]*value=\"29[.]90\"", html);
        Assert.Equal(concorrente ? 0 : 1, app.Livros.Cadastrados.Count);
    }

    [Fact]
    public void ErroDiferenteDeChaveDuplicadaNaoEApresentadoComoIsbnDuplicado()
    {
        var repo = new LivrosEmMemoria { ErroAoCadastrar = ErroMySql(1042) };
        var controller = new LivroController(repo);
        var exception = Assert.Throws<MySqlException>(() => controller.Cadastrar(new LivroModel { Isbn = "123", Nome = "Livro", Valor = 10 }));
        Assert.Equal(1042, exception.Number);
    }

    // O conector não expõe um construtor público com número de erro.
    // A reflexão fica apenas no teste e simula a resposta 1062 do servidor.
    private static MySqlException ErroMySql(int numero) => (MySqlException)Activator.CreateInstance(
        typeof(MySqlException), BindingFlags.Instance | BindingFlags.NonPublic, null,
        ["Erro MySQL simulado", numero], null)!;
}
