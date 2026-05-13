-- ================================================================
-- QUANLY_ESPORTS - COMPLETE RESET DATABASE SCRIPT
-- SQL Server 2019+ | ASP.NET MVC 5 / Bootstrap 5 / jQuery
-- WARNING: Drops and recreates QuanLy_Esports.
-- ================================================================

USE master;
GO
IF DB_ID(N'QuanLy_Esports') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLy_Esports SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLy_Esports;
END
GO
CREATE DATABASE QuanLy_Esports COLLATE Vietnamese_CI_AS;
GO
USE QuanLy_Esports;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

CREATE TABLE dbo.DANH_MUC_TRO_CHOI (
    ma_tro_choi INT IDENTITY(1,1) CONSTRAINT PK_DANH_MUC_TRO_CHOI PRIMARY KEY,
    ten_game NVARCHAR(100) NOT NULL CONSTRAINT UQ_TC_TEN_GAME UNIQUE,
    the_loai NVARCHAR(50) NOT NULL,
    is_active BIT NOT NULL CONSTRAINT DF_TC_ACTIVE DEFAULT 1,
    CONSTRAINT CHK_TC_THE_LOAI CHECK (the_loai IN ('MOBA','FPS','BATTLEROYALE'))
);
GO
CREATE TABLE dbo.DANH_MUC_VI_TRI (
    ma_vi_tri INT IDENTITY(1,1) CONSTRAINT PK_DANH_MUC_VI_TRI PRIMARY KEY,
    ma_tro_choi INT NULL,
    ten_vi_tri NVARCHAR(100) NOT NULL,
    ky_hieu NVARCHAR(20) NOT NULL,
    loai_vi_tri NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_VITRI_GAME FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi),
    CONSTRAINT CHK_VITRI_LOAI CHECK (loai_vi_tri IN ('TuyenThu','HuanLuyen'))
);
GO
INSERT INTO dbo.DANH_MUC_TRO_CHOI(ten_game,the_loai) VALUES
(N'Arena of Valor','MOBA'),(N'League of Legends','MOBA'),(N'Free Fire','BATTLEROYALE'),
(N'PUBG','BATTLEROYALE'),(N'Valorant','FPS'),(N'CS:GO','FPS');
GO
DECLARE @AOV INT=(SELECT ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game=N'Arena of Valor');
DECLARE @LOL INT=(SELECT ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game=N'League of Legends');
DECLARE @FF INT=(SELECT ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game=N'Free Fire');
DECLARE @PUBG INT=(SELECT ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game=N'PUBG');
DECLARE @VAL INT=(SELECT ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game=N'Valorant');
DECLARE @CS INT=(SELECT ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game=N'CS:GO');
INSERT INTO dbo.DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES
(@AOV,N'Duong Caesar',N'DS','TuyenThu'),(@AOV,N'Di rung',N'JG','TuyenThu'),(@AOV,N'Duong giua',N'MID','TuyenThu'),(@AOV,N'Xa thu',N'AD','TuyenThu'),(@AOV,N'Tro thu',N'SP','TuyenThu'),
(@LOL,N'Duong tren',N'TOP','TuyenThu'),(@LOL,N'Di rung',N'JGL','TuyenThu'),(@LOL,N'Duong giua',N'MID','TuyenThu'),(@LOL,N'Xa thu',N'ADC','TuyenThu'),(@LOL,N'Ho tro',N'SUP','TuyenThu'),
(@VAL,N'Duelist',N'DUEL','TuyenThu'),(@VAL,N'Initiator',N'INIT','TuyenThu'),(@VAL,N'Controller',N'CTRL','TuyenThu'),(@VAL,N'Sentinel',N'SENT','TuyenThu'),
(@CS,N'Entry Fragger',N'ENTRY','TuyenThu'),(@CS,N'AWPer',N'AWP','TuyenThu'),(@CS,N'In-game Leader',N'IGL','TuyenThu'),(@CS,N'Support',N'SUP','TuyenThu'),
(@FF,N'Rusher',N'RUSH','TuyenThu'),(@FF,N'Sniper',N'SNIPER','TuyenThu'),(@FF,N'Support',N'SUPPORT','TuyenThu'),
(@PUBG,N'Chi huy',N'IGL','TuyenThu'),(@PUBG,N'Trinh sat',N'SCOUT','TuyenThu'),(@PUBG,N'Tan cong',N'FRAG','TuyenThu'),
(NULL,N'Huan luyen vien',N'HLV','HuanLuyen'),(NULL,N'Phan tich vien',N'PT','HuanLuyen'),(NULL,N'Quan ly',N'QL','HuanLuyen');
GO

