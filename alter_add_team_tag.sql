-- ================================================================
-- ALTER TABLE: Add team abbreviation/tag field to DOI
-- Purpose: Support team abbreviation/tag display
-- Run this on existing QuanLy_Esports database
-- ================================================================

USE QuanLy_Esports;
GO

-- Add team abbreviation/tag column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DOI') AND name = 'ten_viet_tat')
BEGIN
    ALTER TABLE DOI ADD ten_viet_tat NVARCHAR(20) NULL;
END
GO

PRINT 'Team abbreviation field added successfully!';
GO
