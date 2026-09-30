/* Run after 00_CreateDatabase.sql. */
USE [DiveViewLab];
GO

IF SCHEMA_ID(N'lab') IS NULL
    EXECUTE(N'CREATE SCHEMA [lab] AUTHORIZATION [dbo];');
GO

IF SCHEMA_ID(N'benchmark') IS NULL
    EXECUTE(N'CREATE SCHEMA [benchmark] AUTHORIZATION [dbo];');
GO

IF SCHEMA_ID(N'telemetry') IS NULL
    EXECUTE(N'CREATE SCHEMA [telemetry] AUTHORIZATION [dbo];');
GO

IF SCHEMA_ID(N'reporting') IS NULL
    EXECUTE(N'CREATE SCHEMA [reporting] AUTHORIZATION [dbo];');
GO
