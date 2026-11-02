namespace ProjetoLivraria.Models
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;
    using ProjetoLivraria.ModelBinders;

    public class LivroModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Informe o ISBN do livro.")]
        [StringLength(20, ErrorMessage = "O ISBN deve ter no máximo 20 caracteres.")]
        [Display(Name = "ISBN")]
        public string Isbn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o nome do livro.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Informe o valor do livro.")]
        [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true,
            ErrorMessage = "O valor deve ser entre R$ 0,01 e R$ 99.999.999,99.")]
        [ModelBinder(BinderType = typeof(ValorMonetarioModelBinder))]
        public decimal Valor { get; set; }
    }
}
