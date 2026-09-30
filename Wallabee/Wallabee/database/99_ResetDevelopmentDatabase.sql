/*
    DEVELOPMENT ONLY. This permanently removes DiveViewLab.
    Explicitly confirm with:
      sqlcmd -S .\SQLEXPRESS -E -v ConfirmReset="YES" -i database\99_ResetDevelopmentDatabase.sql
*/
IF N'$(ConfirmReset)' <> N'YES'
    THROW 51000, 'Reset cancelled. Re-run with sqlcmd variable ConfirmReset=YES.', 1;
GO

USE [master];
GO

IF DB_ID(N'DiveViewLab') IS NOT NULL
BEGIN
    ALTER DATABASE [DiveViewLab] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DiveViewLab];
END;
GO
