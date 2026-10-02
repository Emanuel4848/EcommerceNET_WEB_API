# E-Commerce API with ASP.NET Core

[Versión en español](README.md)

REST API for managing an e-commerce catalog. It handles categories, products, and users, including JWT authentication, role-based authorization, API versioning, image uploads, pagination, and initial data seeding.

**Live API:** [Azure App Service](https://ecommerce-api-bzhxamaredawf4d4.mexicocentral-01.azurewebsites.net)

**Public demos:** [List categories](https://ecommerce-api-bzhxamaredawf4d4.mexicocentral-01.azurewebsites.net/api/v1/Categories) | [List products](https://ecommerce-api-bzhxamaredawf4d4.mexicocentral-01.azurewebsites.net/api/v1/Products)

### Test collection

![Postman collection organized by module](docs/images/postman-collection.png)

[Download the Postman collection (.json)](API-Ecommerce.postman_collection.json)

### Create a test user

Any visitor can register an account with the `User` role:

```http
POST https://ecommerce-api-bzhxamaredawf4d4.mexicocentral-01.azurewebsites.net/api/v1/Users
Content-Type: application/json
```

```json
{
  "name": "Demo User",
  "userName": "unique-email@example.com",
  "password": "ChangeThisPassword123!",
  "role": "User"
}
```

The visitor can then call `POST /api/v1/Users/Login` to obtain a JWT. The server always assigns the `User` role, regardless of the value sent in `role`.

## Features

- Category and product CRUD operations.
- User registration, lookup, and login.
- JSON Web Token authentication.
- Role-based authorization with `Admin` and `User`.
- ASP.NET Core Identity for users, passwords, and roles.
- Route-based API versions `v1` and `v2`.
- Product search by name or description.
- Product filtering by category.
- Product pagination.
- Stock validation and reduction during purchases.
- Image uploads through `multipart/form-data`.
- Static delivery of uploaded images.
- Entity and DTO mapping with Mapster.
- SQL Server persistence through Entity Framework Core.
- Initial roles, categories, products, and users.
- Response caching for category queries.
- OpenAPI and Swagger documentation in development.
- Postman collection for endpoint testing.
- Deployment to Azure App Service with Azure SQL Database.

## Architecture

The project follows a layered organization with the Repository Pattern. A request generally follows this path:

```text
HTTP client
    |
    v
Controller
    |
    +-- Input/output DTO
    +-- Mapster
    |
    v
Repository interface
    |
    v
Repository
    |
    v
ApplicationDbContext / Entity Framework Core
    |
    v
SQL Server
```

### Responsibilities

- **Controllers:** define routes, receive requests, validate data, coordinate repositories, and build HTTP responses.
- **DTOs:** control the data entering and leaving the API without exposing entities directly.
- **Mapping:** contains Mapster configurations for DTO, entity, and response transformations.
- **Repository/IRepository:** interfaces define contracts, while repositories implement queries and persistence operations.
- **Models:** represent the `Category`, `Product`, and `ApplicationUser` entities.
- **Data:** contains the Entity Framework Core context and initial data setup.
- **Migrations:** keep the SQL Server schema history.
- **Constants:** centralize policy names and cache profiles.
- **Program.cs:** configures dependencies, database access, Identity, JWT, versioning, CORS, caching, Swagger, and middleware.

## Data model

### Category

Represents a catalog category.

- `IdCategory`
- `Name`
- `CreationDate`

### Product

Represents a product belonging to one category.

- `ProductId`
- `Name`
- `Description`
- `Price`
- `SKU`
- `Stock`
- `ImgUrl`
- `ImgUrlLocal`
- `CreationDate`
- `UpdateDate`
- `CategoryId`

The relationship is **Category 1:N Product**. Entity Framework Core uses `CategoryId` as the foreign key and `Category` as the navigation property.

### ApplicationUser

Extends `IdentityUser` with the person's name. Identity manages the identifier, username, email, password hash, roles, and other security-related fields.

## DTOs and mapping

The API uses specific DTOs for each operation:

- `CategoryDto` and `CreateCategoryDto`.
- `ProductDto`, `CreateProductDto`, and `UpdateProductDto`.
- `CreateUserDto`, `UserDto`, and `UserDataDto`.
- `UserLoginDto` and `UserLoginResponseDto`.
- `PaginationResponse<T>` for paginated results.

Mapster registers mapping configurations when the application starts. The profiles transform categories, products, and users without injecting a mapper into each controller. Product mapping also obtains `CategoryName` from the related `Category`.

## Rules and validation

- Categories and products cannot be created with an already registered name.
- A category name is required and must contain between 3 and 50 characters.
- A product can only be created or updated with an existing category.
- Price and stock values cannot be negative.
- A purchase requires a positive quantity and sufficient stock.
- Every public registration receives the `User` role, even if the client sends a different value.
- Catalog write operations and user queries require the `Admin` role.
- Controllers return HTTP responses such as `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, and `404 Not Found`, depending on the result.

## Authentication and authorization

Registration and login are public. After a successful login, the API creates a JWT signed with HMAC SHA-256 and valid for two hours.

The token contains:

- User identifier.
- Username.
- Role.

Administrative endpoints require:

```http
Authorization: Bearer <token>
```

New registrations always receive the `User` role. The API does not trust a role sent in the request body. Administrators are created through a controlled process.

## API versioning

Categories provide two versions:

- `/api/v1/Categories`: initial version; its list operation is marked as obsolete.
- `/api/v2/Categories`: updated list ordered by identifier.

Products and users are version-neutral, but their routes still include the version segment:

```text
/api/v1/Products
/api/v1/Users
```

## Main endpoints

Base URL:

```text
https://ecommerce-api-bzhxamaredawf4d4.mexicocentral-01.azurewebsites.net
```

### Categories

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/v1/Categories` | Public | Lists categories |
| GET | `/api/v1/Categories/{id}` | Public | Gets one category |
| POST | `/api/v1/Categories` | Admin | Creates a category |
| PATCH | `/api/v1/Categories/{id}` | Admin | Updates a category |
| DELETE | `/api/v1/Categories/{id}` | Admin | Deletes a category |
| GET | `/api/v2/Categories` | Public | Lists categories with v2 behavior |

### Products

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/v1/Products` | Public | Lists all products |
| GET | `/api/v1/Products/{id}` | Public | Gets one product |
| GET | `/api/v1/Products/Paged?pageNumber=1&pageSize=5` | Public | Lists paginated products |
| GET | `/api/v1/Products/searchProductByCategory/{categoryId}` | Public | Filters by category |
| GET | `/api/v1/Products/searchProductByNameDescription/{term}` | Public | Searches by name or description |
| POST | `/api/v1/Products` | Admin | Creates a product |
| PUT | `/api/v1/Products/{id}` | Admin | Updates a product |
| PATCH | `/api/v1/Products/buyProduct/{name}/{quantity}` | Admin | Reduces stock |
| DELETE | `/api/v1/Products/{id}` | Admin | Deletes a product |

Product creation and updates use `multipart/form-data` to support an optional image.

Paginated responses contain `pageNumber`, `pageSize`, `totalPages`, and `items`.

### Users

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/v1/Users` | Admin | Lists users |
| GET | `/api/v1/Users/{id}` | Admin | Gets one user |
| POST | `/api/v1/Users` | Public | Registers a user with the `User` role |
| POST | `/api/v1/Users/Login` | Public | Validates credentials and returns a JWT |

## Initial data

Entity Framework Core seeding adds the following data when required:

- `Admin` and `User` roles.
- Catalog categories.
- Sample products.
- Regular user.
- Initial administrator with a password read from configuration instead of being stored in the repository.

## Technology stack

| Area | Technology |
| --- | --- |
| Framework | .NET 10, ASP.NET Core Web API |
| Persistence | Entity Framework Core 10 |
| Database | SQL Server, Azure SQL Database |
| Security | ASP.NET Core Identity, JWT Bearer |
| Mapping | Mapster |
| Versioning | Asp.Versioning.Mvc |
| Documentation | OpenAPI, Swagger / Swashbuckle |
| Manual testing | Postman |
| Containers | Docker Compose, SQL Server 2022 |
| Deployment | Azure App Service |

## Project structure

```text
ApiEcommerce/
|-- Constants/
|-- Controllers/
|   |-- V1/
|   |-- V2/
|-- Data/
|-- Mapping/
|-- Migrations/
|-- Models/
|   |-- DTOs/
|       |-- Responses/
|-- Repository/
|   |-- IRepository/
|-- wwwroot/
|   |-- ProductsImages/
|-- Program.cs
|-- docker-compose.yml
|-- API-Ecommerce.postman_collection.json
```

## Configuration

The application requires the following keys. Production values should be configured through environment variables or Azure App Service settings.

| Variable | Purpose |
| --- | --- |
| `ConnectionStrings__ConexionSql` | SQL Server connection |
| `ApiSettings__SecretKey` | JWT signing and validation |
| `SeedAdmin__Password` | Initial administrator password |
| `MSSQL_SA_PASSWORD` | Local SQL Server container password |

Example without real credentials:

```json
{
  "ApiSettings": {
    "SecretKey": "a-long-private-secret"
  },
  "ConnectionStrings": {
    "ConexionSql": "Server=localhost;Database=ApiEcommerceNET;User ID=SA;Password=YOUR_PASSWORD;TrustServerCertificate=true"
  }
}
```

## Running locally

Requirements:

- .NET 10 SDK.
- SQL Server or Docker.
- Entity Framework Core tools.

```bash
dotnet restore
docker compose up -d
dotnet ef database update
dotnet run
```

Swagger is available in development at:

```text
http://localhost:5163/swagger
```

## Postman

Import [API-Ecommerce.postman_collection.json](API-Ecommerce.postman_collection.json) into Postman. Register or log in first, then use the returned JWT for protected requests.

Remove real tokens and passwords from exported examples before committing collection changes.

## Deployment

The API is hosted on Azure App Service and uses Azure SQL Database. The database connection string, JWT key, and initial administrator password are managed through application environment variables.


## Author

**Yuviny Emanuel Velásquez González**

- [GitHub](https://github.com/Emanuel4848)
- [LinkedIn](https://www.linkedin.com/in/yuviny-gonzalez)
