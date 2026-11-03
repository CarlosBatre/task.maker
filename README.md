# TaskMaker API

API REST para la gestión de tareas (crear, listar, consultar, actualizar y eliminar), construida con **.NET 10**, **ASP.NET Core** y **SQLite**.

![CI](https://github.com/CarlosBatre/task.maker/actions/workflows/dotnet-desktop.yml/badge.svg)

---

## Contenido

- [Tecnologías y por qué .NET](#tecnologías-y-por-qué-net)
- [Arquitectura](#arquitectura)
- [Requisitos](#requisitos)
- [Ejecución local](#ejecución-local)
- [Ejecución con Docker](#ejecución-con-docker)
- [Documentación de la API](#documentación-de-la-api)
- [Pruebas automatizadas](#pruebas-automatizadas)
- [Integración continua (CI)](#integración-continua-ci)
- [Estructura del proyecto](#estructura-del-proyecto)

---

## Tecnologías y por qué .NET

| Tecnología | Uso |
|---|---|
| **.NET 10 / C#** | Lenguaje y runtime |
| **ASP.NET Core Web API** | Controllers, validación, inyección de dependencias |
| **Entity Framework Core 10 + SQLite** | Persistencia y migraciones |
| **Mapster** | Mapeo entre entidades y DTOs |
| **Asp.Versioning** | Versionado de la API (`/v1/...`) |
| **Swashbuckle (Swagger)** | Documentación interactiva |
| **Serilog** | Logs estructurados |
| **xUnit + WebApplicationFactory** | Pruebas de integración |
| **Docker / GitHub Actions** | Empaquetado y CI |

**¿Por qué .NET para este caso?**

- **Todo lo necesario viene integrado:** ASP.NET Core trae inyección de dependencias, validación de modelos (`[ApiController]` + Data Annotations), enrutamiento y manejo de respuestas HTTP. No hace falta armar el stack con librerías sueltas.
- **Tipado fuerte:** C# detecta errores en compilación y los DTOs tipados dejan claro el contrato de cada endpoint.
- **EF Core + SQLite:** cumple el requisito de "base de datos ligera, sin infraestructura productiva". La base es un archivo que se crea sola con migraciones al arrancar.
- **Rendimiento:** ASP.NET Core está entre los frameworks web más rápidos y es multiplataforma (Windows, Linux, macOS, contenedores).
- **Pruebas de primera clase:** `WebApplicationFactory` levanta la API completa en memoria para probar los endpoints reales sin desplegarla.
- **Soporte LTS y ecosistema maduro**, con imágenes oficiales de Docker mantenidas por Microsoft.

---

## Arquitectura

El proyecto está organizado en **capas**, cada una con una responsabilidad:

```
WebApi  ──►  Application  ──►  Infrastructure  ──►  SQLite
(HTTP)       (lógica)          (datos)
```

| Capa | Responsabilidad |
|---|---|
| **WebApi** | Controllers, rutas, códigos HTTP, formato de respuesta (`BaseResponse`), Swagger |
| **Application** | Servicios con la lógica de negocio, DTOs, validaciones, mapeos (Mapster) |
| **Infrastructure** | Entidades, `ApplicationDbContext`, repositorios y migraciones de EF Core |
| **WebApi.Tests** | Pruebas de integración de los endpoints |

Decisiones relevantes:
- **Repositorio genérico** (`Repository<T>`) con operaciones comunes y paginación; `TaskItemRepository` agrega consultas propias.
- **Borrado lógico:** `DELETE` marca la tarea como inactiva (`IsActive = false`); deja de aparecer en todas las consultas.
- **Fechas en UTC:** `CreatedAt` y `UpdatedAt` se asignan automáticamente y siempre se devuelven en formato ISO 8601 con `Z`.
- **Títulos únicos:** no se permiten dos tareas activas con el mismo título, sin importar mayúsculas, tildes o espacios extra. Se valida en el servicio y con un índice único en la base.

---

## Requisitos

| Para ejecutar... | Necesitas |
|---|---|
| Localmente | [.NET SDK 10.0.403](https://dotnet.microsoft.com/download/dotnet/10.0) o superior (banda 10.0.4xx, definido en `global.json`) |
| Con Docker | [Docker Desktop](https://www.docker.com/products/docker-desktop/) (no requiere .NET instalado) |

Opcional: `dotnet-ef` para crear migraciones nuevas (`dotnet tool install --global dotnet-ef`).

---

## Ejecución local

```bash
git clone https://github.com/CarlosBatre/task.maker.git
cd task.maker

dotnet restore
dotnet run --project WebApi --launch-profile https
```

Al arrancar, la API **crea la base de datos automáticamente** (`WebApi/TaskMaker.db`) y aplica las migraciones. No hay que ejecutar comandos de base de datos.

| Recurso | URL |
|---|---|
| Swagger | https://localhost:7009/swagger |
| API | https://localhost:7009/v1/api/tasks |
| HTTP (redirige a HTTPS) | http://localhost:5183 |

> Desde Rider o Visual Studio, el navegador se abre directamente en Swagger.
> Si el navegador muestra un aviso de certificado, confía en el certificado de desarrollo con `dotnet dev-certs https --trust`.

**Reiniciar la base de datos:** detén la API y borra `WebApi/TaskMaker.db` (y sus archivos `-shm` / `-wal`). Se vuelve a crear vacía al arrancar.

---

## Ejecución con Docker

```bash
git clone https://github.com/CarlosBatre/task.maker.git
cd task.maker

docker compose up --build -d
```

| Recurso | URL |
|---|---|
| Swagger | http://localhost:8080/swagger |
| API | http://localhost:8080/v1/api/tasks |

Comandos útiles:

```bash
docker compose logs -f api     # ver logs
docker compose down            # detener (los datos se conservan)
docker compose down -v         # detener y borrar la base de datos
```

Detalles de la imagen:
- **Multi-stage:** se compila con `dotnet/sdk:10.0` y se ejecuta con `dotnet/aspnet:10.0` (imagen final ~400 MB).
- Corre con un **usuario sin privilegios** (`app`) en el puerto **8080**.
- La base SQLite se guarda en el volumen `taskmaker-data` (`/app/data/TaskMaker.db`), así que **los datos sobreviven** a reinicios del contenedor.
- `compose.yaml` usa `ASPNETCORE_ENVIRONMENT=Development` para habilitar Swagger.

Sin Compose:

```bash
docker build -t taskmaker-api .
docker run --rm -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development -v taskmaker-data:/app/data taskmaker-api
```

---

## Documentación de la API

La documentación interactiva está disponible en **Swagger** (`/swagger`). Desde ahí se pueden probar todos los endpoints.

### Endpoints

Base URL: `/v1/api/tasks`

| Método | Ruta | Descripción | Éxito | Errores |
|---|---|---|---|---|
| `GET` | `/v1/api/tasks` | Lista las tareas (paginado) | `200` | `400` |
| `GET` | `/v1/api/tasks/{id}` | Obtiene una tarea por id | `200` | `404` |
| `POST` | `/v1/api/tasks` | Crea una tarea | `201` | `400`, `409` |
| `PUT` | `/v1/api/tasks/{id}` | Actualiza una tarea | `200` | `400`, `404`, `409` |
| `DELETE` | `/v1/api/tasks/{id}` | Elimina una tarea | `204` | `404` |

Cualquier error inesperado responde `500` con el mensaje `"Error interno del servidor"`.

### Formato de respuesta

Todas las respuestas (excepto `204`) usan el mismo formato:

```json
{
  "success": true,
  "message": "Operación realizada exitosamente",
  "statusCode": 200,
  "data": { },
  "errors": []
}
```

### Modelo de tarea

| Campo | Tipo | Descripción |
|---|---|---|
| `id` | `int` | Identificador (lo genera la base) |
| `title` | `string` | **Obligatorio**, no vacío, máximo 200 caracteres, único |
| `description` | `string` \| `null` | Opcional, máximo 1000 caracteres |
| `isCompleted` | `bool` | Por defecto `false` |
| `createdAt` | `datetime` (UTC) | Fecha de creación, asignada automáticamente |
| `updatedAt` | `datetime` (UTC) | Fecha de última modificación, asignada automáticamente |

### `GET /v1/api/tasks` — Listar tareas

Parámetros de consulta (opcionales):

| Parámetro | Por defecto | Rango |
|---|---|---|
| `pageNumber` | `1` | `>= 1` |
| `pageSize` | `10` | `1 – 100` |

Las tareas se ordenan de la más reciente a la más antigua.

```http
GET /v1/api/tasks?pageNumber=1&pageSize=2
```

```json
{
  "success": true,
  "message": "Operación realizada exitosamente",
  "statusCode": 200,
  "data": {
    "items": [
      {
        "id": 2,
        "title": "Estudiar C#",
        "description": null,
        "isCompleted": false,
        "createdAt": "2026-11-03T06:07:26.6885655Z",
        "updatedAt": "2026-11-03T06:07:26.6885663Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 2,
    "totalCount": 5,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "errors": []
}
```

### `GET /v1/api/tasks/{id}` — Obtener una tarea

```http
GET /v1/api/tasks/1
```

`200` con la tarea en `data`, o `404`:

```json
{
  "success": false,
  "message": "No se encontró la tarea con Id 1",
  "statusCode": 404,
  "data": null,
  "errors": []
}
```

### `POST /v1/api/tasks` — Crear una tarea

```http
POST /v1/api/tasks
Content-Type: application/json

{
  "title": "Comprar pan",
  "description": "En la panadería de la esquina",
  "isCompleted": false
}
```

Responde `201 Created`, con el header `Location` apuntando a la tarea creada:

```json
{
  "success": true,
  "message": "Tarea creada exitosamente",
  "statusCode": 201,
  "data": {
    "id": 1,
    "title": "Comprar pan",
    "description": "En la panadería de la esquina",
    "isCompleted": false,
    "createdAt": "2026-11-03T14:33:10.5525033Z",
    "updatedAt": "2026-11-03T14:33:10.5525040Z"
  },
  "errors": []
}
```

### `PUT /v1/api/tasks/{id}` — Actualizar una tarea

```http
PUT /v1/api/tasks/1
Content-Type: application/json

{
  "title": "Comprar pan integral",
  "description": null,
  "isCompleted": true
}
```

Responde `200` con la tarea actualizada. `createdAt` se conserva y `updatedAt` se actualiza.

### `DELETE /v1/api/tasks/{id}` — Eliminar una tarea

```http
DELETE /v1/api/tasks/1
```

Responde `204 No Content` sin cuerpo, o `404` si no existe.

### Validaciones y errores

| Caso | Código | Mensaje |
|---|---|---|
| `title` vacío, solo espacios o ausente | `400` | `El título es obligatorio.` |
| `title` con más de 200 caracteres | `400` | `El título no puede superar los 200 caracteres.` |
| `description` con más de 1000 caracteres | `400` | `La descripción no puede superar los 1000 caracteres.` |
| JSON mal formado o tipo de dato incorrecto | `400` | `El JSON enviado no tiene un formato válido` |
| `pageNumber` < 1 | `400` | `El número de página debe ser mayor o igual a 1.` |
| `pageSize` fuera de 1–100 | `400` | `El tamaño de página debe estar entre 1 y 100.` |
| Id inexistente o tarea eliminada | `404` | `No se encontró la tarea con Id {id}` |
| Título duplicado (ignora mayúsculas, tildes y espacios extra) | `409` | `Ya existe una tarea con el título '{title}'.` |

Ejemplo de `400`:

```json
{
  "success": false,
  "message": "La solicitud contiene datos inválidos",
  "statusCode": 400,
  "data": null,
  "errors": [ "El título es obligatorio." ]
}
```

---

## Pruebas automatizadas

Pruebas de **integración** con xUnit y `WebApplicationFactory`: levantan la API completa en memoria con una base SQLite temporal (que se borra al terminar) y llaman a los endpoints reales.

```bash
dotnet test
```

| Prueba | Verifica |
|---|---|
| `Create_WithValidData_Returns201AndTask` | Creación exitosa: `201`, header `Location` y datos correctos |
| `Create_WithEmptyTitle_Returns400WithValidationError` | Error de validación con título vacío y solo espacios: `400` |
| `GetById_WhenTaskDoesNotExist_Returns404` | Recurso inexistente: `404` con mensaje |
| `Create_WithDuplicateTitleIgnoringCase_Returns409` | Título duplicado en mayúsculas: `409` |
| `Delete_WhenTaskDoesNotExist_Returns404` | Eliminar inexistente: `404` |

---

## Integración continua (CI)

El workflow [`.github/workflows/dotnet-desktop.yml`](.github/workflows/dotnet-desktop.yml) se ejecuta en cada **push** a `main` y `feature/**`, y en cada **pull request** a `main`:

1. **build (Debug / Release)** en `windows-latest`: instala el SDK definido en `global.json`, restaura dependencias, compila y ejecuta las pruebas. Publica los resultados como artefactos (`test-results-Debug`, `test-results-Release`).
2. **docker-build** en `ubuntu-latest`: construye la imagen de Docker para validar que el `Dockerfile` sigue funcionando.

---

## Estructura del proyecto

```
task.maker/
├── .github/workflows/dotnet-desktop.yml   # Pipeline de CI
├── Application/
│   ├── DTOs/                              # TaskItem DTOs, paginación
│   ├── Exceptions/                        # DuplicateTaskTitleException
│   ├── Interfaces/                        # ITaskItemService
│   ├── Mappings/                          # MapsterConfig
│   └── Services/                          # TaskItemService
├── Infrastructure/
│   ├── Database/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Converters/                    # UtcDateTimeConverter
│   │   └── Migrations/
│   ├── Entities/                          # BaseEntity, TaskItem
│   ├── Interfaces/                        # IRepository, ITaskItemRepository
│   └── Repositories/                      # Repository, TaskItemRepository
├── WebApi/
│   ├── Controllers/v1/                    # TasksController
│   ├── Models/Responses/                  # BaseResponse
│   └── Program.cs
├── WebApi.Tests/                          # Pruebas de integración
├── Dockerfile
├── compose.yaml
├── global.json
└── task.maker.web.api.sln
```
