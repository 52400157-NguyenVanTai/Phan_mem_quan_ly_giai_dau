-- ==============================================================
-- QUANLY_ESPORTS — COMPLETE DATABASE INITIALIZATION SCRIPT
-- Version : 3.0 (Clean Install)
-- Engine  : SQL Server 2019+
-- Encoding: UTF-8 / NVARCHAR throughout
-- ==============================================================
-- HOW TO USE:
--   1. Run this script once on a fresh SQL Server instance.
--   2. Do NOT run on an existing QuanLy_Esports database —
--      drop it first if you need a clean reset.
-- ==============================================================

USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'QuanLy_Esports')
BEGIN
    ALTER DATABASE QuanLy_Esports SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLy_Esports;
END
GO

CREATE DATABASE QuanLy_Esports
    COLLATE Vietnamese_CI_AS;   -- Hỗ trợ tiếng Việt có dấu, không phân biệt HOA/thường
GO

USE QuanLy_Esports;
GO

-- ==============================================================
-- SECTION 1 — LOOKUP / MASTER DATA
-- ==============================================================

-- ---------------------------------------------------------------
-- 1.1  TRO_CHOI  (Game catalogue — chỉ 6 tựa game được phép)
-- ---------------------------------------------------------------
CREATE TABLE TRO_CHOI (
    ma_tro_choi  INT          IDENTITY PRIMARY KEY,
    ten_game     NVARCHAR(100) NOT NULL,
    the_loai     NVARCHAR(50)  NOT NULL,   -- MOBA | FPS | BATTLEROYALE
    is_active    BIT           NOT NULL CONSTRAINT DF_TC_ACTIVE   DEFAULT 1,

    CONSTRAINT UQ_TC_TEN_GAME CHECK (ten_game IN (
        N'Arena of Valor',
        N'League of Legends',
        N'Free Fire',
        N'PUBG',
        N'Valorant',
        N'CS:GO'
    )),
    CONSTRAINT CHK_TC_THE_LOAI CHECK (the_loai IN ('MOBA','FPS','BATTLEROYALE'))
);
GO

-- ---------------------------------------------------------------
-- 1.2  DANH_MUC_VI_TRI  (Position master data)
--
-- loai_vi_tri = 'ThuDau'   → vị trí thi đấu theo game
-- loai_vi_tri = 'HuanLuyen'→ HLV, Chiến thuật, Trợ lí, Quản lý
--               không ràng buộc theo game (ma_tro_choi = NULL)
-- ---------------------------------------------------------------
CREATE TABLE DANH_MUC_VI_TRI (
    ma_vi_tri    INT          IDENTITY PRIMARY KEY,
    ma_tro_choi  INT          NULL,          -- NULL → áp dụng cho ban huấn luyện
    ten_vi_tri   NVARCHAR(100) NOT NULL,
    ky_hieu      NVARCHAR(20)  NOT NULL,
    loai_vi_tri  NVARCHAR(20)  NOT NULL,

    CONSTRAINT CHK_VITRI_LOAI CHECK (loai_vi_tri IN ('ThuDau','HuanLuyen')),
    CONSTRAINT FK_VITRI_GAME  FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi)
);
GO

-- ==============================================================
-- SECTION 2 — IDENTITY & USER
-- ==============================================================

-- ---------------------------------------------------------------
-- 2.1  NGUOI_DUNG
-- ---------------------------------------------------------------
CREATE TABLE NGUOI_DUNG (
    ma_nguoi_dung     INT           IDENTITY PRIMARY KEY,
    ten_dang_nhap     NVARCHAR(100) NOT NULL,
    email             NVARCHAR(150) NOT NULL,
    mat_khau_ma_hoa   NVARCHAR(255) NOT NULL,
    vai_tro_he_thong  NVARCHAR(10)  NOT NULL CONSTRAINT DF_ND_VAITRO  DEFAULT 'user',
    avatar_url        NVARCHAR(400) NULL,
    bio               NVARCHAR(500) NULL,
    is_banned         BIT           NOT NULL CONSTRAINT DF_ND_BANNED  DEFAULT 0,
    ly_do_ban         NVARCHAR(500) NULL,
    thoi_gian_ban     DATETIME      NULL,
    ma_admin_ban      INT           NULL,
    ngay_tao          DATETIME      NOT NULL CONSTRAINT DF_ND_NGAYTAO DEFAULT GETDATE(),

    CONSTRAINT UQ_ND_USERNAME CHECK (ten_dang_nhap IS NOT NULL), -- NOT NULL enforced here
    CONSTRAINT UQ_ND_EMAIL    UNIQUE (email),
    CONSTRAINT UQ_ND_TENDANGNHAP UNIQUE (ten_dang_nhap),
    CONSTRAINT CHK_ND_VAITRO  CHECK (vai_tro_he_thong IN ('admin','user')),
    CONSTRAINT FK_ND_ADMIN_BAN FOREIGN KEY (ma_admin_ban) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);
GO

-- ---------------------------------------------------------------
-- 2.2  HO_SO_IN_GAME  (1 user × 1 game = 1 profile)
--
-- Ban huấn luyện: in_game_id / in_game_name có thể NULL
-- ---------------------------------------------------------------
CREATE TABLE HO_SO_IN_GAME (
    ma_ho_so             INT           IDENTITY PRIMARY KEY,
    ma_nguoi_dung        INT           NOT NULL,
    ma_tro_choi          INT           NOT NULL,
    in_game_id           NVARCHAR(100) NULL,   -- Optional cho BanHuanLuyen
    in_game_name         NVARCHAR(100) NULL,   -- Optional cho BanHuanLuyen
    ma_vi_tri_so_truong  INT           NULL,   -- Vị trí sở trường
    ngay_cap_nhat        DATETIME      NOT NULL CONSTRAINT DF_HSG_UPDATE DEFAULT GETDATE(),

    CONSTRAINT UQ_HSG_PROFILE  UNIQUE (ma_nguoi_dung, ma_tro_choi),
    CONSTRAINT FK_HSG_ND       FOREIGN KEY (ma_nguoi_dung)       REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_HSG_TC       FOREIGN KEY (ma_tro_choi)         REFERENCES TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_HSG_VITRI    FOREIGN KEY (ma_vi_tri_so_truong) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri)
);
GO

-- ==============================================================
-- SECTION 3 — TEAM & SQUAD
-- ==============================================================

