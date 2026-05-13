-- ================================================================
-- QUANLY_ESPORTS - DU LIEU MAU TONG HOP
-- Chay sau database.sql.
-- Mat khau tai khoan mau trong cac seed: 123456
-- ================================================================


-- ================================================================
-- BEGIN: seed_aov_sample.sql
-- ================================================================

-- Seed du lieu mau AOV:
-- - 2 admin he thong
-- - 2 user thuong doc lap
-- - 5 doi Arena of Valor, moi doi 5 thanh vien gom 1 chu tich, 1 doi truong, 3 thanh vien
-- Mat khau tat ca tai khoan mau: 123456

USE QuanLy_Esports;
GO

SET NOCOUNT ON;

DECLARE @PasswordHash NVARCHAR(255) = N'PBKDF2$100000$AQIDBAUGBwgJCgsMDQ4PEA==$gX0bTflqCjSgps4WRDCI1xtjk/h96ukaUfpnl/iu+QY=';
DECLARE @AOV INT = (SELECT ma_tro_choi FROM DANH_MUC_TRO_CHOI WHERE ten_game = N'Arena of Valor');

IF @AOV IS NULL
BEGIN
    RAISERROR(N'Chua co game Arena of Valor trong DANH_MUC_TRO_CHOI. Hay chay database.sql truoc.', 16, 1);
    RETURN;
END;

DECLARE @NguoiDungMau TABLE (
    username NVARCHAR(100) NOT NULL,
    email NVARCHAR(150) NOT NULL,
    vai_tro_he_thong NVARCHAR(10) NOT NULL,
    team_no INT NULL,
    member_no INT NULL,
    vai_tro_noi_bo NVARCHAR(20) NULL,
    ky_hieu_vi_tri NVARCHAR(20) NULL
);

INSERT INTO @NguoiDungMau (username, email, vai_tro_he_thong, team_no, member_no, vai_tro_noi_bo, ky_hieu_vi_tri)
VALUES
    (N'admin_seed_1', N'admin_seed_1@example.com', N'admin', NULL, NULL, NULL, NULL),
    (N'admin_seed_2', N'admin_seed_2@example.com', N'admin', NULL, NULL, NULL, NULL),
    (N'user_seed_1',  N'user_seed_1@example.com',  N'user',  NULL, NULL, NULL, NULL),
    (N'user_seed_2',  N'user_seed_2@example.com',  N'user',  NULL, NULL, NULL, NULL),

    (N'aov_phoenix_president', N'aov_phoenix_president@example.com', N'user', 1, 1, N'chu_tich', N'DS'),
    (N'aov_phoenix_jungle',    N'aov_phoenix_jungle@example.com',    N'user', 1, 2, N'doi_truong', N'JG'),
    (N'aov_phoenix_mid',       N'aov_phoenix_mid@example.com',       N'user', 1, 3, N'thanh_vien', N'MID'),
    (N'aov_phoenix_ad',        N'aov_phoenix_ad@example.com',        N'user', 1, 4, N'thanh_vien', N'AD'),
    (N'aov_phoenix_support',   N'aov_phoenix_support@example.com',   N'user', 1, 5, N'thanh_vien', N'SP'),

    (N'aov_dragon_president', N'aov_dragon_president@example.com', N'user', 2, 1, N'chu_tich', N'DS'),
    (N'aov_dragon_jungle',    N'aov_dragon_jungle@example.com',    N'user', 2, 2, N'doi_truong', N'JG'),
    (N'aov_dragon_mid',       N'aov_dragon_mid@example.com',       N'user', 2, 3, N'thanh_vien', N'MID'),
    (N'aov_dragon_ad',        N'aov_dragon_ad@example.com',        N'user', 2, 4, N'thanh_vien', N'AD'),
    (N'aov_dragon_support',   N'aov_dragon_support@example.com',   N'user', 2, 5, N'thanh_vien', N'SP'),

    (N'aov_titan_president', N'aov_titan_president@example.com', N'user', 3, 1, N'chu_tich', N'DS'),
    (N'aov_titan_jungle',    N'aov_titan_jungle@example.com',    N'user', 3, 2, N'doi_truong', N'JG'),
    (N'aov_titan_mid',       N'aov_titan_mid@example.com',       N'user', 3, 3, N'thanh_vien', N'MID'),
    (N'aov_titan_ad',        N'aov_titan_ad@example.com',        N'user', 3, 4, N'thanh_vien', N'AD'),
    (N'aov_titan_support',   N'aov_titan_support@example.com',   N'user', 3, 5, N'thanh_vien', N'SP'),

    (N'aov_shadow_president', N'aov_shadow_president@example.com', N'user', 4, 1, N'chu_tich', N'DS'),
    (N'aov_shadow_jungle',    N'aov_shadow_jungle@example.com',    N'user', 4, 2, N'doi_truong', N'JG'),
    (N'aov_shadow_mid',       N'aov_shadow_mid@example.com',       N'user', 4, 3, N'thanh_vien', N'MID'),
    (N'aov_shadow_ad',        N'aov_shadow_ad@example.com',        N'user', 4, 4, N'thanh_vien', N'AD'),
    (N'aov_shadow_support',   N'aov_shadow_support@example.com',   N'user', 4, 5, N'thanh_vien', N'SP'),

    (N'aov_lotus_president', N'aov_lotus_president@example.com', N'user', 5, 1, N'chu_tich', N'DS'),
    (N'aov_lotus_jungle',    N'aov_lotus_jungle@example.com',    N'user', 5, 2, N'doi_truong', N'JG'),
    (N'aov_lotus_mid',       N'aov_lotus_mid@example.com',       N'user', 5, 3, N'thanh_vien', N'MID'),
    (N'aov_lotus_ad',        N'aov_lotus_ad@example.com',        N'user', 5, 4, N'thanh_vien', N'AD'),
    (N'aov_lotus_support',   N'aov_lotus_support@example.com',   N'user', 5, 5, N'thanh_vien', N'SP');

INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong)
SELECT u.username, u.email, @PasswordHash, u.vai_tro_he_thong
FROM @NguoiDungMau u
WHERE NOT EXISTS (
    SELECT 1
    FROM NGUOI_DUNG nd
    WHERE nd.ten_dang_nhap = u.username OR nd.email = u.email
);

DECLARE @DoiMau TABLE (
    team_no INT NOT NULL PRIMARY KEY,
    ten_doi NVARCHAR(150) NOT NULL,
    ten_viet_tat NVARCHAR(20) NOT NULL,
    president_username NVARCHAR(100) NOT NULL,
    slogan NVARCHAR(300) NOT NULL
);

INSERT INTO @DoiMau (team_no, ten_doi, ten_viet_tat, president_username, slogan)
VALUES
    (1, N'AOV Phoenix', N'PHX', N'aov_phoenix_president', N'Lua chien khong tat.'),
    (2, N'AOV Dragon',  N'DRG', N'aov_dragon_president',  N'Ban linh rong thieng.'),
    (3, N'AOV Titan',   N'TTN', N'aov_titan_president',   N'Vung nhu thanh tri.'),
    (4, N'AOV Shadow',  N'SHD', N'aov_shadow_president',  N'Danh nhanh, bien mat nhanh.'),
    (5, N'AOV Lotus',   N'LTS', N'aov_lotus_president',   N'Dep mat, sac don.');

INSERT INTO DOI (ten_doi, ten_viet_tat, ma_doi_truong, ma_tro_choi, slogan, mo_ta, dang_tuyen)
SELECT d.ten_doi, d.ten_viet_tat, nd.ma_nguoi_dung, @AOV, d.slogan, N'Doi mau Arena of Valor dung de test luong quan ly doi.', 1
FROM @DoiMau d
INNER JOIN NGUOI_DUNG nd ON nd.ten_dang_nhap = d.president_username
WHERE NOT EXISTS (
    SELECT 1
    FROM DOI doi
    WHERE doi.ten_doi = d.ten_doi AND doi.ma_tro_choi = @AOV
);

INSERT INTO HO_SO_IN_GAME (ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong, thanh_tich)
SELECT
    nd.ma_nguoi_dung,
    @AOV,
    CONCAT(N'AOV-', UPPER(u.username)),
    u.username,
    vt.ma_vi_tri,
    N'Ho so mau AOV'
FROM @NguoiDungMau u
INNER JOIN NGUOI_DUNG nd ON nd.ten_dang_nhap = u.username
LEFT JOIN DANH_MUC_VI_TRI vt ON vt.ma_tro_choi = @AOV AND vt.ky_hieu = u.ky_hieu_vi_tri
WHERE u.team_no IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM HO_SO_IN_GAME hs
      WHERE hs.ma_nguoi_dung = nd.ma_nguoi_dung AND hs.ma_tro_choi = @AOV
  );

INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_doi, ma_vi_tri, vai_tro_noi_bo, phan_he, trang_thai_duyet, trang_thai_hop_dong)
SELECT
    nd.ma_nguoi_dung,
    doi.ma_doi,
    vt.ma_vi_tri,
    u.vai_tro_noi_bo,
    N'TuyenThu',
    N'da_duyet',
    N'dang_hieu_luc'
