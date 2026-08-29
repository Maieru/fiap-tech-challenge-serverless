# FIAP Tech Challenge - Serverless

## OrdemServicoAuthorizer

Lambda Authorizer para o HTTP API Gateway. A funcao valida se o CPF recebido no
header `X-CPF` pertence ao cliente vinculado a ordem de servico informada no path
parameter `id`.

A consulta usa um `DbContext` minimo do Entity Framework, isolado da aplicacao
principal, com apenas as entidades de leitura de cliente e ordem de servico. O
CPF tambem e representado por um value object local e nao ha referencias para os
projetos ou pacotes internos da aplicacao.

### Contrato de entrada

O authorizer utiliza o payload `2.0` do API Gateway:

```json
{
  "headers": {
    "x-cpf": "529.982.247-25"
  },
  "pathParameters": {
    "id": "9be8b471-bd51-4f44-a0e0-3db12cb342c3"
  }
}
```

Uma requisicao autorizada retorna:

```json
{
  "isAuthorized": true,
  "context": {
    "ordemServicoId": "9be8b471-bd51-4f44-a0e0-3db12cb342c3"
  }
}
```

CPF invalido, OS invalida, OS inexistente ou pertencente a outro CPF retornam
`isAuthorized: false`, sem expor qual validacao falhou.

### Configuracao

Em ambiente AWS, configure `DATABASE_SECRET_ID` com o nome ou ARN do segredo. O
JSON armazenado no Secrets Manager deve conter a chave
`ConnectionStrings__DefaultConnection`.

Para desenvolvimento local, a variavel `ConnectionStrings__DefaultConnection`
pode ser informada diretamente e tem precedencia sobre o Secrets Manager.

### Compilar e testar

```powershell
dotnet build FiapTechChallengeServerless.slnx
dotnet test FiapTechChallengeServerless.slnx
```

### Empacotar

Com a ferramenta `Amazon.Lambda.Tools` instalada:

```powershell
dotnet lambda package `
  --project-location src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer `
  --output-package artifacts/ordem-servico-authorizer.zip
```

O handler configurado e:

```text
FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer::FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Function::FunctionHandler
```
