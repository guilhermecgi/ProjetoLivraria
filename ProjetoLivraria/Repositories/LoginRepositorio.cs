using MySql.Data.MySqlClient;
using ProjetoLivraria.Models;

namespace ProjetoLivraria.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly string _conexao;

        public LoginRepository(IConfiguration configuration)
        {
            _conexao = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Configure a conexão MySQL DefaultConnection.");
        }

        public LoginModel? BuscarPorEmail(string email)
        {
            using var conexao = new MySqlConnection(_conexao);
            conexao.Open();

            const string sql = @"SELECT id, nome, email, senhahash
                                 FROM login
                                 WHERE email = @Email
                                 LIMIT 1";

            using var comando = new MySqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Email", email);
            using var reader = comando.ExecuteReader();
            if (!reader.Read()) return null;

            return new LoginModel
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                Email = reader.GetString("email"),
                SenhaHash = reader.GetString("senhahash")
            };
        }

        public void Cadastrar(LoginModel usuario)
        {
            using var conexao = new MySqlConnection(_conexao);
            conexao.Open();

            const string sql = @"INSERT INTO login (nome, email, senhahash)
                                 VALUES (@Nome, @Email, @SenhaHash)";
            using var comando = new MySqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Nome", usuario.Nome);
            comando.Parameters.AddWithValue("@Email", usuario.Email);
            comando.Parameters.AddWithValue("@SenhaHash", usuario.SenhaHash);
            comando.ExecuteNonQuery();
        }
    }
}
