USE QuanLy_Esports;
GO

-- ============================================================
-- SEED: Dữ liệu nền game và vị trí
-- ============================================================
DELETE FROM HO_SO_IN_GAME;
DELETE FROM DANH_MUC_VI_TRI;
DELETE FROM TRO_CHOI;

SET IDENTITY_INSERT TRO_CHOI ON;
INSERT INTO TRO_CHOI (ma_tro_choi, ten_game, the_loai, is_active) VALUES
(1, N'Arena of Valor',    'MOBA',        1),
(2, N'League of Legends', 'MOBA',        1),
(3, N'Free Fire',         'BATTLEROYALE', 1),
(4, N'PUBG',              'BATTLEROYALE', 1),
(5, N'Valorant',          'FPS',          1),
(6, N'CS:GO',             'FPS',          1);
SET IDENTITY_INSERT TRO_CHOI OFF;

-- ============================================================
-- SEED: Vị trí thi đấu theo từng game + Ban Huấn Luyện chung
-- ============================================================

-- Arena of Valor (5 vị trí)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(1, N'Đường Trên',     'DS',   'ThuDau'),
(1, N'Đi Rừng',        'JG',   'ThuDau'),
(1, N'Đường Giữa',     'MID',  'ThuDau'),
(1, N'Xạ Thủ',         'ADC',  'ThuDau'),
(1, N'Hỗ Trợ',         'SUP',  'ThuDau');

-- League of Legends (5 vị trí)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(2, N'Đường Trên',     'TOP',  'ThuDau'),
(2, N'Đi Rừng',        'JG',   'ThuDau'),
(2, N'Đường Giữa',     'MID',  'ThuDau'),
(2, N'Xạ Thủ',         'ADC',  'ThuDau'),
(2, N'Hỗ Trợ',         'SUP',  'ThuDau');

-- Free Fire (4 vị trí)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(3, N'Rusher',          'RUSH', 'ThuDau'),
(3, N'Sniper',          'SNP',  'ThuDau'),
(3, N'Support',         'SUP',  'ThuDau'),
(3, N'IGL (Chỉ huy)',   'IGL',  'ThuDau');

-- PUBG (4 vị trí)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(4, N'Fragger',         'FRAG', 'ThuDau'),
(4, N'Sniper',          'SNP',  'ThuDau'),
(4, N'Support',         'SUP',  'ThuDau'),
(4, N'IGL (Chỉ huy)',   'IGL',  'ThuDau');

-- Valorant (5 vị trí)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(5, N'Duelist',         'DUEL', 'ThuDau'),
(5, N'Initiator',       'INIT', 'ThuDau'),
(5, N'Controller',      'CTRL', 'ThuDau'),
(5, N'Sentinel',        'SENT', 'ThuDau'),
(5, N'Flex',            'FLEX', 'ThuDau');

-- CS:GO (5 vị trí)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(6, N'Entry Fragger',   'ENTRY','ThuDau'),
(6, N'AWPer',           'AWP',  'ThuDau'),
(6, N'Lurker',          'LURK', 'ThuDau'),
(6, N'Support',         'SUP',  'ThuDau'),
(6, N'IGL (Chỉ huy)',   'IGL',  'ThuDau');

-- Ban Huấn Luyện (chung cho mọi game, ma_tro_choi = NULL)
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES
(NULL, N'Huấn luyện viên trưởng',    'HC',  'HuanLuyen'),
(NULL, N'HLV Chiến thuật',           'CT',  'HuanLuyen'),
(NULL, N'Trợ lý HLV',               'TL',  'HuanLuyen'),
(NULL, N'Quản lý đội',              'QL',  'HuanLuyen');

-- ============================================================
-- SEED: Tài khoản người dùng
-- ============================================================
DELETE FROM NGUOI_DUNG;

INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, bio) 
VALUES 
('admin_tong', 'admin@esports.vn', '$2a$12$RsdYzkI.Pj69T0e3rY0pLuinlWRFgjb4Ar/Tw0zaacV1OuLcjTY12', 'admin', N'Quản trị viên cấp cao của hệ thống'),

('manager_t1', 'manager.t1@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Quản lý đội tuyển T1'),
('manager_geng', 'manager.geng@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Quản lý đội tuyển Gen.G'),
('coach_kkoma', 'kkoma@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Huấn luyện viên trưởng'),
('coach_score', 'score@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Huấn luyện viên chiến thuật'),
('player_faker', 'faker@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Tuyển thủ Đường Giữa - Quỷ Vương'),
('player_chovy', 'chovy@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Tuyển thủ Đường Giữa - Cỗ máy farm'),
('player_oner', 'oner@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Tuyển thủ Đi Rừng'),
('player_canyon', 'canyon@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Tuyển thủ Đi Rừng'),
('player_gumayusi', 'gumayusi@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Tuyển thủ Xạ Thủ'),
('referee_chinh', 'trongtai@esports.vn', '$2a$12$CmghxarlP9QssLWCVdacXOdmLm.vNBA1szNsFV1emK6ZD/blLCX1m', 'user', N'Trọng tài chính bắt giải');

PRINT N'Seed mẫu đã chạy xong (TRO_CHOI, DANH_MUC_VI_TRI, NGUOI_DUNG).';
GO