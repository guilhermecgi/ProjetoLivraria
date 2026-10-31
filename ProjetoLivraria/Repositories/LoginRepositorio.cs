using MySql.Data.MySqlClient;
using ProjetoLivraria.Models;

namespace ProjetoLivraria.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly string _conexao;

        public LoginRepository(IConfiguration configuration)
        {
            _conexao = configuration.GetConnectionString("DefaultConnection");
        }

        public LoginModel BuscarPorEmail(string email)
        {
            LoginModel usuario = null;

            using (MySqlConnection conexao = new MySqlConnection(_conexao))
            {
                conexao.Open();

                string sql = @"SELECT id, nome, email, senhahash
                               FROM login
                               WHERE email = @Email";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@Email", email);

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new LoginModel
                            {
                                Id = reader.GetInt32("id"),
                                Nome = reader.GetString("nome"),
                                Email = reader.GetString("email"),
                                SenhaHash = reader.GetString("senhahash")
                            };
                        }
                    }
                }
            }

            return usuario;
        }
    }
}