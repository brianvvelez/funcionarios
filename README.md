# Gestión de Funcionarios — Aplicación CRUD

Aplicación de escritorio en **C# .NET 8 / Windows Forms** que implementa un CRUD sobre la tabla `funcionarios`, aplicando el **patrón DAO** y **manejo de excepciones** según lo solicitado en la actividad.

## 1. Tecnologías

| Componente              | Tecnología                                |
|-------------------------|-------------------------------------------|
| Lenguaje                | C# 12 / .NET 8                             |
| UI                      | Windows Forms                              |
| Motor de base de datos  | SQL Server                                 |
| Acceso a datos          | ADO.NET (`Microsoft.Data.SqlClient`)       |
| Patrón                  | DAO (Data Access Object)                   |

## 2. Estructura del proyecto

```
GestionFuncionarios/
├── GestionFuncionarios.csproj   # Archivo de proyecto .NET 8 WinForms
├── App.config                    # Cadena de conexión
├── Program.cs                    # Punto de entrada
├── script_bd.sql                 # Script DDL + DML (creación + poblado)
├── Models/
│   ├── Funcionario.cs            # Entidad principal
│   ├── TipoDocumento.cs
│   ├── Cargo.cs
│   └── Dependencia.cs
├── Exceptions/
│   └── DAOException.cs           # Excepción personalizada
├── Util/
│   └── ConexionDB.cs             # Gestor de conexión a SQL Server
├── DAO/
│   ├── IFuncionarioDAO.cs        # Contrato del DAO
│   ├── FuncionarioDAO.cs         # Implementación CRUD
│   └── LookupDAO.cs              # Sólo lectura para combos
└── Forms/
    ├── FrmPrincipal.cs           # Listado + botones CRUD
    └── FrmFuncionarioEditor.cs   # Crear / editar
```

## 3. Modelo relacional

Entidades:

- **tipos_documento** (id_tipo_documento PK, codigo, descripcion)
- **dependencias** (id_dependencia PK, nombre, descripcion, activo)
- **cargos** (id_cargo PK, nombre, descripcion, salario_base, activo)
- **funcionarios** (id_funcionario PK, id_tipo_documento FK, numero_documento UK, nombres, apellidos, email UK, telefono, direccion, fecha_nacimiento, fecha_ingreso, id_cargo FK, id_dependencia FK, salario, activo)

Relaciones:

- `funcionarios.id_tipo_documento` → `tipos_documento.id_tipo_documento`
- `funcionarios.id_cargo` → `cargos.id_cargo`
- `funcionarios.id_dependencia` → `dependencias.id_dependencia`

## 4. Instalación

### 4.1 Base de datos
1. Abre **SQL Server Management Studio** (o Azure Data Studio).
2. Conéctate a tu instancia de SQL Server.
3. Abre el archivo `script_bd.sql` y ejecútalo completo. Esto crea la base `GestionFuncionarios`, sus tablas y los datos de prueba.

### 4.2 Configuración de la cadena de conexión
Edita `App.config` y ajusta `Server` según tu entorno:

```xml
<add name="GestionFuncionariosDB"
     connectionString="Server=localhost;Database=GestionFuncionarios;Integrated Security=True;TrustServerCertificate=True;"
     providerName="Microsoft.Data.SqlClient" />
```

Casos comunes:
- `Server=localhost\SQLEXPRESS` para SQL Server Express.
- `User Id=sa;Password=TU_CLAVE;` si usas autenticación SQL en lugar de Windows.

### 4.3 Compilar y ejecutar
Desde la carpeta del proyecto:

```bash
dotnet restore
dotnet run
```

O abre `GestionFuncionarios.csproj` con Visual Studio 2022 y presiona F5.

## 5. Funcionalidades

- **Listar** funcionarios con tipo de documento, cargo y dependencia (JOIN).
- **Crear** un nuevo funcionario con validaciones (campos obligatorios, formato de correo, fechas y edad mínima).
- **Editar** un funcionario existente.
- **Eliminar** con confirmación.

## 6. Evidencia del patrón DAO

| Capa            | Archivo                                           |
|-----------------|---------------------------------------------------|
| Contrato        | `DAO/IFuncionarioDAO.cs`                           |
| Implementación  | `DAO/FuncionarioDAO.cs`                            |
| Modelo          | `Models/Funcionario.cs`                            |
| Cliente del DAO | `Forms/FrmPrincipal.cs`, `Forms/FrmFuncionarioEditor.cs` |

Los formularios consumen el DAO **a través de la interfaz** `IFuncionarioDAO`, sin conocer detalles de SQL Server. Esto permite reemplazar la implementación (por otra base de datos o un mock para pruebas) sin tocar la UI.

## 7. Evidencia del manejo de excepciones

- Excepción personalizada: `Exceptions/DAOException.cs`.
- En `FuncionarioDAO` cada operación captura `SqlException` y la traduce a `DAOException` con un mensaje amigable para el usuario.
- Casos diferenciados por código de error de SQL Server:
  - `2627` / `2601` → unique violation (documento o email duplicado).
  - `547` → violación de FK (tipo doc/cargo/dependencia inexistente).
- En `Program.cs` se prueba la conexión al iniciar y se muestra un mensaje claro si falla.
- Los formularios capturan `DAOException` y muestran `MessageBox` con el mensaje al usuario, sin exponer la traza interna.

## 8. Notas

- El alcance del CRUD es exclusivamente la tabla `funcionarios`, conforme al enunciado.
- Las tablas relacionadas (`tipos_documento`, `cargos`, `dependencias`) se incluyen en el modelo y el script para evidenciar el modelo relacional completo, y se consumen únicamente en modo lectura para poblar los `ComboBox` del formulario editor.
