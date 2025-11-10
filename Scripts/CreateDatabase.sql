-- Script de creación de base de datos y tablas
-- Para Segundo Parcial - Lenguajes Visuales 2

-- Crear base de datos
-- La base de datos ya existe en Somee, solo usarla
-- USE ClienteDB; (Somee ya está conectado a la BD)

-- Tabla Cliente
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cliente')
BEGIN
    CREATE TABLE Cliente (
        CI VARCHAR(20) PRIMARY KEY NOT NULL,
        Nombres VARCHAR(200) NOT NULL,
        Direccion VARCHAR(300) NOT NULL,
        Telefono VARCHAR(20) NOT NULL,
        FotoCasa1 VARCHAR(500) NULL,
        FotoCasa2 VARCHAR(500) NULL,
        FotoCasa3 VARCHAR(500) NULL,
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Tabla ArchivoCliente
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArchivoCliente')
BEGIN
    CREATE TABLE ArchivoCliente (
        IdArchivo INT PRIMARY KEY IDENTITY(1,1),
        CICliente VARCHAR(20) NOT NULL,
        NombreArchivo VARCHAR(255) NOT NULL,
        UrlArchivo VARCHAR(500) NOT NULL,
        TipoArchivo VARCHAR(50) NULL,
        TamanoBytes BIGINT NOT NULL DEFAULT 0,
        FechaCarga DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ArchivoCliente_Cliente FOREIGN KEY (CICliente) 
            REFERENCES Cliente(CI) ON DELETE CASCADE
    );
END
GO

-- Tabla LogApi
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LogApi')
BEGIN
    CREATE TABLE LogApi (
        IdLog INT PRIMARY KEY IDENTITY(1,1),
        DateTime DATETIME NOT NULL DEFAULT GETDATE(),
        TipoLog VARCHAR(50) NOT NULL,
        RequestBody NVARCHAR(MAX) NULL,
        ResponseBody NVARCHAR(MAX) NULL,
        UrlEndpoint VARCHAR(500) NULL,
        MetodoHttp VARCHAR(10) NULL,
        DireccionIp VARCHAR(50) NULL,
        Detalle NVARCHAR(MAX) NULL,
        StatusCode INT NULL
    );
END
GO

-- Crear índices para mejorar rendimiento
CREATE NONCLUSTERED INDEX IX_ArchivoCliente_CICliente 
ON ArchivoCliente(CICliente);
GO

CREATE NONCLUSTERED INDEX IX_LogApi_DateTime 
ON LogApi(DateTime DESC);
GO

CREATE NONCLUSTERED INDEX IX_LogApi_TipoLog 
ON LogApi(TipoLog);
GO

PRINT 'Base de datos ClienteDB creada exitosamente';
PRINT 'Tablas: Cliente, ArchivoCliente, LogApi';
GO