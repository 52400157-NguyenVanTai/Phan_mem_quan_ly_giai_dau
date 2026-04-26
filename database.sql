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

-- ==============================================================
-- 1. HỆ THỐNG TÀI KHOẢN & NGƯỜI DÙNG
-- ==============================================================
IF OBJECT_ID('NGUOI_DUNG', 'U') IS NULL
CREATE TABLE NGUOI_DUNG (
    ma_nguoi_dung INT IDENTITY PRIMARY KEY,
    ten_dang_nhap NVARCHAR(100) UNIQUE ,
    email NVARCHAR(150) UNIQUE NOT NULL,
    mat_khau_ma_hoa NVARCHAR(255) NOT NULL,
    vai_tro_he_thong NVARCHAR(50) DEFAULT 'user', -- Chỉ phân Admin và User hệ thống
    ngay_tao DATETIME DEFAULT GETDATE(),
    CONSTRAINT chk_vai_tro_ht CHECK (vai_tro_he_thong IN ('admin','user'))
);

IF OBJECT_ID('TRO_CHOI', 'U') IS NULL
CREATE TABLE TRO_CHOI (
    ma_tro_choi INT IDENTITY PRIMARY KEY,
    ten_game NVARCHAR(100),
    the_loai NVARCHAR(100),
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

-- ==============================================================
-- 7. BẢNG XẾP HẠNG (LEADERBOARD)
-- ==============================================================
IF OBJECT_ID('BANG_XEP_HANG', 'U') IS NULL
CREATE TABLE BANG_XEP_HANG (
    ma_bxh INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT,
    ma_nhom INT, -- Đội tuyển (Nhóm theo game)
    ma_giai_doan INT,
    
    -- Thống kê chung
    so_tran_da_dau INT DEFAULT 0,
    
    -- Dành cho MOBA/FPS (Tính Thắng/Thua/Hòa)
    so_tran_thang INT DEFAULT 0,
    so_tran_thua INT DEFAULT 0,
    hieu_so_phu INT DEFAULT 0, -- Hiệu số ván thắng/thua (Kill/Death cho FPS)
    
    -- Dành cho Battle Royale (Tính Điểm)
    tong_diem_hang FLOAT DEFAULT 0, -- Điểm vị trí (Placement Points)
    tong_diem_kill FLOAT DEFAULT 0, -- Điểm hạ gục (Kill Points)
    so_lan_top_1 INT DEFAULT 0, -- Tiêu chí Tie-breaker
    
    -- Điểm tổng quy đổi và Thứ hạng
    diem_tong_ket FLOAT DEFAULT 0, 
    thu_hang_hien_tai INT DEFAULT 0, -- Hạng 1, 2, 3...
    
    FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT uq_bxh_nhom UNIQUE (ma_giai_dau, ma_nhom) -- Một đội chỉ có 1 dòng xếp hạng trong 1 giải
);
GO

-- ==============================================================
-- 8. DANH MỤC VỊ TRÍ THEO GAME (Master Data)
-- ==============================================================
IF OBJECT_ID('DANH_MUC_VI_TRI', 'U') IS NULL
CREATE TABLE DANH_MUC_VI_TRI (
    ma_vi_tri INT IDENTITY PRIMARY KEY,
    ma_tro_choi INT,
    ten_vi_tri NVARCHAR(50), -- Ví dụ: Mid, AD, Jungle, Sniper, IGL...
    ky_hieu NVARCHAR(10), -- Ví dụ: MID, ADC, SUP
    loai_vi_tri NVARCHAR(50), -- ChuyenMon hoặc BanHuanLuyen
    FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi)
);

-- ==============================================================
-- 9. CHI TIẾT CHỈ SỐ NGƯỜI CHƠI THEO TRẬN (Để tính MVP Trận)
-- ==============================================================
IF OBJECT_ID('CHI_TIET_NGUOI_CHOI_TRAN', 'U') IS NULL
CREATE TABLE CHI_TIET_NGUOI_CHOI_TRAN (
    ma_chi_tiet_user_tran INT IDENTITY PRIMARY KEY,
    ma_tran INT,
    ma_nguoi_dung INT,
    ma_vi_tri INT,
    so_kill INT DEFAULT 0,
    so_death INT DEFAULT 0,
    so_assist INT DEFAULT 0,
    diem_kda_tran FLOAT, -- Công thức: (Kill + Assist) / Max(1, Death)
    is_mvp_tran BIT DEFAULT 0, -- Đánh dấu nếu là người giỏi nhất trận
    FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran),
    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri)
);

-- ==============================================================
-- 10. BẢNG XẾP HẠNG CÁ NHÂN GIẢI ĐẤU (Để tính MVP Giải)
-- ==============================================================
IF OBJECT_ID('BANG_XEP_HANG_CA_NHAN', 'U') IS NULL
CREATE TABLE BANG_XEP_HANG_CA_NHAN (
    ma_bxh_ca_nhan INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT,
    ma_nguoi_dung INT,
    tong_kill INT DEFAULT 0,
    tong_death INT DEFAULT 0,
    tong_assist INT DEFAULT 0,
    diem_kda_trung_binh FLOAT DEFAULT 0,
    so_lan_dat_mvp_tran INT DEFAULT 0, -- Tiêu chí phụ cực quan trọng cho MVP Giải
    FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT uq_bxh_ca_nhan UNIQUE (ma_giai_dau, ma_nguoi_dung)
);



