using MySql.Data.MySqlClient;
using ProjetoLivraria.Models;

namespace ProjetoLivraria.Repositories
{
    public class LivroRepository : ILivroRepository
    {
        private readonly string _conexao;

        public LivroRepository(IConfiguration configuration)
        {
            _conexao = configuration.GetConnectionString("DefaultConnection");
        }

        // Cadastrar livro
        public void Cadastrar(LivroModel livro)
        {
            using (MySqlConnection conexao = new MySqlConnection(_conexao))
            {
                conexao.Open();

                string sql = @"INSERT INTO livro
                               (isbn, nome, descricao, valor)
                               VALUES
                               (@Isbn, @Nome, @Descricao, @Valor)";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@Isbn", livro.Isbn);
                    comando.Parameters.AddWithValue("@Nome", livro.Nome);
                    comando.Parameters.AddWithValue("@Descricao",
                        (object)livro.Descricao ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Valor", livro.Valor);

                    comando.ExecuteNonQuery();
                }
            }
        }

        // Listar todos os livros
        public List<LivroModel> Listar()
        {
            List<LivroModel> livros = new List<LivroModel>();

            using (MySqlConnection conexao = new MySqlConnection(_conexao))
            {
                conexao.Open();

                string sql = @"SELECT id, isbn, nome, descricao, valor
                               FROM livro";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LivroModel livro = new LivroModel
                            {
                                Id = reader.GetInt32("id"),
                                Isbn = reader.GetString("isbn"),
                                Nome = reader.GetString("nome"),
                                Descricao = reader.IsDBNull(reader.GetOrdinal("descricao"))
                                    ? null
                                    : reader.GetString("descricao"),
                                Valor = reader.GetDecimal("valor")
                            };

                            livros.Add(livro);
                        }
                    }
                }
            }

            return livros;
        }

        // Verificar se o ISBN já existe
        public bool IsbnExiste(string isbn)
        {
            using (MySqlConnection conexao = new MySqlConnection(_conexao))
            {
                conexao.Open();

                string sql = @"SELECT COUNT(*)
                               FROM livro
                               WHERE isbn = @Isbn";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@Isbn", isbn);

                    int quantidade = Convert.ToInt32(comando.ExecuteScalar());

                    return quantidade > 0;
                }
            }
        }
    }
}