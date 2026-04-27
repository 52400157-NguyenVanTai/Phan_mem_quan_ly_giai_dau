-- ================================================================
-- ALTER TABLE: Add chairman role to THANH_VIEN_DOI
-- Purpose: Support Chairman/Member role separation
-- Run this on existing QuanLy_Esports database
-- ================================================================

USE QuanLy_Esports;
GO

-- Drop existing CHECK constraint
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_TV_VAITRO')
BEGIN
    ALTER TABLE THANH_VIEN_DOI DROP CONSTRAINT CHK_TV_VAITRO;
END
GO

-- Re-add CHECK constraint with chairman role
ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT CHK_TV_VAITRO 
    CHECK (vai_tro_noi_bo IN ('chairman','leader','coach','captain','member'));
GO

PRINT 'Chairman role added successfully!';
GO
