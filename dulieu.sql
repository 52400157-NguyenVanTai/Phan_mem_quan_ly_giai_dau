USE QuanLy_Esports;
GO

/*
  Seed data tương thích schema mới.
  Tài khoản mẫu dùng cùng mật khẩu: password
  BCrypt hash của "123456789":
  '$2a$12$HYM5ai8pKv9FQEXvZmact.udJI.eSup7L7O9Vez7DW/KlT6ULTbPi'
*/

DECLARE @PwdHash NVARCHAR(255) = '$2a$12$HYM5ai8pKv9FQEXvZmact.udJI.eSup7L7O9Vez7DW/KlT6ULTbPi';


-- ========================================
-- 1) USERS (admin + users)
-- ========================================
IF NOT EXISTS (SELECT 1 FROM NGUOI_DUNG WHERE ten_dang_nhap = 'admin')
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
VALUES ('admin', 'admin@esport.local', @PwdHash, 'admin', 'https://i.pravatar.cc/120?img=1', N'Quản trị hệ thống');

IF NOT EXISTS (SELECT 1 FROM NGUOI_DUNG WHERE ten_dang_nhap = 'captain01')
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
VALUES ('captain01', 'captain01@esport.local', @PwdHash, 'user', 'https://i.pravatar.cc/120?img=2', N'Captain đội chính');

IF NOT EXISTS (SELECT 1 FROM NGUOI_DUNG WHERE ten_dang_nhap = 'player01')
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
VALUES ('player01', 'player01@esport.local', @PwdHash, 'user', 'https://i.pravatar.cc/120?img=3', N'Tuyển thủ đường giữa');

IF NOT EXISTS (SELECT 1 FROM NGUOI_DUNG WHERE ten_dang_nhap = 'coach01')
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
VALUES ('coach01', 'coach01@esport.local', @PwdHash, 'user', 'https://i.pravatar.cc/120?img=4', N'Ban huấn luyện');

IF NOT EXISTS (SELECT 1 FROM NGUOI_DUNG WHERE ten_dang_nhap = 'freeagent01')
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
VALUES ('freeagent01', 'freeagent01@esport.local', @PwdHash, 'user', 'https://i.pravatar.cc/120?img=5', N'Free agent để test tuyển dụng');

IF NOT EXISTS (SELECT 1 FROM NGUOI_DUNG WHERE ten_dang_nhap = 'organizer01')
INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
VALUES ('organizer01', 'organizer01@esport.local', @PwdHash, 'user', 'https://i.pravatar.cc/120?img=6', N'User gửi yêu cầu tạo giải');
GO

-- ========================================
-- 2) GAMES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'Liên Minh Huyền Thoại')
INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'Liên Minh Huyền Thoại', 'MOBA');

IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'Valorant')
INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'Valorant', 'FPS');

IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'PUBG')
INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'PUBG', 'BATTLEROYALE');
GO

DECLARE @MaLoL INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Liên Minh Huyền Thoại');
DECLARE @MaValorant INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Valorant');

-- ========================================
-- 3) POSITIONS
-- ========================================
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @MaLoL AND ky_hieu = 'MID')
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri)
VALUES (@MaLoL, N'Đường Giữa', 'MID', 'ChuyenMon');

IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @MaLoL AND ky_hieu = 'JGL')
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri)
VALUES (@MaLoL, N'Đi Rừng', 'JGL', 'ChuyenMon');

IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @MaLoL AND ky_hieu = 'CO')
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri)
VALUES (@MaLoL, N'Huấn Luyện Viên', 'CO', 'BanHuanLuyen');

IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @MaValorant AND ky_hieu = 'IGL')
INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri)
VALUES (@MaValorant, N'In-Game Leader', 'IGL', 'ChuyenMon');
GO

DECLARE @MaLoL INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Liên Minh Huyền Thoại');
DECLARE @MaValorant INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Valorant');

DECLARE @UserCaptain INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'captain01');
DECLARE @UserPlayer INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'player01');
DECLARE @UserCoach INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'coach01');
DECLARE @UserFreeAgent INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'freeagent01');
DECLARE @UserOrganizer INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'organizer01');

