# FIAP Tech Challenge — Serverless

Autorizador Lambda .NET 10 para HTTP API Gateway, com token por CPF e código de aprovação. É um componente independente: o ciclo AWS de OS/Execution/Billing não o publica. A autenticação administrativa e os links privados dos microsserviços são validados pelo OS.

## Contrato e dados

O handler aceita o payload 2.0 do HTTP API Gateway:

```json
{
  "queryStringParameters": {
    "token": "d80e8c958d265c7455badf562f4cbd7914a38bda2698edb1cff74357e9bbc2d6"
  },
  "pathParameters": {
    "id": "9be8b471-bd51-4f44-a0e0-3db12cb342c3"
  }
}
```

O token esperado é o SHA-256, representado em 64 caracteres hexadecimais, do CPF normalizado concatenado ao `CodigoAprovacao` da ordem em formato UUID com hífens. O exemplo ilustra somente o formato; a autorização depende dos dados do banco.

Uma autorização retorna:

```json
{
  "isAuthorized": true,
  "context": { "ordemServicoId": "9be8b471-bd51-4f44-a0e0-3db12cb342c3" }
}
```

Ausência/formato inválido de token ou ID, ordem inexistente/inativa ou token sem correspondência retornam `isAuthorized: false`. Falhas de configuração, acesso ao Secrets Manager ou banco são erros operacionais e precisam ser diagnosticadas; não equivalem a token inválido.

A leitura EF espera tabelas `Clientes` e `OrdensServico` com os campos mapeados no [AuthorizationDbContext](src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer/Persistence/AuthorizationDbContext.cs), incluindo `Cpf`, `Ativo`, `ClienteId` e `CodigoAprovacao`. Esse contrato não corresponde ao schema atual do OS; não aponte o componente para o banco OS sem uma adaptação explícita. A função não cria schema nem aplica migrations.

## Pré-requisitos e configuração

.NET SDK 10 e acesso NuGet bastam para build e testes atuais. Os exemplos usam PowerShell 7. AWS CLI e permissões AWS são necessários somente para operação remota.

| Variável | Uso |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Conexão PostgreSQL direta; tem precedência quando não vazia |
| `DATABASE_SECRET_ID` | Nome ou ARN do segredo usado quando não há conexão direta |
| `AWS_REGION` | Região para os clientes AWS quando não fornecida pelo runtime; os defaults de empacotamento usam us-east-1 |

O segredo precisa conter `ConnectionStrings__DefaultConnection` em JSON. A role da função deve poder ler esse segredo e alcançar o banco; o código usa a cadeia padrão de credenciais AWS. Não versione conexão, token ou dados pessoais.

## Build e testes

Na raiz:

```powershell
dotnet restore FiapTechChallengeServerless.slnx
dotnet build FiapTechChallengeServerless.slnx -c Release --no-restore
dotnet test FiapTechChallengeServerless.slnx -c Release --no-build --no-restore
```

Os testes verificam CPF/token, handler e repositório com provedor EF InMemory. Não precisam de banco AWS ou Docker e não comprovam SQL PostgreSQL, permissões IAM ou integração com API Gateway. Os projetos ficam em `src` e `tests`.

## Empacotamento

O handler é:

```text
Ftc.Authorizer::FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Function::FunctionHandler
```

O [arquivo de defaults](src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer/aws-lambda-tools-defaults.json) define runtime `dotnet10`, Release e região us-east-1. Para usar o comando específico de Lambda, instale `Amazon.Lambda.Tools` em uma versão compatível com seu SDK e execute:

```powershell
dotnet lambda package --project-location src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer --output-package artifacts/ordem-servico-authorizer.zip
```

Como alternativa, publique o projeto e compacte o conteúdo, sem uma pasta pai dentro do ZIP:

```powershell
dotnet publish src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.csproj -c Release -o artifacts/publish
Compress-Archive -Path artifacts/publish/* -DestinationPath artifacts/ordem-servico-authorizer.zip -Force
```

Use um diretório de publicação limpo e configure runtime, arquitetura e handler de forma compatível no destino. `artifacts` é gerado localmente e ignorado pelo Git.

## Operação AWS

Não há workflow de deploy neste repositório. `infra/serverless` no repositório Infra contém declarações do componente, mas não é estágio aceito pelo ciclo atual dos microsserviços. Criar função, roles, rede, schema e rotas exige um procedimento independente e revisão de compatibilidade; o [guia central](https://github.com/Maieru/fiap-tech-challenge/blob/main/docs/operacao/plataforma-aws.md) descreve o ciclo das três APIs.

Somente para uma função já provisionada e compatível, o upload manual é:

```powershell
aws lambda update-function-code --function-name '<nome-da-funcao>' --zip-file fileb://artifacts/ordem-servico-authorizer.zip --region us-east-1
aws lambda wait function-updated-v2 --function-name '<nome-da-funcao>' --region us-east-1
```

Atualizar código não configura handler, variáveis, rede, authorizer ou rotas. Publicar uma versão também não muda automaticamente aliases ou o destino configurado no Gateway.

## Diagnóstico

- Erro por `DATABASE_SECRET_ID` ausente: forneça conexão direta ou o identificador do segredo.
- `AccessDenied`: confira credenciais, role e ARN do segredo; não exponha o JSON do segredo no diagnóstico.
- Erro SQL ou timeout: confira schema, conexão, TLS, DNS e alcance de rede.
- `isAuthorized: false`: confira ID, registros ativos e o token correspondente, sem registrar CPF ou token em logs.
