# TaskTrackerAPI

ASP.NET Core Web API for task and project management.

## Tech Stack
- ASP.NET Core Web API (.NET 10)
- C#
- Entity Framework Core
- SQL Server
- JWT Authentication and Authorization

## Features
- User registration and login with password hashing
- JWT authentication and role-based authorization
- Role-based and resource-based task update permissions
- CRUD operations for projects, tasks, users, and comments
- Task filtering by project, status, and assigned user
- Task statistics and validation
- Layered architecture using Services, Repositories, Unit of Work, DTOs, and Dependency Injection

## Setup
1. Clone the repository.
2. Copy `TaskTrackerAPI/appsettings.example.json` to `TaskTrackerAPI/appsettings.json`.
3. Configure your local SQL Server connection string and JWT secret.
4. Run `dotnet restore` from the project directory.
5. Apply migrations using `dotnet ef database update`.
6. Run the API using `dotnet run`.

Open the Swagger URL shown when the API starts to explore the endpoints.

**Note:** Local application settings and secrets are not included in this repository.
