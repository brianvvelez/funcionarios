# Gestión de Inventarios — Web API (ASP.NET Core 8)

Web API REST en **C# / .NET 8** que extiende el proyecto previo
*GestionFuncionarios* (WinForms) con **autenticación JWT**,
**autorización por rol** y un módulo CRUD completo para la
gestión de inventarios de equipos.

Mantiene el mismo estilo de la entrega anterior: acceso a datos
con **ADO.NET (`Microsoft.Data.SqlClient`)** y **patrón DAO**
(NO se usa Entity Framework), para evidenciar el patrón
solicitado en la rúbrica.

---

## 1. Stack tecnológico

| Componente              | Tecnología                                          |
|-------------------------|-----------------------------------------------------|
| Lenguaje / runtime      | C# 12 / .NET 8                                      |
| Framework               | ASP.NET Core 8 Web API                              |
| Persistencia            | SQL Server (ADO.NET, sin ORM)                       |
| Patrón de datos         | DAO (Data Access Object)                            |
| Autenticación           | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Hashing de contraseñas  | BCrypt (`BCrypt.Net-Next`)                          |
| Documentación           | Swagger / OpenAPI (`Swashbuckle.AspNetCore`)        |
| Manejo de excepciones   | Middleware → ProblemDetails (RFC 7807)              |

---

## 2. Estructura del proyecto

```
GestionInventarios.Api/
├── Controllers/
│   ├── AuthController.cs              # POST /api/auth/login
│   ├── UsuariosController.cs          # CRUD usuarios (sólo Admin)
│   ├── InventariosController.cs       # GET para autenticados, resto sólo Admin
│   ├── EstadosEquiposController.cs    # CRUD (sólo Admin)
│   ├── MarcasController.cs            # CRUD (sólo Admin)
│   └── TiposEquiposController.cs      # CRUD (sólo Admin)
├── Models/
│   ├── Entities/   # Usuario, Inventario, EstadoEquipo, Marca, TipoEquipo
│   └── DTOs/       # LoginRequest/Response, *CreateDto, *ResponseDto, ...
├── DAO/
│   ├── Interfaces/        # IUsuarioDAO, IInventarioDAO, IEstadoEquipoDAO, IMarcaDAO, ITipoEquipoDAO
│   └── Implementations/   # *DAO (ADO.NET puro)
├── Services/
│   ├── IAuthService.cs / AuthService.cs       # Login + generación JWT
│   ├── IPasswordHasher.cs / PasswordHasher.cs # Wrapper BCrypt
│   └── DataSeeder.cs                          # Seed idempotente de usuarios
├── Exceptions/
│   ├── DAOException.cs
│   ├── AuthenticationException.cs
│   └── NotFoundException.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs         # Captura global → ProblemDetails
├── Util/
│   └── ConexionDB.cs                          # Provee SqlConnection
├── Database/
│   └── script_bd_inventarios.sql              # DDL + datos de catálogos
├── Properties/launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── GestionInventarios.Api.csproj
```

---

## 3. Modelo relacional

- **usuarios** (id_usuario PK, nombres, apellidos, email UK, password_hash, rol [`Administrador`|`Docente`], fecha_creacion, activo)
- **estados_equipos** (id_estado PK, nombre UK, descripcion, activo)
- **marcas** (id_marca PK, nombre UK, activo)
- **tipos_equipos** (id_tipo PK, nombre UK, descripcion, activo)
- **inventarios** (id_inventario PK, serial UK, id_marca FK, id_tipo FK, id_estado FK, descripcion, fecha_ingreso, ubicacion, activo)

---

## 4. Cómo ejecutar el proyecto

### 4.1 Base de datos

1. Abre **SQL Server Management Studio** (o Azure Data Studio).
2. Conéctate a tu instancia de SQL Server.
3. Abre el archivo `Database/script_bd_inventarios.sql` y ejecútalo
   completo. Esto crea la base `GestionInventarios`, las tablas con
   FKs y constraints, y los datos iniciales de catálogos (3 estados,
   3 marcas, 3 tipos y 5 inventarios de ejemplo).

> Los **usuarios iniciales** NO están en el script SQL: se crean
> automáticamente al primer arranque mediante el *seeder* en C#
> (`Services/DataSeeder.cs`), que hashea las contraseñas con BCrypt
> antes de insertarlas. El seeder es idempotente: si la tabla
> `usuarios` ya tiene registros, no inserta nada.

### 4.2 Configuración

Edita `appsettings.json` y ajusta lo siguiente:

```jsonc
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=GestionInventarios;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "CAMBIAR-ESTA-CLAVE-POR-UNA-DE-AL-MENOS-32-CARACTERES-EN-PRODUCCION",
    "Issuer": "GestionInventarios.Api",
    "Audience": "GestionInventarios.Clients",
    "ExpirationMinutes": 60
  },
  "Seed": {
    "AdminEmail": "admin@empresa.com",
    "AdminPassword": "Admin123*",
    "DocenteEmail": "docente@empresa.com",
    "DocentePassword": "Docente123*"
  }
}
```

> **⚠️ Importante:** la clave `Jwt:Key` del repositorio es un
> *placeholder*. Debes reemplazarla por un secreto fuerte de al
> menos **32 caracteres** (256 bits) antes de desplegar a un
> entorno distinto de tu máquina local. La aplicación falla al
> arrancar si la clave es más corta, como medida preventiva.

### 4.3 Compilar y ejecutar

Desde la carpeta `GestionInventarios.Api/`:

```bash
dotnet restore
dotnet build
dotnet run
```

Por defecto la API queda escuchando en
`http://localhost:5080` y `https://localhost:7080`, y al levantar
en modo Development abre Swagger UI en
`http://localhost:5080/swagger`.

---

## 5. Credenciales de prueba

Se crean en el primer arranque:

| Rol            | Email                  | Contraseña    |
|----------------|------------------------|---------------|
| Administrador  | `admin@empresa.com`    | `Admin123*`   |
| Docente        | `docente@empresa.com`  | `Docente123*` |

---

## 6. Matriz de permisos

| Recurso / Operación                         | Administrador | Docente | Anónimo |
|---------------------------------------------|:-------------:|:-------:|:-------:|
| `POST /api/auth/login`                      | ✔             | ✔       | ✔       |
| `GET /api/usuarios`                         | ✔             | 403     | 401     |
| `GET /api/usuarios/{id}`                    | ✔             | 403     | 401     |
| `POST /api/usuarios`                        | ✔             | 403     | 401     |
| `PUT /api/usuarios/{id}`                    | ✔             | 403     | 401     |
| `DELETE /api/usuarios/{id}`                 | ✔             | 403     | 401     |
| `GET /api/inventarios`                      | ✔             | **✔**   | 401     |
| `GET /api/inventarios/{id}`                 | ✔             | **✔**   | 401     |
| `POST /api/inventarios`                     | ✔             | 403     | 401     |
| `PUT /api/inventarios/{id}`                 | ✔             | 403     | 401     |
| `DELETE /api/inventarios/{id}`              | ✔             | 403     | 401     |
| `GET/POST/PUT/DELETE /api/estados-equipos`  | ✔             | 403     | 401     |
| `GET/POST/PUT/DELETE /api/marcas`           | ✔             | 403     | 401     |
| `GET/POST/PUT/DELETE /api/tipos-equipos`    | ✔             | 403     | 401     |

> `401 Unauthorized` = falta el token o es inválido. `403 Forbidden`
> = el token es válido pero el rol del usuario no tiene permiso.

---

## 7. Ejemplos `curl`

> Cambia `http://localhost:5080` por la URL/puerto que muestre la consola al hacer `dotnet run`.

### 7.1 Login (Administrador)

```bash
curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@empresa.com","password":"Admin123*"}'
```

Respuesta:

```json
{
  "token": "eyJhbGciOi...",
  "email": "admin@empresa.com",
  "rol": "Administrador",
  "expiraEn": "2026-05-24T18:30:00Z"
}
```

Exporta el token a una variable para usarlo en los siguientes ejemplos:

```bash
TOKEN="eyJhbGciOi..."
```

### 7.2 Crear un usuario (con token de Administrador)

```bash
curl -X POST http://localhost:5080/api/usuarios \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
        "nombres":   "María",
        "apellidos": "Gómez",
        "email":     "maria.gomez@empresa.com",
        "password":  "Maria123*",
        "rol":       "Docente"
      }'
```

### 7.3 Login como Docente y listar inventarios

```bash
TOKEN_DOC=$(curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"docente@empresa.com","password":"Docente123*"}' \
  | jq -r .token)

curl http://localhost:5080/api/inventarios \
  -H "Authorization: Bearer $TOKEN_DOC"
```

→ Devuelve `200 OK` con la lista de inventarios.

### 7.4 Intento fallido: Docente creando una marca

```bash
curl -X POST http://localhost:5080/api/marcas \
  -H "Authorization: Bearer $TOKEN_DOC" \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Acer","activo":true}'
```

→ Devuelve **`403 Forbidden`**, demostrando que el rol Docente NO
puede crear marcas (ni estados, ni tipos, ni usuarios, ni operar
inventarios distintos a leerlos).