CREATE TABLE dbo.NGUOI_DUNG (
    ma_nguoi_dung INT IDENTITY(1,1) CONSTRAINT PK_NGUOI_DUNG PRIMARY KEY,
    ten_dang_nhap NVARCHAR(100) NOT NULL CONSTRAINT UQ_ND_TENDANGNHAP UNIQUE,
    email NVARCHAR(150) NOT NULL CONSTRAINT UQ_ND_EMAIL UNIQUE,
    mat_khau_ma_hoa NVARCHAR(255) NOT NULL,
    vai_tro_he_thong NVARCHAR(10) NOT NULL CONSTRAINT DF_ND_VAITRO DEFAULT 'user',
    avatar_url NVARCHAR(400) NULL,
    bio NVARCHAR(500) NULL,
    is_banned BIT NOT NULL CONSTRAINT DF_ND_BANNED DEFAULT 0,
    ly_do_ban NVARCHAR(500) NULL,
    thoi_gian_ban DATETIME NULL,
    ma_admin_ban INT NULL,
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_ND_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT CHK_ND_VAITRO CHECK (vai_tro_he_thong IN ('admin','user')),
    CONSTRAINT FK_ND_ADMIN_BAN FOREIGN KEY (ma_admin_ban) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung)
);
GO
CREATE TABLE dbo.HO_SO_IN_GAME (
    ma_ho_so INT IDENTITY(1,1) CONSTRAINT PK_HO_SO_IN_GAME PRIMARY KEY,
    ma_nguoi_dung INT NOT NULL,
    ma_tro_choi INT NOT NULL,
    in_game_id NVARCHAR(100) NULL,
    in_game_name NVARCHAR(100) NULL,
    ma_vi_tri_so_truong INT NULL,
    thanh_tich NVARCHAR(1000) NULL,
    ngay_cap_nhat DATETIME NOT NULL CONSTRAINT DF_HSG_UPDATE DEFAULT GETDATE(),
    CONSTRAINT UQ_HSG_PROFILE UNIQUE (ma_nguoi_dung, ma_tro_choi),
    CONSTRAINT FK_HSG_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_HSG_TC FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_HSG_VITRI FOREIGN KEY (ma_vi_tri_so_truong) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri)
);
GO
CREATE TABLE dbo.DANG_KY_TRONG_TAI (
    ma_dang_ky INT IDENTITY(1,1) CONSTRAINT PK_DANG_KY_TRONG_TAI PRIMARY KEY,
    ma_nguoi_dung INT NOT NULL,
    ma_tro_choi INT NOT NULL,
    trang_thai NVARCHAR(50) NOT NULL CONSTRAINT DF_DKTT_TRANGTHAI DEFAULT 'cho_duyet',
    ngay_dang_ky DATETIME NOT NULL CONSTRAINT DF_DKTT_NGAY DEFAULT GETDATE(),
    thoi_gian_duyet DATETIME NULL,
    CONSTRAINT FK_DKTT_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_DKTT_TC FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi),
    CONSTRAINT CHK_DKTT_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','da_duyet','tu_choi')),
    CONSTRAINT UQ_DKTT_USER_GAME UNIQUE (ma_nguoi_dung, ma_tro_choi)
);
GO

CREATE TABLE dbo.DOI (
    ma_doi INT IDENTITY(1,1) CONSTRAINT PK_DOI PRIMARY KEY,
    ten_doi NVARCHAR(150) NOT NULL,
    ten_viet_tat NVARCHAR(20) NULL,
    ma_doi_truong INT NOT NULL,
    ma_manager INT NULL,
    ma_tro_choi INT NOT NULL,
    logo_url NVARCHAR(400) NULL,
    slogan NVARCHAR(300) NULL,
    mo_ta NVARCHAR(500) NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_DOI_TRANGTHAI DEFAULT 'dang_hoat_dong',
    dang_tuyen BIT NOT NULL CONSTRAINT DF_DOI_DANGTUYEN DEFAULT 0,
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_DOI_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_DOI_DOITRUONG FOREIGN KEY (ma_doi_truong) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_DOI_MANAGER FOREIGN KEY (ma_manager) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_DOI_TROCHOI FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi),
    CONSTRAINT CHK_DOI_TRANGTHAI CHECK (trang_thai IN ('dang_hoat_dong','tam_dung','da_giai_the'))
);
GO
CREATE TABLE dbo.THANH_VIEN_DOI (
    ma_thanh_vien INT IDENTITY(1,1) CONSTRAINT PK_THANH_VIEN_DOI PRIMARY KEY,
    ma_nguoi_dung INT NOT NULL,
    ma_doi INT NOT NULL,
    ma_vi_tri INT NULL,
    vai_tro_noi_bo NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_VAITRO DEFAULT 'thanh_vien',
    phan_he NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_PHANHE DEFAULT 'TuyenThu',
    trang_thai_duyet NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_DUYET DEFAULT 'da_duyet',
    trang_thai_hop_dong NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_HOPDONG DEFAULT 'dang_hieu_luc',
    ngay_tham_gia DATETIME NOT NULL CONSTRAINT DF_TV_NGAYTG DEFAULT GETDATE(),
    CONSTRAINT FK_TV_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TV_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_TV_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT CHK_TV_VAITRO CHECK (vai_tro_noi_bo IN ('chu_tich','ban_dieu_hanh','doi_truong','thanh_vien')),
    CONSTRAINT CHK_TV_PHANHE CHECK (phan_he IN ('TuyenThu','HuanLuyen')),
    CONSTRAINT CHK_TV_DUYET CHECK (trang_thai_duyet IN ('cho_duyet','da_duyet','bi_tu_choi')),
    CONSTRAINT CHK_TV_HOPDONG CHECK (trang_thai_hop_dong IN ('dang_hieu_luc','tu_do','da_giai_phong'))
);
GO
CREATE OR ALTER TRIGGER dbo.TRG_THANH_VIEN_DOI_RULES ON dbo.THANH_VIEN_DOI AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.THANH_VIEN_DOI tv ON i.ma_nguoi_dung=tv.ma_nguoi_dung JOIN dbo.DOI di ON i.ma_doi=di.ma_doi JOIN dbo.DOI dtv ON tv.ma_doi=dtv.ma_doi WHERE i.trang_thai_duyet='da_duyet' AND i.trang_thai_hop_dong='dang_hieu_luc' AND tv.trang_thai_duyet='da_duyet' AND tv.trang_thai_hop_dong='dang_hieu_luc' AND di.ma_tro_choi=dtv.ma_tro_choi AND i.ma_thanh_vien<>tv.ma_thanh_vien)
    BEGIN RAISERROR(N'Mot nguoi dung chi duoc tham gia mot doi trong cung game.',16,1); ROLLBACK TRANSACTION; RETURN; END;
    IF EXISTS (SELECT 1 FROM dbo.THANH_VIEN_DOI tv WHERE tv.ma_doi IN (SELECT DISTINCT ma_doi FROM inserted) AND tv.vai_tro_noi_bo='doi_truong' AND tv.trang_thai_duyet='da_duyet' AND tv.trang_thai_hop_dong='dang_hieu_luc' GROUP BY tv.ma_doi HAVING COUNT(*)>1)
    BEGIN RAISERROR(N'Moi doi chi duoc co mot doi truong.',16,1); ROLLBACK TRANSACTION; RETURN; END;