FROM @NguoiDungMau u
INNER JOIN @DoiMau dm ON dm.team_no = u.team_no
INNER JOIN DOI doi ON doi.ten_doi = dm.ten_doi AND doi.ma_tro_choi = @AOV
INNER JOIN NGUOI_DUNG nd ON nd.ten_dang_nhap = u.username
LEFT JOIN DANH_MUC_VI_TRI vt ON vt.ma_tro_choi = @AOV AND vt.ky_hieu = u.ky_hieu_vi_tri
WHERE u.team_no IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM THANH_VIEN_DOI tv
      WHERE tv.ma_nguoi_dung = nd.ma_nguoi_dung
        AND tv.ma_doi = doi.ma_doi
        AND tv.trang_thai_duyet = N'da_duyet'
        AND tv.trang_thai_hop_dong = N'dang_hieu_luc'
  );

PRINT N'Da seed 5 doi AOV, 25 thanh vien, 2 admin va 2 user mau. Mat khau: 123456';

GO

-- ================================================================
-- END: seed_aov_sample.sql
-- ================================================================


-- ================================================================
-- BEGIN: seed_test_tournament_8teams.sql
-- ================================================================

-- Seed test tournament data.
-- Run after database.sql.
-- Optional: if you already ran seed_aov_sample.sql, this script reuses those teams.
-- It is idempotent for the tournament named below: rerun will delete and recreate only this test tournament.

USE QuanLy_Esports;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRAN;

DECLARE @TournamentName NVARCHAR(150) = N'Test Swiss + Double Elim 8 Teams';
DECLARE @PasswordHash NVARCHAR(255) = N'PBKDF2$100000$AQIDBAUGBwgJCgsMDQ4PEA==$gX0bTflqCjSgps4WRDCI1xtjk/h96ukaUfpnl/iu+QY=';
DECLARE @AOV INT = (SELECT TOP 1 ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game = N'Arena of Valor');

IF @AOV IS NULL
BEGIN
    RAISERROR(N'Please run database.sql first. Missing Arena of Valor in DANH_MUC_TRO_CHOI.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'admin_seed_1')
BEGIN
    INSERT INTO dbo.NGUOI_DUNG(ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong)
    VALUES(N'admin_seed_1', N'admin_seed_1@example.com', @PasswordHash, N'admin');
END;

DECLARE @Creator INT = (SELECT TOP 1 ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'admin_seed_1');

-- Delete old copy of this test tournament only.
DECLARE @Old TABLE(ma_giai_dau INT PRIMARY KEY);
INSERT INTO @Old(ma_giai_dau)
SELECT ma_giai_dau FROM dbo.GIAI_DAU WHERE ten_giai_dau = @TournamentName;

IF EXISTS (SELECT 1 FROM @Old)
BEGIN
    IF OBJECT_ID('dbo.TRG_LSSKQ_IMMUTABLE', 'TR') IS NOT NULL
        DISABLE TRIGGER dbo.TRG_LSSKQ_IMMUTABLE ON dbo.LICH_SU_SUA_KET_QUA;

    DELETE l
    FROM dbo.LICH_SU_SUA_KET_QUA l
    INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = l.ma_tran
    INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;

    IF OBJECT_ID('dbo.TRG_LSSKQ_IMMUTABLE', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TRG_LSSKQ_IMMUTABLE ON dbo.LICH_SU_SUA_KET_QUA;

    DELETE y FROM dbo.YEU_CAU_MO_KHOA_KET_QUA y INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = y.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE k FROM dbo.KHIEU_NAI_KET_QUA k INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = k.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE kq FROM dbo.KET_QUA_TRAN kq INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = kq.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE kv FROM dbo.KET_QUA_VAN_DAU kv INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = kv.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE cn FROM dbo.CHI_TIET_NGUOI_CHOI_TRAN cn INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = cn.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE ct FROM dbo.CHI_TIET_TRAN_DAU ct INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = ct.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;

    UPDATE td
    SET ma_tran_tiep_theo_thang = NULL,
        ma_tran_tiep_theo_thua = NULL
    FROM dbo.TRAN_DAU td
    INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;

    DELETE td FROM dbo.TRAN_DAU td INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE bcn FROM dbo.BANG_XEP_HANG_CA_NHAN bcn INNER JOIN @Old o ON o.ma_giai_dau = bcn.ma_giai_dau;
    DELETE bxh FROM dbo.BANG_XEP_HANG bxh INNER JOIN @Old o ON o.ma_giai_dau = bxh.ma_giai_dau;
    DELETE dh FROM dbo.DOI_HINH_THI_DAU dh INNER JOIN @Old o ON o.ma_giai_dau = dh.ma_giai_dau;
    DELETE tb FROM dbo.THONG_BAO tb INNER JOIN @Old o ON tb.loai_entity = N'giai_dau' AND tb.ma_entity = o.ma_giai_dau;
    DELETE tt FROM dbo.TUONG_TAC_GIAI_DAU tt INNER JOIN @Old o ON o.ma_giai_dau = tt.ma_giai_dau;
    DELETE tg FROM dbo.THAM_GIA_GIAI tg INNER JOIN @Old o ON o.ma_giai_dau = tg.ma_giai_dau;
    DELETE gt FROM dbo.GIAI_THUONG gt INNER JOIN @Old o ON o.ma_giai_dau = gt.ma_giai_dau;
    DELETE gd FROM dbo.GIAI_DOAN gd INNER JOIN @Old o ON o.ma_giai_dau = gd.ma_giai_dau;
    DELETE tr FROM dbo.TRONG_TAI_GIAI_DAU tr INNER JOIN @Old o ON o.ma_giai_dau = tr.ma_giai_dau;
    DELETE qt FROM dbo.QUAN_TRI_GIAI_DAU qt INNER JOIN @Old o ON o.ma_giai_dau = qt.ma_giai_dau;
    DELETE gd FROM dbo.GIAI_DAU gd INNER JOIN @Old o ON o.ma_giai_dau = gd.ma_giai_dau;
END;

-- Seed 8 teams, 5 members each.
DECLARE @TeamsSeed TABLE(
    team_no INT PRIMARY KEY,
    ten_doi NVARCHAR(150),
    ten_viet_tat NVARCHAR(20),
    slug NVARCHAR(50),
    slogan NVARCHAR(300)
);

INSERT INTO @TeamsSeed(team_no, ten_doi, ten_viet_tat, slug, slogan)
VALUES
(1, N'AOV Phoenix', N'PHX', N'phoenix', N'Lua chien khong tat.'),
(2, N'AOV Dragon', N'DRG', N'dragon', N'Ban linh rong thieng.'),
(3, N'AOV Titan', N'TTN', N'titan', N'Vung nhu thanh tri.'),
(4, N'AOV Shadow', N'SHD', N'shadow', N'Danh nhanh, bien mat nhanh.'),
(5, N'AOV Lotus', N'LTS', N'lotus', N'Dep mat, sac don.'),
(6, N'AOV Eclipse', N'ECL', N'eclipse', N'Che phu ban do.'),
(7, N'AOV Nova', N'NOV', N'nova', N'Bung no dung luc.'),
(8, N'AOV Onyx', N'ONX', N'onyx', N'Lan da den, tim nong.');

DECLARE @Members TABLE(member_no INT, suffix NVARCHAR(30), team_role NVARCHAR(20), pos NVARCHAR(20));
INSERT INTO @Members(member_no, suffix, team_role, pos)
VALUES
(1, N'president', N'chu_tich', N'DS'),
(2, N'jungle', N'doi_truong', N'JG'),
(3, N'mid', N'thanh_vien', N'MID'),
(4, N'ad', N'thanh_vien', N'AD'),
(5, N'support', N'thanh_vien', N'SP');

DECLARE @UsersSeed TABLE(username NVARCHAR(100), email NVARCHAR(150), team_no INT, member_no INT, team_role NVARCHAR(20), pos NVARCHAR(20));
INSERT INTO @UsersSeed(username, email, team_no, member_no, team_role, pos)
SELECT
    N'aov_' + t.slug + N'_' + m.suffix,
    N'aov_' + t.slug + N'_' + m.suffix + N'@example.com',
    t.team_no,
    m.member_no,
    m.team_role,
    m.pos
FROM @TeamsSeed t
CROSS JOIN @Members m;

INSERT INTO dbo.NGUOI_DUNG(ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong)
SELECT u.username, u.email, @PasswordHash, N'user'
FROM @UsersSeed u
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.NGUOI_DUNG nd
    WHERE nd.ten_dang_nhap = u.username OR nd.email = u.email
);

INSERT INTO dbo.DOI(ten_doi, ten_viet_tat, ma_doi_truong, ma_tro_choi, slogan, mo_ta, dang_tuyen)
SELECT t.ten_doi, t.ten_viet_tat, nd.ma_nguoi_dung, @AOV, t.slogan, N'Doi mau AOV dung de test tournament flow.', 1
FROM @TeamsSeed t
INNER JOIN dbo.NGUOI_DUNG nd ON nd.ten_dang_nhap = N'aov_' + t.slug + N'_president'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DOI d
    WHERE d.ten_doi = t.ten_doi AND d.ma_tro_choi = @AOV
);

INSERT INTO dbo.HO_SO_IN_GAME(ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong, thanh_tich)
SELECT nd.ma_nguoi_dung, @AOV, CONCAT(N'AOV-', UPPER(u.username)), u.username, vt.ma_vi_tri, N'Ho so mau AOV'
FROM @UsersSeed u
INNER JOIN dbo.NGUOI_DUNG nd ON nd.ten_dang_nhap = u.username
LEFT JOIN dbo.DANH_MUC_VI_TRI vt ON vt.ma_tro_choi = @AOV AND vt.ky_hieu = u.pos
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.HO_SO_IN_GAME hs
    WHERE hs.ma_nguoi_dung = nd.ma_nguoi_dung AND hs.ma_tro_choi = @AOV
);