-- ==============================================================
-- 11. ĐỘI HÌNH ĐĂNG KÝ THI ĐẤU (Tournament Roster)
-- ==============================================================
IF OBJECT_ID('DOI_HINH_THI_DAU', 'U') IS NULL
CREATE TABLE DOI_HINH_THI_DAU (
    ma_doi_hinh INT IDENTITY PRIMARY KEY,
    ma_tham_gia INT, -- Link tới bảng THAM_GIA_GIAI (Nhóm đội đăng ký giải)
    ma_nguoi_dung INT,
    ma_vi_tri INT, -- Khai báo vị trí đánh giải này (Ví dụ: Mid, AD, HLV)
    is_du_bi BIT DEFAULT 0, -- Có phải dự bị không?
    FOREIGN KEY (ma_tham_gia) REFERENCES THAM_GIA_GIAI(ma_tham_gia),
    FOREIGN KEY (ma_nguoi_dung) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri)
);

-- ==============================================================
-- BẢNG GIAI_ĐOẠN (Quản lý các giai đoạn của một giải đấu)
-- ==============================================================
IF OBJECT_ID('GIAI_DOAN', 'U') IS NULL
CREATE TABLE GIAI_DOAN (
    ma_giai_doan INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT NOT NULL,
    ten_giai_doan NVARCHAR(100) NOT NULL, -- VD: Vòng Bảng, Tứ Kết
    the_thuc NVARCHAR(50) NOT NULL,
    thu_tu INT NOT NULL, -- Thứ tự diễn ra: 1, 2, 3...
    so_doi_di_tiep INT DEFAULT 0, -- Số đội lấy đi tiếp (0 = không cắt đội, hoặc vòng cuối)
    CONSTRAINT FK_GiaiDoan_GiaiDau FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau),
    CONSTRAINT CHK_TheThuc_GiaiDoan CHECK (the_thuc IN (
        'loai_truc_tiep', 
        'nhanh_thang_nhanh_thua', 
        'vong_tron', 
        'league_bang_cheo', 
        'thuy_si', 
        'champion_rush'
    )),
    CONSTRAINT CHK_ThuTu_GiaiDoan CHECK (thu_tu > 0),
    CONSTRAINT CHK_SoDoiDiTiep CHECK (so_doi_di_tiep >= 0),
    CONSTRAINT UQ_GiaiDau_ThuTu UNIQUE (ma_giai_dau, thu_tu)
);
GO


-- A. SỬA BẢNG TRẬN ĐẤU
-- Thêm cột Giai đoạn vào Trận đấu
IF COL_LENGTH('TRAN_DAU', 'ma_giai_doan') IS NULL
    ALTER TABLE TRAN_DAU ADD ma_giai_doan INT;
-- Ràng buộc Khóa ngoại
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TranDau_GiaiDoan')
    ALTER TABLE TRAN_DAU ADD CONSTRAINT FK_TranDau_GiaiDoan FOREIGN KEY (ma_giai_doan) REFERENCES GIAI_DOAN(ma_giai_doan);


-- B. SỬA BẢNG XẾP HẠNG (QUAN TRỌNG NHẤT)
IF COL_LENGTH('BANG_XEP_HANG', 'ma_giai_doan') IS NULL
    ALTER TABLE BANG_XEP_HANG ADD ma_giai_doan INT;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Bxh_GiaiDoan')
    ALTER TABLE BANG_XEP_HANG ADD CONSTRAINT FK_Bxh_GiaiDoan FOREIGN KEY (ma_giai_doan) REFERENCES GIAI_DOAN(ma_giai_doan);

-- GHI CHÚ CHO DEV: 
-- Ngày trước chúng ta khóa Unique (ma_giai_dau, ma_nhom). 
-- Nhưng bây giờ 1 Đội có thể tham gia Giai đoạn 1, xong đi tiếp vào Giai đoạn 2 của CÙNG 1 GIẢI ĐẤU.
-- Nên chúng ta phải XÓA khóa cũ đi, và thay bằng khóa mới: (ma_giai_doan, ma_nhom).

-- Xóa Unique Key cũ (Tên uq_bxh_nhom có thể khác tùy máy SQL tự sinh, Dev cần check lại tên đúng)
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'uq_bxh_nhom' AND parent_object_id = OBJECT_ID('BANG_XEP_HANG'))
    ALTER TABLE BANG_XEP_HANG DROP CONSTRAINT uq_bxh_nhom;

-- Thêm Unique Key mới: Một đội chỉ có 1 dòng xếp hạng trong 1 GIAI_ĐOẠN
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_GiaiDoan_Nhom' AND parent_object_id = OBJECT_ID('BANG_XEP_HANG'))
    ALTER TABLE BANG_XEP_HANG ADD CONSTRAINT UQ_GiaiDoan_Nhom UNIQUE (ma_giai_doan, ma_nhom);
GO

IF COL_LENGTH('GIAI_DAU', 'is_deleted') IS NULL
    ALTER TABLE GIAI_DAU ADD is_deleted BIT DEFAULT 0;
IF COL_LENGTH('GIAI_DAU', 'hien_thi_public') IS NULL
    ALTER TABLE GIAI_DAU ADD hien_thi_public BIT DEFAULT 1;

-- Đảm bảo một nhóm (squad) chỉ xuất hiện 1 lần trong danh sách đăng ký của 1 giải
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_GiaiDau_Nhom' AND parent_object_id = OBJECT_ID('THAM_GIA_GIAI'))
    ALTER TABLE THAM_GIA_GIAI ADD CONSTRAINT UQ_GiaiDau_Nhom UNIQUE (ma_giai_dau, ma_nhom);


-- Thêm cột để check nhanh, tránh phải JOIN nhiều bảng khi kiểm tra trùng player
IF COL_LENGTH('DOI_HINH_THI_DAU', 'ma_giai_dau') IS NULL
    ALTER TABLE DOI_HINH_THI_DAU ADD ma_giai_dau INT;

