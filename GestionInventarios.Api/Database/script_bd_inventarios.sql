/* ============================================================
   Proyecto : Gestión de Inventarios (API REST)
   Motor    : SQL Server
   Autor    : Bryan
   Descrip. : Script de creación de la base de datos para la API
              ASP.NET Core con autenticación JWT y manejo de
              roles (Administrador / Docente).

   NOTA SOBRE USUARIOS INICIALES:
   --------------------------------------------------------------
   Este script NO inserta los usuarios de prueba con sus hashes
   BCrypt hardcodeados. En su lugar, el seeder en C# (clase
   DataSeeder, invocado desde Program.cs en el arranque) se
   encarga de insertarlos generando los hashes en caliente a
   partir de las credenciales declaradas en appsettings.json
   (sección "Seed"). De esa forma:
     * No se versionan hashes BCrypt en el repositorio.
     * Las credenciales por defecto pueden modificarse sólo
       cambiando el archivo de configuración.
     * El seeder es idempotente: si la tabla 'usuarios' ya tiene
       registros, no inserta nada.
   --------------------------------------------------------------
   ============================================================ */

-- 1. Creación de la base de datos
IF DB_ID('GestionInventarios') IS NOT NULL
BEGIN
    ALTER DATABASE GestionInventarios SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GestionInventarios;
END
GO

CREATE DATABASE GestionInventarios;
GO

USE GestionInventarios;
GO

-- ============================================================
-- 2. Tabla: usuarios
--    Almacena las credenciales y rol de acceso a la API.
--    El password_hash guarda únicamente el hash BCrypt; NUNCA
--    se almacena el password en texto plano.
-- ============================================================
CREATE TABLE usuarios (
    id_usuario      INT IDENTITY(1,1) PRIMARY KEY,
    nombres         VARCHAR(100) NOT NULL,
    apellidos       VARCHAR(100) NOT NULL,
    email           VARCHAR(150) NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    rol             VARCHAR(20)  NOT NULL,
    fecha_creacion  DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
    activo          BIT          NOT NULL DEFAULT 1,

    CONSTRAINT UQ_usuarios_email UNIQUE (email),
    CONSTRAINT CK_usuarios_rol   CHECK (rol IN ('Administrador', 'Docente'))
);
GO

-- ============================================================
-- 3. Tabla: estados_equipos
--    Catálogo de estados que puede tener un equipo del
--    inventario (Activo, En Reparación, Dado de Baja, etc.).
-- ============================================================
CREATE TABLE estados_equipos (
    id_estado    INT IDENTITY(1,1) PRIMARY KEY,
    nombre       VARCHAR(80)  NOT NULL,
    descripcion  VARCHAR(255) NULL,
    activo       BIT          NOT NULL DEFAULT 1,

    CONSTRAINT UQ_estados_equipos_nombre UNIQUE (nombre)
);
GO

-- ============================================================
-- 4. Tabla: marcas
-- ============================================================
CREATE TABLE marcas (
    id_marca  INT IDENTITY(1,1) PRIMARY KEY,
    nombre    VARCHAR(80) NOT NULL,
    activo    BIT         NOT NULL DEFAULT 1,

    CONSTRAINT UQ_marcas_nombre UNIQUE (nombre)
);
GO

-- ============================================================
-- 5. Tabla: tipos_equipos
--    Catálogo de tipos (Computador, Impresora, Proyector...).
-- ============================================================
CREATE TABLE tipos_equipos (
    id_tipo      INT IDENTITY(1,1) PRIMARY KEY,
    nombre       VARCHAR(80)  NOT NULL,
    descripcion  VARCHAR(255) NULL,
    activo       BIT          NOT NULL DEFAULT 1,

    CONSTRAINT UQ_tipos_equipos_nombre UNIQUE (nombre)
);
GO

-- ============================================================
-- 6. Tabla: inventarios
--    Entidad principal del módulo. Cada equipo registrado
--    tiene un serial único y referencia a marca, tipo y
--    estado a través de claves foráneas.
-- ============================================================
CREATE TABLE inventarios (
    id_inventario  INT IDENTITY(1,1) PRIMARY KEY,
    serial         VARCHAR(80)  NOT NULL,
    id_marca       INT          NOT NULL,
    id_tipo        INT          NOT NULL,
    id_estado      INT          NOT NULL,
    descripcion    VARCHAR(255) NULL,
    fecha_ingreso  DATE         NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    ubicacion      VARCHAR(150) NULL,
    activo         BIT          NOT NULL DEFAULT 1,

    CONSTRAINT UQ_inventarios_serial UNIQUE (serial),
    CONSTRAINT FK_inventarios_marca
        FOREIGN KEY (id_marca)  REFERENCES marcas(id_marca),
    CONSTRAINT FK_inventarios_tipo
        FOREIGN KEY (id_tipo)   REFERENCES tipos_equipos(id_tipo),
    CONSTRAINT FK_inventarios_estado
        FOREIGN KEY (id_estado) REFERENCES estados_equipos(id_estado)
);
GO

-- ============================================================
-- POBLADO INICIAL DE CATÁLOGOS Y EJEMPLOS
-- ============================================================

-- Estados de equipos
INSERT INTO estados_equipos (nombre, descripcion) VALUES
('Activo',         'Equipo operativo y en uso.'),
('En Reparación',  'Equipo retirado temporalmente por mantenimiento.'),
('Dado de Baja',   'Equipo fuera de servicio definitivamente.');
GO

-- Marcas
INSERT INTO marcas (nombre) VALUES
('Dell'),
('HP'),
('Lenovo');
GO

-- Tipos de equipos
INSERT INTO tipos_equipos (nombre, descripcion) VALUES
('Computador', 'Equipo de cómputo de escritorio o portátil.'),
('Impresora',  'Dispositivo de impresión.'),
('Proyector',  'Dispositivo de proyección de video.');
GO

-- Inventarios de ejemplo
INSERT INTO inventarios (serial, id_marca, id_tipo, id_estado, descripcion, fecha_ingreso, ubicacion)
VALUES
('DELL-PC-0001', 1, 1, 1, 'Computador de escritorio - Oficina TI',     '2024-01-15', 'Sede Norte - Piso 2'),
('HP-LJ-0002',   2, 2, 1, 'Impresora láser monocromática',              '2024-02-10', 'Sede Norte - Recepción'),
('LEN-LT-0003',  3, 1, 1, 'Portátil Lenovo ThinkPad',                   '2024-03-22', 'Sede Sur - Sala de Juntas'),
('DELL-PJ-0004', 1, 3, 2, 'Proyector Dell - en reparación',             '2024-04-05', 'Bodega Soporte'),
('HP-PC-0005',   2, 1, 3, 'PC HP dado de baja por obsolescencia',       '2023-11-30', 'Bodega Bajas');
GO

-- ============================================================
-- Verificación rápida
-- ============================================================
SELECT i.id_inventario, i.serial, m.nombre AS marca, t.nombre AS tipo,
       e.nombre AS estado, i.descripcion, i.ubicacion, i.fecha_ingreso
FROM inventarios     i
JOIN marcas          m ON i.id_marca  = m.id_marca
JOIN tipos_equipos   t ON i.id_tipo   = t.id_tipo
JOIN estados_equipos e ON i.id_estado = e.id_estado
ORDER BY i.id_inventario;
GO
