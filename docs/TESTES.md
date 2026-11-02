# Verificação do fluxo de cadastro

## Resultado nesta etapa — 02/11/2026

- `dotnet restore`: concluído.
- `dotnet build --no-restore`: compilação aprovada; permanecem avisos anteriores de nulabilidade e referência redundante a Microsoft.Extensions.Identity.Core (NU1510).
- `dotnet test`: 50 testes aprovados, nenhum ignorado ou com falha na suíte final.
- O MySQL local não respondeu em `127.0.0.1:3306`, inclusive na verificação fora da restrição de rede. Não foram executados login, leitura, INSERT ou conflito UNIQUE contra o banco real.
- Nenhuma credencial, dado do banco, `bin` ou `obj` foi adicionado aos novos commits.

## Cobertura automatizada

Execute na pasta da solução:

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build --no-restore
```

| Fluxo | Evidência automatizada |
|---|---|
| Login correto | POST no ContaController com hash Identity e token real, criação de cookie, acesso à Home e nome Maria Leitora |
| Login incorreto | Senha incorreta e usuário inexistente exibem erro e continuam sem acesso à Home |
| Proteção | Home, cadastro e listagem redirecionam usuários anônimos para o login |
| Página inicial | Saudação personalizada e link real `/Livro/Cadastrar` |
| Formulário | Campos, ação POST, token antifalsificação e retorno à Home renderizados pelo Razor |
| Campos obrigatórios | ISBN, nome e valor ausentes são recusados pelo MVC sem gravar |
| Limites | ISBN acima de 20, nome acima de 150, textos em branco, valor zero/negativo/acima do limite são recusados; limites válidos aceitos |
| Capacidade da descrição | O limite de 65.535 bytes UTF-8 da coluna TEXT é respeitado para ASCII, acentos e emojis; conteúdo acima desse limite não chega ao repositório |
| Valor monetário | Valor vazio, texto inválido e três casas significativas recusados; `29.90` e `29,90` vinculados a 29,90 sem depender da cultura do servidor |
| Cadastro válido | Dados enviados ao repositório, ID do formulário ignorado, confirmação e POST/Redirect/GET; atualizar o GET não repete a gravação |
| ISBN duplicado | Consulta prévia e MySqlException 1062 simulada retornam erro sem novo registro, mantendo os dados; vírgula decimal é normalizada para continuar visível no campo numérico do navegador |
| Outros erros MySQL | Não são confundidos com ISBN duplicado |
| Listagem | Acervo vazio, todos os campos, descrição opcional, valores pt-BR e HTML escapado |
| Fluxo completo | Login → cadastro → consulta na listagem → segundo envio bloqueado |
| Antifalsificação e logout | POST sem token recusado; logout encerra o acesso autenticado |

Os testes substituem somente `ILoginRepository` e `ILivroRepository`. O erro 1062 é construído pelo teste para verificar o tratamento da Controller; isso não representa execução de uma corrida concorrente no MySQL. Nenhum teste automatizado altera um banco real.

A validação da descrição considera o limite em bytes documentado para [TEXT no manual do MySQL](https://dev.mysql.com/doc/refman/26.10/en/string-type-syntax.html). Essa capacidade foi conferida na documentação; a gravação real depende do banco local.

## Roteiro manual reproduzível com MySQL

Preparação: siga o README, inicie o banco e a aplicação e use uma conta de teste com hash válido. Use um banco local de avaliação e um ISBN ainda não utilizado, por exemplo `TESTE-20261102-01`. Caso já exista, escolha outro identificador com até 20 caracteres. Registre resultados e evidências sem fotografar senhas.

| Nº | Ação | Resultado esperado |
|---|---|---|
| 1 | Em janela privativa, abrir `/Home/Index`, `/Livro/Cadastrar` e `/Livro/Listar` | Redirecionamento para `/Conta/Login` em todos os casos |
| 2 | Informar e-mail existente com senha incorreta; depois e-mail inexistente | Mensagem "E-mail ou senha incorretos."; Home continua protegida |
| 3 | Entrar com credenciais corretas | Home com "Sistema Livraria" e "Bem-vindo(a), [nome da conta]!" |
| 4 | Clicar em "Cadastrar Livros" | URL `/Livro/Cadastrar`, campos ISBN/nome/descrição/valor e botões Salvar Livro/Voltar |
| 5 | Enviar formulário vazio | Erros dos campos obrigatórios, sem registro no banco |
| 6 | Tentar ISBN com 21 caracteres e nome com 151; para testar o servidor, remover os limites do campo nas ferramentas do navegador ou enviar formulário HTTP autenticado | Erros de limite, sem INSERT |
| 7 | Tentar valor vazio, zero, negativo, texto, `100000000` e `1.234` | Rejeição, sem INSERT; valores corretos permanecem preenchidos |
| 8 | Usar ISBN novo, nome "Livro de avaliação", descrição "Cadastro manual" e valor 49,90 conforme o formato do campo numérico do navegador | Redirecionamento GET ao cadastro e mensagem "Livro cadastrado com sucesso!" |
| 9 | Atualizar a página após salvar | Nenhuma nova gravação ou confirmação de reenvio do formulário |
| 10 | Consultar o registro com SQL parametrizado ou pela ferramenta local de banco, filtrando pelo ISBN escolhido | Exatamente uma linha, nome/descrição esperados e valor decimal 49.90 |
| 11 | Tentar cadastrar novamente o mesmo ISBN com outro nome/descrição | "Este ISBN já está cadastrado", campos preservados e uma única linha no banco |
| 12 | Abrir `/Livro/Listar` pela Home | Registro exibido com ISBN, nome, descrição e R$ 49,90; botão de retorno funciona |
| 13 | Reiniciar somente a aplicação e fazer login novamente | Livro continua na listagem, comprovando persistência MySQL |
| 14 | Em banco de avaliação inicialmente sem livros, abrir listagem | "Nenhum livro cadastrado."; não apagar registros reais para criar esse cenário |
| 15 | Testar descrição vazia e títulos com `<script>` ou caracteres acentuados | Cadastro válido; descrição opcional e texto escapado na listagem, sem execução de script |
| 16 | Reduzir a janela para largura de celular | Formulário organizado; tabela pode rolar horizontalmente e botões continuam acessíveis |

Para validar manualmente uma colisão concorrente, duas sessões autenticadas podem enviar o mesmo ISBN ao mesmo tempo: apenas um INSERT deve prevalecer, e a outra sessão deve receber o erro de ISBN. A restrição UNIQUE permanece no banco. A verificação automatizada cobre o tratamento do erro 1062, mas esta execução real permanece pendente enquanto o MySQL estiver indisponível.