-- Khóa cặp (Giải đấu, Người dùng) để một người không thể nằm trong 2 đội hình của cùng 1 giải
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_GiaiDau_Player' AND parent_object_id = OBJECT_ID('DOI_HINH_THI_DAU'))
    ALTER TABLE DOI_HINH_THI_DAU ADD CONSTRAINT UQ_GiaiDau_Player UNIQUE (ma_giai_dau, ma_nguoi_dung);

-- Khóa ngoại đảm bảo ma_giai_dau hợp lệ
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DoiHinh_GiaiDau')
    ALTER TABLE DOI_HINH_THI_DAU ADD CONSTRAINT FK_DoiHinh_GiaiDau FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau);

GO
IF OBJECT_ID('SP_XoaXachGiaiDau', 'P') IS NOT NULL
    DROP PROCEDURE SP_XoaXachGiaiDau;
GO

CREATE PROCEDURE SP_XoaXachGiaiDau
    @MaGiaiDau INT
AS
BEGIN
    -- Bắt đầu một Transaction để đảm bảo tính an toàn
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. XÓA NHÁNH TRẬN ĐẤU (Xóa Cháu trước, Con sau)
        -- Xóa lịch sử sửa điểm của các trận trong giải
        DELETE FROM LICH_SU_SUA_KET_QUA 
        WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        
        -- Xóa kết quả trận đấu
        DELETE FROM KET_QUA_TRAN 
        WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);
        
        -- Xóa chi tiết thống kê K/D/A của người chơi trong trận
        DELETE FROM CHI_TIET_NGUOI_CHOI_TRAN 
        WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);

        -- Xóa điểm số của các đội trong trận
        DELETE FROM CHI_TIET_TRAN_DAU 
        WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);

        -- Xóa khiếu nại kết quả theo trận trước khi xóa bảng TRAN_DAU
        DELETE FROM KHIEU_NAI_KET_QUA
        WHERE ma_tran IN (SELECT ma_tran FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau);

        -- Cuối cùng mới xóa Trận đấu
        DELETE FROM TRAN_DAU WHERE ma_giai_dau = @MaGiaiDau;


        -- 2. XÓA NHÁNH ĐỘI HÌNH VÀ THAM GIA
        -- Xóa đội hình thi đấu (Roster)
        DELETE FROM DOI_HINH_THI_DAU 
        WHERE ma_giai_dau = @MaGiaiDau;

        -- Xóa danh sách đội tham gia giải
        DELETE FROM THAM_GIA_GIAI WHERE ma_giai_dau = @MaGiaiDau;


        -- 3. XÓA CÁC BẢNG LIÊN KẾT KHÁC CỦA GIẢI
        DELETE FROM BANG_XEP_HANG WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM BANG_XEP_HANG_CA_NHAN WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM QUAN_TRI_GIAI_DAU WHERE ma_giai_dau = @MaGiaiDau;
        DELETE FROM GIAI_THUONG WHERE ma_giai_dau = @MaGiaiDau;


        -- 4. BƯỚC CUỐI: XÓA CHÍNH GIẢI ĐẤU ĐÓ (Xóa Cha)
        DELETE FROM GIAI_DAU WHERE ma_giai_dau = @MaGiaiDau;

        -- Nếu mọi thứ suôn sẻ, lưu thay đổi
        COMMIT TRANSACTION;
        PRINT 'Đã xóa thành công toàn bộ dữ liệu của giải đấu!';
    END TRY
    BEGIN CATCH
        -- Nếu có bất kỳ lỗi nào xảy ra ở các bước trên, hoàn tác lại toàn bộ
        ROLLBACK TRANSACTION;
        
        -- Trả về thông báo lỗi cho C#
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

IF COL_LENGTH('HO_SO_IN_GAME', 'ma_vi_tri_so_truong') IS NULL
    ALTER TABLE HO_SO_IN_GAME ADD ma_vi_tri_so_truong INT;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoSo_ViTri')
    ALTER TABLE HO_SO_IN_GAME ADD CONSTRAINT FK_HoSo_ViTri FOREIGN KEY (ma_vi_tri_so_truong) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri);

-- Tìm HLV xuất sắc nhất giải (Dựa vào đội Top 1)
DECLARE @MaGiaiDau INT = (SELECT TOP 1 ma_giai_dau FROM GIAI_DAU ORDER BY ma_giai_dau DESC);
SELECT 
    nd.ten_dang_nhap,
    hsg.in_game_name,
    vt.ten_vi_tri,
    d.ten_doi
FROM BANG_XEP_HANG bxh
JOIN THAM_GIA_GIAI tgg ON bxh.ma_nhom = tgg.ma_nhom AND bxh.ma_giai_dau = tgg.ma_giai_dau
JOIN DOI_HINH_THI_DAU dh ON dh.ma_tham_gia = tgg.ma_tham_gia
JOIN NGUOI_DUNG nd ON dh.ma_nguoi_dung = nd.ma_nguoi_dung
JOIN HO_SO_IN_GAME hsg ON nd.ma_nguoi_dung = hsg.ma_nguoi_dung
JOIN DANH_MUC_VI_TRI vt ON dh.ma_vi_tri = vt.ma_vi_tri
JOIN NHOM_DOI n ON tgg.ma_nhom = n.ma_nhom
JOIN DOI d ON n.ma_doi = d.ma_doi
WHERE bxh.ma_giai_dau = @MaGiaiDau
  AND bxh.thu_hang_hien_tai = 1 -- Lấy đội hạng 1
  AND vt.loai_vi_tri = 'BanHuanLuyen';

