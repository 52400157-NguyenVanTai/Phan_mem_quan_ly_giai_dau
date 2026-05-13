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