-- ---------------------------------------------------------------
-- 3.1  DOI  (Organisation / Club)
-- ---------------------------------------------------------------
CREATE TABLE DOI (
    ma_doi      INT           IDENTITY PRIMARY KEY,
    ten_doi     NVARCHAR(150) NOT NULL,
    ma_doi_truong INT         NOT NULL,   -- Người tạo / chủ đội
    ma_manager  INT           NULL,       -- Manager (có thể khác chủ đội)
    logo_url    NVARCHAR(400) NULL,
    slogan      NVARCHAR(300) NULL,
    mo_ta       NVARCHAR(500) NULL,
    trang_thai  NVARCHAR(30)  NOT NULL CONSTRAINT DF_DOI_TRANGTHAI DEFAULT 'dang_hoat_dong',
    dang_tuyen  BIT           NOT NULL CONSTRAINT DF_DOI_DANGTUYEN DEFAULT 0,
    ngay_tao    DATETIME      NOT NULL CONSTRAINT DF_DOI_NGAYTAO   DEFAULT GETDATE(),

    CONSTRAINT UQ_DOI_TEN      UNIQUE (ten_doi),
    CONSTRAINT CHK_DOI_TRANGTHAI CHECK (trang_thai IN ('dang_hoat_dong','tam_dung','da_giai_the')),
    CONSTRAINT FK_DOI_DOITRUONG FOREIGN KEY (ma_doi_truong) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_DOI_MANAGER  FOREIGN KEY (ma_manager)    REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);
GO

-- ---------------------------------------------------------------
-- 3.2  NHOM_DOI  (Squad per game under a club)
-- ---------------------------------------------------------------
CREATE TABLE NHOM_DOI (
    ma_nhom           INT           IDENTITY PRIMARY KEY,
    ma_doi            INT           NOT NULL,
    ma_tro_choi       INT           NOT NULL,
    ten_nhom          NVARCHAR(150) NOT NULL,
    ma_doi_truong_nhom INT          NULL,   -- Squad captain

    CONSTRAINT FK_NHOM_DOI    FOREIGN KEY (ma_doi)             REFERENCES DOI(ma_doi),
    CONSTRAINT FK_NHOM_TC     FOREIGN KEY (ma_tro_choi)        REFERENCES TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_NHOM_CAP    FOREIGN KEY (ma_doi_truong_nhom) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_NHOM_DOI_GAME_TEN UNIQUE (ma_doi, ma_tro_choi, ten_nhom)
);
GO

-- ---------------------------------------------------------------
-- 3.3  THANH_VIEN_DOI  (Squad membership)
-- ---------------------------------------------------------------
CREATE TABLE THANH_VIEN_DOI (
    ma_thanh_vien      INT          IDENTITY PRIMARY KEY,
    ma_nguoi_dung      INT          NOT NULL,
    ma_nhom            INT          NOT NULL,
    ma_vi_tri          INT          NULL,
    vai_tro_noi_bo     NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_VAITRO    DEFAULT 'member',
    phan_he            NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_PHANHE    DEFAULT 'thi_dau',
    trang_thai_duyet   NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_DUYET     DEFAULT 'cho_duyet',
    trang_thai_hop_dong NVARCHAR(20) NOT NULL CONSTRAINT DF_TV_HOPDONG  DEFAULT 'dang_hieu_luc',
    ngay_tham_gia      DATETIME     NOT NULL CONSTRAINT DF_TV_NGAYTG    DEFAULT GETDATE(),

    CONSTRAINT FK_TV_ND       FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TV_NHOM     FOREIGN KEY (ma_nhom)       REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT FK_TV_VITRI    FOREIGN KEY (ma_vi_tri)     REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT CHK_TV_VAITRO  CHECK (vai_tro_noi_bo     IN ('leader','coach','captain','member')),
    CONSTRAINT CHK_TV_PHANHE  CHECK (phan_he             IN ('thi_dau','ban_huan_luyen')),
    CONSTRAINT CHK_TV_DUYET   CHECK (trang_thai_duyet    IN ('cho_duyet','da_duyet','bi_tu_choi')),
    CONSTRAINT CHK_TV_HOPDONG CHECK (trang_thai_hop_dong IN ('dang_hieu_luc','tu_do','da_giai_phong'))
);
GO

-- ==============================================================
-- SECTION 4 — RECRUITMENT
-- ==============================================================

-- ---------------------------------------------------------------
-- 4.1  BAI_DANG_TUYEN_DUNG  (Job posting by squad)
-- ---------------------------------------------------------------
CREATE TABLE BAI_DANG_TUYEN_DUNG (
    ma_bai_dang INT           IDENTITY PRIMARY KEY,
    ma_doi      INT           NOT NULL,
    ma_nhom     INT           NOT NULL,
    ma_vi_tri   INT           NOT NULL,
    noi_dung    NVARCHAR(500) NOT NULL,
    trang_thai  NVARCHAR(20)  NOT NULL CONSTRAINT DF_BD_TRANGTHAI DEFAULT 'dang_mo',
    ngay_tao    DATETIME      NOT NULL CONSTRAINT DF_BD_NGAYTAO   DEFAULT GETDATE(),

    CONSTRAINT FK_BD_DOI    FOREIGN KEY (ma_doi)    REFERENCES DOI(ma_doi),
    CONSTRAINT FK_BD_NHOM   FOREIGN KEY (ma_nhom)   REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT FK_BD_VITRI  FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT CHK_BD_TRANGTHAI CHECK (trang_thai IN ('dang_mo','tam_dong','da_dong'))
);
GO

-- ---------------------------------------------------------------
-- 4.2  DON_UNG_TUYEN  (Application to a job posting)
-- ---------------------------------------------------------------
CREATE TABLE DON_UNG_TUYEN (
    ma_don      INT          IDENTITY PRIMARY KEY,
    ma_bai_dang INT          NOT NULL,
    ma_ung_vien INT          NOT NULL,
    trang_thai  NVARCHAR(20) NOT NULL CONSTRAINT DF_DUT_TRANGTHAI DEFAULT 'cho_duyet',
    ngay_tao    DATETIME     NOT NULL CONSTRAINT DF_DUT_NGAYTAO   DEFAULT GETDATE(),

    CONSTRAINT FK_DUT_BAIDANG  FOREIGN KEY (ma_bai_dang) REFERENCES BAI_DANG_TUYEN_DUNG(ma_bai_dang),
    CONSTRAINT FK_DUT_UNGVIEN  FOREIGN KEY (ma_ung_vien) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_DUT_UNIQUE   UNIQUE (ma_bai_dang, ma_ung_vien),
    CONSTRAINT CHK_DUT_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','chap_nhan','tu_choi'))
);
GO

