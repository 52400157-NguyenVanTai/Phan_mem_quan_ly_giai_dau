-- ================================================================
-- ALTER TABLE: Allow NULL for ma_tro_choi in NHOM_DOI
-- Purpose: Support management group (nhóm quản lý) without game assignment
-- Run this on existing QuanLy_Esports database
-- ================================================================

USE QuanLy_Esports;
GO

-- 1. Drop foreign key constraint first
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NHOM_TC')
BEGIN
    ALTER TABLE NHOM_DOI DROP CONSTRAINT FK_NHOM_TC;
END
GO

-- 2. Change ma_tro_choi to allow NULL
ALTER TABLE NHOM_DOI ALTER COLUMN ma_tro_choi INT NULL;
GO

-- 3. Re-add foreign key constraint with NULL allowed
ALTER TABLE NHOM_DOI ADD CONSTRAINT FK_NHOM_TC FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi);
GO

PRINT 'Schema updated successfully!';
GO
