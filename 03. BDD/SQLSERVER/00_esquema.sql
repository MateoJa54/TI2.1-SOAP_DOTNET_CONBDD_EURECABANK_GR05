-- =============================================================================
-- 00_esquema.sql - EURECABANK / SQL Server
-- Crea la base indicada por la variable esquema (por defecto eurekabank).
-- Si ya existe, el script se detiene: cargar_bdd_sqlserver.ps1 pide -Recrear para borrarla.
-- =============================================================================
USE master;
GO

IF DB_ID(N'$(esquema)') IS NULL
BEGIN
    CREATE DATABASE [$(esquema)] COLLATE Latin1_General_100_CI_AS;
END
ELSE
BEGIN
    RAISERROR('La base $(esquema) ya existe.', 16, 1);
END
GO