-- ---------------------------------------------------------------
-- 4.3  LOI_MOI_GIA_NHAP  (Invitation from squad to player)
-- ---------------------------------------------------------------
CREATE TABLE LOI_MOI_GIA_NHAP (
    ma_loi_moi        INT          IDENTITY PRIMARY KEY,
    ma_doi            INT          NOT NULL,
    ma_nhom           INT          NOT NULL,
    ma_nguoi_duoc_moi INT          NOT NULL,
    ma_nguoi_gui      INT          NULL,    -- Who sent the invite (leader/captain)
    trang_thai        NVARCHAR(20) NOT NULL CONSTRAINT DF_LM_TRANGTHAI DEFAULT 'cho_phan_hoi',
    ngay_tao          DATETIME     NOT NULL CONSTRAINT DF_LM_NGAYTAO   DEFAULT GETDATE(),

    CONSTRAINT FK_LM_DOI         FOREIGN KEY (ma_doi)            REFERENCES DOI(ma_doi),
    CONSTRAINT FK_LM_NHOM        FOREIGN KEY (ma_nhom)           REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT FK_LM_NGUOIDUOCMOI FOREIGN KEY (ma_nguoi_duoc_moi) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_LM_NGUOIGUI    FOREIGN KEY (ma_nguoi_gui)      REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_LM_NHOM_USER   UNIQUE (ma_nhom, ma_nguoi_duoc_moi),
    CONSTRAINT CHK_LM_TRANGTHAI  CHECK (trang_thai IN ('cho_phan_hoi','chap_nhan','tu_choi'))
);
GO

-- ---------------------------------------------------------------
-- 4.4  XIN_GIA_NHAP  (Player-initiated join request)
-- ---------------------------------------------------------------
CREATE TABLE XIN_GIA_NHAP (
    ma_don_xin    INT          IDENTITY PRIMARY KEY,
    ma_nguoi_dung INT          NOT NULL,
    ma_nhom       INT          NOT NULL,
    ma_ho_so      INT          NULL,    -- Profile in-game đính kèm
    trang_thai    NVARCHAR(20) NOT NULL CONSTRAINT DF_XGN_TRANGTHAI DEFAULT 'cho_duyet',
    ngay_tao      DATETIME     NOT NULL CONSTRAINT DF_XGN_NGAYTAO   DEFAULT GETDATE(),

    CONSTRAINT FK_XGN_ND      FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_XGN_NHOM    FOREIGN KEY (ma_nhom)       REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT FK_XGN_HOSO    FOREIGN KEY (ma_ho_so)      REFERENCES HO_SO_IN_GAME(ma_ho_so),
    CONSTRAINT UQ_XGN_UNIQUE  UNIQUE (ma_nhom, ma_nguoi_dung),
    CONSTRAINT CHK_XGN_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','chap_nhan','tu_choi'))
);
GO

-- ==============================================================
-- SECTION 5 — TOURNAMENT
-- ==============================================================

-- ---------------------------------------------------------------
-- 5.1  GIAI_DAU  (Tournament)
-- ---------------------------------------------------------------
CREATE TABLE GIAI_DAU (
    ma_giai_dau            INT            IDENTITY PRIMARY KEY,
    ten_giai_dau           NVARCHAR(150)  NOT NULL,
    ma_tro_choi            INT            NULL,    -- NULL = giải hỗn hợp
    ma_nguoi_tao           INT            NULL,
    the_thuc               NVARCHAR(50)   NOT NULL,
    banner_url             NVARCHAR(400)  NULL,
    ngay_bat_dau           DATETIME       NULL,
    ngay_ket_thuc          DATETIME       NULL,
    thoi_gian_mo_dang_ky   DATETIME       NULL,
    thoi_gian_dong_dang_ky DATETIME       NULL,
    tong_giai_thuong       DECIMAL(15,2)  NOT NULL CONSTRAINT DF_GD_GIAILTHUONG DEFAULT 0,
    trang_thai             NVARCHAR(30)   NOT NULL CONSTRAINT DF_GD_TRANGTHAI   DEFAULT 'ban_nhap',
    hien_thi_public        BIT            NOT NULL CONSTRAINT DF_GD_PUBLIC      DEFAULT 1,
    is_deleted             BIT            NOT NULL CONSTRAINT DF_GD_DELETED     DEFAULT 0,
    thoi_gian_khoa         DATETIME       NULL,
    ma_nguoi_khoa          INT            NULL,
    ly_do_khoa             NVARCHAR(500)  NULL,

    CONSTRAINT FK_GD_TC        FOREIGN KEY (ma_tro_choi)   REFERENCES TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_GD_NGUOITAO  FOREIGN KEY (ma_nguoi_tao)  REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_GD_NGUOIKHOA FOREIGN KEY (ma_nguoi_khoa) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_GD_THETHUC  CHECK (the_thuc IN (
        'loai_truc_tiep','nhanh_thang_nhanh_thua',
        'dau_theo_bang','vong_tron_tinh_diem','hon_hop'
    )),
    CONSTRAINT CHK_GD_TRANGTHAI CHECK (trang_thai IN (
        'ban_nhap','cho_phe_duyet','mo_dang_ky',
        'sap_dien_ra','dang_dien_ra','ket_thuc','khoa'
    )),
    CONSTRAINT CHK_GD_THOIGIAN_DANGKY CHECK (
        thoi_gian_dong_dang_ky IS NULL
        OR ngay_bat_dau IS NULL
        OR thoi_gian_dong_dang_ky < ngay_bat_dau
    )
);
GO

-- ---------------------------------------------------------------
-- 5.2  QUAN_TRI_GIAI_DAU  (BTC / Referee assignment per tournament)
-- ---------------------------------------------------------------
CREATE TABLE QUAN_TRI_GIAI_DAU (
    ma_giai_dau  INT         NOT NULL,
    ma_nguoi_dung INT        NOT NULL,
    vai_tro_giai NVARCHAR(20) NOT NULL,

    CONSTRAINT PK_QTGD        PRIMARY KEY (ma_giai_dau, ma_nguoi_dung),
    CONSTRAINT FK_QTGD_GD     FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_QTGD_ND     FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_QTGD_VAITRO CHECK (vai_tro_giai IN ('ban_to_chuc','trong_tai'))
);
GO

