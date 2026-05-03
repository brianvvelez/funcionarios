/* ============================================================
   Proyecto: Gestión de Funcionarios
   Motor   : SQL Server
   Autor   : Bryan
   Descrip.: Script de creación de la base de datos y poblado
             inicial. Incluye el modelo relacional completo
             aunque la aplicación solo realiza CRUD sobre la
             tabla funcionarios.
   ============================================================ */

-- 1. Creación de la base de datos
IF DB_ID('GestionFuncionarios') IS NOT NULL
BEGIN
    ALTER DATABASE GestionFuncionarios SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GestionFuncionarios;
END
GO

CREATE DATABASE GestionFuncionarios;
GO

USE GestionFuncionarios;
GO

-- 2. Tabla: tipos_documento
CREATE TABLE tipos_documento (
    id_tipo_documento INT IDENTITY(1,1) PRIMARY KEY,
    codigo            VARCHAR(5)   NOT NULL UNIQUE,
    descripcion       VARCHAR(50)  NOT NULL
);
GO

-- 3. Tabla: dependencias (áreas o departamentos)
CREATE TABLE dependencias (
    id_dependencia INT IDENTITY(1,1) PRIMARY KEY,
    nombre         VARCHAR(100) NOT NULL UNIQUE,
    descripcion    VARCHAR(255) NULL,
    activo         BIT          NOT NULL DEFAULT 1
);
GO

-- 4. Tabla: cargos
CREATE TABLE cargos (
    id_cargo     INT IDENTITY(1,1) PRIMARY KEY,
    nombre       VARCHAR(100)  NOT NULL UNIQUE,
    descripcion  VARCHAR(255)  NULL,
    salario_base DECIMAL(12,2) NOT NULL,
    activo       BIT           NOT NULL DEFAULT 1
);
GO

-- 5. Tabla: funcionarios (entidad principal del CRUD)
CREATE TABLE funcionarios (
    id_funcionario    INT IDENTITY(1,1) PRIMARY KEY,
    id_tipo_documento INT           NOT NULL,
    numero_documento  VARCHAR(20)   NOT NULL UNIQUE,
    nombres           VARCHAR(100)  NOT NULL,
    apellidos         VARCHAR(100)  NOT NULL,
    email             VARCHAR(100)  NOT NULL UNIQUE,
    telefono          VARCHAR(20)   NULL,
    direccion         VARCHAR(200)  NULL,
    fecha_nacimiento  DATE          NOT NULL,
    fecha_ingreso     DATE          NOT NULL,
    id_cargo          INT           NOT NULL,
    id_dependencia    INT           NOT NULL,
    salario           DECIMAL(12,2) NOT NULL,
    activo            BIT           NOT NULL DEFAULT 1,

    CONSTRAINT FK_funcionarios_tipo_documento
        FOREIGN KEY (id_tipo_documento) REFERENCES tipos_documento(id_tipo_documento),
    CONSTRAINT FK_funcionarios_cargo
        FOREIGN KEY (id_cargo) REFERENCES cargos(id_cargo),
    CONSTRAINT FK_funcionarios_dependencia
        FOREIGN KEY (id_dependencia) REFERENCES dependencias(id_dependencia),

    CONSTRAINT CK_funcionarios_salario CHECK (salario >= 0)
);
GO

-- ============================================================
-- POBLADO INICIAL DE DATOS
-- ============================================================

-- Tipos de documento
INSERT INTO tipos_documento (codigo, descripcion) VALUES
('CC', 'Cédula de Ciudadanía'),
('CE', 'Cédula de Extranjería'),
('TI', 'Tarjeta de Identidad'),
('PA', 'Pasaporte');
GO

-- Dependencias
INSERT INTO dependencias (nombre, descripcion) VALUES
('Recursos Humanos', 'Gestión del talento humano'),
('Tecnología',       'Sistemas de información y soporte técnico'),
('Financiera',       'Contabilidad, presupuesto y tesorería'),
('Operaciones',      'Operaciones y producción'),
('Comercial',        'Ventas y atención al cliente');
GO

-- Cargos
INSERT INTO cargos (nombre, descripcion, salario_base) VALUES
('Director',     'Dirección de área',           8000000),
('Coordinador',  'Coordinación de procesos',    5500000),
('Analista',     'Análisis y soporte',          3500000),
('Asistente',    'Asistencia administrativa',   2200000),
('Auxiliar',     'Apoyo operativo',             1800000);
GO

-- Funcionarios iniciales
INSERT INTO funcionarios
    (id_tipo_documento, numero_documento, nombres, apellidos, email, telefono,
     direccion, fecha_nacimiento, fecha_ingreso, id_cargo, id_dependencia, salario)
VALUES
    (1, '1020304050', 'Carlos Andrés',  'Gómez Ramírez',    'cgomez@empresa.com',    '3001234567', 'Cra 50 # 25-30',  '1985-03-15', '2020-05-10', 1, 2, 8500000),
    (1, '1098765432', 'Laura Patricia', 'Mejía Torres',     'lmejia@empresa.com',    '3009876543', 'Cl 80 # 45-12',   '1990-07-22', '2021-02-15', 3, 1, 3700000),
    (1, '1112233445', 'Juan David',     'Rodríguez López',  'jrodriguez@empresa.com','3015556677', 'Cra 70 # 30-50',  '1992-11-08', '2022-01-20', 4, 3, 2300000),
    (1, '1003344556', 'Ana María',      'Castaño Vélez',    'acastano@empresa.com',  '3024445566', 'Cl 10 # 12-08',   '1988-06-30', '2019-09-01', 2, 5, 5700000),
    (2, 'E1234567',   'Mateo',          'Ferrari Bianchi',  'mferrari@empresa.com',  '3033334444', 'Cra 65 # 18-22',  '1991-12-05', '2023-03-10', 3, 4, 3600000);
GO

-- Verificación rápida
SELECT f.id_funcionario, td.descripcion AS tipo_doc, f.numero_documento,
       f.nombres + ' ' + f.apellidos AS nombre_completo,
       c.nombre AS cargo, d.nombre AS dependencia, f.salario
FROM funcionarios f
INNER JOIN tipos_documento td ON f.id_tipo_documento = td.id_tipo_documento
INNER JOIN cargos          c  ON f.id_cargo          = c.id_cargo
INNER JOIN dependencias    d  ON f.id_dependencia    = d.id_dependencia;
GO
