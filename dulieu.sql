USE QuanLy_esports;
GO

-- ========================================
-- NGUOI_DUNG
-- ========================================
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro)
VALUES
('admin', 'admin@gmail.com', '123456', 'quan_tri'),
('player01', 'p1@gmail.com', '123456', 'nguoi_choi'),
('player02', 'p2@gmail.com', '123456', 'nguoi_choi'),
('player03', 'p3@gmail.com', '123456', 'nguoi_choi'),
('org01', 'org@gmail.com', '123456', 'to_chuc');

-- ========================================
-- TRO_CHOI
-- ========================================
INSERT INTO TRO_CHOI (ten, the_loai)
VALUES
('Liên Minh Huyền Thoại', 'MOBA'),
('Dota 2', 'MOBA'),
('CS:GO', 'FPS'),
('Valorant', 'FPS'),
('PUBG', 'BATTLEROYALE'),
('Free Fire', 'BATTLEROYALE');

-- ========================================
-- DOI
-- ========================================
INSERT INTO DOI (ten_doi, so_nguoi_moi_doi)
VALUES
('Team Alpha', 5),
('Team Beta', 5),
('Team Gamma', 5);

-- ========================================
-- NGUOI_CHOI
-- ========================================
INSERT INTO NGUOI_CHOI (ma_nguoi_dung, biet_danh, quoc_gia, vai_tro)
VALUES
(2, 'CarryGod', 'VN', 'Carry'),
(3, 'MidKing', 'VN', 'Mid'),
(4, 'SniperPro', 'VN', 'Sniper');

-- ========================================
-- THANH_VIEN_DOI
-- ========================================
INSERT INTO THANH_VIEN_DOI (ma_doi, ma_nguoi_choi, vai_tro)
VALUES
(1, 1, 'Captain'),
(1, 2, 'Member'),
(2, 3, 'Captain');

-- ========================================
-- GIAI_DAU
-- ========================================
INSERT INTO GIAI_DAU (ten_giai_dau, ma_tro_choi, the_thuc, ngay_bat_dau, ngay_ket_thuc, trang_thai, tong_giai_thuong)
VALUES
('Spring Championship', 1, 'dau_theo_bang', '2026-05-01', '2026-05-10', 'sap_dien_ra', 100000000),
('FPS Master Cup', 3, 'loai_truc_tiep', '2026-06-01', '2026-06-07', 'cho_xet_duyet', 50000000);

-- ========================================
-- THAM_GIA_GIAI
-- ========================================
INSERT INTO THAM_GIA_GIAI (ma_giai_dau, ma_doi, ma_nguoi_choi, hat_giong, trang_thai)
VALUES
(1, 1, NULL, 1, 'dang_ky'),
(1, 2, NULL, 2, 'dang_ky'),
(2, NULL, 3, 1, 'dang_thi_dau');

-- ========================================
-- TRAN_DAU
-- ========================================
INSERT INTO TRAN_DAU (ma_giai_dau, vong, so_tran, thoi_gian_du_kien, trang_thai)
VALUES
(1, 1, 1, '2026-05-02 10:00', 'chua_dau'),
(1, 1, 2, '2026-05-02 14:00', 'chua_dau'),
(2, 1, 1, '2026-06-02 09:00', 'chua_dau');

-- ========================================
-- KET_QUA_TRAN
-- ========================================
INSERT INTO KET_QUA_TRAN (ma_tran, ma_thang, chi_tiet_diem)
VALUES
(1, 1, '2-1'),
(2, 2, '2-0'),
(3, 3, '1-0');

-- ========================================
-- LICH_THI_DAU
-- ========================================
INSERT INTO LICH_THI_DAU (ma_tran, bat_dau, ket_thuc)
VALUES
(1, '2026-05-02 10:00', '2026-05-02 12:00'),
(2, '2026-05-02 14:00', '2026-05-02 16:00'),
(3, '2026-06-02 09:00', '2026-06-02 11:00');

-- ========================================
-- GIAI_THUONG
-- ========================================
INSERT INTO GIAI_THUONG (ma_giai_dau, vi_tri, so_tien)
VALUES
(1, 1, 50000000),
(1, 2, 30000000),
(1, 3, 20000000),
(2, 1, 30000000),
(2, 2, 20000000);