-- ---------------------------------------------------------------
-- 5.3  YEU_CAU_TAO_GIAI_DAU  (Tournament creation request)
-- ---------------------------------------------------------------
CREATE TABLE YEU_CAU_TAO_GIAI_DAU (
    ma_yeu_cau      INT            IDENTITY PRIMARY KEY,
    ma_nguoi_gui    INT            NOT NULL,
    ten_giai_dau    NVARCHAR(150)  NOT NULL,
    ma_tro_choi     INT            NULL,
    the_thuc        NVARCHAR(50)   NOT NULL,
    ngay_bat_dau    DATETIME       NOT NULL,
    ngay_ket_thuc   DATETIME       NOT NULL,
    tong_giai_thuong DECIMAL(15,2) NOT NULL,
    trang_thai      NVARCHAR(20)   NOT NULL CONSTRAINT DF_YCTGD_TRANGTHAI DEFAULT 'cho_duyet',
    ma_admin_duyet  INT            NULL,
    ly_do_huy       NVARCHAR(500)  NULL,
    thoi_gian_gui   DATETIME       NOT NULL CONSTRAINT DF_YCTGD_THOIGIANGUI DEFAULT GETDATE(),
    thoi_gian_duyet DATETIME       NULL,

    CONSTRAINT FK_YCTGD_NGUOIGUI FOREIGN KEY (ma_nguoi_gui)   REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_YCTGD_TC       FOREIGN KEY (ma_tro_choi)    REFERENCES TRO_CHOI(ma_tro_choi),
    CONSTRAINT FK_YCTGD_ADMIN    FOREIGN KEY (ma_admin_duyet) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_YCTGD_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','da_duyet','tu_choi'))
);
GO

-- ---------------------------------------------------------------
-- 5.4  THAM_GIA_GIAI  (Squad registration to a tournament)
-- ---------------------------------------------------------------
CREATE TABLE THAM_GIA_GIAI (
    ma_tham_gia        INT          IDENTITY PRIMARY KEY,
    ma_giai_dau        INT          NOT NULL,
    ma_nhom            INT          NOT NULL,
    trang_thai_duyet   NVARCHAR(20) NOT NULL CONSTRAINT DF_TGG_DUYET    DEFAULT 'cho_duyet',
    trang_thai_tham_gia NVARCHAR(20) NOT NULL CONSTRAINT DF_TGG_THAMGIA DEFAULT 'dang_thi_dau',
    hat_giong          INT          NULL,

    CONSTRAINT FK_TGG_GD     FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_TGG_NHOM   FOREIGN KEY (ma_nhom)     REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT UQ_TGG_GD_NHOM UNIQUE (ma_giai_dau, ma_nhom),
    CONSTRAINT CHK_TGG_DUYET   CHECK (trang_thai_duyet    IN ('cho_duyet','da_duyet','bi_tu_choi')),
    CONSTRAINT CHK_TGG_THAMGIA CHECK (trang_thai_tham_gia IN ('dang_thi_dau','di_tiep','bi_loai'))
);
GO

-- ---------------------------------------------------------------
-- 5.5  DOI_HINH_THI_DAU  (Tournament roster)
-- ---------------------------------------------------------------
CREATE TABLE DOI_HINH_THI_DAU (
    ma_doi_hinh  INT  IDENTITY PRIMARY KEY,
    ma_tham_gia  INT  NOT NULL,
    ma_giai_dau  INT  NOT NULL,   -- Denormalised for quick UNIQUE check
    ma_nguoi_dung INT NOT NULL,
    ma_vi_tri    INT  NULL,
    is_du_bi     BIT  NOT NULL CONSTRAINT DF_DHTS_DUBI DEFAULT 0,

    CONSTRAINT FK_DHTS_THAMGIA  FOREIGN KEY (ma_tham_gia)  REFERENCES THAM_GIA_GIAI(ma_tham_gia),
    CONSTRAINT FK_DHTS_GD       FOREIGN KEY (ma_giai_dau)  REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_DHTS_ND       FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_DHTS_VITRI    FOREIGN KEY (ma_vi_tri)    REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
    -- One player cannot be in two rosters of the same tournament
    CONSTRAINT UQ_DHTS_GD_PLAYER UNIQUE (ma_giai_dau, ma_nguoi_dung)
);
GO

-- ---------------------------------------------------------------
-- 5.6  GIAI_THUONG  (Prize structure)
-- ---------------------------------------------------------------
CREATE TABLE GIAI_THUONG (
    ma_giai_thuong INT           IDENTITY PRIMARY KEY,
    ma_giai_dau    INT           NOT NULL,
    vi_tri_top     INT           NOT NULL,
    so_tien        DECIMAL(15,2) NOT NULL,

    CONSTRAINT FK_GT_GD  FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT CHK_GT_VITRI CHECK (vi_tri_top > 0),
    CONSTRAINT CHK_GT_SOTIEN CHECK (so_tien >= 0)
);
GO

-- ==============================================================
-- SECTION 6 — TOURNAMENT STAGE & BRACKET
-- ==============================================================

-- ---------------------------------------------------------------
-- 6.1  GIAI_DOAN  (Stage / Phase of a tournament)
-- ---------------------------------------------------------------
CREATE TABLE GIAI_DOAN (
    ma_giai_doan          INT           IDENTITY PRIMARY KEY,
    ma_giai_dau           INT           NOT NULL,
    ten_giai_doan         NVARCHAR(100) NOT NULL,
    the_thuc              NVARCHAR(50)  NOT NULL,
    thu_tu                INT           NOT NULL,
    so_doi_di_tiep        INT           NOT NULL CONSTRAINT DF_GDO_DOIDITIEP DEFAULT 0,
    diem_nguong_match_point INT          NULL,
    trang_thai            NVARCHAR(30)  NOT NULL CONSTRAINT DF_GDO_TRANGTHAI DEFAULT 'chua_bat_dau',

    CONSTRAINT FK_GDO_GD      FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT CHK_GDO_THETHUC CHECK (the_thuc IN (
        'loai_truc_tiep','nhanh_thang_nhanh_thua',
        'vong_tron','league_bang_cheo','thuy_si','champion_rush'
    )),
    CONSTRAINT CHK_GDO_TRANGTHAI CHECK (trang_thai IN ('chua_bat_dau','dang_dien_ra','ket_thuc')),
    CONSTRAINT CHK_GDO_THUTHU  CHECK (thu_tu > 0),
    CONSTRAINT CHK_GDO_DOIDITIEP CHECK (so_doi_di_tiep >= 0),
    CONSTRAINT CHK_GDO_MATCHPOINT CHECK (diem_nguong_match_point IS NULL OR diem_nguong_match_point > 0),
    CONSTRAINT UQ_GDO_GD_THUTHU UNIQUE (ma_giai_dau, thu_tu)
);
GO