-- ============================================================== 
-- 12. MIGRATION CHO 6 MODULE CỐT LÕI (Identity - Profile - Team - Squad - Recruitment - RBAC)
-- ============================================================== 

IF COL_LENGTH('NGUOI_DUNG', 'avatar_url') IS NULL
    ALTER TABLE NGUOI_DUNG ADD avatar_url NVARCHAR(255) NULL;

IF COL_LENGTH('NGUOI_DUNG', 'bio') IS NULL
    ALTER TABLE NGUOI_DUNG ADD bio NVARCHAR(500) NULL;

IF COL_LENGTH('DOI', 'ma_manager') IS NULL
    ALTER TABLE DOI ADD ma_manager INT NULL;

IF COL_LENGTH('DOI', 'slogan') IS NULL
    ALTER TABLE DOI ADD slogan NVARCHAR(300) NULL;

IF COL_LENGTH('DOI', 'trang_thai') IS NULL
    ALTER TABLE DOI ADD trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_DOI_TRANGTHAI DEFAULT 'dang_hoat_dong';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DOI_MANAGER')
    ALTER TABLE DOI ADD CONSTRAINT FK_DOI_MANAGER FOREIGN KEY (ma_manager) REFERENCES NGUOI_DUNG(ma_nguoi_dung);

IF COL_LENGTH('NHOM_DOI', 'ma_doi_truong_nhom') IS NULL
    ALTER TABLE NHOM_DOI ADD ma_doi_truong_nhom INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NHOM_DOI_DOI_TRUONG')
    ALTER TABLE NHOM_DOI ADD CONSTRAINT FK_NHOM_DOI_DOI_TRUONG FOREIGN KEY (ma_doi_truong_nhom) REFERENCES NGUOI_DUNG(ma_nguoi_dung);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_DOI_GAME_TENNHOM' AND object_id = OBJECT_ID('NHOM_DOI'))
    CREATE UNIQUE INDEX UQ_DOI_GAME_TENNHOM ON NHOM_DOI(ma_doi, ma_tro_choi, ten_nhom);

IF COL_LENGTH('THANH_VIEN_DOI', 'ma_vi_tri') IS NULL
    ALTER TABLE THANH_VIEN_DOI ADD ma_vi_tri INT NULL;

IF COL_LENGTH('THANH_VIEN_DOI', 'vai_tro_noi_bo') IS NULL
    ALTER TABLE THANH_VIEN_DOI ADD vai_tro_noi_bo NVARCHAR(20) NULL;

IF COL_LENGTH('THANH_VIEN_DOI', 'vai_tro_noi_bo') IS NOT NULL
    EXEC('UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = ''member'' WHERE vai_tro_noi_bo IS NULL;');

IF COL_LENGTH('THANH_VIEN_DOI', 'phan_he') IS NULL
    ALTER TABLE THANH_VIEN_DOI ADD phan_he NVARCHAR(30) NULL;

IF COL_LENGTH('THANH_VIEN_DOI', 'phan_he') IS NOT NULL
    EXEC('UPDATE THANH_VIEN_DOI SET phan_he = ''thi_dau'' WHERE phan_he IS NULL;');

IF COL_LENGTH('THANH_VIEN_DOI', 'trang_thai_hop_dong') IS NULL
    ALTER TABLE THANH_VIEN_DOI ADD trang_thai_hop_dong NVARCHAR(30) NULL;

IF COL_LENGTH('THANH_VIEN_DOI', 'trang_thai_hop_dong') IS NOT NULL
    EXEC('UPDATE THANH_VIEN_DOI SET trang_thai_hop_dong = ''dang_hieu_luc'' WHERE trang_thai_hop_dong IS NULL;');

IF COL_LENGTH('THANH_VIEN_DOI', 'vai_tro_noi_bo') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_THANHVIEN_VAITRO')
    EXEC('ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT DF_THANHVIEN_VAITRO DEFAULT ''member'' FOR vai_tro_noi_bo;');

IF COL_LENGTH('THANH_VIEN_DOI', 'phan_he') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_THANHVIEN_PHANHE')
    EXEC('ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT DF_THANHVIEN_PHANHE DEFAULT ''thi_dau'' FOR phan_he;');

IF COL_LENGTH('THANH_VIEN_DOI', 'trang_thai_hop_dong') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_THANHVIEN_HOPDONG')
    EXEC('ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT DF_THANHVIEN_HOPDONG DEFAULT ''dang_hieu_luc'' FOR trang_thai_hop_dong;');

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_THANHVIEN_VITRI')
    ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT FK_THANHVIEN_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri);

IF COL_LENGTH('THANH_VIEN_DOI', 'vai_tro_noi_bo') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_THANHVIEN_VAITRO_NOIBO')
    EXEC('ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT CHK_THANHVIEN_VAITRO_NOIBO CHECK (vai_tro_noi_bo IN (''leader'', ''captain'', ''member''));');

IF COL_LENGTH('THANH_VIEN_DOI', 'phan_he') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_THANHVIEN_PHANHE')
    EXEC('ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT CHK_THANHVIEN_PHANHE CHECK (phan_he IN (''thi_dau'', ''ban_huan_luyen''));');

IF COL_LENGTH('THANH_VIEN_DOI', 'trang_thai_hop_dong') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_THANHVIEN_HOPDONG')
    EXEC('ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT CHK_THANHVIEN_HOPDONG CHECK (trang_thai_hop_dong IN (''dang_hieu_luc'', ''tu_do'', ''da_giai_phong''));');

