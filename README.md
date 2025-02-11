# CheckCars API

## Overview
CheckCars API is a RESTful service built with .NET 9 and SQL Server. It provides authentication for users and basic CRUD operations to manage car reports. The application is containerized using Docker for easy deployment and scalability.

## Features
- User authentication with JWT tokens.
- CRUD operations for car reports.
- Docker support for containerized deployment.
- SQL Server database integration.

## Technologies Used
- .NET 9
- SQL Server
- Docker
- JWT Authentication
- Entity Framework Core

## Prerequisites
Ensure you have the following installed on your system:
- [Docker](https://www.docker.com/)
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## Getting Started
### 1. Clone the Repository
```sh
git clone https://github.com/yourusername/CheckCarsAPI.git
cd CheckCarsAPI
```

### 2. Environment Variables
The application uses environment variables for configuration. Modify them as needed in `docker-compose.yml`:
```yaml
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=TestCheckCarsUsers;User Id=sa;Password=Siera4388$;TrustServerCertificate=True;
  - ConnectionStrings__ReportsConnection=Server=sqlserver,1433;Database=TestCheckCarsReports;User Id=sa;Password=Siera4388$;TrustServerCertificate=True;
  - Jwt__Issuer=yourdomain.com
  - Jwt__Audience=yourdomain.com
  - Jwt__Key=c3VwZXJzZWN1cmVzc2ltdWx0YXBhc3N3b3JkYmFzaWNhbmdsaWZl
```

### 3. Run the Application with Docker
To start the API and database using Docker Compose, run:
```sh
docker-compose up --build
```

This will:
- Build and start the .NET API container.
- Start an SQL Server instance.

### 4. Accessing the API
Once running, you can access the API at:
- **API Base URL:** `http://localhost:8080`
- **Swagger UI:** `http://localhost:8080/swagger`

## Endpoints
| Method | Endpoint | Description |
|--------|---------|-------------|
| POST | `/api/auth/login` | User authentication |
| POST | `/api/auth/register` | Register a new user |
| GET | `/api/reports` | Retrieve all car reports |
| GET | `/api/reports/{id}` | Retrieve a specific report |
| POST | `/api/reports` | Create a new report |
| PUT | `/api/reports/{id}` | Update an existing report |
| DELETE | `/api/reports/{id}` | Delete a report |

## Stopping the Application
To stop the containers, run:
```sh
docker-compose down
```

## License
This project is licensed under the MIT License.

## Contact
For any questions or issues, feel free to reach out at [stevengazo.co.cr](https://stevengazo.co.cr).