-- ==============================================================
-- SECTION 7 — MATCH & RESULTS
-- ==============================================================

-- ---------------------------------------------------------------
-- 7.1  TRAN_DAU  (Match)
-- ---------------------------------------------------------------
CREATE TABLE TRAN_DAU (
    ma_tran                INT           IDENTITY PRIMARY KEY,
    ma_giai_dau            INT           NOT NULL,
    ma_giai_doan           INT           NULL,
    ma_trong_tai           INT           NULL,
    vong_dau               NVARCHAR(100) NULL,
    the_thuc_tran          NVARCHAR(20)  NOT NULL,
    so_vong                INT           NULL,
    nhanh_dau              NVARCHAR(30)  NULL,    -- winners/losers bracket
    ma_tran_tiep_theo_thang INT          NULL,
    ma_tran_tiep_theo_thua  INT          NULL,
    thoi_gian_bat_dau      DATETIME      NULL,
    thoi_gian_ket_thuc     DATETIME      NULL,
    thoi_gian_nhap_diem    DATETIME      NULL,
    so_lan_sua             INT           NOT NULL CONSTRAINT DF_TD_SOLANSUA DEFAULT 0,
    trang_thai             NVARCHAR(20)  NOT NULL CONSTRAINT DF_TD_TRANGTHAI DEFAULT 'chua_dau',

    CONSTRAINT FK_TD_GD        FOREIGN KEY (ma_giai_dau)             REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_TD_GDO       FOREIGN KEY (ma_giai_doan)            REFERENCES GIAI_DOAN(ma_giai_doan),
    CONSTRAINT FK_TD_TRONGTAL  FOREIGN KEY (ma_trong_tai)            REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TD_NEXT_WIN  FOREIGN KEY (ma_tran_tiep_theo_thang) REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT FK_TD_NEXT_LOSE FOREIGN KEY (ma_tran_tiep_theo_thua)  REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT CHK_TD_TRANGTHAI CHECK (trang_thai IN ('chua_dau','dang_dau','da_hoan_thanh','huy_bo')),
    CONSTRAINT CHK_TD_THETHUCTRAN CHECK (the_thuc_tran IN ('BO1','BO3','BO5','SinhTon'))
);
GO

-- ---------------------------------------------------------------
-- 7.2  CHI_TIET_TRAN_DAU  (Per-team score in a match)
-- ---------------------------------------------------------------
CREATE TABLE CHI_TIET_TRAN_DAU (
    ma_tran   INT          NOT NULL,
    ma_nhom   INT          NOT NULL,
    diem_so   FLOAT        NOT NULL CONSTRAINT DF_CTTD_DIEM DEFAULT 0,
    thu_hang  INT          NULL,     -- Battle Royale placement
    ket_qua   NVARCHAR(10) NULL,     -- 'thang','thua','hoa' (MOBA/FPS)

    CONSTRAINT PK_CTTD     PRIMARY KEY (ma_tran, ma_nhom),
    CONSTRAINT FK_CTTD_TD  FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT FK_CTTD_NHOM FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT CHK_CTTD_KETQUA CHECK (ket_qua IS NULL OR ket_qua IN ('thang','thua','hoa'))
);
GO

-- ---------------------------------------------------------------
-- 7.3  KET_QUA_TRAN  (Match result summary / audit header)
-- ---------------------------------------------------------------
CREATE TABLE KET_QUA_TRAN (
    ma_ket_qua              INT          IDENTITY PRIMARY KEY,
    ma_tran                 INT          NOT NULL,
    thoi_diem_bao_cao_dau_tien DATETIME  NOT NULL CONSTRAINT DF_KQT_THOIGIAN DEFAULT GETDATE(),
    so_lan_chinh_sua        INT          NOT NULL CONSTRAINT DF_KQT_SOLAN    DEFAULT 0,
    thoi_gian_sua_cuoi      DATETIME     NULL,
    chi_tiet_phu            NVARCHAR(MAX) NULL,    -- JSON log

    CONSTRAINT UQ_KQT_TRAN UNIQUE (ma_tran),
    CONSTRAINT FK_KQT_TD   FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran)
);
GO

-- ---------------------------------------------------------------
-- 7.4  LICH_SU_SUA_KET_QUA  (Immutable audit log)
-- ---------------------------------------------------------------
CREATE TABLE LICH_SU_SUA_KET_QUA (
    ma_log      INT           IDENTITY PRIMARY KEY,
    ma_tran     INT           NOT NULL,
    nguoi_sua   INT           NULL,
    thoi_gian_sua DATETIME    NOT NULL CONSTRAINT DF_LSSKQ_TG DEFAULT GETDATE(),
    du_lieu_cu  NVARCHAR(MAX) NULL,
    du_lieu_moi NVARCHAR(MAX) NULL,
    ly_do_sua   NVARCHAR(MAX) NULL,

    CONSTRAINT FK_LSSKQ_TD       FOREIGN KEY (ma_tran)    REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT FK_LSSKQ_NGUOISUA FOREIGN KEY (nguoi_sua)  REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);
GO

-- Audit log: bất biến — không cho UPDATE hoặc DELETE
CREATE TRIGGER TRG_LSSKQ_IMMUTABLE
ON LICH_SU_SUA_KET_QUA
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR(N'LICH_SU_SUA_KET_QUA là audit log bất biến. Không được phép UPDATE/DELETE.', 16, 1);
END;
GO

