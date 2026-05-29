# SoftLedger

Estrutura do repositório:

- `back/SoftLedger`: API .NET com autenticação JWT e inventário de softwares.
- `front`: board React/Vite para gestores acompanharem aplicativos, ativações e produtos Microsoft 365 detectados no inventário.

## Frontend

```powershell
cd front
npm install
npm run dev
```

O frontend usa a API publicada no Railway por padrão:

```text
VITE_API_BASE_URL=https://comfortable-gentleness-production-1da8.up.railway.app
```

Para alterar a URL, edite `front/.env.local` ou configure a mesma variável no ambiente de deploy.

## Backend

```powershell
dotnet run --project back/SoftLedger/SoftLedger.csproj
```

Quando publicar o frontend, configure no backend a variável `CORS_ALLOWED_ORIGINS` com a URL pública do front. Exemplo:

```text
CORS_ALLOWED_ORIGINS=https://seu-front.up.railway.app
```

A visão de Microsoft 365 no frontend é derivada dos softwares retornados por `GET /Software`. Para disponibilidade real de assinaturas por usuário, o próximo passo é adicionar uma rota no backend integrada ao Microsoft Graph.
