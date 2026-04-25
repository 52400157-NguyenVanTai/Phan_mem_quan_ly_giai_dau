-- ==============================================================
-- KHỞI TẠO CƠ SỞ DỮ LIỆU
-- ==============================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLy_Esports')
BEGIN
    CREATE DATABASE QuanLy_Esports;
END
GO
USE QuanLy_Esports;
GO

IF OBJECT_ID('TUONG_TAC_GIAI_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE TUONG_TAC_GIAI_DAU (
        ma_nguoi_dung      INT NOT NULL,
        ma_giai_dau        INT NOT NULL,
        is_liked           BIT NOT NULL DEFAULT 0, -- 1 là đã Like, 0 là chưa/un-like
        is_followed        BIT NOT NULL DEFAULT 0, -- 1 là đang Follow, 0 là un-follow
        thoi_gian_tao      DATETIME DEFAULT GETDATE(),
        thoi_gian_cap_nhat DATETIME DEFAULT GETDATE(),
        
        -- Khóa chính kép: Đảm bảo 1 user chỉ có 1 dòng trạng thái cho 1 giải đấu
        PRIMARY KEY (ma_nguoi_dung, ma_giai_dau),
        
        -- Ràng buộc khóa ngoại
        CONSTRAINT FK_TuongTac_User FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_TuongTac_Giai FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau)
    );
END
GO

-- ==============================================================
-- 1. HỆ THỐNG TÀI KHOẢN & NGƯỜI DÙNG
-- ==============================================================
IF OBJECT_ID('NGUOI_DUNG', 'U') IS NULL
BEGIN
    CREATE TABLE NGUOI_DUNG (
        ma_nguoi_dung    INT IDENTITY PRIMARY KEY,
        ten_dang_nhap    NVARCHAR(100) UNIQUE,
        email            NVARCHAR(150) UNIQUE NOT NULL,
        mat_khau_ma_hoa  NVARCHAR(255) NOT NULL,
        vai_tro_he_thong NVARCHAR(50) NOT NULL DEFAULT 'user',
        avatar_url       NVARCHAR(255) NULL,
        bio              NVARCHAR(500) NULL,
        is_banned        BIT NOT NULL DEFAULT 0,
        ly_do_ban        NVARCHAR(500) NULL,
        thoi_gian_ban    DATETIME NULL,
        ma_admin_ban     INT NULL,         -- FK tự tham chiếu, thêm sau
        ngay_tao         DATETIME DEFAULT GETDATE(),
        CONSTRAINT chk_vai_tro_ht CHECK (vai_tro_he_thong IN ('admin', 'user'))
    );
END
GO

-- Self-referencing FK phải thêm sau khi bảng đã tồn tại
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_ND_ADMIN_BAN') AND parent_object_id = OBJECT_ID(N'NGUOI_DUNG'))
BEGIN
    ALTER TABLE NGUOI_DUNG
        ADD CONSTRAINT FK_ND_ADMIN_BAN FOREIGN KEY (ma_admin_ban) REFERENCES NGUOI_DUNG(ma_nguoi_dung);
END
GO

IF OBJECT_ID('TRO_CHOI', 'U') IS NULL
BEGIN
    CREATE TABLE TRO_CHOI (
        ma_tro_choi INT IDENTITY PRIMARY KEY,
        ten_game    NVARCHAR(100),
        the_loai    NVARCHAR(100),
        is_active   BIT NOT NULL DEFAULT 1,
        CONSTRAINT chk_the_loai CHECK (the_loai IN ('MOBA', 'FPS', 'BATTLEROYALE'))
    );
END
GO

-- ==============================================================
-- 8. DANH MỤC VỊ TRÍ THEO GAME
-- (Dời lên trước HO_SO_IN_GAME để giải quyết phụ thuộc FK)
-- ==============================================================
IF OBJECT_ID('DANH_MUC_VI_TRI', 'U') IS NULL
BEGIN
    CREATE TABLE DANH_MUC_VI_TRI (
        ma_vi_tri   INT IDENTITY PRIMARY KEY,
        ma_tro_choi INT,
        ten_vi_tri  NVARCHAR(50),
        ky_hieu     NVARCHAR(10),
        loai_vi_tri NVARCHAR(50),   -- 'ChuyenMon' hoặc 'BanHuanLuyen'
        FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi)
    );
END
GO

IF OBJECT_ID('HO_SO_IN_GAME', 'U') IS NULL
BEGIN
    CREATE TABLE HO_SO_IN_GAME (
        ma_ho_so             INT IDENTITY PRIMARY KEY,
        ma_nguoi_dung        INT,
        ma_tro_choi          INT,
        in_game_id           NVARCHAR(100),
        in_game_name         NVARCHAR(100),
        ma_vi_tri_so_truong  INT NULL,
        ngay_cap_nhat        DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_tro_choi)   REFERENCES TRO_CHOI(ma_tro_choi),
        CONSTRAINT FK_HoSo_ViTri    FOREIGN KEY (ma_vi_tri_so_truong) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
        CONSTRAINT uq_profile       UNIQUE (ma_nguoi_dung, ma_tro_choi)
    );
END
GO

