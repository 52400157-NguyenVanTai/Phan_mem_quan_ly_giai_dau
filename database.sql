-- ==============================================================
-- KHỞI TẠO CƠ SỞ DỮ LIỆU
-- ==============================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLy_Esport')
BEGIN
    CREATE DATABASE QuanLy_Esport;
END
GO
USE QuanLy_Esport;
GO

-- ==============================================================
-- 1. HỆ THỐNG TÀI KHOẢN & NGƯỜI DÙNG
-- ==============================================================
IF OBJECT_ID('NGUOI_DUNG', 'U') IS NULL
CREATE TABLE NGUOI_DUNG (
    ma_nguoi_dung INT IDENTITY PRIMARY KEY,
    ten_dang_nhap NVARCHAR(100) UNIQUE,
    email NVARCHAR(150) UNIQUE,
    mat_khau_ma_hoa NVARCHAR(255),
    vai_tro_he_thong NVARCHAR(50) DEFAULT 'user', -- Chỉ phân Admin và User hệ thống
    ngay_tao DATETIME DEFAULT GETDATE(),
    CONSTRAINT chk_vai_tro_ht CHECK (vai_tro_he_thong IN ('admin','user'))
);

IF OBJECT_ID('TRO_CHOI', 'U') IS NULL
CREATE TABLE TRO_CHOI (
    ma_tro_choi INT IDENTITY PRIMARY KEY,
    ten_game NVARCHAR(100),
    the_loai NVARCHAR(100)
    CONSTRAINT chk_the_loai CHECK (the_loai IN ('MOBA', 'FPS', 'BATTLEROYALE'))
);

-- Thay thế bảng NGUOI_CHOI: Mỗi user có profile riêng cho từng game
IF OBJECT_ID('HO_SO_IN_GAME', 'U') IS NULL
CREATE TABLE HO_SO_IN_GAME (
    ma_ho_so INT IDENTITY PRIMARY KEY,
    ma_nguoi_dung INT,
    ma_tro_choi INT,
    in_game_id NVARCHAR(100), -- ID trong game (ví dụ Riot ID)
    in_game_name NVARCHAR(100), -- Tên nhân vật
    ngay_cap_nhat DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi),
    CONSTRAINT uq_profile UNIQUE (ma_nguoi_dung, ma_tro_choi)
);

-- ==============================================================
-- 2. QUẢN LÝ ĐỘI TUYỂN & NHÓM THEO GAME
-- ==============================================================
IF OBJECT_ID('DOI', 'U') IS NULL
CREATE TABLE DOI (
    ma_doi INT IDENTITY PRIMARY KEY,
    ten_doi NVARCHAR(150) UNIQUE,
    ma_doi_truong INT, -- Ai là người tạo/quản lý đội
    logo_url NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_doi_truong) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);

-- Đội sẽ được chia ra các nhóm chuyên biệt theo từng game
IF OBJECT_ID('NHOM_DOI', 'U') IS NULL
CREATE TABLE NHOM_DOI (
    ma_nhom INT IDENTITY PRIMARY KEY,
    ma_doi INT,
    ma_tro_choi INT,
    ten_nhom NVARCHAR(150), -- Ví dụ: Team Flash LoL, Team Flash Liên Quân
    FOREIGN KEY (ma_doi) REFERENCES DOI(ma_doi),
    FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi)
);

IF OBJECT_ID('THANH_VIEN_DOI', 'U') IS NULL
CREATE TABLE THANH_VIEN_DOI (
    ma_thanh_vien INT IDENTITY PRIMARY KEY,
    ma_nguoi_dung INT,
    ma_nhom INT, -- Tham gia vào nhóm nào của đội
    trang_thai_duyet NVARCHAR(50) DEFAULT 'cho_duyet', -- cho_duyet, da_duyet, bi_tu_choi
    ngay_tham_gia DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT chk_trang_thai_tv CHECK (trang_thai_duyet IN ('cho_duyet', 'da_duyet', 'bi_tu_choi'))
);

-- ==============================================================
-- 3. QUẢN LÝ GIẢI ĐẤU
-- ==============================================================
IF OBJECT_ID('GIAI_DAU', 'U') IS NULL
CREATE TABLE GIAI_DAU (
    ma_giai_dau INT IDENTITY PRIMARY KEY,
    ten_giai_dau NVARCHAR(150),
    ma_tro_choi INT NULL, -- NULL nếu là giải hỗn hợp nhiều game
    the_thuc NVARCHAR(50),
    ngay_bat_dau DATETIME,
    ngay_ket_thuc DATETIME,
    tong_giai_thuong DECIMAL(12,2),
    trang_thai NVARCHAR(50) DEFAULT 'cho_xet_duyet',
    FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi),
    CONSTRAINT chk_the_thuc CHECK (the_thuc IN ('loai_truc_tiep','nhanh_thanh_nhanh_thua','dau_theo_bang','vong_tron_tinh_diem','hon_hop')),
    CONSTRAINT chk_trang_thai_giai CHECK (trang_thai IN ('cho_xet_duyet','tu_choi','sap_dien_ra','dang_dien_ra','ket_thuc'))
);