DECLARE @ViTriMID INT = (SELECT TOP 1 ma_vi_tri FROM DANH_MUC_VI_TRI WHERE ky_hieu = 'MID' AND ma_tro_choi = @MaLoL);
DECLARE @ViTriJGL INT = (SELECT TOP 1 ma_vi_tri FROM DANH_MUC_VI_TRI WHERE ky_hieu = 'JGL' AND ma_tro_choi = @MaLoL);
DECLARE @ViTriCoach INT = (SELECT TOP 1 ma_vi_tri FROM DANH_MUC_VI_TRI WHERE ky_hieu = 'CO' AND ma_tro_choi = @MaLoL);

-- ========================================
-- 4) IN-GAME PROFILES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @UserCaptain AND ma_tro_choi = @MaLoL)
INSERT INTO HO_SO_IN_GAME (ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong)
VALUES (@UserCaptain, @MaLoL, 'Captain#VN1', 'Captain King', @ViTriJGL);

IF NOT EXISTS (SELECT 1 FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @UserPlayer AND ma_tro_choi = @MaLoL)
INSERT INTO HO_SO_IN_GAME (ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong)
VALUES (@UserPlayer, @MaLoL, 'Player#VN1', 'Mid God', @ViTriMID);

IF NOT EXISTS (SELECT 1 FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @UserCoach AND ma_tro_choi = @MaLoL)
INSERT INTO HO_SO_IN_GAME (ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong)
VALUES (@UserCoach, @MaLoL, 'Coach#VN1', 'Coach Master', @ViTriCoach);

IF NOT EXISTS (SELECT 1 FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @UserFreeAgent AND ma_tro_choi = @MaLoL)
INSERT INTO HO_SO_IN_GAME (ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong)
VALUES (@UserFreeAgent, @MaLoL, 'Free#VN1', 'Free Agent', @ViTriMID);
GO

-- ========================================
-- 5) TEAM + SQUAD + MEMBERS
-- ========================================
DECLARE @Manager INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'captain01');
DECLARE @MaLoL_Game INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Liên Minh Huyền Thoại');
DECLARE @MaViTriMid INT = (SELECT TOP 1 ma_vi_tri FROM DANH_MUC_VI_TRI WHERE ky_hieu = 'MID' AND ma_tro_choi = @MaLoL_Game);
DECLARE @MaViTriCoach2 INT = (SELECT TOP 1 ma_vi_tri FROM DANH_MUC_VI_TRI WHERE ky_hieu = 'CO' AND ma_tro_choi = @MaLoL_Game);

IF NOT EXISTS (SELECT 1 FROM DOI WHERE ten_doi = N'Team Phoenix')
INSERT INTO DOI (ten_doi, ma_doi_truong, ma_manager, logo_url, slogan, trang_thai)
VALUES (N'Team Phoenix', @Manager, @Manager, 'https://logo.clearbit.com/esport.com', N'Rise from the ashes', 'dang_hoat_dong');

DECLARE @MaDoiPhoenix INT = (SELECT TOP 1 ma_doi FROM DOI WHERE ten_doi = N'Team Phoenix');

IF NOT EXISTS (SELECT 1 FROM NHOM_DOI WHERE ma_doi = @MaDoiPhoenix AND ma_tro_choi = @MaLoL_Game AND ten_nhom = N'Phoenix LoL Main')
INSERT INTO NHOM_DOI (ma_doi, ma_tro_choi, ten_nhom, ma_doi_truong_nhom)
VALUES (@MaDoiPhoenix, @MaLoL_Game, N'Phoenix LoL Main', @Manager);

DECLARE @MaNhomPhoenix INT = (
    SELECT TOP 1 ma_nhom FROM NHOM_DOI
    WHERE ma_doi = @MaDoiPhoenix AND ma_tro_choi = @MaLoL_Game AND ten_nhom = N'Phoenix LoL Main'
);

