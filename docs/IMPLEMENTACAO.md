# Relatório da implementação do segundo integrante

## Base e preservação

Repositório real: `ProjectLivraria/ProjetoLivraria`. O diretório externo também contém um repositório Git vazio, que não foi alterado. O histórico existente do primeiro integrante termina em `7f1f223`. Após `git fetch origin`, `origin/main` e `origin/dev` estavam nesse mesmo commit. Foi criada `feature/home-livros` a partir de `origin/dev`.

Identidade efetiva dos novos commits: `EnzoLima(Rajada) <enzplima86@gmail.com>`, já configurada. Nenhuma configuração de identidade, autoria anterior ou data foi alterada. Não foram feitos push ou merge.

O primeiro integrante já havia implementado models, login com hashes e claims, interfaces e repositórios MySQL parametrizados. Foram preservados ContaController, repositórios, configuração de conexão e script do banco. O Program não registrava cookies ou dependências; o registro foi completado para permitir o fluxo autenticado.

## Arquivos criados

- `ProjetoLivraria/Controllers/LivroController.cs`: cadastro protegido, validação, duplicidade e listagem.
- `ProjetoLivraria/ModelBinders/ValorMonetarioModelBinder.cs`: leitura decimal com ponto ou vírgula, mensagem para ausência/formato inválido e rejeição de frações de centavo sem arredondamento silencioso.
- `ProjetoLivraria/Models/Validation/MaximoBytesUtf8Attribute.cs`: validação da capacidade da descrição na coluna TEXT existente, incluindo caracteres multibyte.
- `ProjetoLivraria/Views/Livro/Cadastrar.cshtml`: formulário responsivo com antifalsificação e mensagens.
- `ProjetoLivraria/Views/Livro/Listar.cshtml`: tabela responsiva com moeda brasileira e acervo vazio.
- `ProjetoLivraria.Tests/ProjetoLivraria.Tests.csproj`: infraestrutura xUnit e ASP.NET Core MVC Testing.
- `ProjetoLivraria.Tests/HomeTests.cs`, `AutenticacaoTests.cs`, `LivroValidacaoTests.cs`, `LivroFluxoTests.cs`, `LivroDuplicadoTests.cs`, `LivroListagemTests.cs`: verificações automatizadas do fluxo.
- `ProjetoLivraria.Tests/LivrariaFactory.cs`: servidor de testes com repositórios em memória e conta fictícia.
- `README.md`, `docs/TESTES.md` e este relatório: execução, resultados e roteiro manual.

## Arquivos modificados

- `ProjetoLivraria/Controllers/HomeController.cs`: proteção e nome autenticado; Privacy e Error preservados, Error acessível para tratar exceções.
- `ProjetoLivraria/Views/Home/Index.cshtml`: saudação, apresentação e acessos às duas funcionalidades de livros.
- `ProjetoLivraria/Models/LivroModel.cs`: Data Annotations, limites compatíveis com a tabela e strings inicializadas; tipos e compatibilidade com o repositório mantidos.
- `ProjetoLivraria/Program.cs`: autenticação por cookies, middleware e dependências existentes de login/livro.
- `ProjetoLivraria.slnx`: inclusão do projeto de testes.

## Resultado e limitações

As funcionalidades das dez tarefas foram implementadas em ordem, com alterações reais e commits separados. O cadastro usa os métodos existentes `Cadastrar` e `IsbnExiste`, com SQL parametrizado preservado. Não há mudança no esquema nem remoção da restrição UNIQUE. A confirmação usa TempData e POST/Redirect/GET.

A suíte final contém 50 testes aprovados. A aplicação e os testes compilam. Permanecem os avisos preexistentes de nulabilidade dos arquivos preservados e NU1510 para o pacote Identity já instalado. Consulte `TESTES.md` para detalhamento.

A revisão final identificou e corrigiu duas bordas do formulário: descrição maior que a coluna TEXT e valor enviado com vírgula que precisava continuar visível no campo numérico após um erro. Foram escritos testes que reproduziram as falhas antes das correções. Não houve mudança no esquema.

O MySQL local está indisponível na porta 3306. Por isso, o login com contas reais, a persistência SQL, a leitura real e a colisão UNIQUE real ainda precisam da execução manual documentada. Os testes HTTP usam repositórios em memória e não devem ser apresentados como testes de integração MySQL aprovados.

Os dez hashes e mensagens dos commits são apresentados na entrega final e podem ser conferidos por `git log --oneline -10`. O estado final deve ser conferido com `git status` dentro do repositório real.