-- ---------------------------------------------------------------
-- 7.5  KHIEU_NAI_KET_QUA  (Result dispute)
-- ---------------------------------------------------------------
CREATE TABLE KHIEU_NAI_KET_QUA (
    ma_khieu_nai   INT           IDENTITY PRIMARY KEY,
    ma_tran        INT           NOT NULL,
    ma_nhom        INT           NOT NULL,
    ma_nguoi_gui   INT           NOT NULL,
    noi_dung       NVARCHAR(MAX) NOT NULL,
    trang_thai     NVARCHAR(20)  NOT NULL CONSTRAINT DF_KN_TRANGTHAI DEFAULT 'cho_xu_ly',
    ma_admin_xu_ly INT           NULL,
    phan_hoi_admin NVARCHAR(MAX) NULL,
    thoi_gian_tao  DATETIME      NOT NULL CONSTRAINT DF_KN_THOIGIANTAO DEFAULT GETDATE(),
    thoi_gian_xu_ly DATETIME     NULL,

    CONSTRAINT FK_KN_TD          FOREIGN KEY (ma_tran)        REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT FK_KN_NHOM        FOREIGN KEY (ma_nhom)        REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT FK_KN_NGUOIGUI    FOREIGN KEY (ma_nguoi_gui)   REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_KN_ADMIN       FOREIGN KEY (ma_admin_xu_ly) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_KN_TRANGTHAI  CHECK (trang_thai IN ('cho_xu_ly','da_xu_ly','tu_choi'))
);
GO

-- Filtered unique index: chỉ 1 khiếu nại đang chờ xử lý / (trận, đội)
CREATE UNIQUE INDEX UX_KN_PENDING_TRAN_NHOM
ON KHIEU_NAI_KET_QUA(ma_tran, ma_nhom)
WHERE trang_thai = 'cho_xu_ly';
GO

-- ==============================================================
-- SECTION 8 — PLAYER STATS & LEADERBOARD
-- ==============================================================

-- ---------------------------------------------------------------
-- 8.1  CHI_TIET_NGUOI_CHOI_TRAN  (Per-player stats in a match)
-- ---------------------------------------------------------------
CREATE TABLE CHI_TIET_NGUOI_CHOI_TRAN (
    ma_chi_tiet   INT   IDENTITY PRIMARY KEY,
    ma_tran       INT   NOT NULL,
    ma_nguoi_dung INT   NOT NULL,
    ma_vi_tri     INT   NULL,
    so_kill       INT   NOT NULL CONSTRAINT DF_CTUNCT_KILL   DEFAULT 0,
    so_death      INT   NOT NULL CONSTRAINT DF_CTUNCT_DEATH  DEFAULT 0,
    so_assist     INT   NOT NULL CONSTRAINT DF_CTUNCT_ASSIST DEFAULT 0,
    diem_kda_tran FLOAT NULL,     -- (kill + assist) / MAX(1, death)
    diem_sinh_ton FLOAT NULL,     -- Battle Royale survival score
    is_mvp_tran   BIT   NOT NULL CONSTRAINT DF_CTUNCT_MVP    DEFAULT 0,

    CONSTRAINT FK_CTUNCT_TD    FOREIGN KEY (ma_tran)        REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT FK_CTUNCT_ND    FOREIGN KEY (ma_nguoi_dung)  REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_CTUNCT_VITRI FOREIGN KEY (ma_vi_tri)      REFERENCES DANH_MUC_VI_TRI(ma_vi_tri)
);
GO

-- ---------------------------------------------------------------
-- 8.2  BANG_XEP_HANG  (Team leaderboard per stage)
-- ---------------------------------------------------------------
CREATE TABLE BANG_XEP_HANG (
    ma_bxh             INT   IDENTITY PRIMARY KEY,
    ma_giai_dau        INT   NOT NULL,
    ma_giai_doan       INT   NULL,
    ma_nhom            INT   NOT NULL,
    so_tran_da_dau     INT   NOT NULL CONSTRAINT DF_BXH_SOTRAN   DEFAULT 0,
    -- MOBA / FPS
    so_tran_thang      INT   NOT NULL CONSTRAINT DF_BXH_THANG    DEFAULT 0,
    so_tran_thua       INT   NOT NULL CONSTRAINT DF_BXH_THUA     DEFAULT 0,
    hieu_so_phu        INT   NOT NULL CONSTRAINT DF_BXH_HIEUSOPP DEFAULT 0,
    -- Battle Royale
    tong_diem_hang     FLOAT NOT NULL CONSTRAINT DF_BXH_DIEMHANG DEFAULT 0,
    tong_diem_kill     FLOAT NOT NULL CONSTRAINT DF_BXH_DIEMKILL DEFAULT 0,
    so_lan_top_1       INT   NOT NULL CONSTRAINT DF_BXH_TOP1     DEFAULT 0,
    -- Summary
    diem_tong_ket      FLOAT NOT NULL CONSTRAINT DF_BXH_TONGKET  DEFAULT 0,
    thu_hang_hien_tai  INT   NOT NULL CONSTRAINT DF_BXH_THURANG  DEFAULT 0,
    is_match_point     BIT   NOT NULL CONSTRAINT DF_BXH_MATCHPOINT DEFAULT 0,

    CONSTRAINT FK_BXH_GD    FOREIGN KEY (ma_giai_dau)  REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_BXH_GDO   FOREIGN KEY (ma_giai_doan) REFERENCES GIAI_DOAN(ma_giai_doan),
    CONSTRAINT FK_BXH_NHOM  FOREIGN KEY (ma_nhom)      REFERENCES NHOM_DOI(ma_nhom),
    -- One row per squad per stage
    CONSTRAINT UQ_BXH_GIAIDOAN_NHOM UNIQUE (ma_giai_doan, ma_nhom)
);
GO

-- ---------------------------------------------------------------
-- 8.3  BANG_XEP_HANG_CA_NHAN  (Individual leaderboard — MVP)
-- ---------------------------------------------------------------
CREATE TABLE BANG_XEP_HANG_CA_NHAN (
    ma_bxh_cn          INT   IDENTITY PRIMARY KEY,
    ma_giai_dau        INT   NOT NULL,
    ma_nguoi_dung      INT   NOT NULL,
    tong_kill          INT   NOT NULL CONSTRAINT DF_BXHCN_KILL   DEFAULT 0,
    tong_death         INT   NOT NULL CONSTRAINT DF_BXHCN_DEATH  DEFAULT 0,
    tong_assist        INT   NOT NULL CONSTRAINT DF_BXHCN_ASSIST DEFAULT 0,
    diem_kda_trung_binh FLOAT NOT NULL CONSTRAINT DF_BXHCN_KDA  DEFAULT 0,
    so_lan_dat_mvp_tran INT  NOT NULL CONSTRAINT DF_BXHCN_MVP   DEFAULT 0,

    CONSTRAINT FK_BXHCN_GD  FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT FK_BXHCN_ND  FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_BXHCN     UNIQUE (ma_giai_dau, ma_nguoi_dung)
);
GO

