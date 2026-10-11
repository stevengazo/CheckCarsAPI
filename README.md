<div align="center">

# ⚙️ CheckCars API

**Backend RESTful para la gestión de flotas vehiculares**

![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-9-512BD4)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)

</div>

---

## 📋 Descripción

CheckCars API es el núcleo del ecosistema **CheckCars**. Expone una API RESTful construida con **.NET 9** y **Entity Framework Core** sobre **SQL Server**, con autenticación JWT, tiempo real (SignalR) y despliegue en **Docker**. Da servicio al [dashboard web](https://github.com/stevengazo/checkcars-web) y a la [app móvil](https://github.com/stevengazo/CheckCar).

## ✨ Características

- 🔐 **Autenticación JWT** con ASP.NET Identity y roles (Admin / Manager / User / Guest).
- 🚙 **Gestión de vehículos** (CRUD, imágenes, servicios, adjuntos, recordatorios).
- 📝 **Formularios dinámicos**: define campos y fotos requeridas por tipo de reporte (`FormDefinition`) y captura reportes flexibles (`DynamicReport`) usando **columnas JSON**.
- 📄 **Reportes**: salidas/entradas, averías, accidentes, devoluciones — con fotos y geolocalización.
- 📅 **Reservas** de vehículos con confirmación/cancelación.
- 📊 **Analítica** (endpoints de gráficas por vehículo, usuario, marca, tipo y fecha).
- 💬 **Chat** y 🔔 **notificaciones** en tiempo real vía **SignalR**.
- 🖼️ **Archivos estáticos** servidos bajo `/images`.
- 📝 **Logging** con Serilog.

## 🏗️ Modelo de datos

Dos `DbContext` sobre **bases de datos separadas**:

| Contexto | Base de datos | Contenido |
|----------|---------------|-----------|
| `ApplicationDbContext` | Identity/Users | Usuarios, roles, chats, mensajes, notificaciones |
| `ReportsDbContext` | Reports | Vehículos, reportes (TPC), reservas, servicios, formularios dinámicos |

> Los reportes fijos usan **Table-per-Concrete-class (TPC)**; los formularios dinámicos usan **columnas JSON** (`OwnsMany().ToJson()`).

## 🚀 Puesta en marcha

### Requisitos
- [Docker](https://www.docker.com/) y Docker Compose
- o [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) + SQL Server

### Con Docker Compose
```bash
docker compose up --build
```
Levanta la API + una instancia de SQL Server 2022.

- **API:** `http://localhost:8085`
- **Swagger:** `http://localhost:8085/swagger`

> ℹ️ La app escucha internamente en el **puerto 5050** (`Program.cs`); el `docker-compose` publica ese puerto en el host (p. ej. `8085:5050`).

## 📡 Endpoints principales

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Account/Login` | Autenticación |
| POST | `/api/Account/Register` | Registro de usuario |
| GET/POST/PUT/DELETE | `/api/Cars` | CRUD de vehículos |
| POST | `/api/Cars/{id}/image` | Subir imagen de vehículo |
| GET/POST | `/api/FormDefinitions` | Definiciones de formularios dinámicos |
| POST | `/api/DynamicReports/form` | Crear reporte dinámico (multipart) |
| GET | `/api/DynamicReports/search` | Buscar reportes dinámicos |
| GET/POST | `/api/Bookings` | Reservas |
| GET | `/api/Charts/*` | Analítica |
| GET/POST | `/api/Notifications` | Notificaciones |

## ⚙️ Configuración

Variables clave (ver `docker-compose.yml`):

```yaml
ConnectionStrings__IdentityUsers: Server=sqlserver,1433;Database=CheckCarsUsers;...
ConnectionStrings__ReportsConnection: Server=sqlserver,1433;Database=CheckCarsReports;...
Jwt__Issuer / Jwt__Audience / Jwt__Key
Smtp__Host / Smtp__Port / Smtp__Username / Smtp__Password
StaticFiles__ImagesPath: images
```
> ⚠️ Usa valores seguros (contraseñas, clave JWT) en producción; no uses los de ejemplo.

## 🗃️ Migraciones

```bash
dotnet ef migrations add <Nombre> --context ReportsDbContext
dotnet ef migrations add <Nombre> --context ApplicationDbContext
```
Las migraciones se aplican automáticamente al arrancar.

## 🔗 Ecosistema CheckCars

- ⚙️ **API** — este repositorio
- 🌐 [CheckCars Dashboard](https://github.com/stevengazo/checkcars-web) — frontend React
- 📱 [CheckCar](https://github.com/stevengazo/CheckCar) — app móvil .NET MAUI

## 📄 Licencia

MIT · **Steven Gazo** · [stevengazo.co.cr](https://stevengazo.co.cr)