INSERT INTO dbo.THANH_VIEN_DOI(ma_nguoi_dung, ma_doi, ma_vi_tri, vai_tro_noi_bo, phan_he, trang_thai_duyet, trang_thai_hop_dong)
SELECT nd.ma_nguoi_dung, d.ma_doi, vt.ma_vi_tri, u.team_role, N'TuyenThu', N'da_duyet', N'dang_hieu_luc'
FROM @UsersSeed u
INNER JOIN @TeamsSeed t ON t.team_no = u.team_no
INNER JOIN dbo.DOI d ON d.ten_doi = t.ten_doi AND d.ma_tro_choi = @AOV
INNER JOIN dbo.NGUOI_DUNG nd ON nd.ten_dang_nhap = u.username
LEFT JOIN dbo.DANH_MUC_VI_TRI vt ON vt.ma_tro_choi = @AOV AND vt.ky_hieu = u.pos
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.THANH_VIEN_DOI tv
    WHERE tv.ma_nguoi_dung = nd.ma_nguoi_dung AND tv.ma_doi = d.ma_doi
);

DECLARE @Teams TABLE(seed INT IDENTITY(1,1), ma_doi INT, ten_doi NVARCHAR(150));
INSERT INTO @Teams(ma_doi, ten_doi)
SELECT d.ma_doi, d.ten_doi
FROM @TeamsSeed ts
INNER JOIN dbo.DOI d ON d.ten_doi = ts.ten_doi AND d.ma_tro_choi = @AOV
ORDER BY ts.team_no;

IF (SELECT COUNT(1) FROM @Teams) <> 8
BEGIN
    RAISERROR(N'Could not seed exactly 8 teams.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;

-- Create tournament.
INSERT INTO dbo.GIAI_DAU(
    ten_giai_dau, ma_tro_choi, ma_nguoi_tao, the_thuc, banner_url, mo_ta,
    so_doi_toi_thieu, so_doi_toi_da, min_members_per_team, tong_giai_thuong,
    trang_thai, dang_mo_dang_ky, is_registration_locked
)
VALUES(
    @TournamentName, @AOV, @Creator, N'hon_hop', N'/Content/avatar-default.svg',
    N'Giai test 8 doi: Vong 1 Thuy Si lay top 4, Vong 2 nhanh thang nhanh thua tim nha vo dich.',
    8, 8, 5, 8000000, N'dang_dien_ra', 0, 1
);

DECLARE @GiaiDau INT = SCOPE_IDENTITY();

INSERT INTO dbo.QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES(@GiaiDau, @Creator, N'ban_to_chuc');

INSERT INTO dbo.GIAI_THUONG(ma_giai_dau, vi_tri_top, ten_giai, gia_tri, so_luong)
VALUES
(@GiaiDau, 1, N'Vo dich', 5000000, 1),
(@GiaiDau, 2, N'A quan', 2000000, 1),
(@GiaiDau, 3, N'Hang ba', 1000000, 1);

INSERT INTO dbo.GIAI_DOAN(ma_giai_dau, ten_giai_doan, the_thuc, thu_tu, so_doi, so_doi_di_tiep, nguong_match_point, bang_diem_json, trang_thai)
VALUES
(@GiaiDau, N'Vong 1 - Thuy Si', N'thuy_si', 1, 8, 4, NULL, N'{"win":3,"loss":0,"top_cut":4}', N'dang_dien_ra'),
(@GiaiDau, N'Vong 2 - Nhanh thang nhanh thua', N'nhanh_thang_nhanh_thua', 2, 4, 1, NULL, N'{"champion":1}', N'chua_bat_dau');

DECLARE @StageSwiss INT = (SELECT ma_giai_doan FROM dbo.GIAI_DOAN WHERE ma_giai_dau = @GiaiDau AND thu_tu = 1);
DECLARE @StageDE INT = (SELECT ma_giai_doan FROM dbo.GIAI_DOAN WHERE ma_giai_dau = @GiaiDau AND thu_tu = 2);

INSERT INTO dbo.THAM_GIA_GIAI(ma_giai_dau, ma_doi, trang_thai_duyet, trang_thai_tham_gia, hat_giong)
SELECT @GiaiDau, ma_doi, N'da_duyet', N'dang_thi_dau', seed
FROM @Teams;

INSERT INTO dbo.BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_doi, thu_hang_hien_tai)
SELECT @GiaiDau, @StageSwiss, ma_doi, seed FROM @Teams;

INSERT INTO dbo.BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_doi, thu_hang_hien_tai)
SELECT @GiaiDau, @StageDE, ma_doi, seed FROM @Teams WHERE seed <= 4;

-- Seed lineups for all teams.
DECLARE @Lineups TABLE(ma_tham_gia INT, ma_giai_dau INT, ma_nguoi_dung INT, ma_vi_tri INT, rn INT);
INSERT INTO @Lineups(ma_tham_gia, ma_giai_dau, ma_nguoi_dung, ma_vi_tri, rn)
SELECT
    tg.ma_tham_gia,
    @GiaiDau,
    tv.ma_nguoi_dung,
    tv.ma_vi_tri,
    ROW_NUMBER() OVER (
        PARTITION BY tg.ma_tham_gia
        ORDER BY CASE tv.vai_tro_noi_bo WHEN N'doi_truong' THEN 1 WHEN N'chu_tich' THEN 2 ELSE 3 END, tv.ma_thanh_vien
    )
FROM dbo.THAM_GIA_GIAI tg
INNER JOIN dbo.THANH_VIEN_DOI tv
    ON tv.ma_doi = tg.ma_doi
   AND tv.trang_thai_duyet = N'da_duyet'
   AND tv.trang_thai_hop_dong = N'dang_hieu_luc'
WHERE tg.ma_giai_dau = @GiaiDau;

INSERT INTO dbo.DOI_HINH_THI_DAU(ma_tham_gia, ma_giai_dau, ma_nguoi_dung, ma_vi_tri, is_du_bi)
SELECT ma_tham_gia, ma_giai_dau, ma_nguoi_dung, ma_vi_tri, 0
FROM @Lineups
WHERE rn <= 5;

-- Stage 1: Swiss round 1 pairs. Further Swiss rounds can be generated manually during testing.
DECLARE @Pairs TABLE(a INT, b INT, label NVARCHAR(100));
INSERT INTO @Pairs(a,b,label)
VALUES
(1,8,N'Swiss Round 1 - Match 1'),
(2,7,N'Swiss Round 1 - Match 2'),
(3,6,N'Swiss Round 1 - Match 3'),
(4,5,N'Swiss Round 1 - Match 4');

DECLARE @a INT, @b INT, @label NVARCHAR(100), @match INT;
DECLARE pair_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT a, b, label FROM @Pairs ORDER BY a;

OPEN pair_cursor;
FETCH NEXT FROM pair_cursor INTO @a, @b, @label;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
    VALUES(@GiaiDau, @StageSwiss, @label, N'BO1', 1, N'swiss', N'chua_dau');

    SET @match = SCOPE_IDENTITY();

    INSERT INTO dbo.CHI_TIET_TRAN_DAU(ma_tran, ma_doi)
    SELECT @match, ma_doi FROM @Teams WHERE seed IN (@a, @b);

    FETCH NEXT FROM pair_cursor INTO @a, @b, @label;
END;
CLOSE pair_cursor;
DEALLOCATE pair_cursor;

-- Stage 2: double elimination top 4 bracket scaffold.
DECLARE @WB1 INT, @WB2 INT, @WBF INT, @LBR1 INT, @LBF INT, @GF INT;

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Winners Semifinal 1', N'BO3', 3, N'winners', N'chua_dau');
SET @WB1 = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Winners Semifinal 2', N'BO3', 3, N'winners', N'chua_dau');
SET @WB2 = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Winners Final', N'BO3', 3, N'winners', N'chua_dau');
SET @WBF = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Losers Round 1', N'BO3', 3, N'losers', N'chua_dau');
SET @LBR1 = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Losers Final', N'BO3', 3, N'losers', N'chua_dau');
SET @LBF = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Grand Final - Champion', N'BO5', 5, N'grand_final', N'chua_dau');
SET @GF = SCOPE_IDENTITY();

UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @WBF, ma_tran_tiep_theo_thua = @LBR1 WHERE ma_tran IN (@WB1, @WB2);
UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @GF, ma_tran_tiep_theo_thua = @LBF WHERE ma_tran = @WBF;
UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @LBF WHERE ma_tran = @LBR1;
UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @GF WHERE ma_tran = @LBF;

-- Pre-fill top 4 seeds into winners semifinals so you can test immediately.
INSERT INTO dbo.CHI_TIET_TRAN_DAU(ma_tran, ma_doi)
SELECT @WB1, ma_doi FROM @Teams WHERE seed IN (1,4)
UNION ALL
SELECT @WB2, ma_doi FROM @Teams WHERE seed IN (2,3);

COMMIT TRAN;

