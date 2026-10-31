namespace ProjetoLivraria.Models
{
    public class LoginModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string? Senha { get; set; }
        public string SenhaHash { get; set; }
    }
}