END;
GO

-- ================================================================
-- 4. TEAM RECRUITMENT AND REQUESTS
-- ================================================================
CREATE TABLE dbo.BAI_DANG_TUYEN_DUNG (
    ma_bai_dang INT IDENTITY(1,1) CONSTRAINT PK_BAI_DANG_TUYEN_DUNG PRIMARY KEY,
    ma_doi INT NOT NULL,
    ma_vi_tri INT NOT NULL,
    noi_dung NVARCHAR(500) NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_BD_TRANGTHAI DEFAULT 'dang_mo',
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_BD_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_BD_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_BD_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT CHK_BD_TRANGTHAI CHECK (trang_thai IN ('dang_mo','tam_dong','da_dong'))
);
GO
CREATE TABLE dbo.DON_UNG_TUYEN (
    ma_don INT IDENTITY(1,1) CONSTRAINT PK_DON_UNG_TUYEN PRIMARY KEY,
    ma_bai_dang INT NOT NULL,
    ma_ung_vien INT NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_DUT_TRANGTHAI DEFAULT 'cho_duyet',
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_DUT_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_DUT_BAIDANG FOREIGN KEY (ma_bai_dang) REFERENCES dbo.BAI_DANG_TUYEN_DUNG(ma_bai_dang),
    CONSTRAINT FK_DUT_UNGVIEN FOREIGN KEY (ma_ung_vien) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_DUT_UNIQUE UNIQUE (ma_bai_dang, ma_ung_vien),
    CONSTRAINT CHK_DUT_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','chap_nhan','tu_choi'))
);
GO
CREATE TABLE dbo.LOI_MOI_GIA_NHAP (
    ma_loi_moi INT IDENTITY(1,1) CONSTRAINT PK_LOI_MOI_GIA_NHAP PRIMARY KEY,
    ma_doi INT NOT NULL,
    ma_nguoi_duoc_moi INT NOT NULL,
    ma_nguoi_gui INT NULL,
    ma_vi_tri INT NULL,
    mo_ta NVARCHAR(500) NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_LM_TRANGTHAI DEFAULT 'cho_phan_hoi',
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_LM_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_LM_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_LM_NGUOINHAN FOREIGN KEY (ma_nguoi_duoc_moi) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_LM_NGUOIGUI FOREIGN KEY (ma_nguoi_gui) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_LM_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT UQ_LM_UNIQUE UNIQUE (ma_doi, ma_nguoi_duoc_moi),
    CONSTRAINT CHK_LM_TRANGTHAI CHECK (trang_thai IN ('cho_phan_hoi','chap_nhan','tu_choi','da_het_han'))
);
GO
CREATE TABLE dbo.YEU_CAU_MOI_THANH_VIEN_DOI (
    ma_yeu_cau INT IDENTITY(1,1) CONSTRAINT PK_YEU_CAU_MOI_THANH_VIEN_DOI PRIMARY KEY,
    ma_doi INT NOT NULL,
    ma_nguoi_duoc_moi INT NOT NULL,
    ma_nguoi_gui INT NOT NULL,
    ma_vi_tri INT NULL,
    mo_ta NVARCHAR(500) NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_YCMTV_TRANGTHAI DEFAULT 'cho_duyet',
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_YCMTV_NGAYTAO DEFAULT GETDATE(),
    ngay_duyet DATETIME NULL,
    ma_nguoi_duyet INT NULL,
    CONSTRAINT FK_YCMTV_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_YCMTV_NGUOINHAN FOREIGN KEY (ma_nguoi_duoc_moi) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCMTV_NGUOIGUI FOREIGN KEY (ma_nguoi_gui) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCMTV_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT FK_YCMTV_NGUOIDUYET FOREIGN KEY (ma_nguoi_duyet) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_YCMTV_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','chap_nhan','tu_choi'))
);
GO
CREATE TABLE dbo.XIN_GIA_NHAP (
    ma_don_xin INT IDENTITY(1,1) CONSTRAINT PK_XIN_GIA_NHAP PRIMARY KEY,
    ma_nguoi_dung INT NOT NULL,
    ma_doi INT NOT NULL,
    ma_ho_so INT NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_XG_TRANGTHAI DEFAULT 'cho_duyet',
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_XG_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_XG_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_XG_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_XG_HOSO FOREIGN KEY (ma_ho_so) REFERENCES dbo.HO_SO_IN_GAME(ma_ho_so),
    CONSTRAINT UQ_XINGIANHAP UNIQUE (ma_nguoi_dung, ma_doi),
    CONSTRAINT CHK_XG_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','chap_nhan','tu_choi'))
);
GO
CREATE TABLE dbo.YEU_CAU_XAC_NHAN_LOI_MOI (
    ma_yeu_cau INT IDENTITY(1,1) CONSTRAINT PK_YEU_CAU_XAC_NHAN_LOI_MOI PRIMARY KEY,
    ma_nguoi_gui INT NOT NULL,
    ma_doi INT NOT NULL,
    ma_nguoi_nhan INT NOT NULL,
    trang_thai NVARCHAR(50) NOT NULL CONSTRAINT DF_YCXNLM_TRANGTHAI DEFAULT 'cho_xac_nhan',
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_YCXNLM_NGAYTAO DEFAULT GETDATE(),
    ngay_xac_nhan DATETIME NULL,
    ma_nguoi_xac_nhan INT NULL,
    CONSTRAINT FK_YCXNLM_NGUOI_GUI FOREIGN KEY (ma_nguoi_gui) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCXNLM_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_YCXNLM_NGUOI_NHAN FOREIGN KEY (ma_nguoi_nhan) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCXNLM_NGUOI_XAC_NHAN FOREIGN KEY (ma_nguoi_xac_nhan) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_YCXNLM_TRANGTHAI CHECK (trang_thai IN ('cho_xac_nhan','da_xac_nhan','tu_choi'))
);
GO

