# 📦 Manage Orders API

📦 Manage Orders API es una tarea técnica desarrollada para Voultech. Consiste en una API REST para gestión de órdenes y productos, construida con .NET 10 siguiendo arquitectura limpia. Incluye autenticación JWT, caché con Redis, observabilidad con Prometheus y Grafana, y pipeline CI con GitHub Actions.

---

## 📋 Tabla de contenidos

- [Tecnologías](#-tecnologías)
- [Arquitectura](#-arquitectura)
- [Estructura del proyecto](#-estructura-del-proyecto)
- [Entidades y modelo de datos](#-entidades-y-modelo-de-datos)
- [Endpoints](#-endpoints)
- [Lógica de negocio](#-lógica-de-negocio)
- [Autenticación y autorización](#-autenticación-y-autorización)
- [Caché](#-caché)
- [Observabilidad](#-observabilidad)
- [Configuración y variables de entorno](#-configuración-y-variables-de-entorno)
- [Cómo levantar el proyecto](#-cómo-levantar-el-proyecto)
- [Tests](#-tests)
- [CI/CD](#-cicd)
- [Usuarios de prueba](#-usuarios-de-prueba)

---

## 🛠 Tecnologías

### Backend
| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 10 | Framework principal |
| ASP.NET Core | 10 | API REST |
| Entity Framework Core | 10 | ORM |
| PostgreSQL | 16 | Base de datos principal |
| Redis | 7 | Caché distribuida |
| prometheus-net | latest | Métricas |

### Infraestructura
| Tecnología | Versión | Uso |
|---|---|---|
| Docker | - | Contenedores |
| Docker Compose | - | Orquestación local |
| Prometheus | 2.53 | Recolección de métricas |
| Grafana | 11.1 | Visualización |

### Testing
| Tecnología | Uso |
|---|---|
| xUnit | Tests unitarios |
| Moq | Mocking |
| EF Core InMemory | Tests de repositorios |
| coverlet | Cobertura de código |

---

## 🏛 Arquitectura

El proyecto sigue **Clean Architecture** dividida en 4 capas con dependencias unidireccionales hacia el dominio:

```
┌─────────────────────────────────────┐
│              API                    │  Controllers, Middleware, Program.cs
├─────────────────────────────────────┤
│           Application               │  Services, DTOs, Interfaces, Cache
├─────────────────────────────────────┤
│          Infrastructure             │  Repositories, DbContext, JWT, Redis
├─────────────────────────────────────┤
│             Domain                  │  Entities, Interfaces
└─────────────────────────────────────┘
```

**Regla de dependencias:** Cada capa solo conoce a la capa inferior.

---

## 📁 Estructura del proyecto

```
ManageOrders/
├── API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── OrdenController.cs
│   │   └── ProductoController.cs
│   ├── Middleware/
│   │   └── ExceptionHandler.cs
│   └── Program.cs
│
├── Application/
│   ├── Cache/
│   │   └── CacheKeys.cs
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│       ├── AuthService.cs
│       ├── OrdenService.cs
│       └── ProductoService.cs
│
├── Domain/
│   ├── Entities/
│   │   ├── Cliente.cs
│   │   ├── Orden.cs
│   │   ├── OrdenProducto.cs
│   │   └── Producto.cs
│   ├── Interfaces/
│   └── Services/
│       └── DescuentoService.cs
│
├── Infrastructure/
│   ├── Auth/
│   │   └── JWTService.cs
│   ├── Cache/
│   │   └── RedisCacheService.cs
│   ├── Migrations/
│   ├── Persistence/
│   │   └── AppDbContext.cs
│   ├── Repositories/
│   │   ├── ClienteRepository.cs
│   │   ├── OrdenRepository.cs
│   │   └── ProductoRepository.cs
│   └── Seeders/
│       └── DbSeeder.cs
│
├── Tests/
│   ├── Controllers/
│   ├── Helpers/
│   └── Services/
│
├── observability/
│   ├── prometheus.yml
│   └── grafana/
│       └── provisioning/
│           └── datasources/
│               └── prometheus.yml
│
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## 🗃 Entidades y modelo de datos

```
Cliente ──────────────── Orden
  id (PK)                  id (PK)
  nombre                   clienteId (FK)
  email (único)            fechaCreacion
  passwordHash             total
  rol
                              │
                         OrdenProducto
                              │
                           Producto
                             id (PK)
                             nombre
                             precio
```

### Roles disponibles

| Rol | Descripción |
|---|---|
| `Admin` | Acceso completo — puede crear, leer, actualizar y eliminar |
| `Cliente` | Puede crear sus propias órdenes y consultar productos |

---

## 🔌 Endpoints

### Auth — `/auth`
| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/auth/login` | ❌ | Login con email y contraseña |
| POST | `/auth/register` | ❌ | Registro de nuevo cliente |

### Órdenes — `/ordenes`
| Método | Ruta | Auth | Rol | Descripción |
|---|---|---|---|---|
| GET | `/ordenes` | ✅ | Cualquiera | Listar órdenes paginadas |
| GET | `/ordenes/{id}` | ✅ | Cualquiera | Obtener orden por ID |
| POST | `/ordenes` | ✅ | Cualquiera | Crear orden (ClienteId desde token) |
| PUT | `/ordenes/{id}` | ✅ | Admin | Actualizar orden |
| DELETE | `/ordenes/{id}` | ✅ | Admin | Eliminar orden |

### Productos — `/productos`
| Método | Ruta | Auth | Rol | Descripción |
|---|---|---|---|---|
| GET | `/productos` | ✅ | Cualquiera | Listar productos paginados |
| GET | `/productos/{id}` | ✅ | Cualquiera | Obtener producto por ID |
| POST | `/productos` | ✅ | Cualquiera | Crear producto |

### Sistema
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/health` | Health check |
| GET | `/metrics` | Métricas Prometheus |

### Paginación

Todos los endpoints de listado aceptan query params:

```
GET /ordenes?page=1&size=10
```

| Parámetro | Default | Máximo | Descripción |
|---|---|---|---|
| `page` | 1 | — | Número de página |
| `size` | 10 | 50 | Elementos por página |

La respuesta incluye metadatos:

```json
{
  "data": [...],
  "actualPage": 1,
  "pageSize": 10,
  "total": 45,
  "totalPages": 5,
  "hasNext": true,
  "hasPrevious": false
}
```

---

## 💡 Lógica de negocio

### Descuentos automáticos en órdenes

Al crear o actualizar una orden, `DescuentoService` aplica descuentos automáticamente:

| Condición | Descuento |
|---|---|
| Total > $500 | 10% |
| Más de 5 productos | 5% |
| Ambas condiciones | 15% |

Los descuentos son acumulativos. Ejemplos:

```
2 productos × $300 = $600  →  -10%  →  Total: $540
6 productos × $100 = $600  →  -15%  →  Total: $510
3 productos × $100 = $300  →   0%   →  Total: $300
```

---

## 🔐 Autenticación y autorización

La API usa **JWT Bearer** con claims embebidos en el token.

### Flujo de autenticación

```
POST /auth/login
  { "email": "...", "password": "..." }
      ↓
  Valida con BCrypt
      ↓
  Genera JWT con claims:
    - NameIdentifier (clienteId)
    - Email
    - Name
    - Role
      ↓
  Retorna { token, nombre, email, rol, expiration }
```

El token tiene validez de **8 horas**.

### Uso en Swagger

1. Hacer login en `POST /auth/login`
2. Copiar el campo `token` de la respuesta
3. Click en el botón **Authorize** 🔒 en Swagger
4. Ingresar: `Bearer <token>`
5. Click **Authorize**

### Seguridad de contraseñas

Las contraseñas nunca se almacenan en texto plano — se hashean con **BCrypt** antes de guardarlas. El registro público siempre crea usuarios con rol `Cliente`; los `Admin` solo se crean vía seeder.

---

## ⚡ Caché

Se usa **Redis** con el patrón **Cache-Aside** para los endpoints de detalle.

### Endpoints cacheados

| Endpoint | TTL | Clave |
|---|---|---|
| `GET /ordenes/{id}` | 10 min | `manage-orders:orden:{id}` |
| `GET /productos/{id}` | 10 min | `manage-orders:producto:{id}` |

### Comportamiento

```
Request GET /ordenes/1
    ↓
¿Existe en Redis?
    ├── SÍ  →  Retorna desde caché (sin tocar la BD)
    └── NO  →  Consulta BD → guarda en Redis → retorna
```

Al actualizar o eliminar un recurso, su clave en Redis se invalida automáticamente.

### Tolerancia a fallos

Si Redis no está disponible, el error se loggea como `Warning` y la petición continúa hacia la base de datos. **La API no se cae si Redis falla.**

---

## 📊 Observabilidad

El stack de observabilidad consiste en:

```
API (.NET)  →  /metrics  →  Prometheus  →  Grafana
```

### Métricas disponibles

- **HTTP:** requests totales, duración, tasa de errores, requests en curso
- **Runtime .NET:** CPU, memoria, GC collections
- **Por endpoint:** desglose por controller, action, método y código HTTP

### Acceso

| Servicio | URL |
|---|---|
| Métricas raw | `http://localhost:8080/metrics` |
| Prometheus UI | `http://localhost:9090` |
| Grafana | `http://localhost:3000` |

### Dashboard

El proyecto incluye un dashboard JSON personalizado en `observability/grafana/manage-orders-dashboard.json`. Para importarlo en Grafana:

1. **Dashboards → New → Import**
2. Subir el archivo JSON
3. Seleccionar datasource **Prometheus**
4. Click **Import**

El datasource de Prometheus se configura automáticamente al levantar el stack gracias al provisioning en `observability/grafana/provisioning/datasources/`.

---

## ⚙️ Configuración y variables de entorno

Crea un archivo `.env` en la raíz del proyecto basándote en `.env.example`:

```bash
cp .env.example .env
```

### Variables requeridas

```env
# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=tu_password_seguro
POSTGRES_DB=order_db
POSTGRES_PORT=5432

# Redis
REDIS_PASSWORD=tu_redis_password

# JWT — mínimo 32 caracteres
JWT_KEY=una_clave_secreta_de_minimo_32_caracteres!!
JWT_ISSUER=manage-orders
JWT_AUDIENCE=manage-orders-client

# Grafana
GRAFANA_USER=admin
GRAFANA_PASSWORD=tu_grafana_password
```

> ⚠️ **Nunca commitees el `.env`** — está en `.gitignore`. Solo commitea `.env.example` con los nombres de las variables sin valores.

### Generar una JWT Key segura

```bash
# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))

# Linux / macOS
openssl rand -base64 32
```

---

## 🚀 Cómo levantar el proyecto

### Requisitos previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y corriendo
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (solo para desarrollo local y migraciones)

### Levantar con Docker Compose

```bash
# 1. Clonar el repositorio
git clone <url-del-repo>
cd ManageOrders

# 2. Crear el archivo de variables de entorno
cp .env.example .env
# Editar .env con tus valores

# 3. Levantar todos los servicios
docker compose up -d --build
```

La API aplica las migraciones y ejecuta el seeder automáticamente al iniciar.

### Verificar que todo está corriendo

```bash
docker compose ps
```

Todos los contenedores deben estar en estado `healthy` o `running`:

```
NAME          STATUS
postgres_db   healthy
redis         healthy
order_api     running
prometheus    running
grafana       running
```

### Acceder a los servicios

| Servicio | URL |
|---|---|
| API + Swagger | `http://localhost:8080/swagger` |
| Health Check | `http://localhost:8080/health` |
| Grafana | `http://localhost:3000` |
| Prometheus | `http://localhost:9090` |

### Detener el proyecto

```bash
# Detener sin borrar datos
docker compose down

# Detener y borrar todos los volúmenes (BD, Redis, métricas)
docker compose down -v
```

### Desarrollo local (sin Docker para la API)

Si quieres correr la API localmente apuntando a los servicios de Docker:

```bash
# Levantar solo la infraestructura
docker compose up -d db redis prometheus grafana

# Correr la API localmente
cd API
dotnet run
```

### Crear una migración nueva

```bash
dotnet ef migrations add NombreDeLaMigracion \
  --project Infrastructure \
  --startup-project API
```

> Después de crear una migración, siempre reconstruye la imagen Docker para que la API la aplique al iniciar:
> ```bash
> docker compose up -d --build api
> ```

---

## 🧪 Tests

El proyecto tiene tests unitarios para las tres capas principales usando **xUnit** y **Moq**.

### Correr los tests

```bash
# Todos los tests
dotnet test ManageOrders.slnx 

# Con output detallado
dotnet test ManageOrders.slnx --verbosity normal

# Con reporte de cobertura
dotnet test ManageOrders.slnx --collect:"XPlat Code Coverage"
```

### Estructura de tests

```
Tests/
├── Helpers/
│   └── DataBuilder.cs        # Fábrica de objetos de prueba
├── Services/
│   ├── AuthServiceTests.cs   # Login, register, hash de passwords
│   └── OrdenServiceTests.cs  # CRUD, descuentos, caché
└── Controllers/
    └── OrdenControllerTests.cs  # Respuestas HTTP, claims JWT
```

### Cobertura

| Módulo | Tests |
|---|---|
| `AuthService` | Login exitoso/fallido, register, hash de password, rol por defecto |
| `OrdenService` | CRUD completo, descuentos, caché hit/miss, invalidación |
| `OrdenController` | Códigos HTTP, extracción de clienteId del token, 404s |

---

## 🔄 CI/CD

El proyecto tiene un pipeline de **Integración Continua** con GitHub Actions que se ejecuta en cada push a `main` y en Pull Requests.

### Pipeline (`.github/workflows/pipeline.yml`)

```
Push / PR a main
      ↓
  Checkout código
      ↓
  Setup .NET 10
      ↓
  dotnet restore
      ↓
  dotnet build --no-restore
      ↓
  dotnet test --no-build
```

Si algún test falla o el código no compila, el pipeline falla y bloquea el merge.

---

## 👥 Usuarios de prueba

El seeder crea estos usuarios automáticamente la primera vez que levanta la aplicación:

| Nombre | Email | Password | Rol |
|---|---|---|---|
| Administrador | `admin@manageorders.com` | `Admin1234!` | Admin |
| Nicolas Caceres | `nico@example.com` | `Nico1234!` | Cliente |
| Andres Parra | `andres@example.com` | `Andres1234!` | Cliente |

El **Admin** puede usar PUT y DELETE en órdenes. Los **Clientes** pueden crear órdenes y consultar productos.

---

## 📦 Productos precargados

La base de datos incluye estos productos como seed de migraciones:

| ID | Nombre | Precio |
|---|---|---|
| 1 | Laptop Pro | $1,500.00 |
| 2 | Mouse Gamer | $25.50 |
| 3 | Monitor OLED 4K | $400.00 |
| 4 | Teclado Mecánico | $150.00 |
| 5 | Silla Ergonómica | $500.00 |