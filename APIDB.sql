-- ============================================
-- Script de creación de base de datos APIDB
-- Tablas: Usuario, Logs
-- ============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'APIDB')
BEGIN
    CREATE DATABASE APIDB;
    PRINT 'Base de datos APIDB creada.';
END
ELSE
BEGIN
    PRINT 'La base de datos APIDB ya existe.';
END
GO

USE APIDB;
GO

-- ============================================
-- Crear tablas dentro de una transacción
-- ============================================
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('Logs', 'U') IS NOT NULL
    BEGIN
        DROP TABLE Logs;
        PRINT 'Tabla Logs eliminada.';
    END

    IF OBJECT_ID('Usuario', 'U') IS NOT NULL
    BEGIN
        DROP TABLE Usuario;
        PRINT 'Tabla Usuario eliminada.';
    END

    -- Crear tabla Usuario
    CREATE TABLE Usuario (
        Id INT IDENTITY(1,1) PRIMARY KEY,
		Nombre varchar(20) NOT NULL,
		Email varchar(50) NOT NULL,
		Rol varchar(20) NOT NULL DEFAULT 'Usuario',
		FechaCreacion datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
		FechaActualizacion datetime2 NULL,
        Password NVARCHAR(255) NOT NULL
    );

    -- Índice único en Email
    CREATE UNIQUE INDEX Usuarios_Email ON Usuario(Email);

    -- Crear tabla Logs
    CREATE TABLE Logs (
     ID INT IDENTITY(1,1) PRIMARY KEY,
	UsuarioID int NULL,
	Accion varchar(30) NOT NULL,
	Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
	RealizadoPor varchar(30) NULL,
	Cambios varchar(40) NULL,
    CONSTRAINT FK_Logs_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id) ON DELETE SET NULL
    );

    COMMIT TRANSACTION;
    PRINT 'Tablas creadas correctamente.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error al crear tablas: ' + ERROR_MESSAGE();
END CATCH;
GO

