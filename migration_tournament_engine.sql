-- ================================================================
-- TOURNAMENT ENGINE — MIGRATION SCRIPT
-- Phase 1: Tao Giai Dau + Approval Workflow
-- Date: 09/05/2026
-- ================================================================

USE QuanLy_Esports;
GO

-- ---------------------------------------------------------------
-- 1. Them cot ly_do_tu_choi cho GIAI_DAU
-- ---------------------------------------------------------------
IF COL_LENGTH('dbo.GIAI_DAU', 'ly_do_tu_choi') IS NULL
BEGIN
    ALTER TABLE dbo.GIAI_DAU
    ADD ly_do_tu_choi NVARCHAR(MAX) NULL;
END
GO

-- ---------------------------------------------------------------
-- 2. Them cot is_registration_locked cho GIAI_DAU
-- ---------------------------------------------------------------
IF COL_LENGTH('dbo.GIAI_DAU', 'is_registration_locked') IS NULL
BEGIN
    ALTER TABLE dbo.GIAI_DAU
    ADD is_registration_locked BIT NOT NULL CONSTRAINT DF_GD_REG_LOCKED DEFAULT 0;
END
GO

-- ---------------------------------------------------------------
-- 3. Them cot min_members_per_team cho GIAI_DAU
-- ---------------------------------------------------------------
IF COL_LENGTH('dbo.GIAI_DAU', 'min_members_per_team') IS NULL
BEGIN
    ALTER TABLE dbo.GIAI_DAU
    ADD min_members_per_team INT NOT NULL CONSTRAINT DF_GD_MIN_MEMBERS DEFAULT 1;
END
GO

-- ---------------------------------------------------------------
-- 4. Cap nhat CHECK constraint trang_thai de them trang thai moi
-- ---------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_GD_TRANGTHAI' AND parent_object_id = OBJECT_ID('dbo.GIAI_DAU'))
BEGIN
    ALTER TABLE dbo.GIAI_DAU DROP CONSTRAINT CHK_GD_TRANGTHAI;
END
GO

ALTER TABLE dbo.GIAI_DAU ADD CONSTRAINT CHK_GD_TRANGTHAI
CHECK (trang_thai IN (
    'nhap',                -- Draft
    'cho_xet_duyet',       -- Pending_Approval
    'bi_tu_choi',          -- Rejected
    'sap_dien_ra',         -- Upcoming
    'mo_dang_ky',          -- Registration_Open
    'khoa_dang_ky',        -- Registration_Closed
    'dang_dien_ra',        -- Live
    'ket_thuc',            -- Completed
    'da_huy',              -- Cancelled
    -- Keep old states for compatibility
    'chuan_bi_dien_ra',
    'tong_ket',
    'tam_hoan',
    'khoa'
));
GO

-- ---------------------------------------------------------------
-- 5. Them cot nguong_match_point va bang_diem_json cho GIAI_DOAN
-- ---------------------------------------------------------------
IF COL_LENGTH('dbo.GIAI_DOAN', 'nguong_match_point') IS NULL
BEGIN
    ALTER TABLE dbo.GIAI_DOAN
    ADD nguong_match_point INT NULL;
END
GO

IF COL_LENGTH('dbo.GIAI_DOAN', 'bang_diem_json') IS NULL
BEGIN
    ALTER TABLE dbo.GIAI_DOAN
    ADD bang_diem_json NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH('dbo.GIAI_DOAN', 'so_doi') IS NULL
BEGIN
    ALTER TABLE dbo.GIAI_DOAN
    ADD so_doi INT NOT NULL CONSTRAINT DF_GDO_SODOI DEFAULT 0;
END
GO

-- Cap nhat CHECK constraint the_thuc cua GIAI_DOAN de them BattleRoyale, ChampionRush
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_GDO_THETHUC' AND parent_object_id = OBJECT_ID('dbo.GIAI_DOAN'))
BEGIN
    ALTER TABLE dbo.GIAI_DOAN DROP CONSTRAINT CHK_GDO_THETHUC;
END
GO

ALTER TABLE dbo.GIAI_DOAN ADD CONSTRAINT CHK_GDO_THETHUC
CHECK (the_thuc IN (
    'loai_truc_tiep',           -- Single Elimination
    'nhanh_thang_nhanh_thua',   -- Double Elimination
    'vong_tron',                -- Round Robin
    'thuy_si',                  -- Swiss
    'battle_royale',            -- Battle Royale
    'champion_rush',            -- Champion Rush / Match Point
    'league_bang_cheo'          -- Legacy
));
GO

-- ---------------------------------------------------------------
-- 6. Tao bang TRONG_TAI_GIAI_DAU (Phase 2 prep)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TRONG_TAI_GIAI_DAU')
BEGIN
    CREATE TABLE TRONG_TAI_GIAI_DAU (
        ma_giai_dau INT NOT NULL,
        ma_nguoi_dung INT NOT NULL,
        trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_TRONGTAI_TRANGTHAI DEFAULT 'cho_phan_hoi',
        ngay_cap_quyen DATETIME NOT NULL CONSTRAINT DF_TRONGTAI_NGAY DEFAULT GETDATE(),

        CONSTRAINT PK_TRONGTAI PRIMARY KEY (ma_giai_dau, ma_nguoi_dung),
        CONSTRAINT FK_TRONGTAI_GD FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
        CONSTRAINT FK_TRONGTAI_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT CHK_TRONGTAI_TRANGTHAI CHECK (trang_thai IN ('cho_phan_hoi','da_chap_nhan','tu_choi'))
    );
END
GO

PRINT N'Migration Tournament Engine Phase 1 completed successfully.';
GO
