# PrepWise

AI-powered interview practice platform. Generates dynamic interview questions based on your topic or uploaded CV, evaluates your answers, and gives scored feedback with improvement suggestions.

## Tech Stack

- **Backend** — ASP.NET Core 8 Web API
- **Frontend** — Blazor WebAssembly, served via nginx
- **Database** — SQL Server 2022
- **AI** — Google Gemini API
- **Auth** — JWT Bearer tokens

---

## Running with Docker (recommended)

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Steps

1. Clone the repo
   ```bash
   git clone https://github.com/sahannadiranga-source/PrepWiseAI.git
   cd PrepWiseAI
   ```

2. Create your `.env` file from the example
   ```bash
   cp .env.example .env
   ```
   Then open `.env` and fill in your real values — especially `GEMINI_API_KEY` and a strong `DB_SA_PASSWORD`.

3. Start everything
   ```bash
   docker-compose up --build
   ```

4. Open in browser
   - Client → http://localhost:5000
   - API / Swagger → http://localhost:8080/swagger

> If you get a SQL Server login error on first run, the volume may have an old password cached. Run `docker-compose down -v` then `docker-compose up --build` to reset it.

---

## Running Locally (without Docker)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local instance or SQL Express)

### Steps

1. Create `appsettings.json` from the example
   ```bash
   cp PrepWise.API/appsettings.example.json PrepWise.API/appsettings.json
   ```
   Update the connection string to point to your local SQL Server and add your Gemini API key.

2. Run the API
   ```bash
   cd PrepWise.API
   dotnet run
   ```

3. Run the client (in a separate terminal)
   ```bash
   cd PrepWise.Client
   dotnet run
   ```

4. Open the client URL shown in the terminal (typically https://localhost:7189)

---

## Environment Variables

| Variable | Description |
|---|---|
| `DB_SA_PASSWORD` | SQL Server SA password |
| `JWT_KEY` | Secret key for signing JWT tokens (min 32 chars) |
| `JWT_ISSUER` | JWT issuer name |
| `JWT_AUDIENCE` | JWT audience name |
| `GEMINI_API_KEY` | Google Gemini API key |
| `CORS_BLAZOR_ORIGIN` | Allowed origin for the Blazor client |