-- ==============================================================
-- 2. QUẢN LÝ ĐỘI TUYỂN & NHÓM THEO GAME
-- ==============================================================
IF OBJECT_ID('DOI', 'U') IS NULL
BEGIN
    CREATE TABLE DOI (
        ma_doi      INT IDENTITY PRIMARY KEY,
        ten_doi     NVARCHAR(150) UNIQUE,
        ma_doi_truong INT,
        ma_manager  INT NULL,
        logo_url    NVARCHAR(255),
        slogan      NVARCHAR(300) NULL,
        trang_thai  NVARCHAR(30) NOT NULL DEFAULT 'dang_hoat_dong',
        ngay_tao    DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ma_doi_truong) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_DOI_MANAGER FOREIGN KEY (ma_manager) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
    );
END
GO

IF OBJECT_ID('NHOM_DOI', 'U') IS NULL
BEGIN
    CREATE TABLE NHOM_DOI (
        ma_nhom            INT IDENTITY PRIMARY KEY,
        ma_doi             INT,
        ma_tro_choi        INT,
        ten_nhom           NVARCHAR(150),
        ma_doi_truong_nhom INT NULL,
        FOREIGN KEY (ma_doi)     REFERENCES DOI(ma_doi),
        FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi),
        CONSTRAINT FK_NHOM_DOI_DOI_TRUONG FOREIGN KEY (ma_doi_truong_nhom) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT UQ_DOI_GAME_TENNHOM    UNIQUE (ma_doi, ma_tro_choi, ten_nhom)
    );
END
GO

IF OBJECT_ID('THANH_VIEN_DOI', 'U') IS NULL
BEGIN
    CREATE TABLE THANH_VIEN_DOI (
        ma_thanh_vien       INT IDENTITY PRIMARY KEY,
        ma_nguoi_dung       INT,
        ma_nhom             INT,
        ma_vi_tri           INT NULL,
        trang_thai_duyet    NVARCHAR(50) DEFAULT 'cho_duyet',
        vai_tro_noi_bo      NVARCHAR(20) NOT NULL DEFAULT 'member',     -- leader, captain, member
        phan_he             NVARCHAR(30) NOT NULL DEFAULT 'thi_dau',    -- thi_dau, ban_huan_luyen
        trang_thai_hop_dong NVARCHAR(30) NOT NULL DEFAULT 'dang_hieu_luc',
        ngay_tham_gia       DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_nhom)       REFERENCES NHOM_DOI(ma_nhom),
        CONSTRAINT FK_THANHVIEN_VITRI           FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
        CONSTRAINT chk_trang_thai_tv            CHECK (trang_thai_duyet     IN ('cho_duyet', 'da_duyet', 'bi_tu_choi')),
        CONSTRAINT CHK_THANHVIEN_VAITRO_NOIBO   CHECK (vai_tro_noi_bo       IN ('leader', 'captain', 'member')),
        CONSTRAINT CHK_THANHVIEN_PHANHE         CHECK (phan_he              IN ('thi_dau', 'ban_huan_luyen')),
        CONSTRAINT CHK_THANHVIEN_HOPDONG        CHECK (trang_thai_hop_dong  IN ('dang_hieu_luc', 'tu_do', 'da_giai_phong'))
    );
END
GO

-- ==============================================================
-- 3. QUẢN LÝ GIẢI ĐẤU
-- ==============================================================
IF OBJECT_ID('GIAI_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE GIAI_DAU (
        ma_giai_dau             INT IDENTITY PRIMARY KEY,
        ten_giai_dau            NVARCHAR(150),
        ma_tro_choi             INT NULL,           -- NULL nếu là giải hỗn hợp nhiều game
        the_thuc                NVARCHAR(50),
        ngay_bat_dau            DATETIME,
        ngay_ket_thuc           DATETIME,
        tong_giai_thuong        DECIMAL(12,2),
        trang_thai              NVARCHAR(50) NOT NULL DEFAULT 'ban_nhap',
        is_deleted              BIT DEFAULT 0,
        hien_thi_public         BIT DEFAULT 1,
        ma_nguoi_tao            INT NULL,
        banner_url              NVARCHAR(400) NULL,
        thoi_gian_mo_dang_ky    DATETIME NULL,
        thoi_gian_dong_dang_ky  DATETIME NULL,
        thoi_gian_khoa          DATETIME NULL,
        ma_nguoi_khoa           INT NULL,
        ly_do_khoa              NVARCHAR(500) NULL,
        FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi),
        CONSTRAINT FK_GIAIDAU_NGUOITAO  FOREIGN KEY (ma_nguoi_tao)  REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_GIAIDAU_NGUOIKHOA FOREIGN KEY (ma_nguoi_khoa) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT chk_the_thuc         CHECK (the_thuc IN (
            'loai_truc_tiep', 'nhanh_thanh_nhanh_thua', 'dau_theo_bang', 'vong_tron_tinh_diem', 'hon_hop'
        )),
        CONSTRAINT chk_trang_thai_giai  CHECK (trang_thai IN (
            'ban_nhap', 'cho_phe_duyet', 'mo_dang_ky', 'sap_dien_ra', 'dang_dien_ra', 'ket_thuc', 'khoa'
        )),
        CONSTRAINT CHK_GIAIDAU_THOIGIAN_DANGKY CHECK (
            thoi_gian_dong_dang_ky IS NULL
            OR ngay_bat_dau IS NULL
            OR thoi_gian_dong_dang_ky < ngay_bat_dau
        )
    );
