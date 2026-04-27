-- ================================================================
-- UPDATE TOURNAMENT STATUS SYSTEM
-- Purpose: Add new tournament statuses and suspension tracking
-- ================================================================

USE QuanLy_Esports;
GO

-- Add columns for suspension tracking
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GIAI_DAU') AND name = 'thoi_gian_bat_dau_tam_hoan')
BEGIN
    ALTER TABLE GIAI_DAU
    ADD thoi_gian_bat_dau_tam_hoan DATETIME NULL,
        thoi_gian_ket_thuc_tam_hoan DATETIME NULL,
        thoi_gian_tam_hoan_total INT NULL;  -- Total suspension time in minutes
END
GO

-- Add columns for min/max teams
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GIAI_DAU') AND name = 'so_doi_toi_thieu')
BEGIN
    ALTER TABLE GIAI_DAU
    ADD so_doi_toi_thieu INT NULL DEFAULT 2,
        so_doi_toi_da INT NULL;
END
GO

-- Add column for registration control
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GIAI_DAU') AND name = 'dang_mo_dang_ky')
BEGIN
    ALTER TABLE GIAI_DAU
    ADD dang_mo_dang_ky BIT NULL DEFAULT 0;
END
GO

-- Update the CHECK constraint for trang_thai
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID('CHK_GD_TRANGTHAI'))
BEGIN
    ALTER TABLE GIAI_DAU DROP CONSTRAINT CHK_GD_TRANGTHAI;
END
GO

ALTER TABLE GIAI_DAU
ADD CONSTRAINT CHK_GD_TRANGTHAI CHECK (trang_thai IN (
    'nhap',              -- Nháp (Draft)
    'cho_xet_duyet',     -- Chờ xét duyệt (Pending Approval)
    'chuan_bi_dien_ra',  -- Chuẩn bị diễn ra (Preparing)
    'dang_dien_ra',      -- Đang diễn ra (Ongoing)
    'tong_ket',          -- Tổng kết (Summary)
    'ket_thuc',          -- Kết thúc (Ended)
    'tam_hoan'           -- Tạm hoãn (Suspended)
));
GO

-- Update existing tournaments with old status values
UPDATE GIAI_DAU SET trang_thai = 'nhap' WHERE trang_thai = 'ban_nhap';
UPDATE GIAI_DAU SET trang_thai = 'cho_xet_duyet' WHERE trang_thai = 'cho_phe_duyet';
UPDATE GIAI_DAU SET trang_thai = 'chuan_bi_dien_ra' WHERE trang_thai = 'sap_dien_ra';
GO

-- Set default values for min/max teams
UPDATE GIAI_DAU SET so_doi_toi_thieu = 2 WHERE so_doi_toi_thieu IS NULL;
UPDATE GIAI_DAU SET so_doi_toi_da = NULL WHERE so_doi_toi_da IS NULL;
GO

-- Update GIAI_THUONG table to use gia_tri and so_luong instead of phan_thuong
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GIAI_THUONG') AND name = 'phan_thuong')
BEGIN
    ALTER TABLE GIAI_THUONG
    ADD gia_tri DECIMAL(15,2) NULL DEFAULT 0,
        so_luong INT NULL DEFAULT 1;
    
    -- Migrate existing data if possible (phan_thuong was text, can't auto-convert)
    -- Users will need to re-enter prize values
    ALTER TABLE GIAI_THUONG DROP COLUMN phan_thuong;
END
GO

-- Create table for tournament referees
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TRONG_TAI_GIAI')
BEGIN
    CREATE TABLE TRONG_TAI_GIAI (
        ma_trong_tai INT IDENTITY(1,1) PRIMARY KEY,
        ma_giai_dau INT NOT NULL,
        ma_nguoi_dung INT NOT NULL,
        trang_thai NVARCHAR(50) DEFAULT 'da_duyet',  -- da_duyet, tu_choi
        thoi_gian_moi DATETIME DEFAULT GETDATE(),
        thoi_gian_duyet DATETIME NULL,
        ma_nguoi_moi INT NOT NULL,
        FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_nguoi_moi) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
    );
END
GO

-- Create table for co-organizers
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BAN_TO_CHUC_GIAI')
BEGIN
    CREATE TABLE BAN_TO_CHUC_GIAI (
        ma_ban_to_chuc INT IDENTITY(1,1) PRIMARY KEY,
        ma_giai_dau INT NOT NULL,
        ma_nguoi_dung INT NOT NULL,
        trang_thai NVARCHAR(50) DEFAULT 'da_duyet',  -- da_duyet, tu_choi
        thoi_gian_moi DATETIME DEFAULT GETDATE(),
        thoi_gian_duyet DATETIME NULL,
        ma_nguoi_moi INT NOT NULL,
        FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_nguoi_moi) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
    );
END
GO

PRINT 'Đã cập nhật hệ thống trạng thái giải đấu, bảng giải thưởng, và thêm bảng trọng tài/ban tổ chức.';