-- ================================================================
-- 5. TOURNAMENT CORE
-- ================================================================
CREATE TABLE dbo.GIAI_DAU (
    ma_giai_dau INT IDENTITY(1,1) CONSTRAINT PK_GIAI_DAU PRIMARY KEY,
    ten_giai_dau NVARCHAR(150) NOT NULL,
    ma_tro_choi INT NULL,
    ma_nguoi_tao INT NULL,
    the_thuc NVARCHAR(50) NOT NULL,
    banner_url NVARCHAR(400) NULL,
    mo_ta NVARCHAR(500) NULL,
    kieu_tham_gia NVARCHAR(20) NOT NULL CONSTRAINT DF_GD_KIEU_THAM_GIA DEFAULT 'theo_doi',
    so_nguoi_moi_doi INT NULL,
    so_luong_doi_toi_da INT NULL,
    so_doi_toi_thieu INT NOT NULL CONSTRAINT DF_GD_DOI_TOI_THIEU DEFAULT 2,
    so_doi_toi_da INT NULL,
    luat_giai NVARCHAR(MAX) NULL,
    thong_tin_lien_he NVARCHAR(250) NULL,
    che_do_cong_khai NVARCHAR(30) NOT NULL CONSTRAINT DF_GD_CHE_DO_CONG_KHAI DEFAULT 'cong_khai_sau_duyet',
    dang_mo_dang_ky BIT NOT NULL CONSTRAINT DF_GD_MO_DANG_KY DEFAULT 0,
    tong_giai_thuong DECIMAL(15,2) NOT NULL CONSTRAINT DF_GD_GIAILTHUONG DEFAULT 0,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_GD_TRANGTHAI DEFAULT 'nhap',
    hien_thi_public BIT NOT NULL CONSTRAINT DF_GD_PUBLIC DEFAULT 1,
    is_deleted BIT NOT NULL CONSTRAINT DF_GD_DELETED DEFAULT 0,
    ly_do_tu_choi NVARCHAR(MAX) NULL,
    is_registration_locked BIT NOT NULL CONSTRAINT DF_GD_REG_LOCKED DEFAULT 0,
    min_members_per_team INT NOT NULL CONSTRAINT DF_GD_MIN_MEMBERS DEFAULT 1,
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_GD_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_GD_TC FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_GD_NGUOITAO FOREIGN KEY (ma_nguoi_tao) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_GD_THETHUC CHECK (the_thuc IN ('loai_truc_tiep','nhanh_thang_nhanh_thua','dau_theo_bang','vong_tron','vong_tron_tinh_diem','thuy_si','battle_royale','champion_rush','league_bang_cheo','hon_hop')),
    CONSTRAINT CHK_GD_TRANGTHAI CHECK (trang_thai IN ('nhap','cho_xet_duyet','bi_tu_choi','sap_dien_ra','mo_dang_ky','khoa_dang_ky','dang_dien_ra','ket_thuc','da_huy')),
    CONSTRAINT CHK_GD_KIEU CHECK (kieu_tham_gia IN ('theo_doi','ca_nhan')),
    CONSTRAINT CHK_GD_PUBLIC_MODE CHECK (che_do_cong_khai IN ('cong_khai_sau_duyet','rieng_tu')),
    CONSTRAINT CHK_GD_MIN_TEAM CHECK (so_doi_toi_thieu >= 2),
    CONSTRAINT CHK_GD_MAX_TEAM CHECK (so_doi_toi_da IS NULL OR so_doi_toi_da >= 2)
);
GO
CREATE TABLE dbo.QUAN_TRI_GIAI_DAU (
    ma_giai_dau INT NOT NULL,
    ma_nguoi_dung INT NOT NULL,
    vai_tro_giai NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_QTGD PRIMARY KEY (ma_giai_dau, ma_nguoi_dung),
    CONSTRAINT FK_QTGD_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_QTGD_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_QTGD_VAITRO CHECK (vai_tro_giai IN ('ban_to_chuc','trong_tai'))
);
GO
CREATE TABLE dbo.TRONG_TAI_GIAI_DAU (
    ma_giai_dau INT NOT NULL,
    ma_nguoi_dung INT NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_TRONGTAI_TRANGTHAI DEFAULT 'cho_phan_hoi',
    ngay_cap_quyen DATETIME NOT NULL CONSTRAINT DF_TRONGTAI_NGAY DEFAULT GETDATE(),
    CONSTRAINT PK_TRONGTAI PRIMARY KEY (ma_giai_dau, ma_nguoi_dung),
    CONSTRAINT FK_TRONGTAI_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_TRONGTAI_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_TRONGTAI_TRANGTHAI CHECK (trang_thai IN ('cho_phan_hoi','da_chap_nhan','tu_choi'))
);
GO
CREATE TABLE dbo.YEU_CAU_TAO_GIAI_DAU (
    ma_yeu_cau INT IDENTITY(1,1) CONSTRAINT PK_YEU_CAU_TAO_GIAI_DAU PRIMARY KEY,
    ma_nguoi_gui INT NOT NULL,
    ten_giai_dau NVARCHAR(150) NOT NULL,
    ma_tro_choi INT NULL,
    the_thuc NVARCHAR(50) NOT NULL,
    tong_giai_thuong DECIMAL(15,2) NOT NULL CONSTRAINT DF_YCTGD_GIAI_THUONG DEFAULT 0,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_YCTGD_TRANGTHAI DEFAULT 'cho_duyet',
    ma_admin_duyet INT NULL,
    ly_do_huy NVARCHAR(500) NULL,
    thoi_gian_gui DATETIME NOT NULL CONSTRAINT DF_YCTGD_THOIGIANGUI DEFAULT GETDATE(),
    CONSTRAINT FK_YCTGD_NGUOIGUI FOREIGN KEY (ma_nguoi_gui) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCTGD_TC FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_YCTGD_ADMIN FOREIGN KEY (ma_admin_duyet) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_YCTGD_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','da_duyet','tu_choi'))
);
GO
CREATE TABLE dbo.THAM_GIA_GIAI (
    ma_tham_gia INT IDENTITY(1,1) CONSTRAINT PK_THAM_GIA_GIAI PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    ma_doi INT NOT NULL,
    trang_thai_duyet NVARCHAR(20) NOT NULL CONSTRAINT DF_TGG_DUYET DEFAULT 'cho_duyet',
    trang_thai_tham_gia NVARCHAR(20) NOT NULL CONSTRAINT DF_TGG_THAMGIA DEFAULT 'dang_thi_dau',
    hat_giong INT NULL,
    ngay_dang_ky DATETIME NOT NULL CONSTRAINT DF_TGG_NGAY DEFAULT GETDATE(),
    CONSTRAINT FK_TGG_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_TGG_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT UQ_TGG_GD_DOI UNIQUE (ma_giai_dau, ma_doi),
    CONSTRAINT CHK_TGG_DUYET CHECK (trang_thai_duyet IN ('cho_duyet','da_duyet','bi_tu_choi')),
    CONSTRAINT CHK_TGG_THAMGIA CHECK (trang_thai_tham_gia IN ('dang_thi_dau','di_tiep','bi_loai'))
);
GO

