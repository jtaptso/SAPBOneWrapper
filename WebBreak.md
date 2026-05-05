# SAPBOneWrapper.Web — Project Breakdown

## Technology Stack

The web project is a **Blazor Server** application (.NET 10) using **Interactive Server rendering** — meaning the UI runs on the server with a real-time SignalR circuit to the browser. It is a separate frontend that communicates with the `SAPBOneWrapper.Api` project via HTTP, configured via `ApiBaseUrl` in `appsettings.json`.

---

## Startup (`Program.cs`)

1. **Blazor Server** is registered with `AddRazorComponents().AddInteractiveServerComponents()`.
2. **Cookie authentication** is configured as the scheme, but HTTP-level auth is intentionally bypassed — authorization is enforced purely at the Blazor component level via `[Authorize]` attributes and `AuthorizeRouteView`.
3. **`JwtAuthenticationStateProvider`** is registered as the `AuthenticationStateProvider` — this is the core auth mechanism.
4. Two typed **API clients** are registered as scoped services: `AuthApiClient` and `BusinessPartnerApiClient`.
5. A single `HttpClient` is created pointing at `ApiBaseUrl`.

---

## Authentication Flow (`Services/`)

### `JwtAuthenticationStateProvider`
- Extends Blazor's `AuthenticationStateProvider`.
- Stores the JWT token in **`sessionStorage`** (via JS interop), and also in memory.
- On each `GetAuthenticationStateAsync()` call, it reads the token from memory or `sessionStorage`, parses it with `JwtSecurityTokenHandler`, checks expiry, and builds a `ClaimsPrincipal` from the JWT claims.
- Exposes `SetTokenAsync()` / `ClearTokenAsync()` to store/remove the token and notify Blazor components of auth state changes.

### `AuthApiClient`
- `LoginAsync()` → POSTs to `/api/auth/login`, receives a JWT, calls `authState.SetTokenAsync()` to store it, triggering re-renders of all auth-aware components.
- `RegisterAsync()` → POSTs to `/api/auth/register`.
- `LogoutAsync()` → calls `authState.ClearTokenAsync()`.

### `BusinessPartnerApiClient`
- All methods call `SetAuthHeader()` first, which reads `authState.Token` and sets the `Authorization: Bearer <token>` header on the `HttpClient`.
- Provides full CRUD: `GetAllAsync`, `GetByCodeAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, and `SyncAsync` (triggers SAP sync on the API).

---

## Routing & Layout

### `App.razor`
The HTML shell. It references the Bootstrap CSS, app CSS, the Blazor framework JS, and renders `<Routes>` with `prerender: false` (avoids SSR issues with `sessionStorage`).

### `Routes.razor`
The router. Uses **`AuthorizeRouteView`** with `MainLayout` as the default layout. If a page requires authorization and the user is unauthenticated, it renders `<RedirectToLogin />`.

### `RedirectToLogin.razor`
Redirects to `/login` via `NavigationManager`, but only runs **after** the first interactive render (i.e., after the SignalR circuit connects), not during SSR — preventing redirect loops.

### `MainLayout.razor`
Standard sidebar + main content layout. The top bar shows "Hello, [username]" using `<AuthorizeView>` when logged in.

### `NavMenu.razor`
Navigation adapts to auth state:
- **Unauthenticated**: shows Login and Register links.
- **Authenticated**: shows Business Partners and Logout links.

---

## Pages

| Route | Page | Auth Required |
|---|---|---|
| `/login` | `Login.razor` | No |
| `/register` | `Register.razor` | No |
| `/logout` | `Logout.razor` | No |
| `/business-partners` | `BusinessPartnerList.razor` | Yes (`[Authorize]`) |
| `/business-partners/create` | `BusinessPartnerCreate.razor` | Yes |
| `/business-partners/{code}` | `BusinessPartnerDetail.razor` | Yes |
| `/business-partners/{code}/edit` | `BusinessPartnerEdit.razor` | Yes |

The **BusinessPartnerList** page includes search filtering, pagination, a "Sync from SAP" button (calls `SyncAsync()`), and per-row View/Edit/Delete actions with Bootstrap badges for card type and sync status.

---

## Key Design Decisions

- **No ASP.NET Identity on the web side** — identity lives in the API project. The web project is purely a Blazor frontend.
- **JWT is never sent in a cookie from this app** — it lives in `sessionStorage` and is attached manually as a Bearer header on every API call.
- **`prerender: false`** on the Routes component avoids the common SSR pitfall where `sessionStorage` is unavailable during server-side pre-rendering, which would break the auth state check.
