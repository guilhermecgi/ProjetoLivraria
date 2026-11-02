# Sistema Livraria

Aplicação ASP.NET Core MVC para usuários autenticados cadastrarem e consultarem livros.

## Tecnologias

- .NET 10, C#, Razor e Bootstrap já presentes no projeto.
- MySql.Data 26.10.0 e Microsoft.Extensions.Identity.Core 10.0.14.
- MySQL, banco `dblivraria`, repositórios existentes e autenticação por cookies.

## Executar

Abra o terminal na pasta que contém `ProjetoLivraria.slnx` (a subpasta `ProjetoLivraria` da pasta de trabalho `ProjectLivraria`).

1. Instale o SDK .NET 10 e inicie o MySQL na porta configurada.
2. Em uma instalação nova, execute `script.txt` no MySQL para criar `dblivraria` e as tabelas. O script preserva tabelas existentes; não é necessário recriar um banco já configurado.
3. Configure a conexão local sem salvar credenciais no Git. A aplicação mantém a chave `ConnectionStrings:DefaultConnection`. No PowerShell, uma variável de ambiente pode substituir a configuração existente apenas na sessão:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = Read-Host 'Informe sua conexão MySQL local' -MaskInput
   ```

   A conexão deve indicar servidor, porta, banco `dblivraria` e suas credenciais locais. O `appsettings.json` preexistente foi preservado.

4. Restaure os pacotes e compile:

   ```powershell
   dotnet restore
   dotnet build --no-restore
   dotnet test --no-build --no-restore
   ```

5. Inicie a aplicação:

   ```powershell
   dotnet run --project ProjetoLivraria/ProjetoLivraria.csproj --launch-profile http
   ```

6. Acesse [http://localhost:5117](http://localhost:5117). A Home exige login e redireciona para `/Conta/Login`.
7. Entre com um usuário existente na tabela `login`. O campo `senhahash` precisa conter um hash compatível com `PasswordHasher<LoginModel>` do ASP.NET Core Identity; o campo `senha` não é usado para autenticar. O script não cria usuários e esta etapa não implementa cadastro de contas.

Depois do login, a Home exibe seu nome e os acessos **Cadastrar Livros** e **Listar Livros**. O formulário aceita valores positivos até R$ 99.999.999,99 e duas casas decimais. A descrição é opcional e respeita a capacidade da coluna TEXT existente. A listagem usa moeda brasileira.

## Verificações

O projeto `ProjetoLivraria.Tests` contém testes xUnit. Eles executam o MVC, as Razor Views, os cookies, os hashes de senha e a antifalsificação reais em um servidor de testes. Apenas os repositórios externos são substituídos por dados em memória. As contas e senhas desses testes são fictícias e não funcionam no banco local.

Os testes automatizados não exigem MySQL e **não comprovam persistência no banco real**. Consulte [o roteiro de testes](docs/TESTES.md) para repetir as verificações de integração e [o relatório da implementação](docs/IMPLEMENTACAO.md) para arquivos e limitações.
