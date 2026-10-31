using ProjetoLivraria.Models;

namespace ProjetoLivraria.Repositories
{
    public interface ILoginRepository
    {
        LoginModel BuscarPorEmail(string email);
    }
}