SELECT
    @GiaiDau AS ma_giai_dau,
    @TournamentName AS ten_giai_dau,
    N'/GiaiDau/ChiTiet/' + CONVERT(NVARCHAR(20), @GiaiDau) AS url_test;
GO

GO

-- ================================================================
-- END: seed_test_tournament_8teams.sql
-- ================================================================


-- ================================================================
-- BEGIN: seed_demo_full.sql
-- ================================================================

-- ================================================================
-- QUANLY_ESPORTS - FULL DEMO DATA SEED
-- Run after database.sql and all migration_*.sql files.
--
-- Goal:
-- - Create accounts, profiles, teams, recruitment data, invitations.
-- - Create demo tournaments in multiple statuses.
-- - Create one active mixed-format tournament with lineups, matches,
--   match results, player stats, ranking tables, notifications,
--   referees, disputes, and result-unlock requests.
--
-- Password for every demo account: 123456
-- Main accounts:
--   demo_admin / demo_admin@example.com
--   demo_btc / demo_btc@example.com
--   demo_referee_1 / demo_referee_1@example.com
--   aov_phoenix_president / aov_phoenix_president@example.com
-- ================================================================

USE QuanLy_Esports;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

BEGIN TRAN;

DECLARE @PasswordHash NVARCHAR(255) = N'PBKDF2$100000$AQIDBAUGBwgJCgsMDQ4PEA==$gX0bTflqCjSgps4WRDCI1xtjk/h96ukaUfpnl/iu+QY=';
DECLARE @AOV INT = (SELECT TOP 1 ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game = N'Arena of Valor');
DECLARE @VAL INT = (SELECT TOP 1 ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game = N'Valorant');
DECLARE @LOL INT = (SELECT TOP 1 ma_tro_choi FROM dbo.DANH_MUC_TRO_CHOI WHERE ten_game = N'League of Legends');

IF @AOV IS NULL
BEGIN
    RAISERROR(N'Missing Arena of Valor. Run database.sql first.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;

-- Delete only demo tournaments created by this script so reruns are predictable.
DECLARE @DemoTournamentNames TABLE(ten_giai_dau NVARCHAR(150) PRIMARY KEY);
INSERT INTO @DemoTournamentNames(ten_giai_dau)
VALUES
(N'DEMO AOV Championship 2026'),
(N'DEMO Valorant Open - Mo dang ky'),
(N'DEMO League Creator Cup - Cho duyet'),
(N'DEMO AOV Spring Finals - Ket thuc');

DECLARE @Old TABLE(ma_giai_dau INT PRIMARY KEY);
INSERT INTO @Old(ma_giai_dau)
SELECT gd.ma_giai_dau
FROM dbo.GIAI_DAU gd
INNER JOIN @DemoTournamentNames n ON n.ten_giai_dau = gd.ten_giai_dau;

IF EXISTS (SELECT 1 FROM @Old)
BEGIN
    IF OBJECT_ID('dbo.TRG_LSSKQ_IMMUTABLE', 'TR') IS NOT NULL
        DISABLE TRIGGER dbo.TRG_LSSKQ_IMMUTABLE ON dbo.LICH_SU_SUA_KET_QUA;

    DELETE l FROM dbo.LICH_SU_SUA_KET_QUA l INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = l.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;

    IF OBJECT_ID('dbo.TRG_LSSKQ_IMMUTABLE', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TRG_LSSKQ_IMMUTABLE ON dbo.LICH_SU_SUA_KET_QUA;

    DELETE y FROM dbo.YEU_CAU_MO_KHOA_KET_QUA y INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = y.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE k FROM dbo.KHIEU_NAI_KET_QUA k INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = k.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE kq FROM dbo.KET_QUA_TRAN kq INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = kq.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE kv FROM dbo.KET_QUA_VAN_DAU kv INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = kv.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE cn FROM dbo.CHI_TIET_NGUOI_CHOI_TRAN cn INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = cn.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE ct FROM dbo.CHI_TIET_TRAN_DAU ct INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = ct.ma_tran INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;

    UPDATE td
    SET ma_tran_tiep_theo_thang = NULL,
        ma_tran_tiep_theo_thua = NULL
    FROM dbo.TRAN_DAU td
    INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;

    DELETE td FROM dbo.TRAN_DAU td INNER JOIN @Old o ON o.ma_giai_dau = td.ma_giai_dau;
    DELETE bcn FROM dbo.BANG_XEP_HANG_CA_NHAN bcn INNER JOIN @Old o ON o.ma_giai_dau = bcn.ma_giai_dau;
    DELETE bxh FROM dbo.BANG_XEP_HANG bxh INNER JOIN @Old o ON o.ma_giai_dau = bxh.ma_giai_dau;
    DELETE dh FROM dbo.DOI_HINH_THI_DAU dh INNER JOIN @Old o ON o.ma_giai_dau = dh.ma_giai_dau;
    DELETE tb FROM dbo.THONG_BAO tb INNER JOIN @Old o ON tb.loai_entity = N'giai_dau' AND tb.ma_entity = o.ma_giai_dau;
    DELETE tt FROM dbo.TUONG_TAC_GIAI_DAU tt INNER JOIN @Old o ON o.ma_giai_dau = tt.ma_giai_dau;
    DELETE tg FROM dbo.THAM_GIA_GIAI tg INNER JOIN @Old o ON o.ma_giai_dau = tg.ma_giai_dau;
    DELETE gt FROM dbo.GIAI_THUONG gt INNER JOIN @Old o ON o.ma_giai_dau = gt.ma_giai_dau;
    DELETE gd FROM dbo.GIAI_DOAN gd INNER JOIN @Old o ON o.ma_giai_dau = gd.ma_giai_dau;
    DELETE tr FROM dbo.TRONG_TAI_GIAI_DAU tr INNER JOIN @Old o ON o.ma_giai_dau = tr.ma_giai_dau;
    DELETE qt FROM dbo.QUAN_TRI_GIAI_DAU qt INNER JOIN @Old o ON o.ma_giai_dau = qt.ma_giai_dau;
    DELETE gd FROM dbo.GIAI_DAU gd INNER JOIN @Old o ON o.ma_giai_dau = gd.ma_giai_dau;
END;

DELETE FROM dbo.THONG_BAO WHERE loai_entity = N'demo_seed';

-- ----------------------------------------------------------------
-- Users
-- ----------------------------------------------------------------
DECLARE @CoreUsers TABLE(
    username NVARCHAR(100) PRIMARY KEY,
    email NVARCHAR(150),
    role_name NVARCHAR(10),
    bio NVARCHAR(500),
    is_banned BIT,
    ban_reason NVARCHAR(500) NULL
);

INSERT INTO @CoreUsers(username, email, role_name, bio, is_banned, ban_reason)
VALUES
(N'demo_admin', N'demo_admin@example.com', N'admin', N'Tai khoan admin de duyet giai, xu ly khieu nai va mo khoa ket qua.', 0, NULL),
(N'demo_btc', N'demo_btc@example.com', N'user', N'Ban to chuc demo, tao va van hanh giai dau.', 0, NULL),
(N'demo_referee_1', N'demo_referee_1@example.com', N'user', N'Trong tai chinh cho cac tran BO3/BO5.', 0, NULL),
(N'demo_referee_2', N'demo_referee_2@example.com', N'user', N'Trong tai phu, dang cho phan hoi mot loi moi.', 0, NULL),
(N'demo_free_agent_1', N'demo_free_agent_1@example.com', N'user', N'Tuyen thu tu do dang ung tuyen vao doi.', 0, NULL),
(N'demo_free_agent_2', N'demo_free_agent_2@example.com', N'user', N'Tuyen thu tu do da duoc moi nhung chua xac nhan.', 0, NULL),
(N'demo_banned_user', N'demo_banned_user@example.com', N'user', N'Tai khoan bi khoa de demo quan tri nguoi dung.', 1, N'Spam don dang ky giai dau.');

INSERT INTO dbo.NGUOI_DUNG(ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, bio, is_banned, ly_do_ban, thoi_gian_ban)
SELECT username, email, @PasswordHash, role_name, bio, is_banned, ban_reason, CASE WHEN is_banned = 1 THEN DATEADD(DAY, -2, GETDATE()) ELSE NULL END
FROM @CoreUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.NGUOI_DUNG nd
    WHERE nd.ten_dang_nhap = u.username OR nd.email = u.email
);

UPDATE nd
SET nd.bio = u.bio,
    nd.vai_tro_he_thong = u.role_name,
    nd.is_banned = u.is_banned,
    nd.ly_do_ban = u.ban_reason,
    nd.thoi_gian_ban = CASE WHEN u.is_banned = 1 THEN ISNULL(nd.thoi_gian_ban, DATEADD(DAY, -2, GETDATE())) ELSE NULL END
FROM dbo.NGUOI_DUNG nd
INNER JOIN @CoreUsers u ON u.username = nd.ten_dang_nhap;

DECLARE @DemoAdmin INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'demo_admin');
DECLARE @DemoBTC INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'demo_btc');
DECLARE @Ref1 INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'demo_referee_1');
DECLARE @Ref2 INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'demo_referee_2');