-- ==============================================================
-- SECTION 9 — NOTIFICATIONS
-- ==============================================================
CREATE TABLE THONG_BAO (
    ma_thong_bao  INT           IDENTITY PRIMARY KEY,
    ma_nguoi_nhan INT           NOT NULL,
    tieu_de       NVARCHAR(200) NOT NULL,
    noi_dung      NVARCHAR(MAX) NULL,
    loai_thong_bao NVARCHAR(50) NULL,
    loai_entity   NVARCHAR(50) NULL,     -- 'loi_moi','xin_gia_nhap','giai_dau','doi'
    ma_entity     INT           NULL,    -- FK-free; entity may vary
    hanh_dong     NVARCHAR(50) NULL,     -- 'accept','decline', etc.
    da_doc        BIT           NOT NULL CONSTRAINT DF_TB_DADOC DEFAULT 0,
    ngay_tao      DATETIME      NOT NULL CONSTRAINT DF_TB_NGAYTAO DEFAULT GETDATE(),

    CONSTRAINT FK_TB_ND FOREIGN KEY (ma_nguoi_nhan) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);
GO

-- ==============================================================
-- SECTION 10 — ENGAGEMENT (Like & Follow)
-- ==============================================================
CREATE TABLE TUONG_TAC_GIAI_DAU (
    ma_tuong_tac  INT      IDENTITY PRIMARY KEY,
    ma_nguoi_dung INT      NOT NULL,
    ma_giai_dau   INT      NOT NULL,
    da_like       BIT      NOT NULL CONSTRAINT DF_TTGD_LIKE   DEFAULT 0,
    dang_theo_doi BIT      NOT NULL CONSTRAINT DF_TTGD_FOLLOW DEFAULT 0,
    thoi_gian_tao DATETIME NOT NULL CONSTRAINT DF_TTGD_THOIGIAN DEFAULT GETDATE(),

    CONSTRAINT FK_TTGD_ND  FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TTGD_GD  FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT UQ_TTGD     UNIQUE (ma_nguoi_dung, ma_giai_dau)
);
GO

-- ==============================================================
-- SECTION 11 — VIEWS
-- ==============================================================

-- ---------------------------------------------------------------
-- V1  VW_DASHBOARD_STATS  (Admin global dashboard)
-- ---------------------------------------------------------------
CREATE VIEW VW_DASHBOARD_STATS AS
SELECT
    (SELECT COUNT(1) FROM NGUOI_DUNG     WHERE is_banned   = 0)                                         AS tong_user_active,
    (SELECT COUNT(1) FROM NGUOI_DUNG     WHERE is_banned   = 1)                                         AS tong_user_bi_ban,
    (SELECT COUNT(1) FROM GIAI_DAU       WHERE trang_thai  = 'dang_dien_ra' AND is_deleted = 0)         AS giai_dang_chay,
    (SELECT COUNT(1) FROM GIAI_DAU       WHERE trang_thai IN ('mo_dang_ky','sap_dien_ra','dang_dien_ra') AND is_deleted = 0) AS giai_dang_hoat_dong,
    (SELECT COUNT(1) FROM DOI            WHERE trang_thai  = 'dang_hoat_dong')                          AS tong_doi_hoat_dong,
    (SELECT COUNT(1) FROM KHIEU_NAI_KET_QUA WHERE trang_thai = 'cho_xu_ly')                            AS khieu_nai_cho_xu_ly,
    (SELECT COUNT(1) FROM GIAI_DAU       WHERE trang_thai  = 'cho_phe_duyet' AND is_deleted = 0)        AS giai_cho_duyet,
    (SELECT COUNT(1) FROM TRO_CHOI       WHERE is_active   = 1)                                         AS tong_game_active;
GO

-- ---------------------------------------------------------------
-- V2  VW_TUONG_TAC_TONG_HOP  (Like / Follow count per tournament)
-- ---------------------------------------------------------------
CREATE VIEW VW_TUONG_TAC_TONG_HOP AS
SELECT
    ma_giai_dau,
    SUM(CAST(da_like       AS INT)) AS tong_like,
    SUM(CAST(dang_theo_doi AS INT)) AS tong_theo_doi
FROM TUONG_TAC_GIAI_DAU
GROUP BY ma_giai_dau;
GO

-- ==============================================================
-- SECTION 12 — STORED PROCEDURES
-- ==============================================================

-- ---------------------------------------------------------------
-- SP1  SP_XoaXachGiaiDau — Hard-delete all data of one tournament
-- ---------------------------------------------------------------
CREATE PROCEDURE SP_XoaXachGiaiDau
    @MaGiaiDau INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Audit & match details
        DELETE FROM LICH_SU_SUA_KET_QUA
            WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        DELETE FROM KET_QUA_TRAN
            WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        DELETE FROM CHI_TIET_NGUOI_CHOI_TRAN
            WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        DELETE FROM CHI_TIET_TRAN_DAU
            WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        DELETE FROM KHIEU_NAI_KET_QUA
            WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        DELETE FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau;

        -- Roster & registration
        DELETE FROM DOI_HINH_THI_DAU WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM THAM_GIA_GIAI    WHERE ma_giai_dau = @MaGiaiDau;

        -- Supporting tables
        DELETE FROM BANG_XEP_HANG         WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM BANG_XEP_HANG_CA_NHAN WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM QUAN_TRI_GIAI_DAU     WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM GIAI_THUONG           WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM TUONG_TAC_GIAI_DAU    WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM GIAI_DOAN             WHERE ma_giai_dau = @MaGiaiDau;

        -- Parent
        DELETE FROM GIAI_DAU WHERE ma_giai_dau = @MaGiaiDau;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Msg, 16, 1);
    END CATCH
END;
GO

-- ---------------------------------------------------------------
-- SP2  SP_XoaGiaiBiKhoaQuaHan — Auto-delete tournaments locked > 30 days
-- ---------------------------------------------------------------
CREATE PROCEDURE SP_XoaGiaiBiKhoaQuaHan
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DanhSach TABLE (ma_giai_dau INT PRIMARY KEY);

    INSERT INTO @DanhSach(ma_giai_dau)
    SELECT ma_giai_dau
    FROM   GIAI_DAU
    WHERE  trang_thai    = 'khoa'
      AND  thoi_gian_khoa IS NOT NULL
      AND  thoi_gian_khoa <= DATEADD(DAY, -30, GETDATE());

    DECLARE @Ma INT;
    WHILE EXISTS (SELECT 1 FROM @DanhSach)
    BEGIN
        SELECT TOP 1 @Ma = ma_giai_dau FROM @DanhSach ORDER BY ma_giai_dau;
        EXEC SP_XoaXachGiaiDau @MaGiaiDau = @Ma;
        DELETE FROM @DanhSach WHERE ma_giai_dau = @Ma;
    END
