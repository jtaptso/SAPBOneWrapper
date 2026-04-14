# SAP Business One Wrapper

A full-stack web application for managing SAP Business One Business Partners through the SAP B1 Service Layer REST API. Built with .NET 10, Clean Architecture, and Blazor.

## Architecture

```
┌──────────────┐     ┌──────────────┐     ┌─────────────────┐
│  Blazor Web  │────▶│   REST API   │────▶│ SAP B1 Service  │
│  (Frontend)  │     │ (FastEndpts) │     │     Layer        │
└──────────────┘     └──────┬───────┘     └─────────────────┘
                            │
                     ┌──────▼───────┐
                     │  SQL Server  │
                     │ (Local Cache)│
                     └──────────────┘
```

The solution follows **Clean Architecture** with the following layers:

| Project | Role |
|---------|------|
| `SAPBOneWrapper.Domain` | Entities, enums, repository interfaces |
| `SAPBOneWrapper.Application` | DTOs, CQRS commands/queries (MediatR), validators |
| `SAPBOneWrapper.Infrastructure` | EF Core, ASP.NET Identity, SAP Service Layer client |
| `SAPBOneWrapper.Api` | FastEndpoints REST API with JWT auth |
| `SAPBOneWrapper.Web` | Blazor Web App (Interactive Server) |

## Features

- **CRUD Operations** on Business Partners (Customer, Supplier, Lead)
- **Manual Sync** — pull Business Partners from SAP B1 into local database
- **Two-way sync** — create/update/delete operations push to SAP and cache locally
- **JWT Authentication** with ASP.NET Identity (register, login, logout)
- **Search & Pagination** on Business Partner listings
- **Sync Status Tracking** per record (Synced, PendingCreate, PendingUpdate, PendingDelete, Error)

## Tech Stack

- .NET 10
- Blazor Web App (Interactive Server, Bootstrap 5)
- FastEndpoints (REPR pattern)
- MediatR (CQRS) + FluentValidation
- Entity Framework Core + SQL Server
- ASP.NET Identity + JWT Bearer
- SAP B1 Service Layer REST API

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (or SQL Server Express / LocalDB)
- SAP Business One with Service Layer enabled

## Getting Started

### 1. Clone & Restore

```bash
git clone <repo-url>
cd SAPBOneWrapper
dotnet restore SAPBOneWrapper.slnx
```

### 2. Configure the API

Edit `src/SAPBOneWrapper.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SAPBOneWrapper;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "<your-secret-key-at-least-32-chars>",
    "Issuer": "SAPBOneWrapper.Api",
    "Audience": "SAPBOneWrapper.Web",
    "ExpiryInMinutes": 60
  },
  "SapB1": {
    "ServiceLayerUrl": "https://<your-sap-server>:50000",
    "CompanyDB": "<your-company-db>",
    "UserName": "<sap-username>",
    "Password": "<sap-password>",
    "SessionTimeoutMinutes": 30
  },
  "AllowedOrigins": [
    "http://localhost:5151"
  ]
}
```

### 3. Configure the Web App

Edit `src/SAPBOneWrapper.Web/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:5262"
}
```

### 4. Apply Migrations

```bash
dotnet ef database update \
  --project src/SAPBOneWrapper.Infrastructure \
  --startup-project src/SAPBOneWrapper.Api
```

### 5. Run

Start both projects (in separate terminals):

```bash
# Terminal 1 — API
cd src/SAPBOneWrapper.Api
dotnet run

# Terminal 2 — Web
cd src/SAPBOneWrapper.Web
dotnet run
```

- **API**: http://localhost:5262 (Swagger at `/swagger`)
- **Web**: http://localhost:5151

## API Endpoints

### Auth

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive JWT |
| GET | `/api/auth/me` | Get current user info |

### Business Partners (requires JWT)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/business-partners` | List (search, paged) |
| GET | `/api/business-partners/{CardCode}` | Get by code |
| POST | `/api/business-partners` | Create |
| PUT | `/api/business-partners/{CardCode}` | Update |
| DELETE | `/api/business-partners/{CardCode}` | Delete |
| POST | `/api/business-partners/sync` | Sync from SAP B1 |

## Project Structure

```
SAPBOneWrapper/
├── src/
│   ├── SAPBOneWrapper.Domain/           # Entities, enums, interfaces
│   ├── SAPBOneWrapper.Application/      # DTOs, commands, queries, validators
│   ├── SAPBOneWrapper.Infrastructure/   # EF Core, Identity, SAP client
│   ├── SAPBOneWrapper.Api/              # FastEndpoints REST API
│   └── SAPBOneWrapper.Web/              # Blazor Web App
├── tests/
│   ├── SAPBOneWrapper.UnitTests/
│   └── SAPBOneWrapper.IntegrationTests/
└── SAPBOneWrapper.slnx
```

## License

This project is for internal use.