END
GO

-- ==============================================================
-- GIAI_DOAN
-- (Dời lên trước TRAN_DAU để giải quyết phụ thuộc FK)
-- ==============================================================
IF OBJECT_ID('GIAI_DOAN', 'U') IS NULL
BEGIN
    CREATE TABLE GIAI_DOAN (
        ma_giai_doan            INT IDENTITY PRIMARY KEY,
        ma_giai_dau             INT NOT NULL,
        ten_giai_doan           NVARCHAR(100) NOT NULL,
        the_thuc                NVARCHAR(50) NOT NULL,
        thu_tu                  INT NOT NULL,
        so_doi_di_tiep          INT DEFAULT 0,
        diem_nguong_match_point INT NULL,
        trang_thai              NVARCHAR(30) NOT NULL DEFAULT 'chua_bat_dau',
        CONSTRAINT FK_GiaiDoan_GiaiDau   FOREIGN KEY (ma_giai_dau)  REFERENCES GIAI_DAU(ma_giai_dau),
        CONSTRAINT CHK_TheThuc_GiaiDoan  CHECK (the_thuc IN (
            'loai_truc_tiep', 'nhanh_thang_nhanh_thua', 'vong_tron',
            'league_bang_cheo', 'thuy_si', 'champion_rush'
        )),
        CONSTRAINT CHK_ThuTu_GiaiDoan       CHECK (thu_tu > 0),
        CONSTRAINT CHK_SoDoiDiTiep          CHECK (so_doi_di_tiep >= 0),
        CONSTRAINT CHK_GIAIDOAN_MATCHPOINT  CHECK (diem_nguong_match_point IS NULL OR diem_nguong_match_point > 0),
        CONSTRAINT CHK_GIAIDOAN_TRANGTHAI   CHECK (trang_thai IN ('chua_bat_dau', 'dang_dien_ra', 'ket_thuc')),
        CONSTRAINT UQ_GiaiDau_ThuTu         UNIQUE (ma_giai_dau, thu_tu)
    );
END
GO

-- Cấp quyền Ban Tổ Chức hoặc Trọng tài cho từng Giải đấu cụ thể
IF OBJECT_ID('QUAN_TRI_GIAI_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE QUAN_TRI_GIAI_DAU (
        ma_giai_dau   INT,
        ma_nguoi_dung INT,
        vai_tro_giai  NVARCHAR(50),    -- ban_to_chuc, trong_tai
        PRIMARY KEY (ma_giai_dau, ma_nguoi_dung),
        FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT chk_vai_tro_giai CHECK (vai_tro_giai IN ('ban_to_chuc', 'trong_tai'))
    );
END
GO

IF OBJECT_ID('THAM_GIA_GIAI', 'U') IS NULL
BEGIN
    CREATE TABLE THAM_GIA_GIAI (
        ma_tham_gia         INT IDENTITY PRIMARY KEY,
        ma_giai_dau         INT,
        ma_nhom             INT,    -- Nhóm đội đăng ký tham gia
        trang_thai_duyet    NVARCHAR(50) DEFAULT 'cho_duyet',
        trang_thai_tham_gia NVARCHAR(30) NOT NULL DEFAULT 'dang_thi_dau',
        hat_giong           INT NULL,
        FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
        FOREIGN KEY (ma_nhom)     REFERENCES NHOM_DOI(ma_nhom),
        -- Đảm bảo một nhóm (squad) chỉ xuất hiện 1 lần trong 1 giải
        CONSTRAINT UQ_GiaiDau_Nhom          UNIQUE (ma_giai_dau, ma_nhom),
        CONSTRAINT CHK_TGG_TRANGTHAI_THAMGIA CHECK (trang_thai_tham_gia IN ('dang_thi_dau', 'di_tiep', 'bi_loai'))
    );
END
GO

-- ==============================================================
-- 4. TRẬN ĐẤU & KẾT QUẢ (CÓ AUDIT LOG)
-- ==============================================================
IF OBJECT_ID('TRAN_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE TRAN_DAU (
        ma_tran                 INT IDENTITY PRIMARY KEY,
        ma_giai_dau             INT,
        ma_giai_doan            INT NULL,
        ma_trong_tai            INT NULL,
        vong_dau                NVARCHAR(50),   -- Ví dụ: Vòng Bảng, Bán Kết, Chung Kết
        the_thuc_tran           NVARCHAR(50),   -- BO1, BO3, BO5, SinhTon
        so_vong                 INT NULL,
        nhanh_dau               NVARCHAR(30) NULL,
        ma_tran_tiep_theo_thang INT NULL,
        ma_tran_tiep_theo_thua  INT NULL,
        thoi_gian_bat_dau       DATETIME,
        thoi_gian_ket_thuc      DATETIME,
        thoi_gian_nhap_diem     DATETIME NULL,
        so_lan_sua              INT NOT NULL DEFAULT 0,
        trang_thai              NVARCHAR(50) DEFAULT 'chua_dau',
        FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
        CONSTRAINT FK_TranDau_GiaiDoan  FOREIGN KEY (ma_giai_doan)              REFERENCES GIAI_DOAN(ma_giai_doan),
        FOREIGN KEY (ma_trong_tai)      REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_TRANDAU_NEXT_WIN  FOREIGN KEY (ma_tran_tiep_theo_thang)   REFERENCES TRAN_DAU(ma_tran),
        CONSTRAINT FK_TRANDAU_NEXT_LOSE FOREIGN KEY (ma_tran_tiep_theo_thua)    REFERENCES TRAN_DAU(ma_tran),
        CONSTRAINT chk_tran_trangthai   CHECK (trang_thai IN ('chua_dau', 'dang_dau', 'da_hoan_thanh', 'huy_bo'))
    );
