USE QuanLy_Esports;
GO

SET LANGUAGE Vietnamese;

USE QuanLy_Esports;
GO


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


select * from NGUOI_DUNG;