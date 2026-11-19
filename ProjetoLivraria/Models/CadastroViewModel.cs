using System.ComponentModel.DataAnnotations;

namespace ProjetoLivraria.Models
{
    public class CadastroViewModel
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve conter entre 2 e 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve conter entre 8 e 100 caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua senha.")]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}
