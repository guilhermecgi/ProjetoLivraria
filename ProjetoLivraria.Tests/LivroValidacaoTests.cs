using System.ComponentModel.DataAnnotations;
using ProjetoLivraria.Models;
using Xunit;

namespace ProjetoLivraria.Tests;

public class LivroValidacaoTests
{
    [Theory]
    [InlineData("a", 65535, true)]
    [InlineData("a", 65536, false)]
    [InlineData("á", 32767, true)]
    [InlineData("á", 32768, false)]
    [InlineData("📚", 16383, true)]
    [InlineData("📚", 16384, false)]
    public void DescricaoRespeitaCapacidadeEmBytesDoCampoText(string caractere, int repeticoes, bool valido)
    {
        var model = new LivroModel
        {
            Isbn = "123", Nome = "Livro", Valor = 10,
            Descricao = string.Concat(Enumerable.Repeat(caractere, repeticoes))
        };
        Assert.Equal(valido, Validator.TryValidateObject(model, new ValidationContext(model), [], true));
    }

    [Theory]
    [InlineData("", "Livro", "10", "Isbn")]
    [InlineData("   ", "Livro", "10", "Isbn")]
    [InlineData("123456789012345678901", "Livro", "10", "Isbn")]
    [InlineData("123", "", "10", "Nome")]
    [InlineData("123", "   ", "10", "Nome")]
    [InlineData("123", "Livro", "0", "Valor")]
    [InlineData("123", "Livro", "-1", "Valor")]
    [InlineData("123", "Livro", "100000000", "Valor")]
    public void DadosInvalidosSaoRejeitados(string isbn, string nome, string valor, string campo)
    {
        var model = new LivroModel { Isbn = isbn, Nome = nome, Valor = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture) };
        var errors = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(model, new ValidationContext(model), errors, true));
        Assert.Contains(errors, error => error.MemberNames.Contains(campo));
    }

    [Fact]
    public void NomeAcimaDe150CaracteresERejeitado()
    {
        var model = new LivroModel { Isbn = "123", Nome = new string('A', 151), Valor = 10 };
        Assert.False(Validator.TryValidateObject(model, new ValidationContext(model), [], true));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("99999999.99")]
    public void LimitesValidosEDescricaoOpcionalSaoAceitos(string valor)
    {
        var model = new LivroModel { Isbn = new string('1', 20), Nome = new string('A', 150), Descricao = null, Valor = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture) };
        Assert.True(Validator.TryValidateObject(model, new ValidationContext(model), [], true));
    }
}
