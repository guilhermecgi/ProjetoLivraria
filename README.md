# Sistema Livraria

Projeto ASP.NET Core MVC (.NET 10) com autenticação por cookies, cadastro de usuários e livros em MySQL.

## Preparação

1. Instale o SDK .NET 10 e o MySQL.
2. Execute `script.txt` no MySQL para criar o banco `dblivraria` e as tabelas `login` e `livro` (se ainda não existirem). O e-mail deve ser único na tabela `login`.
3. Configure `ConnectionStrings:DefaultConnection` para o seu MySQL (servidor, porta, banco, usuário e senha). O `appsettings.json` contém os parâmetros locais do projeto original. Não publique essa senha em um repositório público. É possível substituir a conexão por uma variável de ambiente no PowerShell:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3305;Database=dblivraria;Uid=SEU_USUARIO;Pwd=SUA_SENHA;"
   ```

4. Na pasta que contém `ProjetoLivraria.slnx`, execute:

   ```powershell
   dotnet restore
   dotnet build
   dotnet run --project ProjetoLivraria/ProjetoLivraria.csproj --launch-profile http
   ```

5. Acesse a URL indicada no terminal e abra `/Conta/Cadastrar`. Após criar uma conta, o sistema salva `nome`, `email` e `senhahash` no MySQL e redireciona para `/Conta/Login`.

## Cadastro

- Todos os campos são obrigatórios; exige confirmação de senha e ao menos 8 caracteres.
- E-mails duplicados são recusados; o banco deve manter índice UNIQUE na coluna `email`.
- A senha não é salva em texto puro: é armazenada em `senhahash` usando o mesmo `PasswordHasher<LoginModel>` do login.
- O login autentica e leva à Home, onde é possível cadastrar e listar livros.

## Observações

A tabela `login` original tem uma coluna `senha` opcional. O cadastro não a utiliza; só grava `senhahash`. Se seu banco existente tiver uma coluna `senha` definida como NOT NULL sem valor padrão, ajuste-a para aceitar NULL (como no `script.txt`).

O ZIP original referenciava um projeto `ProjetoLivraria.Tests` que não estava incluído. A solução foi ajustada para apontar apenas para o projeto existente.

Não foi possível executar compilação nem testar a conexão MySQL neste ambiente, pois não há SDK .NET nem instância MySQL disponível. Execute os comandos acima no seu computador para validar com seu banco local.