CREATE TABLE dbo.DOI_HINH_THI_DAU (
    ma_doi_hinh INT IDENTITY(1,1) CONSTRAINT PK_DOI_HINH_THI_DAU PRIMARY KEY,
    ma_tham_gia INT NOT NULL,
    ma_giai_dau INT NOT NULL,
    ma_nguoi_dung INT NOT NULL,
    ma_vi_tri INT NULL,
    is_du_bi BIT NOT NULL CONSTRAINT DF_DHTS_DUBI DEFAULT 0,
    CONSTRAINT FK_DHTS_THAMGIA FOREIGN KEY (ma_tham_gia) REFERENCES dbo.THAM_GIA_GIAI(ma_tham_gia),
    CONSTRAINT FK_DHTS_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_DHTS_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_DHTS_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT UQ_DHTS_GD_PLAYER UNIQUE (ma_giai_dau, ma_nguoi_dung)
);
GO
CREATE TABLE dbo.GIAI_THUONG (
    ma_giai_thuong INT IDENTITY(1,1) CONSTRAINT PK_GIAI_THUONG PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    vi_tri_top INT NULL,
    so_tien DECIMAL(15,2) NULL,
    ten_giai NVARCHAR(150) NULL,
    gia_tri DECIMAL(15,2) NOT NULL CONSTRAINT DF_GT_GIA_TRI DEFAULT 0,
    so_luong INT NOT NULL CONSTRAINT DF_GT_SO_LUONG DEFAULT 1,
    mo_ta NVARCHAR(500) NULL,
    CONSTRAINT FK_GT_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau)
);
GO
CREATE TABLE dbo.GIAI_DOAN (
    ma_giai_doan INT IDENTITY(1,1) CONSTRAINT PK_GIAI_DOAN PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    ten_giai_doan NVARCHAR(100) NOT NULL,
    the_thuc NVARCHAR(50) NOT NULL,
    thu_tu INT NOT NULL,
    so_doi_di_tiep INT NOT NULL CONSTRAINT DF_GDO_DOIDITIEP DEFAULT 0,
    diem_nguong_match_point INT NULL,
    nguong_match_point INT NULL,
    bang_diem_json NVARCHAR(MAX) NULL,
    so_doi INT NOT NULL CONSTRAINT DF_GDO_SODOI DEFAULT 0,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_GDO_TRANGTHAI DEFAULT 'chua_bat_dau',
    CONSTRAINT FK_GDO_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT CHK_GDO_THETHUC CHECK (the_thuc IN ('loai_truc_tiep','nhanh_thang_nhanh_thua','vong_tron','vong_tron_tinh_diem','league_bang_cheo','thuy_si','battle_royale','champion_rush')),
    CONSTRAINT CHK_GDO_TRANGTHAI CHECK (trang_thai IN ('chua_bat_dau','dang_dien_ra','ket_thuc')),
    CONSTRAINT UQ_GDO_GD_THUTU UNIQUE (ma_giai_dau, thu_tu)
);
GO
CREATE TABLE dbo.TRAN_DAU (
    ma_tran INT IDENTITY(1,1) CONSTRAINT PK_TRAN_DAU PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    ma_giai_doan INT NULL,
    ma_trong_tai INT NULL,
    vong_dau NVARCHAR(100) NULL,
    the_thuc_tran NVARCHAR(20) NOT NULL,
    so_vong INT NULL,
    nhanh_dau NVARCHAR(30) NULL,
    ma_tran_tiep_theo_thang INT NULL,
    ma_tran_tiep_theo_thua INT NULL,
    id_phong_game NVARCHAR(50) NULL,
    mat_khau_phong NVARCHAR(50) NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_TD_TRANGTHAI DEFAULT 'chua_dau',
    CONSTRAINT FK_TD_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_TD_GDO FOREIGN KEY (ma_giai_doan) REFERENCES dbo.GIAI_DOAN(ma_giai_doan),
    CONSTRAINT FK_TD_TRONGTAI FOREIGN KEY (ma_trong_tai) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TD_NEXT_WIN FOREIGN KEY (ma_tran_tiep_theo_thang) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_TD_NEXT_LOSE FOREIGN KEY (ma_tran_tiep_theo_thua) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT CHK_TD_TRANGTHAI CHECK (trang_thai IN ('chua_dau','chuan_bi','san_sang','dang_dau','dang_thi_dau','cho_ket_qua','da_hoan_thanh','huy_bo','bye')),
    CONSTRAINT CHK_TD_THETHUCTRAN CHECK (the_thuc_tran IN ('BO1','BO3','BO5','BO7','SinhTon'))
);
GO
CREATE TABLE dbo.CHI_TIET_TRAN_DAU (
    ma_tran INT NOT NULL,
    ma_doi INT NOT NULL,
    diem_so FLOAT NOT NULL CONSTRAINT DF_CTTD_DIEM DEFAULT 0,
    thu_hang INT NULL,
    ket_qua NVARCHAR(10) NULL,
    is_check_in BIT NOT NULL CONSTRAINT DF_CTTD_CHECKIN DEFAULT 0,
    so_kill INT NOT NULL CONSTRAINT DF_CTTD_KILL DEFAULT 0,
    url_anh_bang_chung NVARCHAR(500) NULL,
    CONSTRAINT PK_CTTD PRIMARY KEY (ma_tran, ma_doi),
    CONSTRAINT FK_CTTD_TD FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_CTTD_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT CHK_CTTD_KETQUA CHECK (ket_qua IS NULL OR ket_qua IN ('thang','thua','hoa'))
);
GO
CREATE TABLE dbo.KET_QUA_VAN_DAU (
    ma_tran INT NOT NULL,
    so_van INT NOT NULL,
    ma_doi INT NOT NULL,
    ket_qua NVARCHAR(20) NULL,
    thu_hang INT NULL,
    so_kill INT NOT NULL CONSTRAINT DF_KQVD_KILL DEFAULT 0,
    diem_so FLOAT NOT NULL CONSTRAINT DF_KQVD_DIEM DEFAULT 0,
    ngay_cap_nhat DATETIME NOT NULL CONSTRAINT DF_KQVD_NGAY DEFAULT GETDATE(),
    CONSTRAINT PK_KET_QUA_VAN_DAU PRIMARY KEY (ma_tran, so_van, ma_doi),
    CONSTRAINT FK_KQVD_TD FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_KQVD_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT CHK_KQVD_KETQUA CHECK (ket_qua IS NULL OR ket_qua IN ('thang','thua','hoa'))
);
GO
CREATE TABLE dbo.KET_QUA_TRAN (
    ma_ket_qua INT IDENTITY(1,1) CONSTRAINT PK_KET_QUA_TRAN PRIMARY KEY,
    ma_tran INT NOT NULL CONSTRAINT UQ_KQT_TRAN UNIQUE,
    thoi_diem_bao_cao_dau_tien DATETIME NOT NULL CONSTRAINT DF_KQT_THOIGIAN DEFAULT GETDATE(),
    so_lan_chinh_sua INT NOT NULL CONSTRAINT DF_KQT_SOLAN DEFAULT 0,
    thoi_gian_sua_cuoi DATETIME NULL,
    chi_tiet_phu NVARCHAR(MAX) NULL,
    CONSTRAINT FK_KQT_TD FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran)
);
GO
CREATE TABLE dbo.LICH_SU_SUA_KET_QUA (
    ma_log INT IDENTITY(1,1) CONSTRAINT PK_LICH_SU_SUA_KET_QUA PRIMARY KEY,
    ma_tran INT NOT NULL,
    nguoi_sua INT NULL,
    thoi_gian_sua DATETIME NOT NULL CONSTRAINT DF_LSSKQ_TG DEFAULT GETDATE(),
    du_lieu_cu NVARCHAR(MAX) NULL,
    du_lieu_moi NVARCHAR(MAX) NULL,
    ly_do_sua NVARCHAR(MAX) NULL,
    CONSTRAINT FK_LSSKQ_TD FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_LSSKQ_NGUOISUA FOREIGN KEY (nguoi_sua) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung)
);
GO
CREATE OR ALTER TRIGGER dbo.TRG_LSSKQ_IMMUTABLE ON dbo.LICH_SU_SUA_KET_QUA INSTEAD OF UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR(N'LICH_SU_SUA_KET_QUA la audit log bat bien. Khong duoc phep UPDATE/DELETE.',16,1);
END;
GO
CREATE TABLE dbo.KHIEU_NAI_KET_QUA (
    ma_khieu_nai INT IDENTITY(1,1) CONSTRAINT PK_KHIEU_NAI_KET_QUA PRIMARY KEY,
    ma_tran INT NOT NULL,
    ma_doi INT NOT NULL,
    ma_nguoi_gui INT NOT NULL,
    noi_dung NVARCHAR(MAX) NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_KN_TRANGTHAI DEFAULT 'cho_xu_ly',
    ma_admin_xu_ly INT NULL,
    phan_hoi_admin NVARCHAR(MAX) NULL,
    thoi_gian_tao DATETIME NOT NULL CONSTRAINT DF_KN_THOIGIANTAO DEFAULT GETDATE(),
    thoi_gian_xu_ly DATETIME NULL,
    CONSTRAINT FK_KN_TD FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_KN_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT FK_KN_NGUOIGUI FOREIGN KEY (ma_nguoi_gui) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_KN_ADMIN FOREIGN KEY (ma_admin_xu_ly) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_KN_TRANGTHAI CHECK (trang_thai IN ('cho_xu_ly','da_xu_ly','tu_choi'))
);
GO
CREATE TABLE dbo.YEU_CAU_MO_KHOA_KET_QUA (
    ma_yeu_cau INT IDENTITY(1,1) CONSTRAINT PK_YEU_CAU_MO_KHOA_KET_QUA PRIMARY KEY,
    ma_tran INT NOT NULL,
    ma_trong_tai_yeu_cau INT NOT NULL,
    ly_do_yeu_cau NVARCHAR(1000) NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_YCMK_TRANGTHAI DEFAULT 'cho_duyet',
    phan_hoi_admin NVARCHAR(1000) NULL,
    ma_admin_xu_ly INT NULL,
    thoi_gian_tao DATETIME NOT NULL CONSTRAINT DF_YCMK_TAO DEFAULT GETDATE(),
    thoi_gian_xu_ly DATETIME NULL,
    thoi_gian_mo_khoa_den DATETIME NULL,
    CONSTRAINT FK_YCMK_TRAN_DAU FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_YCMK_TRONG_TAI FOREIGN KEY (ma_trong_tai_yeu_cau) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCMK_ADMIN_XULY FOREIGN KEY (ma_admin_xu_ly) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_YCMK_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','da_duyet','tu_choi'))
);
GO