IF OBJECT_ID('BAI_DANG_TUYEN_DUNG', 'U') IS NULL
CREATE TABLE BAI_DANG_TUYEN_DUNG (
    ma_bai_dang INT IDENTITY PRIMARY KEY,
    ma_doi INT NOT NULL,
    ma_nhom INT NOT NULL,
    ma_vi_tri INT NOT NULL,
    noi_dung NVARCHAR(500) NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL DEFAULT 'dang_mo',
    ngay_tao DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ma_doi) REFERENCES DOI(ma_doi),
    FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
    FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
    CONSTRAINT CHK_BAIDANG_TRANGTHAI CHECK (trang_thai IN ('dang_mo', 'tam_dong', 'da_dong'))
);

IF OBJECT_ID('DON_UNG_TUYEN', 'U') IS NULL
CREATE TABLE DON_UNG_TUYEN (
    ma_don INT IDENTITY PRIMARY KEY,
    ma_bai_dang INT NOT NULL,
    ma_ung_vien INT NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL DEFAULT 'cho_duyet',
    ngay_tao DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ma_bai_dang) REFERENCES BAI_DANG_TUYEN_DUNG(ma_bai_dang),
    FOREIGN KEY (ma_ung_vien) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_DON_BAIDANG_UNGVIEN UNIQUE (ma_bai_dang, ma_ung_vien),
    CONSTRAINT CHK_DONUNGTUYEN_TRANGTHAI CHECK (trang_thai IN ('cho_duyet', 'chap_nhan', 'tu_choi'))
);

IF OBJECT_ID('LOI_MOI_GIA_NHAP', 'U') IS NULL
CREATE TABLE LOI_MOI_GIA_NHAP (
    ma_loi_moi INT IDENTITY PRIMARY KEY,
    ma_doi INT NOT NULL,
    ma_nhom INT NOT NULL,
    ma_nguoi_duoc_moi INT NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL DEFAULT 'cho_phan_hoi',
    ngay_tao DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ma_doi) REFERENCES DOI(ma_doi),
    FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
    FOREIGN KEY (ma_nguoi_duoc_moi) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT UQ_LOIMOI_NHOM_USER UNIQUE (ma_nhom, ma_nguoi_duoc_moi),
    CONSTRAINT CHK_LOIMOI_TRANGTHAI CHECK (trang_thai IN ('cho_phan_hoi', 'chap_nhan', 'tu_choi'))
);

IF OBJECT_ID('YEU_CAU_TAO_GIAI_DAU', 'U') IS NULL
CREATE TABLE YEU_CAU_TAO_GIAI_DAU (
    ma_yeu_cau INT IDENTITY PRIMARY KEY,
    ma_nguoi_gui INT NOT NULL,
    ten_giai_dau NVARCHAR(150) NOT NULL,
    ma_tro_choi INT NULL,
    the_thuc NVARCHAR(50) NOT NULL,
    ngay_bat_dau DATETIME NOT NULL,
    ngay_ket_thuc DATETIME NOT NULL,
    tong_giai_thuong DECIMAL(12,2) NOT NULL,
    trang_thai NVARCHAR(20) NOT NULL DEFAULT 'cho_duyet',
    ma_admin_duyet INT NULL,
    ly_do_huy NVARCHAR(500) NULL,
    thoi_gian_gui DATETIME NOT NULL DEFAULT GETDATE(),
    thoi_gian_duyet DATETIME NULL,
    FOREIGN KEY (ma_nguoi_gui) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    FOREIGN KEY (ma_tro_choi) REFERENCES TRO_CHOI(ma_tro_choi),
    FOREIGN KEY (ma_admin_duyet) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_YEUCAU_TRANGTHAI CHECK (trang_thai IN ('cho_duyet', 'da_duyet', 'tu_choi'))
);

-- ============================================================== 
-- 13. MIGRATION TRỤ CỘT 2 - TOURNAMENT BUILDER
-- ============================================================== 

IF COL_LENGTH('GIAI_DAU', 'ma_nguoi_tao') IS NULL
    ALTER TABLE GIAI_DAU ADD ma_nguoi_tao INT NULL;

IF COL_LENGTH('GIAI_DAU', 'banner_url') IS NULL
    ALTER TABLE GIAI_DAU ADD banner_url NVARCHAR(400) NULL;

IF COL_LENGTH('GIAI_DAU', 'thoi_gian_mo_dang_ky') IS NULL
    ALTER TABLE GIAI_DAU ADD thoi_gian_mo_dang_ky DATETIME NULL;

IF COL_LENGTH('GIAI_DAU', 'thoi_gian_dong_dang_ky') IS NULL
    ALTER TABLE GIAI_DAU ADD thoi_gian_dong_dang_ky DATETIME NULL;

IF COL_LENGTH('GIAI_DAU', 'thoi_gian_khoa') IS NULL
    ALTER TABLE GIAI_DAU ADD thoi_gian_khoa DATETIME NULL;

IF COL_LENGTH('GIAI_DAU', 'ma_nguoi_khoa') IS NULL
    ALTER TABLE GIAI_DAU ADD ma_nguoi_khoa INT NULL;

IF COL_LENGTH('GIAI_DAU', 'ly_do_khoa') IS NULL
    ALTER TABLE GIAI_DAU ADD ly_do_khoa NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GIAIDAU_NGUOITAO')
    ALTER TABLE GIAI_DAU ADD CONSTRAINT FK_GIAIDAU_NGUOITAO FOREIGN KEY (ma_nguoi_tao) REFERENCES NGUOI_DUNG(ma_nguoi_dung);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GIAIDAU_NGUOIKHOA')
    ALTER TABLE GIAI_DAU ADD CONSTRAINT FK_GIAIDAU_NGUOIKHOA FOREIGN KEY (ma_nguoi_khoa) REFERENCES NGUOI_DUNG(ma_nguoi_dung);

