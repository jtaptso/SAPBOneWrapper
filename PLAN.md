# Plan: SAP B1 Wrapper — Full-Stack CRUD Application

## TL;DR

Build a full-stack .NET 10 application that wraps the SAP Business One Service Layer REST API, starting with Business Partner CRUD operations. The app uses Clean Architecture with a Blazor Web App (Interactive) frontend, FastEndpoints API, MediatR CQRS, ASP.NET Identity authentication, a local SQL Server database for caching/storage that syncs with SAP B1, and HttpClient-based SAP B1 Service Layer integration. Plain Bootstrap for UI styling.

## Architecture Overview

```
Blazor Web App (Bootstrap 5, Interactive Server)
    ↕ HttpClient + JWT Bearer
ASP.NET Core API (FastEndpoints)
    ↕ MediatR (Commands/Queries)
Application Layer (Handlers, DTOs, Validators)
    ↕
Domain Layer (Entities, Interfaces)
    ↕
Infrastructure Layer
    ├── EF Core + ASP.NET Identity (Local SQL Server DB)
    └── SAP B1 Service Layer Client (HttpClient → SAP REST API)
```

## Solution Structure

```
SAPBOneWrapper/
├── SAPBOneWrapper.sln
├── src/
│   ├── SAPBOneWrapper.Domain/              # Entities, Interfaces, Enums
│   ├── SAPBOneWrapper.Application/         # MediatR Commands/Queries, DTOs, Validators, Mappings
│   ├── SAPBOneWrapper.Infrastructure/      # EF Core, Identity, SAP B1 Client, Repositories
│   ├── SAPBOneWrapper.Api/                 # FastEndpoints, Auth, Program.cs, DI config
│   └── SAPBOneWrapper.Web/                 # Blazor Web App (Interactive Server)
├── tests/
│   ├── SAPBOneWrapper.UnitTests/
│   └── SAPBOneWrapper.IntegrationTests/
```

---

## Phase 1: Solution Scaffolding

### Step 1: Create solution and projects

Create .NET 10 solution with 5 source projects + 2 test projects:

- `SAPBOneWrapper.Domain` — Class Library (.NET 10)
- `SAPBOneWrapper.Application` — Class Library (.NET 10)
- `SAPBOneWrapper.Infrastructure` — Class Library (.NET 10)
- `SAPBOneWrapper.Api` — ASP.NET Core Web API (.NET 10)
- `SAPBOneWrapper.Web` — Blazor Web App (.NET 10, Interactive render mode)
- `SAPBOneWrapper.UnitTests` — xUnit test project
- `SAPBOneWrapper.IntegrationTests` — xUnit test project

### Step 2: Set up project references (dependency flow)

- Domain → (no references)
- Application → Domain
- Infrastructure → Application, Domain
- Api → Application, Infrastructure
- Web → (calls Api via HttpClient, shares Application DTOs)

### Step 3: Install NuGet packages