IF NOT EXISTS (SELECT 1 FROM THANH_VIEN_DOI WHERE ma_nguoi_dung = @Manager AND ma_nhom = @MaNhomPhoenix)
INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, ma_vi_tri, vai_tro_noi_bo, phan_he, trang_thai_duyet, trang_thai_hop_dong)
VALUES (@Manager, @MaNhomPhoenix, NULL, 'leader', 'thi_dau', 'da_duyet', 'dang_hieu_luc');

IF NOT EXISTS (SELECT 1 FROM THANH_VIEN_DOI WHERE ma_nguoi_dung = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'player01') AND ma_nhom = @MaNhomPhoenix)
INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, ma_vi_tri, vai_tro_noi_bo, phan_he, trang_thai_duyet, trang_thai_hop_dong)
VALUES ((SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'player01'), @MaNhomPhoenix, @MaViTriMid, 'captain', 'thi_dau', 'da_duyet', 'dang_hieu_luc');

IF NOT EXISTS (SELECT 1 FROM THANH_VIEN_DOI WHERE ma_nguoi_dung = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'coach01') AND ma_nhom = @MaNhomPhoenix)
INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, ma_vi_tri, vai_tro_noi_bo, phan_he, trang_thai_duyet, trang_thai_hop_dong)
VALUES ((SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'coach01'), @MaNhomPhoenix, @MaViTriCoach2, 'member', 'ban_huan_luyen', 'da_duyet', 'dang_hieu_luc');
GO

-- ========================================
-- 6) RECRUITMENT (post, application, invite)
-- ========================================
DECLARE @DoiRecruit INT = (SELECT TOP 1 ma_doi FROM DOI WHERE ten_doi = N'Team Phoenix');
DECLARE @NhomRecruit INT = (
    SELECT TOP 1 ma_nhom FROM NHOM_DOI
    WHERE ma_doi = @DoiRecruit AND ten_nhom = N'Phoenix LoL Main'
);
DECLARE @ViTriRecruit INT = (
    SELECT TOP 1 ma_vi_tri FROM DANH_MUC_VI_TRI
    WHERE ky_hieu = 'MID' AND ma_tro_choi = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Liên Minh Huyền Thoại')
);
DECLARE @FreeAgentId INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'freeagent01');

IF NOT EXISTS (SELECT 1 FROM BAI_DANG_TUYEN_DUNG WHERE ma_doi = @DoiRecruit AND ma_nhom = @NhomRecruit AND ma_vi_tri = @ViTriRecruit)
INSERT INTO BAI_DANG_TUYEN_DUNG (ma_doi, ma_nhom, ma_vi_tri, noi_dung, trang_thai)
VALUES (@DoiRecruit, @NhomRecruit, @ViTriRecruit, N'Tuyển Mid lane rank Cao Thủ+, luyện tập tối thiểu 5 buổi/tuần.', 'dang_mo');

DECLARE @MaBaiDang INT = (
    SELECT TOP 1 ma_bai_dang FROM BAI_DANG_TUYEN_DUNG
    WHERE ma_doi = @DoiRecruit AND ma_nhom = @NhomRecruit AND ma_vi_tri = @ViTriRecruit
    ORDER BY ma_bai_dang DESC
);

IF NOT EXISTS (SELECT 1 FROM DON_UNG_TUYEN WHERE ma_bai_dang = @MaBaiDang AND ma_ung_vien = @FreeAgentId)
INSERT INTO DON_UNG_TUYEN (ma_bai_dang, ma_ung_vien, trang_thai)
VALUES (@MaBaiDang, @FreeAgentId, 'cho_duyet');

IF NOT EXISTS (SELECT 1 FROM LOI_MOI_GIA_NHAP WHERE ma_nhom = @NhomRecruit AND ma_nguoi_duoc_moi = @FreeAgentId)
INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nhom, ma_nguoi_duoc_moi, trang_thai)
VALUES (@DoiRecruit, @NhomRecruit, @FreeAgentId, 'cho_phan_hoi');
GO