-- ================================================================
-- 7. LINEUP, STATS, NOTIFICATION, VIEWS, INDEXES
-- ================================================================
CREATE TABLE dbo.CHI_TIET_NGUOI_CHOI_TRAN (
    ma_chi_tiet INT IDENTITY(1,1) CONSTRAINT PK_CHI_TIET_NGUOI_CHOI_TRAN PRIMARY KEY,
    ma_tran INT NOT NULL,
    ma_nguoi_dung INT NOT NULL,
    ma_vi_tri INT NULL,
    so_kill INT NOT NULL CONSTRAINT DF_CTUNCT_KILL DEFAULT 0,
    so_death INT NOT NULL CONSTRAINT DF_CTUNCT_DEATH DEFAULT 0,
    so_assist INT NOT NULL CONSTRAINT DF_CTUNCT_ASSIST DEFAULT 0,
    diem_kda_tran FLOAT NULL,
    diem_sinh_ton FLOAT NULL,
    is_mvp_tran BIT NOT NULL CONSTRAINT DF_CTUNCT_MVP DEFAULT 0,
    CONSTRAINT FK_CTUNCT_TD FOREIGN KEY (ma_tran) REFERENCES dbo.TRAN_DAU(ma_tran),
    CONSTRAINT FK_CTUNCT_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_CTUNCT_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES dbo.DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT UQ_CTUNCT_TRAN_PLAYER UNIQUE (ma_tran, ma_nguoi_dung)
);
GO
CREATE TABLE dbo.BANG_XEP_HANG (
    ma_bxh INT IDENTITY(1,1) CONSTRAINT PK_BANG_XEP_HANG PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    ma_giai_doan INT NULL,
    ma_doi INT NOT NULL,
    so_tran_da_dau INT NOT NULL CONSTRAINT DF_BXH_SOTRAN DEFAULT 0,
    so_tran_thang INT NOT NULL CONSTRAINT DF_BXH_THANG DEFAULT 0,
    so_tran_thua INT NOT NULL CONSTRAINT DF_BXH_THUA DEFAULT 0,
    hieu_so_phu INT NOT NULL CONSTRAINT DF_BXH_HIEUSOPP DEFAULT 0,
    tong_diem_hang FLOAT NOT NULL CONSTRAINT DF_BXH_DIEMHANG DEFAULT 0,
    tong_diem_kill FLOAT NOT NULL CONSTRAINT DF_BXH_DIEMKILL DEFAULT 0,
    so_lan_top_1 INT NOT NULL CONSTRAINT DF_BXH_TOP1 DEFAULT 0,
    diem_tong_ket FLOAT NOT NULL CONSTRAINT DF_BXH_TONGKET DEFAULT 0,
    thu_hang_hien_tai INT NOT NULL CONSTRAINT DF_BXH_THURANG DEFAULT 0,
    is_match_point BIT NOT NULL CONSTRAINT DF_BXH_MATCHPOINT DEFAULT 0,
    CONSTRAINT FK_BXH_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_BXH_GDO FOREIGN KEY (ma_giai_doan) REFERENCES dbo.GIAI_DOAN(ma_giai_doan),
    CONSTRAINT FK_BXH_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi),
    CONSTRAINT UQ_BXH_GD_GDO_DOI UNIQUE (ma_giai_dau, ma_giai_doan, ma_doi)
);
GO
CREATE TABLE dbo.BANG_XEP_HANG_CA_NHAN (
    ma_bxh_cn INT IDENTITY(1,1) CONSTRAINT PK_BANG_XEP_HANG_CA_NHAN PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    ma_nguoi_dung INT NOT NULL,
    tong_kill INT NOT NULL CONSTRAINT DF_BXHCN_KILL DEFAULT 0,
    tong_death INT NOT NULL CONSTRAINT DF_BXHCN_DEATH DEFAULT 0,
    tong_assist INT NOT NULL CONSTRAINT DF_BXHCN_ASSIST DEFAULT 0,
    diem_kda_trung_binh FLOAT NOT NULL CONSTRAINT DF_BXHCN_KDA DEFAULT 0,
    so_lan_dat_mvp_tran INT NOT NULL CONSTRAINT DF_BXHCN_MVP DEFAULT 0,
    CONSTRAINT FK_BXHCN_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_BXHCN_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_BXHCN UNIQUE (ma_giai_dau, ma_nguoi_dung)
);
GO
CREATE TABLE dbo.THONG_BAO (
    ma_thong_bao INT IDENTITY(1,1) CONSTRAINT PK_THONG_BAO PRIMARY KEY,
    ma_nguoi_nhan INT NOT NULL,
    tieu_de NVARCHAR(200) NOT NULL,
    noi_dung NVARCHAR(MAX) NULL,
    loai_thong_bao NVARCHAR(50) NULL,
    loai_entity NVARCHAR(50) NULL,
    ma_entity INT NULL,
    ma_doi INT NULL,
    hanh_dong NVARCHAR(50) NULL,
    da_doc BIT NOT NULL CONSTRAINT DF_TB_DADOC DEFAULT 0,
    ngay_tao DATETIME NOT NULL CONSTRAINT DF_TB_NGAYTAO DEFAULT GETDATE(),
    CONSTRAINT FK_TB_ND FOREIGN KEY (ma_nguoi_nhan) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TB_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi)
);
GO
CREATE TABLE dbo.TUONG_TAC_GIAI_DAU (
    ma_tuong_tac INT IDENTITY(1,1) CONSTRAINT PK_TUONG_TAC_GIAI_DAU PRIMARY KEY,
    ma_nguoi_dung INT NOT NULL,
    ma_giai_dau INT NOT NULL,
    da_like BIT NOT NULL CONSTRAINT DF_TTGD_LIKE DEFAULT 0,
    dang_theo_doi BIT NOT NULL CONSTRAINT DF_TTGD_FOLLOW DEFAULT 0,
    thoi_gian_tao DATETIME NOT NULL CONSTRAINT DF_TTGD_THOIGIAN DEFAULT GETDATE(),
    CONSTRAINT FK_TTGD_ND FOREIGN KEY (ma_nguoi_dung) REFERENCES dbo.NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TTGD_GD FOREIGN KEY (ma_giai_dau) REFERENCES dbo.GIAI_DAU(ma_giai_dau),
    CONSTRAINT UQ_TTGD UNIQUE (ma_nguoi_dung, ma_giai_dau)
);
GO