UPDATE GIAI_DAU SET trang_thai = 'cho_phe_duyet' WHERE trang_thai = 'cho_xet_duyet';
UPDATE GIAI_DAU SET trang_thai = 'khoa' WHERE trang_thai = 'tu_choi';

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'chk_trang_thai_giai' AND parent_object_id = OBJECT_ID('GIAI_DAU'))
    ALTER TABLE GIAI_DAU DROP CONSTRAINT chk_trang_thai_giai;

ALTER TABLE GIAI_DAU
ADD CONSTRAINT chk_trang_thai_giai CHECK (trang_thai IN ('ban_nhap', 'cho_phe_duyet', 'mo_dang_ky', 'sap_dien_ra', 'dang_dien_ra', 'ket_thuc', 'khoa'));

DECLARE @DefaultTrangThaiGiai NVARCHAR(128);
SELECT TOP 1 @DefaultTrangThaiGiai = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('GIAI_DAU')
  AND c.name = 'trang_thai';

DECLARE @SqlDropDefaultTrangThaiGiai NVARCHAR(400);
IF @DefaultTrangThaiGiai IS NOT NULL
BEGIN
    SET @SqlDropDefaultTrangThaiGiai = N'ALTER TABLE GIAI_DAU DROP CONSTRAINT ' + QUOTENAME(@DefaultTrangThaiGiai);
    EXEC(@SqlDropDefaultTrangThaiGiai);
END

ALTER TABLE GIAI_DAU
ADD CONSTRAINT DF_GIAIDAU_TRANGTHAI DEFAULT 'ban_nhap' FOR trang_thai;

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_GIAIDAU_THOIGIAN_DANGKY' AND parent_object_id = OBJECT_ID('GIAI_DAU'))
    ALTER TABLE GIAI_DAU DROP CONSTRAINT CHK_GIAIDAU_THOIGIAN_DANGKY;

