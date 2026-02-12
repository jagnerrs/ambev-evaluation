# Ambev Developer Evaluation

API for managing sales records with full CRUD operations, built with DDD (Domain-Driven Design) principles and the External Identities pattern.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (for PostgreSQL database)
- [Docker Compose](https://docs.docker.com/compose/) (included with Docker Desktop)
ou
- [WSL](learn.microsoft.com/pt-br/windows/wsl/install) (Linux)

## Configuration

### JWT Authentication

The API uses JWT Bearer authentication. Configure the secret key in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong"
  }
}
```

The secret key must be at least 32 characters for HS256.

### Database Connection

The application uses PostgreSQL. Configure the connection string in `appsettings.json` or via environment variables.

**Default connection string** (when using docker-compose):

```
Server=ambev.developerevaluation.database;Database=developer_evaluation;User Id=developer;Password=ev@luAt10n
```

**Local development** (when running database in Docker):

```
Server=localhost;Port=5432;Database=developer_evaluation;User Id=developer;Password=ev@luAt10n
```

## Execution

### 1. Start the Database

```bash
docker-compose up -d ambev.developerevaluation.database
```

### 2. Apply Migrations

If `dotnet-ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

From the solution root:

```bash
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

### 3. Run the Application

```bash
docker-compose up -d ambev.developerevaluation.webapi
```

The API will be available at:
- HTTP: http://localhost:8080 
- Swagger UI: http://localhost:8080/swagger

ou

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

The API will be available at:
- HTTP: http://localhost:5000 (or the port configured in launchSettings.json)
- Swagger UI: http://localhost:5000/swagger (when running in Development)

## Authentication

The **Sales API** and most **Users API** endpoints require authentication via JWT Bearer token. Only **Auth** (`POST /api/auth`) and **Create User** (`POST /api/users`) remain public for login and user registration.

**Roles:** `Admin`, `Manager`, `Customer`. Some endpoints restrict access by role (e.g. Delete User requires Admin; List Users requires Admin or Manager).

### Obtaining a Token

1. Create a user (if needed) via `POST /api/users`
2. Authenticate via `POST /api/auth`:

```json
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

The response contains a `token` in the `data` object. Use this token to access protected endpoints.

### Using the Token

Include the token in the `Authorization` header:

```
Authorization: Bearer {seu_token}
```

### Swagger UI

1. Open the Swagger UI at `/swagger`
2. Click **Authorize**
3. Enter the token in the format: `Bearer {seu_token}` or just `{seu_token}`
4. Click **Authorize** and **Close**
5. All requests to protected endpoints will now include the token

## Testing

Run all tests:

```bash
dotnet test .\tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj
dotnet test .\tests\Ambev.DeveloperEvaluation.Integration\Ambev.DeveloperEvaluation.Integration.csproj
dotnet test .\tests\Ambev.DeveloperEvaluation.Unit\Ambev.DeveloperEvaluation.Unit.csproj
```

Run with coverage report:

```bash
# Windows
coverage-report.bat

# Linux/macOS
./coverage-report.sh
```

## API Endpoints

### Auth API (Public)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth` | Authenticate user and obtain JWT token |

**Request body:**
```json
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

### Users API

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| POST | `/api/users` | Create a new user | **Public** |
| GET | `/api/users` | List users (paginated, query: `page`, `pageSize`) | **Admin**, **Manager** |
| GET | `/api/users/{id}` | Get user by ID | **Authenticated** (any role) |
| DELETE | `/api/users/{id}` | Delete a user | **Admin** |

### Sales API (Requires Authentication)

All endpoints below require the `Authorization: Bearer {token}` header.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sales` | Create a new sale |
| GET | `/api/sales/{id}` | Get sale by ID |
| GET | `/api/sales` | List sales (paginated, query: `page`, `pageSize`) |
| PUT | `/api/sales/{id}` | Update a sale |
| DELETE | `/api/sales/{id}` | Delete a sale |
| POST | `/api/sales/{id}/cancel` | Cancel a sale |
| POST | `/api/sales/{id}/items/{itemId}/cancel` | Cancel a sale item |

### Create Sale Request Example

```json
{
  "saleDate": "2025-02-08T12:00:00Z",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "John Doe",
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "branchName": "Downtown Branch",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "productName": "Product A",
      "quantity": 5,
      "unitPrice": 10.00
    }
  ]
}
```

## Business Rules

- **Quantity discount**: 4-9 identical items = 10% discount; 10-20 = 20% discount
- **Maximum quantity**: 20 identical items per product
- **No discount** for quantities below 4 items

## Project Structure

```
src/
├── Ambev.DeveloperEvaluation.Application/   # CQRS handlers, commands
├── Ambev.DeveloperEvaluation.Common/        # Cross-cutting concerns
├── Ambev.DeveloperEvaluation.Domain/        # Entities, domain logic
├── Ambev.DeveloperEvaluation.IoC/           # Dependency injection
├── Ambev.DeveloperEvaluation.ORM/           # EF Core, repositories
└── Ambev.DeveloperEvaluation.WebApi/        # Controllers, API layer

tests/
├── Ambev.DeveloperEvaluation.Unit/
├── Ambev.DeveloperEvaluation.Integration/
└── Ambev.DeveloperEvaluation.Functional/
```
