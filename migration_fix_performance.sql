-- Migration: Optimize performance for Team management
-- 1. Add missing indexes
-- 2. Ensure THONG_BAO schema is correct

USE QuanLy_Esports;
GO

-- Index for THANH_VIEN_DOI to speed up joins on ma_doi
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_THANH_VIEN_DOI_MA_DOI' AND object_id = OBJECT_ID('dbo.THANH_VIEN_DOI'))
BEGIN
    CREATE INDEX IX_THANH_VIEN_DOI_MA_DOI ON dbo.THANH_VIEN_DOI(ma_doi);
END;
GO

-- Index for THANH_VIEN_DOI to speed up "My Teams" lookup
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_THANH_VIEN_DOI_MA_ND' AND object_id = OBJECT_ID('dbo.THANH_VIEN_DOI'))
BEGIN
    CREATE INDEX IX_THANH_VIEN_DOI_MA_ND ON dbo.THANH_VIEN_DOI(ma_nguoi_dung);
END;
GO

-- Ensure ma_doi column exists in THONG_BAO (moved from C# code to DB script)
IF COL_LENGTH('dbo.THONG_BAO', 'ma_doi') IS NULL
BEGIN
    ALTER TABLE dbo.THONG_BAO ADD ma_doi INT NULL;
END;
GO