CREATE VIEW dbo.VW_DASHBOARD_STATS AS
SELECT
    (SELECT COUNT(1) FROM dbo.NGUOI_DUNG WHERE is_banned=0) AS tong_user_active,
    (SELECT COUNT(1) FROM dbo.NGUOI_DUNG WHERE is_banned=1) AS tong_user_bi_ban,
    (SELECT COUNT(1) FROM dbo.GIAI_DAU WHERE trang_thai='dang_dien_ra' AND is_deleted=0) AS giai_dang_chay,
    (SELECT COUNT(1) FROM dbo.GIAI_DAU WHERE trang_thai IN ('sap_dien_ra','mo_dang_ky','khoa_dang_ky','dang_dien_ra') AND is_deleted=0) AS giai_dang_hoat_dong,
    (SELECT COUNT(1) FROM dbo.DOI WHERE trang_thai='dang_hoat_dong') AS tong_doi_hoat_dong,
    (SELECT COUNT(1) FROM dbo.KHIEU_NAI_KET_QUA WHERE trang_thai='cho_xu_ly') AS khieu_nai_cho_xu_ly,
    (SELECT COUNT(1) FROM dbo.GIAI_DAU WHERE trang_thai='cho_xet_duyet' AND is_deleted=0) AS giai_cho_duyet,
    (SELECT COUNT(1) FROM dbo.DANH_MUC_TRO_CHOI WHERE is_active=1) AS tong_game_active;