END;
GO

-- ---------------------------------------------------------------
-- SP3  SP_TaoJob_DonDepGiaiKhoaQuaHan — Register SQL Agent Job
--      Run once manually after deployment:
--      EXEC dbo.SP_TaoJob_DonDepGiaiKhoaQuaHan;
-- ---------------------------------------------------------------
CREATE PROCEDURE SP_TaoJob_DonDepGiaiKhoaQuaHan
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN')
        EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN';

    EXEC msdb.dbo.sp_add_job
        @job_name   = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @enabled    = 1,
        @description = N'Xóa cứng giải đấu ở trạng thái khóa quá 30 ngày';

    EXEC msdb.dbo.sp_add_jobstep
        @job_name     = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @step_name    = N'Step_Execute_Cleanup',
        @subsystem    = N'TSQL',
        @database_name = N'QuanLy_Esports',
        @command      = N'EXEC dbo.SP_XoaGiaiBiKhoaQuaHan;';

    EXEC msdb.dbo.sp_add_schedule
        @schedule_name    = N'SCH_DAILY_1AM_DON_DEP_GIAI_KHOA',
        @freq_type        = 4,     -- Daily
        @freq_interval    = 1,
        @active_start_time = 010000;  -- 01:00 AM

    EXEC msdb.dbo.sp_attach_schedule
        @job_name      = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @schedule_name = N'SCH_DAILY_1AM_DON_DEP_GIAI_KHOA';

    EXEC msdb.dbo.sp_add_jobserver
        @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN';
END;
GO

-- ==============================================================
-- SECTION 13 — RECOMMENDED INDEXES
-- (Beyond the ones created automatically for PK / UNIQUE)
-- ==============================================================

-- Fast lookup: notifications by recipient + read status
CREATE INDEX IX_TB_NGUOINHAN_DADOC
    ON THONG_BAO(ma_nguoi_nhan, da_doc)
    INCLUDE (tieu_de, ngay_tao);

-- Fast lookup: tournaments by status (public listing)
CREATE INDEX IX_GD_TRANGTHAI_PUBLIC
    ON GIAI_DAU(trang_thai, is_deleted, hien_thi_public)
    INCLUDE (ten_giai_dau, ngay_bat_dau, ma_tro_choi);

-- Fast lookup: squad members by squad
CREATE INDEX IX_TV_NHOM_TRANGTHAI
    ON THANH_VIEN_DOI(ma_nhom, trang_thai_duyet)
    INCLUDE (ma_nguoi_dung, vai_tro_noi_bo);

-- Fast lookup: match list per tournament stage
CREATE INDEX IX_TD_GD_GDO_TRANGTHAI
    ON TRAN_DAU(ma_giai_dau, ma_giai_doan, trang_thai)
    INCLUDE (thoi_gian_bat_dau, the_thuc_tran);

-- Fast lookup: player stats per match
CREATE INDEX IX_CTUNCT_TRAN
    ON CHI_TIET_NGUOI_CHOI_TRAN(ma_tran)
    INCLUDE (ma_nguoi_dung, so_kill, so_death, so_assist, is_mvp_tran);

-- Fast lookup: individual leaderboard per tournament
CREATE INDEX IX_BXHCN_GD_KDA
    ON BANG_XEP_HANG_CA_NHAN(ma_giai_dau, diem_kda_trung_binh DESC)
    INCLUDE (ma_nguoi_dung, so_lan_dat_mvp_tran);

GO

-- ==============================================================
-- DONE
-- ==============================================================
<<<<<<< HEAD

-- Bảng lưu trạng thái Like và Follow của từng user với từng giải đấu.
-- Mỗi cặp (ma_nguoi_dung, ma_giai_dau) chỉ có 1 dòng duy nhất.
IF OBJECT_ID('TUONG_TAC_GIAI_DAU', 'U') IS NULL
CREATE TABLE TUONG_TAC_GIAI_DAU (
    ma_tuong_tac   INT IDENTITY PRIMARY KEY,
    ma_nguoi_dung  INT NOT NULL,
    ma_giai_dau    INT NOT NULL,
    da_like        BIT NOT NULL DEFAULT 0,
    dang_theo_doi  BIT NOT NULL DEFAULT 0,
    thoi_gian_tao  DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_TTGD_ND    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_TTGD_GD    FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT UQ_TTGD       UNIQUE (ma_nguoi_dung, ma_giai_dau)
);
GO

-- View tổng hợp đếm like / follow nhanh theo giải đấu
IF OBJECT_ID('VW_TUONG_TAC_TONG_HOP', 'V') IS NOT NULL
    DROP VIEW VW_TUONG_TAC_TONG_HOP;
GO

CREATE VIEW VW_TUONG_TAC_TONG_HOP AS
SELECT
    ma_giai_dau,
    SUM(CAST(da_like       AS INT)) AS tong_like,
    SUM(CAST(dang_theo_doi AS INT)) AS tong_theo_doi
FROM TUONG_TAC_GIAI_DAU
GROUP BY ma_giai_dau;
GO

-- ==============================================================
-- UPDATE THEO YEU CAU TAO GIAI DAU
-- ==============================================================
IF COL_LENGTH('GIAI_DAU', 'mo_ta') IS NULL
    ALTER TABLE GIAI_DAU ADD mo_ta NVARCHAR(MAX) NULL;

IF COL_LENGTH('GIAI_DAU', 'so_nguoi_moi_doi') IS NULL
    ALTER TABLE GIAI_DAU ADD so_nguoi_moi_doi INT NULL;

IF COL_LENGTH('GIAI_THUONG', 'ten_giai') IS NULL
    ALTER TABLE GIAI_THUONG ADD ten_giai NVARCHAR(200) NULL;

IF COL_LENGTH('GIAI_THUONG', 'mo_ta') IS NULL
    ALTER TABLE GIAI_THUONG ADD mo_ta NVARCHAR(MAX) NULL;

IF COL_LENGTH('GIAI_THUONG', 'phan_thuong') IS NULL
    ALTER TABLE GIAI_THUONG ADD phan_thuong NVARCHAR(MAX) NULL;
GO

=======
PRINT N'QuanLy_Esports database initialized successfully.';
GO
>>>>>>> b055aa5 (new)
