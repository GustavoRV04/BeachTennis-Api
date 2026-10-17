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

## Publicação

O Vercel hospeda o front-end Vite, não esta API ASP.NET. Publique a API em um host que execute ASP.NET Core e configure nela:

- `ConnectionStrings__DefaultConnection`: connection string do Supabase, mantida como segredo no host;
- `Auth__SigningKey`: chave aleatória com pelo menos 32 bytes;
- `Admin__Email` e `Admin__Password`: credenciais do administrador inicial (senha com pelo menos 12 caracteres);
- `Cors__AllowedOrigins__0`: origem exata do front-end publicado, por exemplo `https://seu-projeto.vercel.app`.

No Vercel, configure `VITE_API_URL` com a URL pública da API e faça um novo deploy. Aplique as migrations no banco de produção antes de abrir o sistema.

> A API tem autenticação JWT e autorização por perfil, mas ainda não oferece recuperação de senha, verificação de e-mail nem limitação de tentativas de login. Não reutilize a senha administrativa em outros serviços e use HTTPS no host público.