IF @DemoAdmin IS NULL OR @DemoBTC IS NULL OR @Ref1 IS NULL
BEGIN
    RAISERROR(N'Could not create core demo users.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;

UPDATE dbo.NGUOI_DUNG
SET ma_admin_ban = @DemoAdmin
WHERE ten_dang_nhap = N'demo_banned_user' AND is_banned = 1;

-- ----------------------------------------------------------------
-- Teams and members
-- ----------------------------------------------------------------
DECLARE @TeamsSeed TABLE(
    team_no INT PRIMARY KEY,
    ten_doi NVARCHAR(150),
    ten_viet_tat NVARCHAR(20),
    slug NVARCHAR(50),
    slogan NVARCHAR(300),
    dang_tuyen BIT
);

INSERT INTO @TeamsSeed(team_no, ten_doi, ten_viet_tat, slug, slogan, dang_tuyen)
VALUES
(1, N'AOV Phoenix', N'PHX', N'phoenix', N'Lua chien khong tat.', 1),
(2, N'AOV Dragon', N'DRG', N'dragon', N'Ban linh rong thieng.', 0),
(3, N'AOV Titan', N'TTN', N'titan', N'Vung nhu thanh tri.', 1),
(4, N'AOV Shadow', N'SHD', N'shadow', N'Danh nhanh, bien mat nhanh.', 0),
(5, N'AOV Lotus', N'LTS', N'lotus', N'Dep mat, sac don.', 1),
(6, N'AOV Eclipse', N'ECL', N'eclipse', N'Che phu ban do.', 0),
(7, N'AOV Nova', N'NOV', N'nova', N'Bung no dung luc.', 1),
(8, N'AOV Onyx', N'ONX', N'onyx', N'Lan da den, tim nong.', 0);

DECLARE @Members TABLE(member_no INT, suffix NVARCHAR(30), team_role NVARCHAR(20), pos NVARCHAR(20));
INSERT INTO @Members(member_no, suffix, team_role, pos)
VALUES
(1, N'president', N'chu_tich', N'DS'),
(2, N'jungle', N'doi_truong', N'JG'),
(3, N'mid', N'thanh_vien', N'MID'),
(4, N'ad', N'thanh_vien', N'AD'),
(5, N'support', N'thanh_vien', N'SP');

DECLARE @UsersSeed TABLE(username NVARCHAR(100), email NVARCHAR(150), team_no INT, member_no INT, team_role NVARCHAR(20), pos NVARCHAR(20));
INSERT INTO @UsersSeed(username, email, team_no, member_no, team_role, pos)
SELECT
    N'aov_' + t.slug + N'_' + m.suffix,
    N'aov_' + t.slug + N'_' + m.suffix + N'@example.com',
    t.team_no,
    m.member_no,
    m.team_role,
    m.pos
FROM @TeamsSeed t
CROSS JOIN @Members m;

INSERT INTO dbo.NGUOI_DUNG(ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, bio)
SELECT u.username, u.email, @PasswordHash, N'user', N'Tai khoan thanh vien doi mau phuc vu demo.'
FROM @UsersSeed u
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.NGUOI_DUNG nd
    WHERE nd.ten_dang_nhap = u.username OR nd.email = u.email
);

INSERT INTO dbo.DOI(ten_doi, ten_viet_tat, ma_doi_truong, ma_manager, ma_tro_choi, logo_url, slogan, mo_ta, dang_tuyen)
SELECT
    t.ten_doi,
    t.ten_viet_tat,
    nd.ma_nguoi_dung,
    @DemoBTC,
    @AOV,
    N'/Content/avatar-default.svg',
    t.slogan,
    N'Doi mau Arena of Valor co day du thanh vien, ho so, tuyen dung va lich thi dau.',
    t.dang_tuyen
FROM @TeamsSeed t
INNER JOIN dbo.NGUOI_DUNG nd ON nd.ten_dang_nhap = N'aov_' + t.slug + N'_president'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DOI d
    WHERE d.ten_doi = t.ten_doi AND d.ma_tro_choi = @AOV
);

INSERT INTO dbo.HO_SO_IN_GAME(ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong, thanh_tich)
SELECT
    nd.ma_nguoi_dung,
    @AOV,
    CONCAT(N'AOV-', UPPER(u.username)),
    REPLACE(u.username, N'aov_', N''),
    vt.ma_vi_tri,
    N'Demo profile: rank Cao Thu/Thach Dau, da co kinh nghiem scrim va dau cup cong dong.'
FROM @UsersSeed u
INNER JOIN dbo.NGUOI_DUNG nd ON nd.ten_dang_nhap = u.username
LEFT JOIN dbo.DANH_MUC_VI_TRI vt ON vt.ma_tro_choi = @AOV AND vt.ky_hieu = u.pos
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.HO_SO_IN_GAME hs
    WHERE hs.ma_nguoi_dung = nd.ma_nguoi_dung AND hs.ma_tro_choi = @AOV
);

INSERT INTO dbo.HO_SO_IN_GAME(ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong, thanh_tich)
SELECT nd.ma_nguoi_dung, @AOV, CONCAT(N'FA-', nd.ten_dang_nhap), nd.ten_dang_nhap, vt.ma_vi_tri, N'Tuyen thu tu do dang tim doi.'
FROM dbo.NGUOI_DUNG nd
CROSS APPLY (SELECT TOP 1 ma_vi_tri FROM dbo.DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu IN (N'MID', N'AD') ORDER BY ma_vi_tri) vt
WHERE nd.ten_dang_nhap IN (N'demo_free_agent_1', N'demo_free_agent_2')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.HO_SO_IN_GAME hs
      WHERE hs.ma_nguoi_dung = nd.ma_nguoi_dung AND hs.ma_tro_choi = @AOV
  );

INSERT INTO dbo.THANH_VIEN_DOI(ma_nguoi_dung, ma_doi, ma_vi_tri, vai_tro_noi_bo, phan_he, trang_thai_duyet, trang_thai_hop_dong)
SELECT nd.ma_nguoi_dung, d.ma_doi, vt.ma_vi_tri, u.team_role, N'TuyenThu', N'da_duyet', N'dang_hieu_luc'
FROM @UsersSeed u
INNER JOIN @TeamsSeed t ON t.team_no = u.team_no
INNER JOIN dbo.DOI d ON d.ten_doi = t.ten_doi AND d.ma_tro_choi = @AOV
INNER JOIN dbo.NGUOI_DUNG nd ON nd.ten_dang_nhap = u.username
LEFT JOIN dbo.DANH_MUC_VI_TRI vt ON vt.ma_tro_choi = @AOV AND vt.ky_hieu = u.pos
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.THANH_VIEN_DOI tv
    WHERE tv.ma_nguoi_dung = nd.ma_nguoi_dung AND tv.ma_doi = d.ma_doi
);

DECLARE @Phoenix INT = (SELECT ma_doi FROM dbo.DOI WHERE ten_doi = N'AOV Phoenix' AND ma_tro_choi = @AOV);
DECLARE @Titan INT = (SELECT ma_doi FROM dbo.DOI WHERE ten_doi = N'AOV Titan' AND ma_tro_choi = @AOV);
DECLARE @Lotus INT = (SELECT ma_doi FROM dbo.DOI WHERE ten_doi = N'AOV Lotus' AND ma_tro_choi = @AOV);
DECLARE @Nova INT = (SELECT ma_doi FROM dbo.DOI WHERE ten_doi = N'AOV Nova' AND ma_tro_choi = @AOV);
DECLARE @PosMid INT = (SELECT TOP 1 ma_vi_tri FROM dbo.DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'MID');
DECLARE @PosAd INT = (SELECT TOP 1 ma_vi_tri FROM dbo.DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'AD');

-- Recruitment posts, applications, invitations, and confirmation requests.
IF @Phoenix IS NOT NULL AND @PosMid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.BAI_DANG_TUYEN_DUNG WHERE ma_doi = @Phoenix AND ma_vi_tri = @PosMid AND trang_thai = N'dang_mo')
        INSERT INTO dbo.BAI_DANG_TUYEN_DUNG(ma_doi, ma_vi_tri, noi_dung, trang_thai)
        VALUES(@Phoenix, @PosMid, N'Tuyen mid lane di du bi cho giai DEMO AOV Championship 2026.', N'dang_mo');
END;

IF @Titan IS NOT NULL AND @PosAd IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.BAI_DANG_TUYEN_DUNG WHERE ma_doi = @Titan AND ma_vi_tri = @PosAd AND trang_thai = N'dang_mo')
        INSERT INTO dbo.BAI_DANG_TUYEN_DUNG(ma_doi, ma_vi_tri, noi_dung, trang_thai)
        VALUES(@Titan, @PosAd, N'Tuyen xa thu co the shotcall giai doan late game.', N'dang_mo');
END;

DECLARE @PostPhoenix INT = (SELECT TOP 1 ma_bai_dang FROM dbo.BAI_DANG_TUYEN_DUNG WHERE ma_doi = @Phoenix AND ma_vi_tri = @PosMid ORDER BY ma_bai_dang DESC);
DECLARE @Free1 INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'demo_free_agent_1');
DECLARE @Free2 INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'demo_free_agent_2');
DECLARE @Free1Profile INT = (SELECT ma_ho_so FROM dbo.HO_SO_IN_GAME WHERE ma_nguoi_dung = @Free1 AND ma_tro_choi = @AOV);
DECLARE @PhoenixCaptain INT = (SELECT ma_nguoi_dung FROM dbo.NGUOI_DUNG WHERE ten_dang_nhap = N'aov_phoenix_jungle');