END
GO

-- Liên kết các đội tham gia vào 1 trận đấu
-- (Rất cần thiết cho Battle Royale có nhiều đội 1 trận)
IF OBJECT_ID('CHI_TIET_TRAN_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE CHI_TIET_TRAN_DAU (
        ma_tran  INT,
        ma_nhom  INT,
        diem_so  FLOAT DEFAULT 0,   -- Điểm kill/Top hoặc số ván thắng
        thu_hang INT NULL,          -- Dành cho Battle Royale
        ket_qua  NVARCHAR(50),      -- thang, thua, hoa (MOBA/FPS)
        PRIMARY KEY (ma_tran, ma_nhom),
        FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran),
        FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom)
    );
END
GO

-- Quản lý logic cập nhật điểm của trọng tài
IF OBJECT_ID('KET_QUA_TRAN', 'U') IS NULL
BEGIN
    CREATE TABLE KET_QUA_TRAN (
        ma_ket_qua                 INT IDENTITY PRIMARY KEY,
        ma_tran                    INT UNIQUE,
        thoi_diem_bao_cao_dau_tien DATETIME DEFAULT GETDATE(),
        so_lan_chinh_sua           INT DEFAULT 0,   -- Để dev check < 1
        thoi_gian_sua_cuoi         DATETIME,
        chi_tiet_phu               NVARCHAR(MAX),   -- Lưu JSON log trận đấu nếu cần
        FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran)
    );
END
GO

-- Bảng Log lưu vết sửa điểm của trọng tài (Audit Log)
IF OBJECT_ID('LICH_SU_SUA_KET_QUA', 'U') IS NULL
BEGIN
    CREATE TABLE LICH_SU_SUA_KET_QUA (
        ma_log           INT IDENTITY PRIMARY KEY,
        ma_tran          INT,
        ma_trong_tai_sua INT,
        nguoi_sua        INT NULL,
        thoi_gian_sua    DATETIME DEFAULT GETDATE(),
        ly_do_sua        NVARCHAR(MAX),
        du_lieu_cu       NVARCHAR(MAX) NULL,
        du_lieu_moi      NVARCHAR(MAX) NULL,
        FOREIGN KEY (ma_tran)          REFERENCES TRAN_DAU(ma_tran),
        FOREIGN KEY (ma_trong_tai_sua) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_LSSKQ_NGUOI_SUA  FOREIGN KEY (nguoi_sua) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
    );
END
GO

-- ==============================================================
-- 5. HỆ THỐNG THÔNG BÁO (NOTIFICATIONS)
-- ==============================================================
IF OBJECT_ID('THONG_BAO', 'U') IS NULL
BEGIN
    CREATE TABLE THONG_BAO (
        ma_thong_bao   INT IDENTITY PRIMARY KEY,
        ma_nguoi_nhan  INT,
        tieu_de        NVARCHAR(200),
        noi_dung       NVARCHAR(MAX),
        loai_thong_bao NVARCHAR(50),   -- xin_vao_doi, duyet_giai, moi_trong_tai
        da_doc         BIT DEFAULT 0,
        ngay_tao       DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (ma_nguoi_nhan) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
    );
END
GO

-- ==============================================================
-- 6. GIẢI THƯỞNG
-- ==============================================================
IF OBJECT_ID('GIAI_THUONG', 'U') IS NULL
BEGIN
    CREATE TABLE GIAI_THUONG (
        ma_giai_thuong INT IDENTITY PRIMARY KEY,
        ma_giai_dau    INT,
        vi_tri_top     INT,            -- Top 1, Top 2...
        so_tien        DECIMAL(12,2),
        FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau)
    );
END
GO

