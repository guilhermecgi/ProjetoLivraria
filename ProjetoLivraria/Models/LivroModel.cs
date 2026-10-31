namespace ProjetoLivraria.Models
{
    public class LivroModel
    {
        public int Id { get; set; }
        public string Isbn { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
    }
}