IF @PostPhoenix IS NOT NULL AND @Free1 IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.DON_UNG_TUYEN WHERE ma_bai_dang = @PostPhoenix AND ma_ung_vien = @Free1)
        INSERT INTO dbo.DON_UNG_TUYEN(ma_bai_dang, ma_ung_vien, trang_thai)
        VALUES(@PostPhoenix, @Free1, N'cho_duyet');

    IF NOT EXISTS (SELECT 1 FROM dbo.XIN_GIA_NHAP WHERE ma_nguoi_dung = @Free1 AND ma_doi = @Phoenix)
        INSERT INTO dbo.XIN_GIA_NHAP(ma_nguoi_dung, ma_doi, ma_ho_so, trang_thai)
        VALUES(@Free1, @Phoenix, @Free1Profile, N'cho_duyet');
END;

IF @Lotus IS NOT NULL AND @Free2 IS NOT NULL AND @PhoenixCaptain IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.LOI_MOI_GIA_NHAP WHERE ma_doi = @Lotus AND ma_nguoi_duoc_moi = @Free2)
        INSERT INTO dbo.LOI_MOI_GIA_NHAP(ma_doi, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta, trang_thai)
        VALUES(@Lotus, @Free2, @PhoenixCaptain, @PosAd, N'Moi tham gia doi hinh du bi cho vong playoff.', N'cho_phan_hoi');

    IF NOT EXISTS (SELECT 1 FROM dbo.YEU_CAU_XAC_NHAN_LOI_MOI WHERE ma_doi = @Lotus AND ma_nguoi_nhan = @Free2 AND trang_thai = N'cho_xac_nhan')
        INSERT INTO dbo.YEU_CAU_XAC_NHAN_LOI_MOI(ma_nguoi_gui, ma_doi, ma_nguoi_nhan, trang_thai)
        VALUES(@PhoenixCaptain, @Lotus, @Free2, N'cho_xac_nhan');

    IF NOT EXISTS (SELECT 1 FROM dbo.YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_doi = @Lotus AND ma_nguoi_duoc_moi = @Free2 AND trang_thai = N'cho_duyet')
        INSERT INTO dbo.YEU_CAU_MOI_THANH_VIEN_DOI(ma_doi, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta, trang_thai)
        VALUES(@Lotus, @Free2, @PhoenixCaptain, @PosAd, N'Yeu cau moi thanh vien tu man hinh quan ly doi.', N'cho_duyet');
END;

-- Referee registrations.
IF NOT EXISTS (SELECT 1 FROM dbo.DANG_KY_TRONG_TAI WHERE ma_nguoi_dung = @Ref1 AND ma_tro_choi = @AOV)
    INSERT INTO dbo.DANG_KY_TRONG_TAI(ma_nguoi_dung, ma_tro_choi, trang_thai, thoi_gian_duyet)
    VALUES(@Ref1, @AOV, N'da_duyet', DATEADD(DAY, -5, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM dbo.DANG_KY_TRONG_TAI WHERE ma_nguoi_dung = @Ref2 AND ma_tro_choi = @AOV)
    INSERT INTO dbo.DANG_KY_TRONG_TAI(ma_nguoi_dung, ma_tro_choi, trang_thai)
    VALUES(@Ref2, @AOV, N'cho_duyet');

-- ----------------------------------------------------------------
-- Main tournament with bracket, results, stats, disputes.
-- ----------------------------------------------------------------
DECLARE @MainTournament NVARCHAR(150) = N'DEMO AOV Championship 2026';

INSERT INTO dbo.GIAI_DAU(
    ten_giai_dau, ma_tro_choi, ma_nguoi_tao, the_thuc, banner_url, mo_ta,
    kieu_tham_gia, so_doi_toi_thieu, so_doi_toi_da, min_members_per_team,
    luat_giai, thong_tin_lien_he, dang_mo_dang_ky, tong_giai_thuong,
    trang_thai, hien_thi_public, is_registration_locked
)
VALUES(
    @MainTournament, @AOV, @DemoBTC, N'hon_hop', N'/Content/avatar-default.svg',
    N'Giai demo tong hop: Swiss round 1 da co ket qua, top 4 vao nhanh thang nhanh thua, co doi hinh, BXH, thong bao, khieu nai va yeu cau mo khoa ket qua.',
    N'theo_doi', 8, 8, 5,
    N'Moi tran BO1/BO3/BO5. Doi can check-in truoc tran. Ket qua bi khoa sau khi trong tai nop.',
    N'demo_btc@example.com', 0, 25000000, N'dang_dien_ra', 1, 1
);

DECLARE @GiaiDau INT = SCOPE_IDENTITY();

INSERT INTO dbo.QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES(@GiaiDau, @DemoBTC, N'ban_to_chuc'), (@GiaiDau, @DemoAdmin, N'ban_to_chuc');

INSERT INTO dbo.TRONG_TAI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, trang_thai)
VALUES(@GiaiDau, @Ref1, N'da_chap_nhan'), (@GiaiDau, @Ref2, N'cho_phan_hoi');

INSERT INTO dbo.GIAI_THUONG(ma_giai_dau, vi_tri_top, ten_giai, gia_tri, so_luong, mo_ta)
VALUES
(@GiaiDau, 1, N'Vo dich', 15000000, 1, N'Cup, huy chuong va tien thuong'),
(@GiaiDau, 2, N'A quan', 7000000, 1, N'Tien thuong'),
(@GiaiDau, 3, N'Hang ba', 3000000, 1, N'Tien thuong'),
(@GiaiDau, NULL, N'MVP giai dau', 1000000, 1, N'Giai ca nhan');

INSERT INTO dbo.GIAI_DOAN(ma_giai_dau, ten_giai_doan, the_thuc, thu_tu, so_doi, so_doi_di_tiep, nguong_match_point, bang_diem_json, trang_thai)
VALUES
(@GiaiDau, N'Vong 1 - Thuy Si', N'thuy_si', 1, 8, 4, NULL, N'{"win":3,"loss":0,"top_cut":4}', N'dang_dien_ra'),
(@GiaiDau, N'Vong 2 - Nhanh thang nhanh thua', N'nhanh_thang_nhanh_thua', 2, 4, 1, NULL, N'{"winner_bracket":true,"loser_bracket":true}', N'chua_bat_dau');

DECLARE @StageSwiss INT = (SELECT ma_giai_doan FROM dbo.GIAI_DOAN WHERE ma_giai_dau = @GiaiDau AND thu_tu = 1);
DECLARE @StageDE INT = (SELECT ma_giai_doan FROM dbo.GIAI_DOAN WHERE ma_giai_dau = @GiaiDau AND thu_tu = 2);

DECLARE @Teams TABLE(seed INT IDENTITY(1,1), ma_doi INT, ten_doi NVARCHAR(150));
INSERT INTO @Teams(ma_doi, ten_doi)
SELECT d.ma_doi, d.ten_doi
FROM @TeamsSeed ts
INNER JOIN dbo.DOI d ON d.ten_doi = ts.ten_doi AND d.ma_tro_choi = @AOV
ORDER BY ts.team_no;

IF (SELECT COUNT(1) FROM @Teams) <> 8
BEGIN
    RAISERROR(N'Could not seed exactly 8 AOV teams.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;

INSERT INTO dbo.THAM_GIA_GIAI(ma_giai_dau, ma_doi, trang_thai_duyet, trang_thai_tham_gia, hat_giong)
SELECT @GiaiDau, ma_doi, N'da_duyet', CASE WHEN seed <= 4 THEN N'di_tiep' ELSE N'dang_thi_dau' END, seed
FROM @Teams;

INSERT INTO dbo.DOI_HINH_THI_DAU(ma_tham_gia, ma_giai_dau, ma_nguoi_dung, ma_vi_tri, is_du_bi)
SELECT tg.ma_tham_gia, @GiaiDau, tv.ma_nguoi_dung, tv.ma_vi_tri, 0
FROM dbo.THAM_GIA_GIAI tg
INNER JOIN dbo.THANH_VIEN_DOI tv ON tv.ma_doi = tg.ma_doi
WHERE tg.ma_giai_dau = @GiaiDau
  AND tv.trang_thai_duyet = N'da_duyet'
  AND tv.trang_thai_hop_dong = N'dang_hieu_luc';

INSERT INTO dbo.BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_doi, so_tran_da_dau, so_tran_thang, so_tran_thua, hieu_so_phu, tong_diem_kill, diem_tong_ket, thu_hang_hien_tai)
SELECT
    @GiaiDau,
    @StageSwiss,
    ma_doi,
    1,
    CASE WHEN seed IN (1,2,3,4) THEN 1 ELSE 0 END,
    CASE WHEN seed IN (1,2,3,4) THEN 0 ELSE 1 END,
    CASE WHEN seed IN (1,2,3,4) THEN 1 ELSE -1 END,
    CASE seed WHEN 1 THEN 18 WHEN 2 THEN 15 WHEN 3 THEN 14 WHEN 4 THEN 12 WHEN 5 THEN 9 WHEN 6 THEN 8 WHEN 7 THEN 7 ELSE 6 END,
    CASE WHEN seed IN (1,2,3,4) THEN 3 ELSE 0 END,
    seed
FROM @Teams;

INSERT INTO dbo.BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_doi, thu_hang_hien_tai)
SELECT @GiaiDau, @StageDE, ma_doi, seed
FROM @Teams
WHERE seed <= 4;