-- ==============================================================
-- 7. BẢNG XẾP HẠNG (LEADERBOARD)
-- ==============================================================
IF OBJECT_ID('BANG_XEP_HANG', 'U') IS NULL
BEGIN
    CREATE TABLE BANG_XEP_HANG (
        ma_bxh            INT IDENTITY PRIMARY KEY,
        ma_giai_dau       INT,
        ma_nhom           INT,
        ma_giai_doan      INT NULL,

        -- Thống kê chung
        so_tran_da_dau    INT DEFAULT 0,

        -- Dành cho MOBA/FPS (Tính Thắng/Thua/Hòa)
        so_tran_thang     INT DEFAULT 0,
        so_tran_thua      INT DEFAULT 0,
        hieu_so_phu       INT DEFAULT 0,   -- Hiệu số ván thắng/thua (Kill/Death cho FPS)

        -- Dành cho Battle Royale (Tính Điểm)
        tong_diem_hang    FLOAT DEFAULT 0, -- Điểm vị trí (Placement Points)
        tong_diem_kill    FLOAT DEFAULT 0, -- Điểm hạ gục (Kill Points)
        so_lan_top_1      INT DEFAULT 0,   -- Tiêu chí Tie-breaker

        -- Điểm tổng quy đổi, Thứ hạng và Match Point
        diem_tong_ket     FLOAT DEFAULT 0,
        thu_hang_hien_tai INT DEFAULT 0,
        is_match_point    BIT NOT NULL DEFAULT 0,

        FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
        FOREIGN KEY (ma_nhom)     REFERENCES NHOM_DOI(ma_nhom),
        CONSTRAINT FK_Bxh_GiaiDoan  FOREIGN KEY (ma_giai_doan) REFERENCES GIAI_DOAN(ma_giai_doan),
        -- Một đội chỉ có 1 dòng xếp hạng trong 1 GIAI_DOAN
        CONSTRAINT UQ_GiaiDoan_Nhom UNIQUE (ma_giai_doan, ma_nhom)
    );
END
GO

-- ==============================================================
-- 9. CHI TIẾT CHỈ SỐ NGƯỜI CHƠI THEO TRẬN (Để tính MVP Trận)
-- ==============================================================
IF OBJECT_ID('CHI_TIET_NGUOI_CHOI_TRAN', 'U') IS NULL
BEGIN
    CREATE TABLE CHI_TIET_NGUOI_CHOI_TRAN (
        ma_chi_tiet_user_tran INT IDENTITY PRIMARY KEY,
        ma_tran               INT,
        ma_nguoi_dung         INT,
        ma_vi_tri             INT,
        so_kill               INT DEFAULT 0,
        so_death              INT DEFAULT 0,
        so_assist             INT DEFAULT 0,
        diem_kda_tran         FLOAT,          -- Công thức: (Kill + Assist) / Max(1, Death)
        diem_sinh_ton         FLOAT NULL,
        is_mvp_tran           BIT DEFAULT 0,  -- Đánh dấu nếu là người giỏi nhất trận
        FOREIGN KEY (ma_tran)       REFERENCES TRAN_DAU(ma_tran),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_vi_tri)     REFERENCES DANH_MUC_VI_TRI(ma_vi_tri)
    );
END
GO

-- ==============================================================
-- 10. BẢNG XẾP HẠNG CÁ NHÂN GIẢI ĐẤU (Để tính MVP Giải)
-- ==============================================================
IF OBJECT_ID('BANG_XEP_HANG_CA_NHAN', 'U') IS NULL
BEGIN
    CREATE TABLE BANG_XEP_HANG_CA_NHAN (
        ma_bxh_ca_nhan      INT IDENTITY PRIMARY KEY,
        ma_giai_dau         INT,
        ma_nguoi_dung       INT,
        tong_kill           INT DEFAULT 0,
        tong_death          INT DEFAULT 0,
        tong_assist         INT DEFAULT 0,
        diem_kda_trung_binh FLOAT DEFAULT 0,
        so_lan_dat_mvp_tran INT DEFAULT 0,  -- Tiêu chí phụ cực quan trọng cho MVP Giải
        FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT uq_bxh_ca_nhan UNIQUE (ma_giai_dau, ma_nguoi_dung)
    );
END
GO

-- ==============================================================
-- 11. ĐỘI HÌNH ĐĂNG KÝ THI ĐẤU (Tournament Roster)
-- ==============================================================
IF OBJECT_ID('DOI_HINH_THI_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE DOI_HINH_THI_DAU (
        ma_doi_hinh   INT IDENTITY PRIMARY KEY,
        ma_tham_gia   INT,       -- Link tới bảng THAM_GIA_GIAI (Nhóm đội đăng ký giải)
        ma_nguoi_dung INT,
        ma_vi_tri     INT,       -- Khai báo vị trí đánh giải này (Ví dụ: Mid, AD, HLV)
        ma_giai_dau   INT NULL,  -- Dùng để check nhanh, tránh JOIN nhiều bảng khi kiểm tra trùng player
        is_du_bi      BIT DEFAULT 0,
        FOREIGN KEY (ma_tham_gia)   REFERENCES THAM_GIA_GIAI(ma_tham_gia),
        FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_vi_tri)     REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
        CONSTRAINT FK_DoiHinh_GiaiDau   FOREIGN KEY (ma_giai_dau)   REFERENCES GIAI_DAU(ma_giai_dau),
        -- Một người không thể nằm trong 2 đội hình của cùng 1 giải
        CONSTRAINT UQ_GiaiDau_Player    UNIQUE (ma_giai_dau, ma_nguoi_dung)
    );
END
GO