GO
CREATE VIEW dbo.VW_TUONG_TAC_TONG_HOP AS
SELECT ma_giai_dau, SUM(CAST(da_like AS INT)) AS tong_like, SUM(CAST(dang_theo_doi AS INT)) AS tong_theo_doi
FROM dbo.TUONG_TAC_GIAI_DAU
GROUP BY ma_giai_dau;
GO

CREATE INDEX IX_HSG_USER_GAME ON dbo.HO_SO_IN_GAME(ma_nguoi_dung, ma_tro_choi);
CREATE INDEX IX_DOI_GAME_STATUS ON dbo.DOI(ma_tro_choi, trang_thai) INCLUDE (ten_doi, logo_url);
CREATE INDEX IX_TV_DOI_TRANGTHAI ON dbo.THANH_VIEN_DOI(ma_doi, trang_thai_duyet, trang_thai_hop_dong) INCLUDE (ma_nguoi_dung, vai_tro_noi_bo);
CREATE INDEX IX_YCXNLM_NguoiGui ON dbo.YEU_CAU_XAC_NHAN_LOI_MOI(ma_nguoi_gui);
CREATE INDEX IX_YCXNLM_Doi ON dbo.YEU_CAU_XAC_NHAN_LOI_MOI(ma_doi);
CREATE INDEX IX_YCXNLM_NguoiNhan ON dbo.YEU_CAU_XAC_NHAN_LOI_MOI(ma_nguoi_nhan);
CREATE INDEX IX_YCXNLM_TrangThai ON dbo.YEU_CAU_XAC_NHAN_LOI_MOI(trang_thai);
CREATE INDEX IX_GD_TRANGTHAI_PUBLIC ON dbo.GIAI_DAU(trang_thai, is_deleted, hien_thi_public) INCLUDE (ten_giai_dau, ma_tro_choi);
CREATE INDEX IX_TGG_GD_DUYET ON dbo.THAM_GIA_GIAI(ma_giai_dau, trang_thai_duyet) INCLUDE (ma_doi, trang_thai_tham_gia);
CREATE INDEX IX_TD_GD_GDO_TRANGTHAI ON dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, trang_thai) INCLUDE (the_thuc_tran, so_vong);
CREATE INDEX IX_CTTD_DOI ON dbo.CHI_TIET_TRAN_DAU(ma_doi, ma_tran);
CREATE INDEX IX_KQVD_TRAN_VAN ON dbo.KET_QUA_VAN_DAU(ma_tran, so_van);
CREATE INDEX IX_CTUNCT_TRAN ON dbo.CHI_TIET_NGUOI_CHOI_TRAN(ma_tran) INCLUDE (ma_nguoi_dung, so_kill, so_death, so_assist, is_mvp_tran);
CREATE INDEX IX_BXH_GD_STAGE ON dbo.BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, diem_tong_ket DESC, hieu_so_phu DESC);
CREATE INDEX IX_BXHCN_GD_KDA ON dbo.BANG_XEP_HANG_CA_NHAN(ma_giai_dau, diem_kda_trung_binh DESC) INCLUDE (ma_nguoi_dung, so_lan_dat_mvp_tran);
CREATE INDEX IX_TB_NGUOINHAN_DADOC ON dbo.THONG_BAO(ma_nguoi_nhan, da_doc) INCLUDE (tieu_de, ngay_tao, loai_thong_bao, loai_entity, ma_entity, ma_doi, hanh_dong);
CREATE INDEX IX_YCMK_TRANGTHAI_TAO ON dbo.YEU_CAU_MO_KHOA_KET_QUA(trang_thai, thoi_gian_tao DESC);
GO
CREATE UNIQUE INDEX UX_KN_PENDING_TRAN_DOI ON dbo.KHIEU_NAI_KET_QUA(ma_tran, ma_doi) WHERE trang_thai='cho_xu_ly';
GO
CREATE UNIQUE INDEX UX_YCMK_PENDING_TRAN ON dbo.YEU_CAU_MO_KHOA_KET_QUA(ma_tran) WHERE trang_thai='cho_duyet';
GO
PRINT N'QuanLy_Esports database initialized successfully.';
GO