DECLARE @CompletedMatches TABLE(match_no INT IDENTITY(1,1), ma_tran INT, win_seed INT, lose_seed INT);
DECLARE @Match INT;

DECLARE @Pairs TABLE(a INT, b INT, winner INT, label NVARCHAR(100), room_id NVARCHAR(50), room_pass NVARCHAR(50));
INSERT INTO @Pairs(a, b, winner, label, room_id, room_pass)
VALUES
(1, 8, 1, N'Swiss Round 1 - Match 1', N'AOV-DEMO-101', N'PHXONX'),
(2, 7, 2, N'Swiss Round 1 - Match 2', N'AOV-DEMO-102', N'DRGNOV'),
(3, 6, 3, N'Swiss Round 1 - Match 3', N'AOV-DEMO-103', N'TTNECL'),
(4, 5, 4, N'Swiss Round 1 - Match 4', N'AOV-DEMO-104', N'SHDLTS');

DECLARE @a INT, @b INT, @winner INT, @label NVARCHAR(100), @roomId NVARCHAR(50), @roomPass NVARCHAR(50);
DECLARE pair_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT a, b, winner, label, room_id, room_pass FROM @Pairs ORDER BY a;

OPEN pair_cursor;
FETCH NEXT FROM pair_cursor INTO @a, @b, @winner, @label, @roomId, @roomPass;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, ma_trong_tai, vong_dau, the_thuc_tran, so_vong, nhanh_dau, id_phong_game, mat_khau_phong, trang_thai)
    VALUES(@GiaiDau, @StageSwiss, @Ref1, @label, N'BO1', 1, N'swiss', @roomId, @roomPass, N'da_hoan_thanh');

    SET @Match = SCOPE_IDENTITY();

    INSERT INTO @CompletedMatches(ma_tran, win_seed, lose_seed)
    VALUES(@Match, @winner, CASE WHEN @winner = @a THEN @b ELSE @a END);

    INSERT INTO dbo.CHI_TIET_TRAN_DAU(ma_tran, ma_doi, diem_so, thu_hang, ket_qua, is_check_in, so_kill, url_anh_bang_chung)
    SELECT @Match, ma_doi,
        CASE WHEN seed = @winner THEN 1 ELSE 0 END,
        CASE WHEN seed = @winner THEN 1 ELSE 2 END,
        CASE WHEN seed = @winner THEN N'thang' ELSE N'thua' END,
        1,
        CASE WHEN seed = @winner THEN 12 + seed ELSE 6 + seed END,
        N'/Content/avatar-default.svg'
    FROM @Teams
    WHERE seed IN (@a, @b);

    INSERT INTO dbo.KET_QUA_VAN_DAU(ma_tran, so_van, ma_doi, ket_qua, thu_hang, so_kill, diem_so)
    SELECT @Match, 1, ma_doi,
        CASE WHEN seed = @winner THEN N'thang' ELSE N'thua' END,
        CASE WHEN seed = @winner THEN 1 ELSE 2 END,
        CASE WHEN seed = @winner THEN 12 + seed ELSE 6 + seed END,
        CASE WHEN seed = @winner THEN 1 ELSE 0 END
    FROM @Teams
    WHERE seed IN (@a, @b);

    INSERT INTO dbo.KET_QUA_TRAN(ma_tran, so_lan_chinh_sua, thoi_gian_sua_cuoi, chi_tiet_phu)
    VALUES(@Match, 1, DATEADD(MINUTE, -20, GETDATE()), N'{"source":"demo_seed","locked":true}');

    INSERT INTO dbo.LICH_SU_SUA_KET_QUA(ma_tran, nguoi_sua, du_lieu_cu, du_lieu_moi, ly_do_sua)
    VALUES(@Match, @Ref1, N'{}', N'{"status":"submitted"}', N'Nop ket qua demo sau tran');

    FETCH NEXT FROM pair_cursor INTO @a, @b, @winner, @label, @roomId, @roomPass;
END;
CLOSE pair_cursor;
DEALLOCATE pair_cursor;

-- Player stats for completed matches.
;WITH TeamMembers AS (
    SELECT
        tv.ma_doi,
        tv.ma_nguoi_dung,
        tv.ma_vi_tri,
        ROW_NUMBER() OVER (
            PARTITION BY tv.ma_doi
            ORDER BY CASE tv.vai_tro_noi_bo WHEN N'doi_truong' THEN 1 WHEN N'chu_tich' THEN 2 ELSE 3 END, tv.ma_thanh_vien
        ) AS rn
    FROM dbo.THANH_VIEN_DOI tv
    WHERE tv.trang_thai_duyet = N'da_duyet'
      AND tv.trang_thai_hop_dong = N'dang_hieu_luc'
)
INSERT INTO dbo.CHI_TIET_NGUOI_CHOI_TRAN(ma_tran, ma_nguoi_dung, ma_vi_tri, so_kill, so_death, so_assist, diem_kda_tran, diem_sinh_ton, is_mvp_tran)
SELECT
    cm.ma_tran,
    tm.ma_nguoi_dung,
    tm.ma_vi_tri,
    CASE WHEN t.seed = cm.win_seed THEN 3 + tm.rn ELSE 1 + tm.rn END,
    CASE WHEN t.seed = cm.win_seed THEN 1 ELSE 3 END,
    CASE WHEN t.seed = cm.win_seed THEN 6 + tm.rn ELSE 3 + tm.rn END,
    CAST(CASE WHEN t.seed = cm.win_seed THEN 8 + tm.rn ELSE 4 + tm.rn END AS FLOAT),
    CAST(CASE WHEN t.seed = cm.win_seed THEN 80 + tm.rn ELSE 55 + tm.rn END AS FLOAT),
    CASE WHEN t.seed = cm.win_seed AND tm.rn = 2 THEN 1 ELSE 0 END
FROM @CompletedMatches cm
INNER JOIN @Teams t ON t.seed IN (cm.win_seed, cm.lose_seed)
INNER JOIN TeamMembers tm ON tm.ma_doi = t.ma_doi
WHERE tm.rn <= 5
  AND NOT EXISTS (
      SELECT 1 FROM dbo.CHI_TIET_NGUOI_CHOI_TRAN c
      WHERE c.ma_tran = cm.ma_tran AND c.ma_nguoi_dung = tm.ma_nguoi_dung
  );

INSERT INTO dbo.BANG_XEP_HANG_CA_NHAN(ma_giai_dau, ma_nguoi_dung, tong_kill, tong_death, tong_assist, diem_kda_trung_binh, so_lan_dat_mvp_tran)
SELECT
    @GiaiDau,
    c.ma_nguoi_dung,
    SUM(c.so_kill),
    SUM(c.so_death),
    SUM(c.so_assist),
    AVG(ISNULL(c.diem_kda_tran, 0)),
    SUM(CASE WHEN c.is_mvp_tran = 1 THEN 1 ELSE 0 END)
FROM dbo.CHI_TIET_NGUOI_CHOI_TRAN c
INNER JOIN dbo.TRAN_DAU td ON td.ma_tran = c.ma_tran
WHERE td.ma_giai_dau = @GiaiDau
GROUP BY c.ma_nguoi_dung;

-- Double elimination scaffold, with first winners semifinals ready to play.
DECLARE @WB1 INT, @WB2 INT, @WBF INT, @LBR1 INT, @LBF INT, @GF INT;

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, ma_trong_tai, vong_dau, the_thuc_tran, so_vong, nhanh_dau, id_phong_game, mat_khau_phong, trang_thai)
VALUES(@GiaiDau, @StageDE, @Ref1, N'Winners Semifinal 1', N'BO3', 3, N'winners', N'AOV-DEMO-201', N'WB1', N'san_sang');
SET @WB1 = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, ma_trong_tai, vong_dau, the_thuc_tran, so_vong, nhanh_dau, id_phong_game, mat_khau_phong, trang_thai)
VALUES(@GiaiDau, @StageDE, @Ref2, N'Winners Semifinal 2', N'BO3', 3, N'winners', N'AOV-DEMO-202', N'WB2', N'chuan_bi');
SET @WB2 = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Winners Final', N'BO3', 3, N'winners', N'chua_dau');
SET @WBF = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Losers Round 1', N'BO3', 3, N'losers', N'chua_dau');
SET @LBR1 = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Losers Final', N'BO3', 3, N'losers', N'chua_dau');
SET @LBF = SCOPE_IDENTITY();

INSERT INTO dbo.TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
VALUES(@GiaiDau, @StageDE, N'Grand Final - Champion', N'BO5', 5, N'grand_final', N'chua_dau');
SET @GF = SCOPE_IDENTITY();

UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @WBF, ma_tran_tiep_theo_thua = @LBR1 WHERE ma_tran IN (@WB1, @WB2);
UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @GF, ma_tran_tiep_theo_thua = @LBF WHERE ma_tran = @WBF;
UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @LBF WHERE ma_tran = @LBR1;
UPDATE dbo.TRAN_DAU SET ma_tran_tiep_theo_thang = @GF WHERE ma_tran = @LBF;

INSERT INTO dbo.CHI_TIET_TRAN_DAU(ma_tran, ma_doi, is_check_in)
SELECT @WB1, ma_doi, CASE WHEN seed = 1 THEN 1 ELSE 0 END FROM @Teams WHERE seed IN (1,4)
UNION ALL
SELECT @WB2, ma_doi, 0 FROM @Teams WHERE seed IN (2,3);

