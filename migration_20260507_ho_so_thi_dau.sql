USE QuanLy_Esports;
GO

IF COL_LENGTH('HO_SO_IN_GAME', 'thanh_tich') IS NULL
BEGIN
    ALTER TABLE HO_SO_IN_GAME ADD thanh_tich NVARCHAR(1000) NULL;
END
GO

;WITH DuplicateProfiles AS (
    SELECT ma_ho_so,
           ROW_NUMBER() OVER (
               PARTITION BY ma_nguoi_dung, ma_tro_choi
               ORDER BY ngay_cap_nhat DESC, ma_ho_so DESC
           ) AS row_number
    FROM HO_SO_IN_GAME
)
DELETE FROM DuplicateProfiles
WHERE row_number > 1;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE name = 'UQ_HSG_PROFILE'
      AND parent_object_id = OBJECT_ID('HO_SO_IN_GAME')
)
BEGIN
    ALTER TABLE HO_SO_IN_GAME
    ADD CONSTRAINT UQ_HSG_PROFILE UNIQUE (ma_nguoi_dung, ma_tro_choi);
END
GO

IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'Arena of Valor')
    INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'Arena of Valor', 'MOBA');
IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'League of Legends')
    INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'League of Legends', 'MOBA');
IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'Free Fire')
    INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'Free Fire', 'BATTLEROYALE');
IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'PUBG')
    INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'PUBG', 'BATTLEROYALE');
IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'Valorant')
    INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'Valorant', 'FPS');
IF NOT EXISTS (SELECT 1 FROM TRO_CHOI WHERE ten_game = N'CS:GO')
    INSERT INTO TRO_CHOI (ten_game, the_loai) VALUES (N'CS:GO', 'FPS');
GO

DECLARE @AOV INT = (SELECT ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Arena of Valor');
DECLARE @LOL INT = (SELECT ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'League of Legends');
DECLARE @FREEFIRE INT = (SELECT ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Free Fire');
DECLARE @PUBG INT = (SELECT ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'PUBG');
DECLARE @VALORANT INT = (SELECT ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'Valorant');
DECLARE @CSGO INT = (SELECT ma_tro_choi FROM TRO_CHOI WHERE ten_game = N'CS:GO');

IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'DS')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@AOV, N'Đường Caesar', N'DS', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'JG')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@AOV, N'Đi rừng', N'JG', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'MID')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@AOV, N'Đường giữa', N'MID', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'AD')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@AOV, N'Xạ thủ', N'AD', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @AOV AND ky_hieu = N'SP')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@AOV, N'Trợ thủ', N'SP', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @LOL AND ky_hieu = N'TOP')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@LOL, N'Đường trên', N'TOP', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @LOL AND ky_hieu = N'JGL')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@LOL, N'Đi rừng', N'JGL', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @LOL AND ky_hieu = N'MID')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@LOL, N'Đường giữa', N'MID', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @LOL AND ky_hieu = N'ADC')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@LOL, N'Xạ thủ', N'ADC', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @LOL AND ky_hieu = N'SUP')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@LOL, N'Hỗ trợ', N'SUP', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @VALORANT AND ky_hieu = N'DUEL')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@VALORANT, N'Duelist', N'DUEL', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @VALORANT AND ky_hieu = N'INIT')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@VALORANT, N'Initiator', N'INIT', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @VALORANT AND ky_hieu = N'CTRL')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@VALORANT, N'Controller', N'CTRL', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @VALORANT AND ky_hieu = N'SENT')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@VALORANT, N'Sentinel', N'SENT', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @CSGO AND ky_hieu = N'ENTRY')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@CSGO, N'Entry Fragger', N'ENTRY', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @CSGO AND ky_hieu = N'AWP')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@CSGO, N'AWPer', N'AWP', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @CSGO AND ky_hieu = N'IGL')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@CSGO, N'In-game Leader', N'IGL', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @CSGO AND ky_hieu = N'SUP')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@CSGO, N'Support', N'SUP', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @FREEFIRE AND ky_hieu = N'RUSH')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@FREEFIRE, N'Rusher', N'RUSH', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @FREEFIRE AND ky_hieu = N'SNIPER')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@FREEFIRE, N'Sniper', N'SNIPER', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @FREEFIRE AND ky_hieu = N'SUPPORT')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@FREEFIRE, N'Support', N'SUPPORT', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @PUBG AND ky_hieu = N'IGL')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@PUBG, N'Chỉ huy', N'IGL', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @PUBG AND ky_hieu = N'SCOUT')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@PUBG, N'Trinh sát', N'SCOUT', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi = @PUBG AND ky_hieu = N'FRAG')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (@PUBG, N'Tấn công', N'FRAG', 'TuyenThu');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi IS NULL AND ky_hieu = N'HLV')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (NULL, N'Huấn luyện viên', N'HLV', 'HuanLuyen');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi IS NULL AND ky_hieu = N'PT')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (NULL, N'Phân tích viên', N'PT', 'HuanLuyen');
IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi IS NULL AND ky_hieu = N'QL')
    INSERT INTO DANH_MUC_VI_TRI (ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri) VALUES (NULL, N'Quản lý', N'QL', 'HuanLuyen');
GO

PRINT N'Migration hồ sơ thi đấu đã hoàn tất.';
GO