-- ==============================================================
-- 12. TUYỂN DỤNG & LỜI MỜI (Recruitment)
-- ==============================================================
IF OBJECT_ID('BAI_DANG_TUYEN_DUNG', 'U') IS NULL
BEGIN
    CREATE TABLE BAI_DANG_TUYEN_DUNG (
        ma_bai_dang INT IDENTITY PRIMARY KEY,
        ma_doi      INT NOT NULL,
        ma_nhom     INT NOT NULL,
        ma_vi_tri   INT NOT NULL,
        noi_dung    NVARCHAR(500) NOT NULL,
        trang_thai  NVARCHAR(20) NOT NULL DEFAULT 'dang_mo',
        ngay_tao    DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (ma_doi)    REFERENCES DOI(ma_doi),
        FOREIGN KEY (ma_nhom)   REFERENCES NHOM_DOI(ma_nhom),
        FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
        CONSTRAINT CHK_BAIDANG_TRANGTHAI CHECK (trang_thai IN ('dang_mo', 'tam_dong', 'da_dong'))
    );
END
GO

IF OBJECT_ID('DON_UNG_TUYEN', 'U') IS NULL
BEGIN
    CREATE TABLE DON_UNG_TUYEN (
        ma_don      INT IDENTITY PRIMARY KEY,
        ma_bai_dang INT NOT NULL,
        ma_ung_vien INT NOT NULL,
        trang_thai  NVARCHAR(20) NOT NULL DEFAULT 'cho_duyet',
        ngay_tao    DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (ma_bai_dang) REFERENCES BAI_DANG_TUYEN_DUNG(ma_bai_dang),
        FOREIGN KEY (ma_ung_vien) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT UQ_DON_BAIDANG_UNGVIEN    UNIQUE (ma_bai_dang, ma_ung_vien),
        CONSTRAINT CHK_DONUNGTUYEN_TRANGTHAI CHECK (trang_thai IN ('cho_duyet', 'chap_nhan', 'tu_choi'))
    );
END
GO

IF OBJECT_ID('LOI_MOI_GIA_NHAP', 'U') IS NULL
BEGIN
    CREATE TABLE LOI_MOI_GIA_NHAP (
        ma_loi_moi        INT IDENTITY PRIMARY KEY,
        ma_doi            INT NOT NULL,
        ma_nhom           INT NOT NULL,
        ma_nguoi_duoc_moi INT NOT NULL,
        trang_thai        NVARCHAR(20) NOT NULL DEFAULT 'cho_phan_hoi',
        ngay_tao          DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (ma_doi)            REFERENCES DOI(ma_doi),
        FOREIGN KEY (ma_nhom)           REFERENCES NHOM_DOI(ma_nhom),
        FOREIGN KEY (ma_nguoi_duoc_moi) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT UQ_LOIMOI_NHOM_USER  UNIQUE (ma_nhom, ma_nguoi_duoc_moi),
        CONSTRAINT CHK_LOIMOI_TRANGTHAI CHECK (trang_thai IN ('cho_phan_hoi', 'chap_nhan', 'tu_choi'))
    );
END
GO

IF OBJECT_ID('YEU_CAU_TAO_GIAI_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE YEU_CAU_TAO_GIAI_DAU (
        ma_yeu_cau       INT IDENTITY PRIMARY KEY,
        ma_nguoi_gui     INT NOT NULL,
        ten_giai_dau     NVARCHAR(150) NOT NULL,
        ma_tro_choi      INT NULL,
        the_thuc         NVARCHAR(50) NOT NULL,
        ngay_bat_dau     DATETIME NOT NULL,
        ngay_ket_thuc    DATETIME NOT NULL,
        tong_giai_thuong DECIMAL(12,2) NOT NULL,
        trang_thai       NVARCHAR(20) NOT NULL DEFAULT 'cho_duyet',
        ma_admin_duyet   INT NULL,
        ly_do_huy        NVARCHAR(500) NULL,
        thoi_gian_gui    DATETIME NOT NULL DEFAULT GETDATE(),
        thoi_gian_duyet  DATETIME NULL,
        FOREIGN KEY (ma_nguoi_gui)   REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        FOREIGN KEY (ma_tro_choi)    REFERENCES TRO_CHOI(ma_tro_choi),
        FOREIGN KEY (ma_admin_duyet) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT CHK_YEUCAU_TRANGTHAI CHECK (trang_thai IN ('cho_duyet', 'da_duyet', 'tu_choi'))
    );
END
GO

