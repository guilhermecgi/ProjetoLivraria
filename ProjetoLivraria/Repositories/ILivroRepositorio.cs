using ProjetoLivraria.Models;

namespace ProjetoLivraria.Repositories
{
    public interface ILivroRepository
    {
        void Cadastrar(LivroModel livro);

        List<LivroModel> Listar();

        bool IsbnExiste(string isbn);
    }
}