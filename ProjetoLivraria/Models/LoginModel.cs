namespace ProjetoLivraria.Models
{
    public class LoginModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Senha { get; set; }
        public string SenhaHash { get; set; } = string.Empty;
    }
}