-- ==============================================================
-- 14. CỔNG TRỌNG TÀI & ANTI-CHEAT: KHIẾU NẠI KẾT QUẢ
-- ==============================================================
IF OBJECT_ID('KHIEU_NAI_KET_QUA', 'U') IS NULL
BEGIN
    CREATE TABLE KHIEU_NAI_KET_QUA (
        ma_khieu_nai    INT IDENTITY PRIMARY KEY,
        ma_tran         INT NOT NULL,
        ma_nhom         INT NOT NULL,
        ma_nguoi_gui    INT NOT NULL,
        noi_dung        NVARCHAR(MAX) NOT NULL,
        trang_thai      NVARCHAR(30) NOT NULL DEFAULT 'cho_xu_ly',
        ma_admin_xu_ly  INT NULL,
        phan_hoi_admin  NVARCHAR(MAX) NULL,
        thoi_gian_tao   DATETIME NOT NULL DEFAULT GETDATE(),
        thoi_gian_xu_ly DATETIME NULL,
        CONSTRAINT FK_KN_TRAN        FOREIGN KEY (ma_tran)        REFERENCES TRAN_DAU(ma_tran),
        CONSTRAINT FK_KN_NHOM        FOREIGN KEY (ma_nhom)        REFERENCES NHOM_DOI(ma_nhom),
        CONSTRAINT FK_KN_NGUOI_GUI   FOREIGN KEY (ma_nguoi_gui)   REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_KN_ADMIN_XU_LY FOREIGN KEY (ma_admin_xu_ly) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT CHK_KN_TRANGTHAI  CHECK (trang_thai IN ('cho_xu_ly', 'da_xu_ly', 'tu_choi'))
    );
END
GO

-- Filtered unique index: Mỗi nhóm chỉ có 1 khiếu nại đang chờ xử lý cho 1 trận
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_KN_PENDING_TRAN_NHOM' AND object_id = OBJECT_ID('KHIEU_NAI_KET_QUA'))
BEGIN
    CREATE UNIQUE INDEX UX_KN_PENDING_TRAN_NHOM
        ON KHIEU_NAI_KET_QUA(ma_tran, ma_nhom)
        WHERE trang_thai = 'cho_xu_ly';
END
GO

-- ==============================================================
-- STORED PROCEDURES
-- ==============================================================

-- SP_XoaXachGiaiDau: Xóa toàn bộ dữ liệu liên quan của 1 giải đấu
IF OBJECT_ID('SP_XoaXachGiaiDau', 'P') IS NOT NULL
    DROP PROCEDURE SP_XoaXachGiaiDau;
GO

CREATE PROCEDURE SP_XoaXachGiaiDau
    @MaGiaiDau INT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. XÓA NHÁNH TRẬN ĐẤU (Xóa Cháu trước, Con sau)
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

        -- 2. XÓA NHÁNH ĐỘI HÌNH VÀ THAM GIA
        DELETE FROM DOI_HINH_THI_DAU WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM THAM_GIA_GIAI     WHERE ma_giai_dau = @MaGiaiDau;

        -- 3. XÓA CÁC BẢNG LIÊN KẾT KHÁC
        DELETE FROM BANG_XEP_HANG         WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM BANG_XEP_HANG_CA_NHAN WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM QUAN_TRI_GIAI_DAU     WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM GIAI_THUONG           WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM GIAI_DOAN             WHERE ma_giai_dau = @MaGiaiDau;

        -- 4. BƯỚC CUỐI: XÓA CHÍNH GIẢI ĐẤU ĐÓ
        DELETE FROM GIAI_DAU WHERE ma_giai_dau = @MaGiaiDau;

        COMMIT TRANSACTION;
        PRINT 'Đã xóa thành công toàn bộ dữ liệu của giải đấu!';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

-- SP_XoaGiaiBiKhoaQuaHan: Xóa các giải đấu bị khóa quá 30 ngày
IF OBJECT_ID('SP_XoaGiaiBiKhoaQuaHan', 'P') IS NOT NULL
    DROP PROCEDURE SP_XoaGiaiBiKhoaQuaHan;
GO

CREATE PROCEDURE SP_XoaGiaiBiKhoaQuaHan
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DanhSach TABLE (ma_giai_dau INT PRIMARY KEY);

    INSERT INTO @DanhSach(ma_giai_dau)
    SELECT ma_giai_dau
    FROM GIAI_DAU
    WHERE trang_thai = 'khoa'
      AND thoi_gian_khoa IS NOT NULL
      AND thoi_gian_khoa <= DATEADD(DAY, -30, GETDATE());

    DECLARE @MaGiaiDau INT;

    WHILE EXISTS (SELECT 1 FROM @DanhSach)
    BEGIN
        SELECT TOP 1 @MaGiaiDau = ma_giai_dau FROM @DanhSach ORDER BY ma_giai_dau;
        EXEC SP_XoaXachGiaiDau @MaGiaiDau = @MaGiaiDau;
        DELETE FROM @DanhSach WHERE ma_giai_dau = @MaGiaiDau;
    END
END;
GO

-- SP_TaoJob_DonDepGiaiKhoaQuaHan: Tạo SQL Server Agent Job chạy tự động hàng ngày lúc 1AM
IF OBJECT_ID('SP_TaoJob_DonDepGiaiKhoaQuaHan', 'P') IS NOT NULL
    DROP PROCEDURE SP_TaoJob_DonDepGiaiKhoaQuaHan;
GO