-- Dispute and unlock request for admin/referee screens.
DECLARE @FirstCompletedMatch INT = (SELECT TOP 1 ma_tran FROM @CompletedMatches ORDER BY match_no);
DECLARE @LosingTeamFirstMatch INT = (SELECT t.ma_doi FROM @CompletedMatches cm INNER JOIN @Teams t ON t.seed = cm.lose_seed WHERE cm.ma_tran = @FirstCompletedMatch);
DECLARE @LosingCaptain INT = (
    SELECT TOP 1 tv.ma_nguoi_dung
    FROM dbo.THANH_VIEN_DOI tv
    WHERE tv.ma_doi = @LosingTeamFirstMatch AND tv.vai_tro_noi_bo = N'doi_truong'
);

INSERT INTO dbo.KHIEU_NAI_KET_QUA(ma_tran, ma_doi, ma_nguoi_gui, noi_dung, trang_thai)
VALUES(@FirstCompletedMatch, @LosingTeamFirstMatch, @LosingCaptain, N'Demo khieu nai: doi thua bao cao co van de check-in va can admin xem lai bang chung.', N'cho_xu_ly');

INSERT INTO dbo.YEU_CAU_MO_KHOA_KET_QUA(ma_tran, ma_trong_tai_yeu_cau, ly_do_yeu_cau, trang_thai)
VALUES(@FirstCompletedMatch, @Ref1, N'Demo: trong tai can mo khoa ket qua de bo sung anh bang chung.', N'cho_duyet');

-- Interactions and notifications.
INSERT INTO dbo.TUONG_TAC_GIAI_DAU(ma_nguoi_dung, ma_giai_dau, da_like, dang_theo_doi)
SELECT nd.ma_nguoi_dung, @GiaiDau,
    CASE WHEN nd.ten_dang_nhap IN (N'demo_free_agent_1', N'aov_phoenix_president', N'aov_dragon_president') THEN 1 ELSE 0 END,
    1
FROM dbo.NGUOI_DUNG nd
WHERE nd.ten_dang_nhap IN (N'demo_free_agent_1', N'demo_free_agent_2', N'aov_phoenix_president', N'aov_dragon_president', N'aov_titan_president');

INSERT INTO dbo.THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity, ma_doi, hanh_dong, da_doc)
VALUES
(@DemoAdmin, N'Co khieu nai ket qua moi', N'AOV Onyx gui khieu nai tran Swiss Round 1. Mo man hinh yeu cau de xu ly.', N'khieu_nai', N'demo_seed', @GiaiDau, NULL, N'xem_khieu_nai', 0),
(@DemoAdmin, N'Yeu cau mo khoa ket qua', N'Trong tai demo_referee_1 can mo khoa ket qua de cap nhat bang chung.', N'mo_khoa_ket_qua', N'demo_seed', @GiaiDau, NULL, N'duyet_mo_khoa', 0),
(@DemoBTC, N'Giai dau dang dien ra', N'DEMO AOV Championship 2026 da co 4 tran hoan thanh va 2 tran playoff san sang.', N'giai_dau', N'demo_seed', @GiaiDau, NULL, N'xem_giai', 0),
(@Free1, N'Don ung tuyen dang cho duyet', N'AOV Phoenix dang xem xet don ung tuyen cua ban.', N'ung_tuyen', N'demo_seed', @GiaiDau, @Phoenix, N'xem_doi', 0),
(@Free2, N'Ban co loi moi gia nhap', N'AOV Lotus moi ban vao doi hinh du bi.', N'loi_moi', N'demo_seed', @GiaiDau, @Lotus, N'phan_hoi_loi_moi', 0);

-- ----------------------------------------------------------------
-- Extra tournaments in other statuses for list filters and admin flows.
-- ----------------------------------------------------------------
DECLARE @OpenTournament INT;
INSERT INTO dbo.GIAI_DAU(ten_giai_dau, ma_tro_choi, ma_nguoi_tao, the_thuc, banner_url, mo_ta, so_doi_toi_thieu, so_doi_toi_da, min_members_per_team, dang_mo_dang_ky, tong_giai_thuong, trang_thai, hien_thi_public, is_registration_locked)
VALUES(N'DEMO Valorant Open - Mo dang ky', ISNULL(@VAL, @AOV), @DemoBTC, N'loai_truc_tiep', N'/Content/avatar-default.svg', N'Giai mo dang ky de demo form dang ky va khoa dang ky.', 4, 16, 5, 1, 5000000, N'mo_dang_ky', 1, 0);
SET @OpenTournament = SCOPE_IDENTITY();

INSERT INTO dbo.GIAI_THUONG(ma_giai_dau, vi_tri_top, ten_giai, gia_tri, so_luong)
VALUES(@OpenTournament, 1, N'Vo dich', 3000000, 1), (@OpenTournament, 2, N'A quan', 2000000, 1);

INSERT INTO dbo.GIAI_DOAN(ma_giai_dau, ten_giai_doan, the_thuc, thu_tu, so_doi, so_doi_di_tiep, trang_thai)
VALUES(@OpenTournament, N'Playoff', N'loai_truc_tiep', 1, 8, 1, N'chua_bat_dau');

INSERT INTO dbo.QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES(@OpenTournament, @DemoBTC, N'ban_to_chuc');

INSERT INTO dbo.THAM_GIA_GIAI(ma_giai_dau, ma_doi, trang_thai_duyet, trang_thai_tham_gia, hat_giong)
SELECT @OpenTournament, ma_doi,
    CASE WHEN seed <= 2 THEN N'da_duyet' ELSE N'cho_duyet' END,
    N'dang_thi_dau',
    seed
FROM @Teams
WHERE seed <= 4;

INSERT INTO dbo.GIAI_DAU(ten_giai_dau, ma_tro_choi, ma_nguoi_tao, the_thuc, banner_url, mo_ta, so_doi_toi_thieu, so_doi_toi_da, min_members_per_team, dang_mo_dang_ky, tong_giai_thuong, trang_thai, hien_thi_public)
VALUES(N'DEMO League Creator Cup - Cho duyet', ISNULL(@LOL, @AOV), @DemoBTC, N'vong_tron_tinh_diem', N'/Content/avatar-default.svg', N'Ban nhap da gui len admin, dung de demo phe duyet/tu choi giai.', 4, 8, 5, 0, 12000000, N'cho_xet_duyet', 0);

DECLARE @PendingTournament INT = SCOPE_IDENTITY();
INSERT INTO dbo.QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES(@PendingTournament, @DemoBTC, N'ban_to_chuc');

IF NOT EXISTS (SELECT 1 FROM dbo.YEU_CAU_TAO_GIAI_DAU WHERE ten_giai_dau = N'DEMO League Creator Cup - Cho duyet' AND ma_nguoi_gui = @DemoBTC)
    INSERT INTO dbo.YEU_CAU_TAO_GIAI_DAU(ma_nguoi_gui, ten_giai_dau, ma_tro_choi, the_thuc, tong_giai_thuong, trang_thai)
    VALUES(@DemoBTC, N'DEMO League Creator Cup - Cho duyet', ISNULL(@LOL, @AOV), N'vong_tron_tinh_diem', 12000000, N'cho_duyet');

DECLARE @Finished INT;
INSERT INTO dbo.GIAI_DAU(ten_giai_dau, ma_tro_choi, ma_nguoi_tao, the_thuc, banner_url, mo_ta, so_doi_toi_thieu, so_doi_toi_da, min_members_per_team, dang_mo_dang_ky, tong_giai_thuong, trang_thai, hien_thi_public, is_registration_locked)
VALUES(N'DEMO AOV Spring Finals - Ket thuc', @AOV, @DemoBTC, N'loai_truc_tiep', N'/Content/avatar-default.svg', N'Giai da ket thuc de demo lich su va trang thai ket thuc.', 4, 4, 5, 0, 9000000, N'ket_thuc', 1, 1);
SET @Finished = SCOPE_IDENTITY();

INSERT INTO dbo.QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES(@Finished, @DemoBTC, N'ban_to_chuc');

INSERT INTO dbo.THAM_GIA_GIAI(ma_giai_dau, ma_doi, trang_thai_duyet, trang_thai_tham_gia, hat_giong)
SELECT @Finished, ma_doi, N'da_duyet', CASE WHEN seed = 1 THEN N'di_tiep' ELSE N'bi_loai' END, seed
FROM @Teams
WHERE seed <= 4;

INSERT INTO dbo.BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_doi, so_tran_da_dau, so_tran_thang, so_tran_thua, diem_tong_ket, thu_hang_hien_tai)
SELECT @Finished, NULL, ma_doi, 3, CASE WHEN seed = 1 THEN 3 ELSE 1 END, CASE WHEN seed = 1 THEN 0 ELSE 2 END, CASE WHEN seed = 1 THEN 9 ELSE 3 END, seed
FROM @Teams
WHERE seed <= 4;

COMMIT TRAN;

SELECT
    @GiaiDau AS main_demo_giai_dau_id,
    @MainTournament AS main_demo_giai_dau,
    N'/GiaiDau/ChiTiet/' + CONVERT(NVARCHAR(20), @GiaiDau) AS main_demo_url,
    N'Password for all demo accounts: 123456' AS demo_password;
GO

GO

-- ================================================================
-- END: seed_demo_full.sql
-- ================================================================