EXEC(N'
ALTER TABLE GIAI_DAU
ADD CONSTRAINT CHK_GIAIDAU_THOIGIAN_DANGKY CHECK (
    thoi_gian_dong_dang_ky IS NULL
    OR ngay_bat_dau IS NULL
    OR thoi_gian_dong_dang_ky < ngay_bat_dau
);');

IF COL_LENGTH('GIAI_DOAN', 'diem_nguong_match_point') IS NULL
    ALTER TABLE GIAI_DOAN ADD diem_nguong_match_point INT NULL;

IF COL_LENGTH('GIAI_DOAN', 'trang_thai') IS NULL
    ALTER TABLE GIAI_DOAN ADD trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_GIAIDOAN_TRANGTHAI DEFAULT 'chua_bat_dau';

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_GIAIDOAN_TRANGTHAI')
    EXEC(N'ALTER TABLE GIAI_DOAN ADD CONSTRAINT CHK_GIAIDOAN_TRANGTHAI CHECK (trang_thai IN (''chua_bat_dau'', ''dang_dien_ra'', ''ket_thuc''));');

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_GIAIDOAN_MATCHPOINT' AND parent_object_id = OBJECT_ID('GIAI_DOAN'))
    ALTER TABLE GIAI_DOAN DROP CONSTRAINT CHK_GIAIDOAN_MATCHPOINT;

EXEC(N'ALTER TABLE GIAI_DOAN ADD CONSTRAINT CHK_GIAIDOAN_MATCHPOINT CHECK (diem_nguong_match_point IS NULL OR diem_nguong_match_point > 0);');

IF COL_LENGTH('TRAN_DAU', 'so_vong') IS NULL
    ALTER TABLE TRAN_DAU ADD so_vong INT NULL;

IF COL_LENGTH('TRAN_DAU', 'nhanh_dau') IS NULL
    ALTER TABLE TRAN_DAU ADD nhanh_dau NVARCHAR(30) NULL;

IF COL_LENGTH('TRAN_DAU', 'ma_tran_tiep_theo_thang') IS NULL
    ALTER TABLE TRAN_DAU ADD ma_tran_tiep_theo_thang INT NULL;

IF COL_LENGTH('TRAN_DAU', 'ma_tran_tiep_theo_thua') IS NULL
    ALTER TABLE TRAN_DAU ADD ma_tran_tiep_theo_thua INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TRANDAU_NEXT_WIN')
    ALTER TABLE TRAN_DAU ADD CONSTRAINT FK_TRANDAU_NEXT_WIN FOREIGN KEY (ma_tran_tiep_theo_thang) REFERENCES TRAN_DAU(ma_tran);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TRANDAU_NEXT_LOSE')
    ALTER TABLE TRAN_DAU ADD CONSTRAINT FK_TRANDAU_NEXT_LOSE FOREIGN KEY (ma_tran_tiep_theo_thua) REFERENCES TRAN_DAU(ma_tran);

IF COL_LENGTH('BANG_XEP_HANG', 'is_match_point') IS NULL
    ALTER TABLE BANG_XEP_HANG ADD is_match_point BIT NOT NULL CONSTRAINT DF_BXH_MATCHPOINT DEFAULT 0;

IF COL_LENGTH('THAM_GIA_GIAI', 'trang_thai_tham_gia') IS NULL
    ALTER TABLE THAM_GIA_GIAI ADD trang_thai_tham_gia NVARCHAR(30) NOT NULL CONSTRAINT DF_TGG_TRANGTHAI_THAMGIA DEFAULT 'dang_thi_dau';

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_TGG_TRANGTHAI_THAMGIA')
    EXEC(N'ALTER TABLE THAM_GIA_GIAI ADD CONSTRAINT CHK_TGG_TRANGTHAI_THAMGIA CHECK (trang_thai_tham_gia IN (''dang_thi_dau'', ''di_tiep'', ''bi_loai''));');

GO
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

GO
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
        @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @enabled = 1,
        @description = N'Xóa cứng giải đấu ở trạng thái khóa quá 30 ngày';

    EXEC msdb.dbo.sp_add_jobstep
        @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @step_name = N'Step_Execute_Cleanup',
        @subsystem = N'TSQL',
        @database_name = N'QuanLy_Esports',
        @command = N'EXEC dbo.SP_XoaGiaiBiKhoaQuaHan;';

    EXEC msdb.dbo.sp_add_schedule
        @schedule_name = N'SCH_DAILY_1AM_DON_DEP_GIAI_KHOA',
        @freq_type = 4,
        @freq_interval = 1,
        @active_start_time = 010000;

    EXEC msdb.dbo.sp_attach_schedule
        @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN',
        @schedule_name = N'SCH_DAILY_1AM_DON_DEP_GIAI_KHOA';

    EXEC msdb.dbo.sp_add_jobserver
        @job_name = N'JOB_DON_DEP_GIAI_KHOA_QUA_HAN';
END;
GO

-- Chạy 1 lần để tạo SQL Server Agent Job dọn dẹp tự động
-- EXEC dbo.SP_TaoJob_DonDepGiaiKhoaQuaHan;

-- ============================================================== 
-- 14. TRỤ CỘT 3: CỔNG TRỌNG TÀI & ANTI-CHEAT
-- ============================================================== 
IF COL_LENGTH('TRAN_DAU', 'thoi_gian_nhap_diem') IS NULL
    ALTER TABLE TRAN_DAU ADD thoi_gian_nhap_diem DATETIME NULL;

IF COL_LENGTH('TRAN_DAU', 'so_lan_sua') IS NULL
    ALTER TABLE TRAN_DAU ADD so_lan_sua INT NOT NULL CONSTRAINT DF_TRANDAU_SOLANSUA DEFAULT 0;

IF COL_LENGTH('CHI_TIET_NGUOI_CHOI_TRAN', 'diem_sinh_ton') IS NULL
    ALTER TABLE CHI_TIET_NGUOI_CHOI_TRAN ADD diem_sinh_ton FLOAT NULL;

IF COL_LENGTH('LICH_SU_SUA_KET_QUA', 'nguoi_sua') IS NULL
    ALTER TABLE LICH_SU_SUA_KET_QUA ADD nguoi_sua INT NULL;

IF COL_LENGTH('LICH_SU_SUA_KET_QUA', 'du_lieu_cu') IS NULL
    ALTER TABLE LICH_SU_SUA_KET_QUA ADD du_lieu_cu NVARCHAR(MAX) NULL;

IF COL_LENGTH('LICH_SU_SUA_KET_QUA', 'du_lieu_moi') IS NULL
    ALTER TABLE LICH_SU_SUA_KET_QUA ADD du_lieu_moi NVARCHAR(MAX) NULL;

IF COL_LENGTH('LICH_SU_SUA_KET_QUA', 'ly_do_sua') IS NULL
    ALTER TABLE LICH_SU_SUA_KET_QUA ADD ly_do_sua NVARCHAR(MAX) NULL;

IF COL_LENGTH('LICH_SU_SUA_KET_QUA', 'ma_trong_tai_sua') IS NOT NULL
AND COL_LENGTH('LICH_SU_SUA_KET_QUA', 'nguoi_sua') IS NOT NULL
BEGIN
    EXEC('UPDATE LICH_SU_SUA_KET_QUA SET nguoi_sua = ISNULL(nguoi_sua, ma_trong_tai_sua) WHERE nguoi_sua IS NULL;');
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LSSKQ_NGUOI_SUA')
AND COL_LENGTH('LICH_SU_SUA_KET_QUA', 'nguoi_sua') IS NOT NULL
BEGIN
    EXEC('ALTER TABLE LICH_SU_SUA_KET_QUA ADD CONSTRAINT FK_LSSKQ_NGUOI_SUA FOREIGN KEY (nguoi_sua) REFERENCES NGUOI_DUNG(ma_nguoi_dung);');
END;


IF OBJECT_ID('KHIEU_NAI_KET_QUA', 'U') IS NULL
CREATE TABLE KHIEU_NAI_KET_QUA (
    ma_khieu_nai INT IDENTITY PRIMARY KEY,
    ma_tran INT NOT NULL,
    ma_nhom INT NOT NULL,
    ma_nguoi_gui INT NOT NULL,
    noi_dung NVARCHAR(MAX) NOT NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_KN_TRANGTHAI DEFAULT 'cho_xu_ly',
    ma_admin_xu_ly INT NULL,
    phan_hoi_admin NVARCHAR(MAX) NULL,
    thoi_gian_tao DATETIME NOT NULL CONSTRAINT DF_KN_TAO DEFAULT GETDATE(),
    thoi_gian_xu_ly DATETIME NULL,
    CONSTRAINT FK_KN_TRAN FOREIGN KEY (ma_tran) REFERENCES TRAN_DAU(ma_tran),
    CONSTRAINT FK_KN_NHOM FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
    CONSTRAINT FK_KN_NGUOI_GUI FOREIGN KEY (ma_nguoi_gui) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT FK_KN_ADMIN_XU_LY FOREIGN KEY (ma_admin_xu_ly) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
    CONSTRAINT CHK_KN_TRANGTHAI CHECK (trang_thai IN ('cho_xu_ly', 'da_xu_ly', 'tu_choi'))
);
GO


IF OBJECT_ID('KHIEU_NAI_KET_QUA', 'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_KN_PENDING_TRAN_NHOM'
      AND object_id = OBJECT_ID('KHIEU_NAI_KET_QUA')
)
    CREATE UNIQUE INDEX UX_KN_PENDING_TRAN_NHOM
    ON KHIEU_NAI_KET_QUA(ma_tran, ma_nhom)
    WHERE trang_thai = 'cho_xu_ly';

GO
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

-- Dữ liệu mẫu kiểm tra nhanh
IF OBJECT_ID('NGUOI_DUNG', 'U') IS NOT NULL
    SELECT TOP 20 * FROM NGUOI_DUNG ORDER BY ma_nguoi_dung DESC;
IF OBJECT_ID('HO_SO_IN_GAME', 'U') IS NOT NULL
    SELECT TOP 20 * FROM HO_SO_IN_GAME ORDER BY ma_ho_so DESC;
IF OBJECT_ID('DOI', 'U') IS NOT NULL
    SELECT TOP 20 * FROM DOI ORDER BY ma_doi DESC;
IF OBJECT_ID('NHOM_DOI', 'U') IS NOT NULL
    SELECT TOP 20 * FROM NHOM_DOI ORDER BY ma_nhom DESC;
IF OBJECT_ID('THANH_VIEN_DOI', 'U') IS NOT NULL
    SELECT TOP 20 * FROM THANH_VIEN_DOI ORDER BY ma_thanh_vien DESC;
IF OBJECT_ID('YEU_CAU_TAO_GIAI_DAU', 'U') IS NOT NULL
    SELECT TOP 20 * FROM YEU_CAU_TAO_GIAI_DAU ORDER BY ma_yeu_cau DESC;




-- ==============================================================
-- 15. MIGRATION TRỤ CỘT 5: DATA INTEGRITY & ADMIN
-- ==============================================================

-- M1: Bảng NGUOI_DUNG — thêm cột Ban/Unban
IF COL_LENGTH('NGUOI_DUNG', 'is_banned') IS NULL
    ALTER TABLE NGUOI_DUNG ADD is_banned BIT NOT NULL CONSTRAINT DF_ND_BANNED DEFAULT 0;

IF COL_LENGTH('NGUOI_DUNG', 'ly_do_ban') IS NULL
    ALTER TABLE NGUOI_DUNG ADD ly_do_ban NVARCHAR(500) NULL;

IF COL_LENGTH('NGUOI_DUNG', 'thoi_gian_ban') IS NULL
    ALTER TABLE NGUOI_DUNG ADD thoi_gian_ban DATETIME NULL;

IF COL_LENGTH('NGUOI_DUNG', 'ma_admin_ban') IS NULL
    ALTER TABLE NGUOI_DUNG ADD ma_admin_ban INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ND_ADMIN_BAN')
AND COL_LENGTH('NGUOI_DUNG', 'ma_admin_ban') IS NOT NULL
BEGIN
    EXEC('ALTER TABLE NGUOI_DUNG ADD CONSTRAINT FK_ND_ADMIN_BAN FOREIGN KEY (ma_admin_ban) REFERENCES NGUOI_DUNG(ma_nguoi_dung);');
END;


-- M2: Bảng TRO_CHOI — thêm is_active để ẩn game thay vì xóa
IF COL_LENGTH('TRO_CHOI', 'is_active') IS NULL
    ALTER TABLE TRO_CHOI ADD is_active BIT NOT NULL CONSTRAINT DF_TC_ACTIVE DEFAULT 1;

GO

-- M3: View thống kê nhanh cho Global Dashboard
IF OBJECT_ID('VW_DASHBOARD_STATS', 'V') IS NOT NULL
    DROP VIEW VW_DASHBOARD_STATS;
GO

CREATE VIEW VW_DASHBOARD_STATS AS
SELECT
    (SELECT COUNT(1) FROM NGUOI_DUNG WHERE ISNULL(is_banned, 0) = 0) AS tong_user_active,
    (SELECT COUNT(1) FROM NGUOI_DUNG WHERE ISNULL(is_banned, 0) = 1) AS tong_user_bi_ban,
    (SELECT COUNT(1) FROM GIAI_DAU WHERE trang_thai = 'dang_dien_ra' AND ISNULL(is_deleted, 0) = 0) AS giai_dang_chay,
    (SELECT COUNT(1) FROM GIAI_DAU WHERE trang_thai IN ('mo_dang_ky', 'sap_dien_ra', 'dang_dien_ra') AND ISNULL(is_deleted, 0) = 0) AS giai_dang_hoat_dong,
    (SELECT COUNT(1) FROM DOI WHERE trang_thai = 'dang_hoat_dong') AS tong_doi_hoat_dong,
    (SELECT COUNT(1) FROM KHIEU_NAI_KET_QUA WHERE trang_thai = 'cho_xu_ly') AS khieu_nai_cho_xu_ly,
    (SELECT COUNT(1) FROM GIAI_DAU WHERE trang_thai = 'cho_phe_duyet' AND ISNULL(is_deleted, 0) = 0) AS giai_cho_duyet,
    (SELECT COUNT(1) FROM TRO_CHOI WHERE ISNULL(is_active, 1) = 1) AS tong_game_active;
GO



GO

-- ==============================================================
-- 16. MIGRATION TƯƠNG TÁC GIẢI ĐẤU (Like & Follow)
-- ==============================================================

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