**Domain:** (none — pure C#)

**Application:**

- MediatR
- FluentValidation
- FluentValidation.DependencyInjectionExtensions
- Mapster (lightweight mapping)

**Infrastructure:**

- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.Extensions.Http (for IHttpClientFactory)

**Api:**

- FastEndpoints
- FastEndpoints.Swagger (for OpenAPI docs)
- Microsoft.AspNetCore.Authentication.JwtBearer

**Web:**

- Microsoft.AspNetCore.Components.WebAssembly (if using WASM interactivity)

---

## Phase 2: Domain Layer

### Step 4: Define Business Partner entity

File: `src/SAPBOneWrapper.Domain/Entities/BusinessPartner.cs`

- Properties: CardCode (string, PK), CardName, CardType (enum), Phone, Email, Address, City, Country, PostCode, Currency, CreditLimit, TaxId, Active, Remarks
- SyncStatus enum property: Synced, PendingCreate, PendingUpdate, PendingDelete, Error
- Timestamps: CreatedAt, UpdatedAt, LastSyncedAt

### Step 5: Define enums

File: `src/SAPBOneWrapper.Domain/Enums/`

- `CardType.cs` — Customer, Supplier, Lead
- `SyncStatus.cs` — Synced, PendingCreate, PendingUpdate, PendingDelete, Error

### Step 6: Define repository interfaces

File: `src/SAPBOneWrapper.Domain/Interfaces/`

- `IBusinessPartnerRepository.cs` — GetByCardCode, GetAll (with paging/filtering), Add, Update, Delete
- `ISapB1ServiceLayerClient.cs` — Login, Logout, GetBusinessPartner, GetBusinessPartners, CreateBusinessPartner, UpdateBusinessPartner, DeleteBusinessPartner

---

## Phase 3: Application Layer

### Step 7: Define DTOs

File: `src/SAPBOneWrapper.Application/DTOs/`

- `BusinessPartnerDto.cs` — Response DTO
- `CreateBusinessPartnerDto.cs` — Create request
- `UpdateBusinessPartnerDto.cs` — Update request
- `PagedResultDto<T>.cs` — Generic paged response
- `BusinessPartnerFilterDto.cs` — Filter/search parameters

### Step 8: Define MediatR Commands

File: `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/`

- `CreateBusinessPartnerCommand.cs` + `CreateBusinessPartnerHandler.cs`
  - Creates locally in DB + calls SAP B1 Service Layer POST /BusinessPartners
- `UpdateBusinessPartnerCommand.cs` + `UpdateBusinessPartnerHandler.cs`
  - Updates local DB + calls SAP B1 Service Layer PATCH /BusinessPartners('{CardCode}')
- `DeleteBusinessPartnerCommand.cs` + `DeleteBusinessPartnerHandler.cs`
  - Deletes from local DB + calls SAP B1 Service Layer DELETE /BusinessPartners('{CardCode}')
- `SyncBusinessPartnersCommand.cs` + `SyncBusinessPartnersHandler.cs`
  - Pulls all BPs from SAP B1 and upserts into local DB

### Step 9: Define MediatR Queries

File: `src/SAPBOneWrapper.Application/Features/BusinessPartners/Queries/`

- `GetBusinessPartnerByCodeQuery.cs` + Handler — returns single BP
- `GetBusinessPartnersQuery.cs` + Handler — returns paged list with filtering

### Step 10: Define FluentValidation validators

File: `src/SAPBOneWrapper.Application/Features/BusinessPartners/Validators/`

- `CreateBusinessPartnerValidator.cs` — CardCode required (3-15 chars), CardName required, CardType required
- `UpdateBusinessPartnerValidator.cs` — CardName max 100, valid email format if provided

### Step 11: Define Mapster mapping configuration

File: `src/SAPBOneWrapper.Application/Mappings/BusinessPartnerMappingConfig.cs`

- BusinessPartner ↔ BusinessPartnerDto
- CreateBusinessPartnerDto → BusinessPartner
- BusinessPartner → SAP API model (for Service Layer calls)

### Step 12: Define MediatR pipeline behaviors

File: `src/SAPBOneWrapper.Application/Behaviors/`

- `ValidationBehavior.cs` — Runs FluentValidation before handlers
- `LoggingBehavior.cs` — Logs command/query execution (optional, can defer)

### Step 13: Register Application services

File: `src/SAPBOneWrapper.Application/DependencyInjection.cs`

- Extension method `AddApplicationServices()` registering MediatR, FluentValidation, Mapster

---

## Phase 4: Infrastructure Layer

### Step 14: EF Core DbContext

File: `src/SAPBOneWrapper.Infrastructure/Data/AppDbContext.cs`

- Inherit from `IdentityDbContext<ApplicationUser>` (includes ASP.NET Identity tables)
- `DbSet<BusinessPartner>`
- Entity config: CardCode as PK, indexes on CardName, CardType

File: `src/SAPBOneWrapper.Infrastructure/Data/Configurations/BusinessPartnerConfiguration.cs`

- EF Core Fluent API configuration

### Step 14b: Define ApplicationUser entity

File: `src/SAPBOneWrapper.Infrastructure/Identity/ApplicationUser.cs`

- Inherits `IdentityUser`
- Optional extra fields: FullName, CreatedAt (can be extended later)

### Step 15: Repository implementation

File: `src/SAPBOneWrapper.Infrastructure/Repositories/BusinessPartnerRepository.cs`

- Implements `IBusinessPartnerRepository` using EF Core
- Supports paging ($skip/$top), filtering, ordering

### Step 16: SAP B1 Service Layer client

File: `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/SapB1ServiceLayerClient.cs`

- Implements `ISapB1ServiceLayerClient`
- Uses `IHttpClientFactory` with named client "SapB1"
- Session management: Login → store B1SESSION cookie → reuse across requests → auto-relogin on 401
- Methods:
  - `LoginAsync()` — POST /b1s/v1/Login with CompanyDB, UserName, Password
  - `LogoutAsync()` — POST /b1s/v1/Logout
  - `GetBusinessPartnerAsync(cardCode)` — GET /b1s/v1/BusinessPartners('{cardCode}')
  - `GetBusinessPartnersAsync(filter, top, skip)` — GET /b1s/v1/BusinessPartners?$filter=...&$top=...&$skip=...
  - `CreateBusinessPartnerAsync(model)` — POST /b1s/v1/BusinessPartners
  - `UpdateBusinessPartnerAsync(cardCode, model)` — PATCH /b1s/v1/BusinessPartners('{cardCode}')
  - `DeleteBusinessPartnerAsync(cardCode)` — DELETE /b1s/v1/BusinessPartners('{cardCode}')

File: `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/SapB1SessionHandler.cs`

- DelegatingHandler for HttpClient pipeline
- Handles B1SESSION cookie injection, auto-relogin on 401

File: `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/Models/`

- `SapLoginRequest.cs` — { CompanyDB, UserName, Password }
- `SapLoginResponse.cs` — { SessionId }
- `SapBusinessPartnerModel.cs` — SAP API representation
- `SapErrorResponse.cs` — { error: { code, message: { lang, value } } }
- `SapQueryResponse<T>.cs` — { value: T[], odata.nextLink }

### Step 17: SAP B1 configuration

File: `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/SapB1Options.cs`

- ServiceLayerUrl, CompanyDB, UserName, Password, SessionTimeout
- Bound from appsettings.json section `SapB1`

### Step 18: Register Infrastructure services

File: `src/SAPBOneWrapper.Infrastructure/DependencyInjection.cs`

- Extension method `AddInfrastructureServices(IConfiguration)`
- Registers EF Core DbContext with `IdentityDbContext<ApplicationUser>` (SQL Server connection string)
- Registers ASP.NET Identity: `AddIdentity<ApplicationUser, IdentityRole>()` with EF stores
- Registers `IBusinessPartnerRepository`
- Registers `ISapB1ServiceLayerClient` with named HttpClient + `SapB1SessionHandler`
- Configures `SapB1Options` from configuration

---

## Phase 5: API Layer (FastEndpoints)

### Step 19: Define FastEndpoints for Business Partners

File: `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/`

**Create:**

- `CreateBusinessPartnerEndpoint.cs` — POST /api/business-partners → sends CreateBusinessPartnerCommand via MediatR

**Get by Code:**

- `GetBusinessPartnerEndpoint.cs` — GET /api/business-partners/{cardCode} → sends GetBusinessPartnerByCodeQuery

**List:**

- `ListBusinessPartnersEndpoint.cs` — GET /api/business-partners?page=1&pageSize=20&search=... → sends GetBusinessPartnersQuery

**Update:**

- `UpdateBusinessPartnerEndpoint.cs` — PUT /api/business-partners/{cardCode} → sends UpdateBusinessPartnerCommand

**Delete:**

- `DeleteBusinessPartnerEndpoint.cs` — DELETE /api/business-partners/{cardCode} → sends DeleteBusinessPartnerCommand

**Sync (manual pull from SAP):**

- `SyncBusinessPartnersEndpoint.cs` — POST /api/business-partners/sync → pulls all BPs from SAP and upserts into local DB

### Step 20: Auth endpoints

File: `src/SAPBOneWrapper.Api/Endpoints/Auth/`

- `LoginEndpoint.cs` — POST /api/auth/login (username + password → JWT token)
- `RegisterEndpoint.cs` — POST /api/auth/register (create new user)
- `MeEndpoint.cs` — GET /api/auth/me (return current user info, requires auth)

All Business Partner endpoints require `[Authorize]`.

### Step 21: API Program.cs configuration

File: `src/SAPBOneWrapper.Api/Program.cs`

- `AddApplicationServices()`
- `AddInfrastructureServices(Configuration)`
- `AddFastEndpoints()`
- `AddAuthentication` + `AddJwtBearer` (JWT tokens for API auth)
- `AddAuthorization`
- `SwaggerDocument()` with JWT bearer security definition
- CORS policy for Blazor Web App
- Global error handling

File: `src/SAPBOneWrapper.Api/appsettings.json`

- SAP B1 config: ServiceLayerUrl, CompanyDB, UserName, Password
- SQL Server connection string
- JWT settings: Key, Issuer, Audience, ExpiryInMinutes

### Step 22: Create EF Core initial migration

- `dotnet ef migrations add InitialCreate` — creates Identity + BusinessPartner tables
- `dotnet ef database update` — applies migration

---

## Phase 6: Blazor Web App (Frontend)

### Step 23: Blazor project setup

File: `src/SAPBOneWrapper.Web/`

- Blazor Web App with Interactive Server render mode
- Register HttpClient pointing to API base URL
- Configure routing, layout with Bootstrap 5
- Add `AuthenticationStateProvider` for JWT-based auth
- Store JWT token in protected browser storage
- Add `AuthorizeView` to redirect unauthenticated users to login

### Step 24: Auth pages

File: `src/SAPBOneWrapper.Web/Components/Pages/Auth/`

- `Login.razor` — Login form (username + password), calls POST /api/auth/login, stores JWT
- `Register.razor` — Registration form, calls POST /api/auth/register
- `Logout.razor` — Clears JWT, redirects to login
- Custom `JwtAuthenticationStateProvider.cs` — Parses JWT claims, exposes auth state to Blazor

### Step 25: Business Partner pages

File: `src/SAPBOneWrapper.Web/Components/Pages/BusinessPartners/`

**List Page** — `BusinessPartnerList.razor`

- Data grid/table with paging, sorting, search
- Columns: CardCode, CardName, CardType, Phone, Email, City, SyncStatus
- Actions: View, Edit, Delete buttons
- "Add New" button
- "Sync from SAP" button
- Search bar with debounced input

**Create Page** — `BusinessPartnerCreate.razor`

- Form with validation (CardCode, CardName, CardType dropdown, Phone, Email, Address fields)
- Submit → POST to API → redirect to list on success
- Error display

**Edit Page** — `BusinessPartnerEdit.razor`

- Pre-populated form loaded from GET /api/business-partners/{cardCode}
- CardCode readonly (PK)
- Submit → PUT to API → redirect to list on success

**Detail Page** — `BusinessPartnerDetail.razor`

- Read-only view of all BP fields
- Edit and Delete action buttons
- Sync status indicator

### Step 26: Shared components

File: `src/SAPBOneWrapper.Web/Components/Shared/`

- `Pagination.razor` — Reusable pagination component
- `ConfirmDialog.razor` — Delete confirmation modal
- `Alert.razor` — Success/error notifications
- `SearchBar.razor` — Debounced search input

### Step 27: API client service

File: `src/SAPBOneWrapper.Web/Services/BusinessPartnerApiClient.cs`

- Typed HttpClient service
- Methods: GetAllAsync, GetByCodeAsync, CreateAsync, UpdateAsync, DeleteAsync, SyncAsync
- Sends JWT in Authorization header
- Error handling and deserialization

### Step 28: Navigation

- Add "Business Partners" to sidebar/nav menu
- Auth-aware navigation (show/hide based on login state)
- Routes: /business-partners, /business-partners/create, /business-partners/{cardCode}, /business-partners/{cardCode}/edit
- Auth routes: /login, /register, /logout

---

## Phase 7: Testing & Verification

### Step 29: Unit tests

File: `tests/SAPBOneWrapper.UnitTests/`

- Test CreateBusinessPartnerHandler — verify local DB save + SAP API call
- Test UpdateBusinessPartnerHandler
- Test DeleteBusinessPartnerHandler
- Test SyncBusinessPartnersHandler
- Test FluentValidation validators
- Test SapB1ServiceLayerClient (mock HttpClient)

### Step 30: Integration tests

File: `tests/SAPBOneWrapper.IntegrationTests/`

- Test API endpoints with WebApplicationFactory
- Test EF Core repository with in-memory or test SQL Server DB
- Test auth endpoints (register, login, protected routes)

---

## Relevant Files (to create)

### Domain

- `src/SAPBOneWrapper.Domain/Entities/BusinessPartner.cs`
- `src/SAPBOneWrapper.Domain/Enums/CardType.cs`
- `src/SAPBOneWrapper.Domain/Enums/SyncStatus.cs`
- `src/SAPBOneWrapper.Domain/Interfaces/IBusinessPartnerRepository.cs`
- `src/SAPBOneWrapper.Domain/Interfaces/ISapB1ServiceLayerClient.cs`

### Application

- `src/SAPBOneWrapper.Application/DTOs/BusinessPartnerDto.cs`
- `src/SAPBOneWrapper.Application/DTOs/CreateBusinessPartnerDto.cs`
- `src/SAPBOneWrapper.Application/DTOs/UpdateBusinessPartnerDto.cs`
- `src/SAPBOneWrapper.Application/DTOs/PagedResultDto.cs`
- `src/SAPBOneWrapper.Application/DTOs/BusinessPartnerFilterDto.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/CreateBusinessPartnerCommand.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/CreateBusinessPartnerHandler.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/UpdateBusinessPartnerCommand.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/UpdateBusinessPartnerHandler.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/DeleteBusinessPartnerCommand.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/DeleteBusinessPartnerHandler.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/SyncBusinessPartnersCommand.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Commands/SyncBusinessPartnersHandler.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Queries/GetBusinessPartnerByCodeQuery.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Queries/GetBusinessPartnerByCodeHandler.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Queries/GetBusinessPartnersQuery.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Queries/GetBusinessPartnersHandler.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Validators/CreateBusinessPartnerValidator.cs`
- `src/SAPBOneWrapper.Application/Features/BusinessPartners/Validators/UpdateBusinessPartnerValidator.cs`
- `src/SAPBOneWrapper.Application/Mappings/BusinessPartnerMappingConfig.cs`
- `src/SAPBOneWrapper.Application/Behaviors/ValidationBehavior.cs`
- `src/SAPBOneWrapper.Application/DependencyInjection.cs`

### Infrastructure

- `src/SAPBOneWrapper.Infrastructure/Identity/ApplicationUser.cs`
- `src/SAPBOneWrapper.Infrastructure/Data/AppDbContext.cs`
- `src/SAPBOneWrapper.Infrastructure/Data/Configurations/BusinessPartnerConfiguration.cs`
- `src/SAPBOneWrapper.Infrastructure/Repositories/BusinessPartnerRepository.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/SapB1ServiceLayerClient.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/SapB1SessionHandler.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/SapB1Options.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/Models/SapLoginRequest.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/Models/SapLoginResponse.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/Models/SapBusinessPartnerModel.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/Models/SapErrorResponse.cs`
- `src/SAPBOneWrapper.Infrastructure/SapServiceLayer/Models/SapQueryResponse.cs`
- `src/SAPBOneWrapper.Infrastructure/DependencyInjection.cs`

### Api

- `src/SAPBOneWrapper.Api/Program.cs`
- `src/SAPBOneWrapper.Api/appsettings.json`
- `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/CreateBusinessPartnerEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/GetBusinessPartnerEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/ListBusinessPartnersEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/UpdateBusinessPartnerEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/DeleteBusinessPartnerEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/BusinessPartners/SyncBusinessPartnersEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/Auth/LoginEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/Auth/RegisterEndpoint.cs`
- `src/SAPBOneWrapper.Api/Endpoints/Auth/MeEndpoint.cs`

### Web (Blazor)

- `src/SAPBOneWrapper.Web/Program.cs`
- `src/SAPBOneWrapper.Web/Services/BusinessPartnerApiClient.cs`
- `src/SAPBOneWrapper.Web/Services/AuthApiClient.cs`
- `src/SAPBOneWrapper.Web/Services/JwtAuthenticationStateProvider.cs`
- `src/SAPBOneWrapper.Web/Components/Pages/Auth/Login.razor`
- `src/SAPBOneWrapper.Web/Components/Pages/Auth/Register.razor`
- `src/SAPBOneWrapper.Web/Components/Pages/Auth/Logout.razor`
- `src/SAPBOneWrapper.Web/Components/Pages/BusinessPartners/BusinessPartnerList.razor`
- `src/SAPBOneWrapper.Web/Components/Pages/BusinessPartners/BusinessPartnerCreate.razor`
- `src/SAPBOneWrapper.Web/Components/Pages/BusinessPartners/BusinessPartnerEdit.razor`
- `src/SAPBOneWrapper.Web/Components/Pages/BusinessPartners/BusinessPartnerDetail.razor`
- `src/SAPBOneWrapper.Web/Components/Shared/Pagination.razor`
- `src/SAPBOneWrapper.Web/Components/Shared/ConfirmDialog.razor`
- `src/SAPBOneWrapper.Web/Components/Shared/Alert.razor`
- `src/SAPBOneWrapper.Web/Components/Shared/SearchBar.razor`

---

## Verification

1. **Build** — `dotnet build SAPBOneWrapper.sln` compiles with zero errors
2. **Migration** — `dotnet ef database update` creates Identity + BusinessPartner tables
3. **Swagger** — Run API, navigate to /swagger, verify 9 endpoints (6 BP + 3 Auth) with JWT security scheme
4. **Unit Tests** — `dotnet test` passes all handler, validator, and client tests
5. **Manual E2E** — Register → Login → Create BP → List → Edit → Delete → Sync from SAP → Logout
6. **SAP Sync** — (Requires SAP B1 instance) Click "Sync from SAP" → BPs pulled into local DB

---

## Decisions

- **Blazor Web App (Interactive Server)** — .NET 10 Blazor Web App with interactive server-side rendering
- **Local DB + SAP Sync** — Business Partners stored locally in SQL Server with SyncStatus tracking
- **FastEndpoints** — REPR pattern (Request-Endpoint-Response) for clean, minimal API endpoints
- **MediatR CQRS** — Commands and Queries with handlers for separation of concerns and testability
- **Mapster over AutoMapper** — Faster, lighter mapping library
- **SAP B1 Session Management** — Custom DelegatingHandler manages B1SESSION cookie lifecycle (auto-login, relogin on 401)
- **CardCode as primary key** — Matches SAP B1's natural key
- **Write-through sync** — Local changes push to SAP immediately; pull-sync is an explicit manual action
- **ASP.NET Identity + JWT** — Local user/password auth stored in SQL Server, API protected with JWT Bearer tokens
- **Plain Bootstrap 5** — No extra UI library, manual styling
- **Manual sync only** — Explicit "Sync from SAP" button/endpoint; periodic sync deferred
- **SAP is source of truth** — Sync conflicts resolved by SAP data overwriting local data

## Deferred (Future Phases)

1. **Periodic SAP sync** — Background job (e.g., Hangfire) to pull BPs from SAP on a schedule
2. **UI library upgrade** — Can switch from Bootstrap to MudBlazor/Radzen later if needed
3. **Additional entities** — Users, Items, Orders, Invoices, etc.
4. **Role-based access** — Admin vs User roles with granular permissions
