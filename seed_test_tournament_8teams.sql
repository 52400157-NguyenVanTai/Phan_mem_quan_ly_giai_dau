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
