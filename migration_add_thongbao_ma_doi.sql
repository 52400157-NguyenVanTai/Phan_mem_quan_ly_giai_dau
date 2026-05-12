USE QuanLy_Esports;
GO

IF COL_LENGTH('dbo.THONG_BAO', 'ma_doi') IS NULL
BEGIN
    ALTER TABLE dbo.THONG_BAO ADD ma_doi INT NULL;
END
GO

PRINT N'Added THONG_BAO.ma_doi if missing.';
GO