CREATE PROCEDURE SP_TaoJob_DonDepGiaiKhoaQuaHan
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN')
    BEGIN
        EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN';
    END

    EXEC msdb.dbo.sp_add_job
        @job_name    = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @enabled     = 1,
        @description = N'Xóa cứng giải đấu ở trạng thái khóa quá 30 ngày';

    EXEC msdb.dbo.sp_add_jobstep
        @job_name     = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @step_name    = N'Step_Execute_Cleanup',
        @subsystem    = N'TSQL',
        @database_name= N'QuanLy_Esports',
        @command      = N'EXEC dbo.SP_XoaGiaiBiKhoaQuaHan;';

    EXEC msdb.dbo.sp_add_schedule
        @schedule_name    = N'SCH_DAILY_1AM_DON_DEP_GIAI_KHOA',
        @freq_type        = 4,
        @freq_interval    = 1,
        @active_start_time= 010000;

    EXEC msdb.dbo.sp_attach_schedule
        @job_name      = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @schedule_name = N'SCH_DAILY_1AM_DON_DEP_GIAI_KHOA';

    EXEC msdb.dbo.sp_add_jobserver
        @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN';
END;
GO

-- Chạy 1 lần để tạo SQL Server Agent Job dọn dẹp tự động
-- EXEC dbo.SP_TaoJob_DonDepGiaiKhoaQuaHan;

-- ==============================================================
-- TRIGGERS
-- ==============================================================

-- Bảo vệ LICH_SU_SUA_KET_QUA: audit log bất biến
IF OBJECT_ID('TRG_LICH_SU_SUA_KET_QUA_IMMUTABLE', 'TR') IS NOT NULL
    DROP TRIGGER TRG_LICH_SU_SUA_KET_QUA_IMMUTABLE;
GO

CREATE TRIGGER TRG_LICH_SU_SUA_KET_QUA_IMMUTABLE
ON LICH_SU_SUA_KET_QUA
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR(N'LICH_SU_SUA_KET_QUA là audit log bất biến, không cho phép UPDATE/DELETE.', 16, 1);
END;
GO

-- ==============================================================
-- VIEWS
-- ==============================================================

-- View thống kê nhanh cho Global Dashboard
IF OBJECT_ID('VW_DASHBOARD_STATS', 'V') IS NOT NULL
    DROP VIEW VW_DASHBOARD_STATS;
GO

CREATE VIEW VW_DASHBOARD_STATS AS
SELECT
    (SELECT COUNT(1) FROM NGUOI_DUNG WHERE ISNULL(is_banned, 0) = 0)                                                      AS tong_user_active,
    (SELECT COUNT(1) FROM NGUOI_DUNG WHERE ISNULL(is_banned, 0) = 1)                                                      AS tong_user_bi_ban,
    (SELECT COUNT(1) FROM GIAI_DAU  WHERE trang_thai = 'dang_dien_ra'                            AND ISNULL(is_deleted, 0) = 0)   AS giai_dang_chay,
    (SELECT COUNT(1) FROM GIAI_DAU  WHERE trang_thai IN ('mo_dang_ky','sap_dien_ra','dang_dien_ra') AND ISNULL(is_deleted, 0) = 0) AS giai_dang_hoat_dong,
    (SELECT COUNT(1) FROM DOI       WHERE trang_thai = 'dang_hoat_dong')                                                  AS tong_doi_hoat_dong,
    (SELECT COUNT(1) FROM KHIEU_NAI_KET_QUA WHERE trang_thai = 'cho_xu_ly')                                               AS khieu_nai_cho_xu_ly,
    (SELECT COUNT(1) FROM GIAI_DAU  WHERE trang_thai = 'cho_phe_duyet'                           AND ISNULL(is_deleted, 0) = 0)   AS giai_cho_duyet,
    (SELECT COUNT(1) FROM TRO_CHOI  WHERE ISNULL(is_active, 1) = 1)                                                       AS tong_game_active;
GO

-- ==============================================================
-- QUERY MẪU: Tìm HLV xuất sắc nhất giải (Dựa vào đội Top 1)
-- ==============================================================
DECLARE @MaGiaiDau INT = (SELECT TOP 1 ma_giai_dau FROM GIAI_DAU ORDER BY ma_giai_dau DESC);
SELECT
    nd.ten_dang_nhap,
    hsg.in_game_name,
    vt.ten_vi_tri,
    d.ten_doi
FROM BANG_XEP_HANG bxh
JOIN THAM_GIA_GIAI      tgg ON bxh.ma_nhom      = tgg.ma_nhom AND bxh.ma_giai_dau = tgg.ma_giai_dau
JOIN DOI_HINH_THI_DAU   dh  ON dh.ma_tham_gia  = tgg.ma_tham_gia
JOIN NGUOI_DUNG         nd  ON dh.ma_nguoi_dung = nd.ma_nguoi_dung
JOIN HO_SO_IN_GAME      hsg ON nd.ma_nguoi_dung = hsg.ma_nguoi_dung
JOIN DANH_MUC_VI_TRI    vt  ON dh.ma_vi_tri     = vt.ma_vi_tri
JOIN NHOM_DOI           n   ON tgg.ma_nhom       = n.ma_nhom
JOIN DOI                d   ON n.ma_doi           = d.ma_doi
WHERE bxh.ma_giai_dau      = @MaGiaiDau
  AND bxh.thu_hang_hien_tai = 1          -- Lấy đội hạng 1
  AND vt.loai_vi_tri        = 'BanHuanLuyen';
GO