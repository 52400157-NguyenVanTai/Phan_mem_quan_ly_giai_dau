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

-- ==============================================================
-- 7. BẢNG XẾP HẠNG (LEADERBOARD)
-- ==============================================================
IF OBJECT_ID('BANG_XEP_HANG', 'U') IS NULL
CREATE TABLE BANG_XEP_HANG (
    ma_bxh INT IDENTITY PRIMARY KEY,
    ma_giai_dau INT,
    ma_nhom INT, -- Đội tuyển (Nhóm theo game)
    
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
ALTER TABLE TRAN_DAU ADD ma_giai_doan INT;
-- Ràng buộc Khóa ngoại
ALTER TABLE TRAN_DAU ADD CONSTRAINT FK_TranDau_GiaiDoan FOREIGN KEY (ma_giai_doan) REFERENCES GIAI_DOAN(ma_giai_doan);


-- B. SỬA BẢNG XẾP HẠNG (QUAN TRỌNG NHẤT)
ALTER TABLE BANG_XEP_HANG ADD ma_giai_doan INT;
ALTER TABLE BANG_XEP_HANG ADD CONSTRAINT FK_Bxh_GiaiDoan FOREIGN KEY (ma_giai_doan) REFERENCES GIAI_DOAN(ma_giai_doan);

-- GHI CHÚ CHO DEV: 
-- Ngày trước chúng ta khóa Unique (ma_giai_dau, ma_nhom). 
-- Nhưng bây giờ 1 Đội có thể tham gia Giai đoạn 1, xong đi tiếp vào Giai đoạn 2 của CÙNG 1 GIẢI ĐẤU.
-- Nên chúng ta phải XÓA khóa cũ đi, và thay bằng khóa mới: (ma_giai_doan, ma_nhom).

-- Xóa Unique Key cũ (Tên uq_bxh_nhom có thể khác tùy máy SQL tự sinh, Dev cần check lại tên đúng)
ALTER TABLE BANG_XEP_HANG DROP CONSTRAINT uq_bxh_nhom;

-- Thêm Unique Key mới: Một đội chỉ có 1 dòng xếp hạng trong 1 GIAI_ĐOẠN
ALTER TABLE BANG_XEP_HANG ADD CONSTRAINT UQ_GiaiDoan_Nhom UNIQUE (ma_giai_doan, ma_nhom);
GO

ALTER TABLE GIAI_DAU 
ADD is_deleted BIT DEFAULT 0,          -- Xóa mềm (Chỉ dùng khi giải chưa duyệt)
    hien_thi_public BIT DEFAULT 1;     -- 1: Mọi người thấy, 0: Bị Admin ẩn đi

-- Đảm bảo một nhóm (squad) chỉ xuất hiện 1 lần trong danh sách đăng ký của 1 giải
ALTER TABLE THAM_GIA_GIAI 
ADD CONSTRAINT UQ_GiaiDau_Nhom UNIQUE (ma_giai_dau, ma_nhom);


-- Thêm cột để check nhanh, tránh phải JOIN nhiều bảng khi kiểm tra trùng player
ALTER TABLE DOI_HINH_THI_DAU ADD ma_giai_dau INT;

-- Khóa cặp (Giải đấu, Người dùng) để một người không thể nằm trong 2 đội hình của cùng 1 giải
ALTER TABLE DOI_HINH_THI_DAU 
ADD CONSTRAINT UQ_GiaiDau_Player UNIQUE (ma_giai_dau, ma_nguoi_dung);

-- Khóa ngoại đảm bảo ma_giai_dau hợp lệ
ALTER TABLE DOI_HINH_THI_DAU 
ADD CONSTRAINT FK_DoiHinh_GiaiDau FOREIGN KEY (ma_giai_dau) REFERENCES GIAI_DAU(ma_giai_dau);

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

ALTER TABLE HO_SO_IN_GAME 
ADD ma_vi_tri_so_truong INT;

ALTER TABLE HO_SO_IN_GAME 
ADD CONSTRAINT FK_HoSo_ViTri FOREIGN KEY (ma_vi_tri_so_truong) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri);

-- Tìm HLV xuất sắc nhất giải (Dựa vào đội Top 1)
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