-- ========================================
-- 7) TOURNAMENT REQUEST FLOW
-- ========================================
DECLARE @UserOrg INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'organizer01');
DECLARE @AdminId INT = (SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = 'admin');
DECLARE @GameLoL INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Liên Minh Huyền Thoại');

IF NOT EXISTS (SELECT 1 FROM YEU_CAU_TAO_GIAI_DAU WHERE ten_giai_dau = N'Phoenix Spring Cup 2026')
INSERT INTO YEU_CAU_TAO_GIAI_DAU
(
    ma_nguoi_gui, ten_giai_dau, ma_tro_choi, the_thuc,
    ngay_bat_dau, ngay_ket_thuc, tong_giai_thuong, trang_thai
)
VALUES
(
    @UserOrg, N'Phoenix Spring Cup 2026', @GameLoL, 'dau_theo_bang',
    '2026-06-01', '2026-06-15', 120000000, 'cho_duyet'
);

-- 1 yêu cầu đã duyệt để test màn admin
IF NOT EXISTS (SELECT 1 FROM YEU_CAU_TAO_GIAI_DAU WHERE ten_giai_dau = N'Valorant Pro Open 2026')
INSERT INTO YEU_CAU_TAO_GIAI_DAU
(
    ma_nguoi_gui, ten_giai_dau, ma_tro_choi, the_thuc,
    ngay_bat_dau, ngay_ket_thuc, tong_giai_thuong, trang_thai,
    ma_admin_duyet, thoi_gian_duyet
)
VALUES
(
    @UserOrg, N'Valorant Pro Open 2026', (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Valorant'), 'loai_truc_tiep',
    '2026-07-01', '2026-07-10', 80000000, 'da_duyet',
    @AdminId, GETDATE()
);

IF NOT EXISTS (SELECT 1 FROM GIAI_DAU WHERE ten_giai_dau = N'Valorant Pro Open 2026')
INSERT INTO GIAI_DAU
(
    ten_giai_dau, ma_tro_choi, the_thuc, ngay_bat_dau, ngay_ket_thuc,
    tong_giai_thuong, trang_thai, is_deleted, hien_thi_public
)
VALUES
(
    N'Valorant Pro Open 2026',
    (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Valorant'),
    'loai_truc_tiep', '2026-07-01', '2026-07-10',
    80000000, 'sap_dien_ra', 0, 1
);

IF NOT EXISTS
(
    SELECT 1
    FROM QUAN_TRI_GIAI_DAU
    WHERE ma_giai_dau = (SELECT TOP 1 ma_giai_dau FROM GIAI_DAU WHERE ten_giai_dau = N'Valorant Pro Open 2026')
      AND ma_nguoi_dung = @UserOrg
)
INSERT INTO QUAN_TRI_GIAI_DAU (ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES
(
    (SELECT TOP 1 ma_giai_dau FROM GIAI_DAU WHERE ten_giai_dau = N'Valorant Pro Open 2026'),
    @UserOrg,
    'ban_to_chuc'
);
GO

-- ========================================
-- 8) QUICK CHECK
-- ========================================
SELECT TOP 20 ma_nguoi_dung, ten_dang_nhap, email, vai_tro_he_thong FROM NGUOI_DUNG ORDER BY ma_nguoi_dung DESC;
SELECT TOP 20 * FROM HO_SO_IN_GAME ORDER BY ma_ho_so DESC;
SELECT TOP 20 * FROM DOI ORDER BY ma_doi DESC;
SELECT TOP 20 * FROM NHOM_DOI ORDER BY ma_nhom DESC;
SELECT TOP 20 * FROM THANH_VIEN_DOI ORDER BY ma_thanh_vien DESC;
SELECT TOP 20 * FROM BAI_DANG_TUYEN_DUNG ORDER BY ma_bai_dang DESC;
SELECT TOP 20 * FROM DON_UNG_TUYEN ORDER BY ma_don DESC;
SELECT TOP 20 * FROM LOI_MOI_GIA_NHAP ORDER BY ma_loi_moi DESC;
SELECT TOP 20 * FROM YEU_CAU_TAO_GIAI_DAU ORDER BY ma_yeu_cau DESC;