-- Cấp quyền Ban Tổ Chức hoặc Trọng tài cho từng Giải đấu cụ thể
IF OBJECT_ID('QUAN_TRI_GIAI_DAU', 'U') IS NULL
CREATE TABLE QUAN_TRI_GIAI_DAU (
    ma_giai_dau INT,
    ma_nguoi_dung INT,
    vai_tro_giai NVARCHAR(50), -- ban_to_chuc, trong_tai
    PRIMARY KEY (ma_giai_dau, ma_nguoi_dung),
    FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT chk_vai_tro_giai CHECK (vai_tro_giai IN ('ban_to_chuc', 'trong_tai'))
);

IF OBJECT_ID('THAM_GIA_GIAI', 'U') IS NULL
CREATE TABLE THAM_GIA_GIAI (
    ma_tham_gia INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT,
    ma_nhom INT, -- Nhóm đội đăng ký tham gia
    trang_thai_duyet NVARCHAR(50) DEFAULT 'cho_duyet',
    hat_giong INT NULL,
    FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom)
);

-- ==============================================================
-- 4. TRẬN ĐẤU & KẾT QUẢ (CÓ AUDIT LOG)
-- ==============================================================
IF OBJECT_ID('TRAN_DAU', 'U') IS NULL
CREATE TABLE TRAN_DAU (
    ma_tran INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT,
    ma_trong_tai INT NULL, -- Trọng tài phụ trách trận này
    vong_dau NVARCHAR(50), -- Ví dụ: Vòng Bảng, Bán Kết, Chung Kết
    the_thuc_tran NVARCHAR(50), -- BO1, BO3, BO5, SinhTon
    thoi_gian_bat_dau DATETIME,
    thoi_gian_ket_thuc DATETIME,
    trang_thai NVARCHAR(50) DEFAULT 'chua_dau',
    FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    FOREIGN KEY (ma_trong_tai) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT chk_tran_trangthai CHECK (trang_thai IN ('chua_dau','dang_dau','da_hoan_thanh','huy_bo'))
);

-- Liên kết các đội tham gia vào 1 trận đấu (Rất cần thiết cho Battle Royale có nhiều đội 1 trận)
IF OBJECT_ID('CHI_TIET_TRAN_DAU', 'U') IS NULL
CREATE TABLE CHI_TIET_TRAN_DAU (
    ma_tran INT,
    ma_nhom INT, -- Nhóm thi đấu
    diem_so FLOAT DEFAULT 0, -- Điểm kill/Top hoặc số ván thắng
    thu_hang INT NULL, -- Dành cho Battle Royale
    ket_qua NVARCHAR(50), -- thang, thua, hoa (MOBA/FPS)
    PRIMARY KEY (ma_tran, ma_nhom),
    FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran),
    FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom)
);

-- Quản lý logic cập nhật điểm của trọng tài
IF OBJECT_ID('KET_QUA_TRAN', 'U') IS NULL
CREATE TABLE KET_QUA_TRAN (
    ma_ket_qua INT IDENTITY PRIMARY KEY,
    ma_tran INT UNIQUE,
    thoi_diem_bao_cao_dau_tien DATETIME DEFAULT GETDATE(),
    so_lan_chinh_sua INT DEFAULT 0, -- Để dev check < 1
    thoi_gian_sua_cuoi DATETIME,
    chi_tiet_phu NVARCHAR(MAX), -- Lưu JSON log trận đấu nếu cần
    FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran)
);

-- Bảng Log lưu vết sửa điểm của trọng tài (Audit Log)
IF OBJECT_ID('LICH_SU_SUA_KET_QUA', 'U') IS NULL
CREATE TABLE LICH_SU_SUA_KET_QUA (
    ma_log INT IDENTITY PRIMARY KEY,
    ma_tran INT,
    ma_trong_tai_sua INT,
    thoi_gian_sua DATETIME DEFAULT GETDATE(),
    ly_do_sua NVARCHAR(MAX),
    FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran),
    FOREIGN KEY (ma_trong_tai_sua) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);

-- ==============================================================
-- 5. HỆ THỐNG THÔNG BÁO (NOTIFICATIONS)
-- ==============================================================
IF OBJECT_ID('THONG_BAO', 'U') IS NULL
CREATE TABLE THONG_BAO (
    ma_thong_bao INT IDENTITY PRIMARY KEY,
    ma_nguoi_nhan INT,
    tieu_de NVARCHAR(200),
    noi_dung NVARCHAR(MAX),
    loai_thong_bao NVARCHAR(50), -- xin_vao_doi, duyet_giai, moi_trong_tai
    da_doc BIT DEFAULT 0,
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_nguoi_nhan) REFERENCES NGUOI_DUNG(ma_nguoi_dung)
);

-- ==============================================================
-- 6. GIẢI THƯỞNG
-- ==============================================================
IF OBJECT_ID('GIAI_THUONG', 'U') IS NULL
CREATE TABLE GIAI_THUONG (
    ma_giai_thuong INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT,
    vi_tri_top INT, -- Top 1, Top 2...
    so_tien DECIMAL(12,2),
    FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau)
);
GO

SELECT * FROM NGUOI_DUNG;
SELECT * FROM TRO_CHOI;
SELECT * FROM DOI;
SELECT * FROM GIAI_DAU;
SELECT * FROM NGUOI_CHOI;
SELECT * FROM THANH_VIEN_DOI;
SELECT * FROM THAM_GIA_GIAI;
SELECT * FROM TRAN_DAU;
SELECT * FROM KET_QUA_TRAN;
SELECT * FROM LICH_THI_DAU;
SELECT * FROM GIAI_THUONG;
