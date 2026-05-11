-- ================================================================
-- TOURNAMENT ENGINE — PHASE 2 MIGRATION
-- Format Engine + Referee Portal + Match Lifecycle
-- ================================================================

USE QuanLy_Esports;
GO

-- 1. Add room/password columns to TRAN_DAU
IF COL_LENGTH('dbo.TRAN_DAU', 'id_phong_game') IS NULL
BEGIN
    ALTER TABLE dbo.TRAN_DAU ADD id_phong_game NVARCHAR(50) NULL;
END
GO

IF COL_LENGTH('dbo.TRAN_DAU', 'mat_khau_phong') IS NULL
BEGIN
    ALTER TABLE dbo.TRAN_DAU ADD mat_khau_phong NVARCHAR(50) NULL;
END
GO

-- 2. Add check-in columns to CHI_TIET_TRAN_DAU
IF COL_LENGTH('dbo.CHI_TIET_TRAN_DAU', 'is_check_in') IS NULL
BEGIN
    ALTER TABLE dbo.CHI_TIET_TRAN_DAU ADD is_check_in BIT NOT NULL CONSTRAINT DF_CTTD_CHECKIN DEFAULT 0;
END
GO

IF COL_LENGTH('dbo.CHI_TIET_TRAN_DAU', 'so_kill') IS NULL
BEGIN
    ALTER TABLE dbo.CHI_TIET_TRAN_DAU ADD so_kill INT NOT NULL CONSTRAINT DF_CTTD_KILL DEFAULT 0;
END
GO

IF COL_LENGTH('dbo.CHI_TIET_TRAN_DAU', 'url_anh_bang_chung') IS NULL
BEGIN
    ALTER TABLE dbo.CHI_TIET_TRAN_DAU ADD url_anh_bang_chung NVARCHAR(500) NULL;
END
GO

-- 3. Update TRAN_DAU trang_thai check to include new states
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_TD_TRANGTHAI' AND parent_object_id = OBJECT_ID('dbo.TRAN_DAU'))
BEGIN
    ALTER TABLE dbo.TRAN_DAU DROP CONSTRAINT CHK_TD_TRANGTHAI;
END
GO

ALTER TABLE dbo.TRAN_DAU ADD CONSTRAINT CHK_TD_TRANGTHAI
CHECK (trang_thai IN ('chua_dau','san_sang','dang_dau','cho_ket_qua','da_hoan_thanh','huy_bo','bye'));
GO

-- 4. Update GIAI_DOAN the_thuc check to include battle_royale
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_GDO_THETHUC' AND parent_object_id = OBJECT_ID('dbo.GIAI_DOAN'))
BEGIN
    ALTER TABLE dbo.GIAI_DOAN DROP CONSTRAINT CHK_GDO_THETHUC;
END
GO

ALTER TABLE dbo.GIAI_DOAN ADD CONSTRAINT CHK_GDO_THETHUC
CHECK (the_thuc IN (
    'loai_truc_tiep','nhanh_thang_nhanh_thua',
    'vong_tron','league_bang_cheo','thuy_si',
    'battle_royale','champion_rush'
));
GO

PRINT N'Migration Phase 2 completed.';
GO