### 7.5 Intento fallido: credenciales inválidas

```bash
curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@empresa.com","password":"incorrecta"}'
```

→ Devuelve **`401 Unauthorized`** con `ProblemDetails`:

```json
{
  "status": 401,
  "title": "Credenciales inválidas.",
  "detail": "Credenciales inválidas.",
  "type": "https://httpstatuses.io/401"
}
```

El mensaje es genérico para no facilitar la enumeración de
cuentas válidas (mismo texto si el email no existe o si la
contraseña es incorrecta).

---

## 8. Probar desde Swagger

1. Ejecuta `dotnet run`.
2. Abre `http://localhost:5080/swagger`.
3. Llama a `POST /api/auth/login` y copia el `token` de la respuesta.
4. Pulsa el botón **Authorize** (arriba a la derecha) e introduce
   `Bearer {token}` o sólo el token: Swagger lo enviará en cada
   petición.
5. Prueba los endpoints protegidos.

---

## 9. Cumplimiento de la rúbrica

| Criterio                                                              | Cómo se cumple                                                                                                                  |
|-----------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| Web API ASP.NET Core 8                                                | `GestionInventarios.Api.csproj` (`Microsoft.NET.Sdk.Web`, `TargetFramework=net8.0`).                                            |
| Persistencia con SQL Server + ADO.NET (sin EF)                        | `Util/ConexionDB.cs` y todos los `DAO/Implementations/*DAO.cs` usan `SqlConnection`, `SqlCommand`, `SqlDataReader`.             |
| Patrón DAO                                                            | Interfaces en `DAO/Interfaces/` consumidas por los controllers vía DI; implementaciones en `DAO/Implementations/`.              |
| Autenticación JWT                                                     | `Services/AuthService.cs` genera el token; `Program.cs` registra `AddJwtBearer` como esquema por defecto.                       |
| Contraseñas almacenadas con hash (NO texto plano)                     | `Services/PasswordHasher.cs` (BCrypt, work factor 12). El campo `password_hash` es `VARCHAR(255)` y nunca se devuelve en HTTP.  |
| Autorización por rol                                                  | `[Authorize(Roles = "Administrador")]` en todos los CRUD de catálogos + usuarios. Inventarios usa atributos por verbo HTTP.     |
| Docente sólo puede listar inventarios                                 | `InventariosController` aplica `Authorize(Roles="Administrador,Docente")` a GET y `Authorize(Roles="Administrador")` al resto.  |
| CRUD de usuarios sólo para Administrador                              | `UsuariosController` con `[Authorize(Roles = "Administrador")]` a nivel de clase.                                               |
| Módulos: estados de equipos, marcas, tipos de equipos, inventarios    | Cuatro tablas + cuatro controllers + cuatro DAOs.                                                                               |
| Manejo de excepciones personalizado                                   | `Exceptions/` + `Middleware/ExceptionHandlingMiddleware.cs` → respuesta uniforme en `ProblemDetails`.                           |
| Mapeo de errores SQL (UK, FK)                                         | Cada DAO captura `SqlException` con códigos `2627`, `2601`, `547` y los traduce a mensajes amigables (igual que en `Funcionario`).|
| Inyección de dependencias                                             | `AddScoped` para DAOs y `AuthService`; `AddSingleton` para `ConexionDB` y `PasswordHasher`.                                     |
| Swagger con Bearer                                                    | `Program.cs` define el esquema `Bearer` y lo agrega como `SecurityRequirement` para que la UI pida el token.                    |
| DTOs separados de entidades                                           | Carpetas `Models/Entities/` vs `Models/DTOs/`. `password_hash` jamás aparece en respuestas.                                     |
| Async / await en controllers                                          | El login es `async Task<IActionResult>`; los CRUD usan `ActionResult<>` (sincrónicos a propósito, los DAO no son `async`).      |
| Datos de prueba                                                       | Script SQL + seeder C# crean 2 usuarios, 3 estados, 3 marcas, 3 tipos y 5 inventarios.                                          |

---

## 10. Notas de seguridad

- La clave JWT del `appsettings.json` versionado es un placeholder.
  Para un entorno real, usar *user-secrets* o variables de entorno
  (`Jwt__Key`).
- Las contraseñas se guardan **siempre** como hash BCrypt; la
  capa de respuesta nunca devuelve `password_hash`.
- El mensaje de error de login es el mismo (`"Credenciales
  inválidas"`) para los tres casos (usuario inexistente, contraseña
  errada o cuenta inactiva), evitando la enumeración de cuentas.
- Toda consulta SQL usa parámetros (`@param`), nunca concatenación
  de strings, lo que previene SQL Injection.
