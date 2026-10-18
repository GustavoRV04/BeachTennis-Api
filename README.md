# BeachTennis API

API ASP.NET Core com persistência PostgreSQL via EF Core/Npgsql. A arquitetura em camadas e o provedor permanecem os mesmos; não é necessário executar Docker para desenvolver ou publicar.

## Banco de dados Supabase

1. Crie um projeto PostgreSQL no Supabase e copie a connection string do **Session pooler**.
2. Configure os segredos de desenvolvimento. No PowerShell:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string-do-supabase>" --project BeachTennis.Api
   dotnet user-secrets set "Admin:Email" "<email-do-administrador>" --project BeachTennis.Api
   dotnet user-secrets set "Admin:Password" "<senha-forte-com-ao-menos-12-caracteres>" --project BeachTennis.Api

   $keyBytes = [byte[]]::new(48)
   [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($keyBytes)
   $jwtKey = [Convert]::ToBase64String($keyBytes)
   dotnet user-secrets set "Auth:SigningKey" $jwtKey --project BeachTennis.Api
   ```

   O usuário administrativo é criado (ou atualizado) com esses dados quando a API inicia. Mantenha a senha e a chave JWT somente em User Secrets ou no cofre de segredos do host.

3. Instale o comando do EF Core, se necessário:

   ```powershell
   dotnet tool install --global dotnet-ef --version 10.0.11
   ```

4. Ative o ambiente de desenvolvimento para que o comando carregue os User Secrets e aplique as migrations existentes:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Development"
   dotnet ef database update --project BeachTennis.Infrastructure --startup-project BeachTennis.Api
   ```

   Use a connection string do Supabase com SSL habilitado. Não salve senha ou connection string em `appsettings*.json`, no front-end ou no repositório.

## Executar localmente

Com os segredos configurados e as migrations aplicadas:

```powershell
dotnet run --project BeachTennis.Api --launch-profile http
```

A API inicia em `http://localhost:5030` pelo perfil HTTP. O front-end Vite (`http://localhost:5173`) já está autorizado por CORS para desenvolvimento.

## Contas e permissões

- Atletas se cadastram em `/acesso`; o cadastro cria a conta e inicia a sessão. O login recebe um JWT com validade configurada em `Auth:ExpirationMinutes` (60 minutos por padrão).
- A conta administrativa não tem cadastro público. `Admin:Email` e `Admin:Password` são usados para provisioná-la; altere os segredos no host para trocar a senha.
- Apenas usuários com perfil `Admin` podem cadastrar, editar ou desativar quadras e consultar a lista administrativa. Atletas podem criar reservas e consultar/cancelar apenas as próprias.
- A consulta de disponibilidade pública retorna somente quadra, horário e status; não expõe IDs nem dados do atleta.
- A aplicação mantém o token no armazenamento local do navegador. Para computadores compartilhados, saia da conta ao terminar.

Para cadastrar quadras iniciais, use `POST /api/quadras` no Swagger durante o desenvolvimento.

## Publicação: Render, Supabase e Vercel

O Vercel publica o front-end Vite, não executa esta API ASP.NET. Para a demonstração, a configuração prevista é:

- Supabase: banco PostgreSQL;
- Render: API ASP.NET, usando o `Dockerfile` desta pasta para compilar e executar a API;
- Vercel: front-end React/Vite.

O Dockerfile é somente o pacote de execução da API no Render; o banco continua hospedado no Supabase e não há necessidade de executar PostgreSQL em Docker.

### 1. Preparar o banco

Crie o projeto Supabase e copie a connection string ADO.NET do **Session pooler**. Configure-a nos User Secrets locais e aplique as migrations antes de iniciar a API hospedada, seguindo os comandos da seção [Banco de dados Supabase](#banco-de-dados-supabase).

### 2. Publicar a API no Render

O código precisa estar em um repositório GitHub acessível ao Render. Se os dois projetos estiverem no mesmo repositório, crie um **Web Service** conectado a esse repositório e configure:

- **Root Directory**: `BeachTennis`;
- **Runtime/Language**: `Docker`;
- **Dockerfile Path**: `Dockerfile`;
- **Plan**: `Free` para testes e apresentação.

Configure estas variáveis de ambiente no serviço. Os valores secretos devem ser inseridos diretamente no painel do Render, nunca no repositório:

| Variável | Valor |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Connection string do Supabase |
| `Auth__SigningKey` | Chave aleatória com pelo menos 32 bytes |
| `Admin__Email` | E-mail da conta administrativa |
| `Admin__Password` | Senha forte com pelo menos 12 caracteres |
| `Cors__AllowedOrigins__0` | Origem exata do front publicado, por exemplo `https://seu-projeto.vercel.app` |

O endereço `onrender.com` que o Render atribuir ao serviço é a URL base da API. Depois do primeiro deploy, uma resposta de `/api/quadras` confirma que a API e o banco estão acessíveis.

### 3. Publicar o front-end no Vercel

Importe o mesmo repositório no Vercel e configure **Root Directory** como `BeachT-Front`. O Vercel detecta Vite; confirme build `npm run build` e diretório de saída `dist`.

Configure `VITE_API_URL` como a URL base HTTPS do serviço Render, sem acrescentar `/api`, e publique. Copie o domínio final do front (`https://...vercel.app`) e atualize `Cors__AllowedOrigins__0` no Render para esse domínio exato. Se mudar a variável `VITE_API_URL` ou o domínio de CORS, faça um novo deploy do serviço correspondente.

### Limitações do plano gratuito

O Render Free suspende o serviço web depois de 15 minutos sem tráfego. A primeira visita seguinte pode aguardar cerca de um minuto enquanto a API volta a iniciar. Para a apresentação, abra o site e teste login/reserva alguns minutos antes; para evitar essa espera, será necessário usar um plano de serviço ativo. Verifique os limites atuais do provedor antes de publicar.

Não envie `.env.local`, connection strings, senha administrativa ou chave JWT ao GitHub. Aplique as migrations no Supabase antes do primeiro boot da API: o provisionamento da conta administrativa precisa das tabelas já criadas.

> A API tem autenticação JWT e autorização por perfil, mas ainda não oferece recuperação de senha, verificação de e-mail nem limitação de tentativas de login. Não reutilize a senha administrativa em outros serviços e use HTTPS no host público.
