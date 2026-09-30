/*
    DiveViewLab SQL Server bootstrap.
    Run this script while connected to the SQL Server instance, typically using master.
*/
USE [master];
GO

IF DB_ID(N'DiveViewLab') IS NULL
BEGIN
    CREATE DATABASE [DiveViewLab];
END;
GO

ALTER DATABASE [DiveViewLab] SET RECOVERY SIMPLE;
GO

ALTER DATABASE [DiveViewLab] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO
