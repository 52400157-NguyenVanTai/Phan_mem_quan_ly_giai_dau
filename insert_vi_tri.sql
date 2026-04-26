-- ============================================================
-- INSERT VỊ TRÍ THEO GAME (Chạy sau khi có dữ liệu TRO_CHOI)
-- ============================================================
USE QuanLy_Esports;
GO

-- Lấy mã game
DECLARE @MaAoV   INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game LIKE N'%Liên Quân%' OR ten_game LIKE N'%Arena of Valor%' OR ten_game LIKE N'%AoV%');
DECLARE @MaLoL   INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game LIKE N'%Liên Minh%' OR ten_game LIKE N'%League of Legends%');
DECLARE @MaCS    INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game LIKE N'%CS%' OR ten_game LIKE N'%Counter%');
DECLARE @MaValo  INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game LIKE N'%Valorant%');
DECLARE @MaPUBG  INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game LIKE N'%PUBG%');
DECLARE @MaFF    INT = (SELECT TOP 1 ma_tro_choi FROM TRO_CHOI WHERE ten_game LIKE N'%Free Fire%');

-- ============================================================
-- 1. MOBA — Liên Quân Mobile (AoV)
-- ============================================================
IF @MaAoV IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='TOP')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Đường trên','TOP','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='JUG')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Đi rừng','JUG','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='MID')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Đường giữa','MID','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='ADC')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Xạ thủ','ADC','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='SUP')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Trợ thủ','SUP','ChuyenMon');
    -- Ban HLV
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='HLV')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Huấn luyện viên trưởng','HLV','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='TACTICAL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'HLV Chiến thuật','TACTICAL','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='ASSISTANT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Trợ lý HLV','ASSISTANT','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaAoV AND ky_hieu='MANAGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaAoV,N'Quản lý','MANAGER','BanHuanLuyen');
END

-- ============================================================
-- 2. MOBA — League of Legends
-- ============================================================
IF @MaLoL IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='TOP')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Đường trên','TOP','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='JUG')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Đi rừng','JUG','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='MID')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Đường giữa','MID','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='ADC')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Xạ thủ','ADC','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='SUP')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Trợ thủ','SUP','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='HLV')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Huấn luyện viên trưởng','HLV','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='TACTICAL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'HLV Chiến thuật','TACTICAL','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='ASSISTANT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Trợ lý HLV','ASSISTANT','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaLoL AND ky_hieu='MANAGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaLoL,N'Quản lý','MANAGER','BanHuanLuyen');
END

-- ============================================================
-- 3. FPS — CS:GO / CS2
-- ============================================================
IF @MaCS IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='LEADER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Đội trưởng','LEADER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='FRAGGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Người mở đường','FRAGGER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='AWPER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Bắn tỉa','AWPER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='SUPPORT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Hỗ trợ','SUPPORT','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='LURKER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Người đi sau','LURKER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='PLAYMAKER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Playmaker','PLAYMAKER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='HLV')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Huấn luyện viên trưởng','HLV','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='TACTICAL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'HLV Chiến thuật','TACTICAL','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='ASSISTANT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Trợ lý HLV','ASSISTANT','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaCS AND ky_hieu='MANAGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaCS,N'Quản lý','MANAGER','BanHuanLuyen');
END

-- ============================================================
-- 4. FPS — Valorant
-- ============================================================
IF @MaValo IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='DUELIST')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Đối đầu (Duelist)','DUELIST','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='INITIATOR')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Khởi tranh (Initiator)','INITIATOR','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='CONTROLLER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Kiểm soát (Controller)','CONTROLLER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='SENTINEL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Hộ vệ (Sentinel)','SENTINEL','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='HLV')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Huấn luyện viên trưởng','HLV','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='TACTICAL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'HLV Chiến thuật','TACTICAL','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='ASSISTANT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Trợ lý HLV','ASSISTANT','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaValo AND ky_hieu='MANAGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaValo,N'Quản lý','MANAGER','BanHuanLuyen');
END

-- ============================================================
-- 5. Battle Royale — PUBG
-- ============================================================
IF @MaPUBG IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='LEADER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Đội trưởng','LEADER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='SNIPER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Bắn tỉa','SNIPER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='FRAGGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Người mở đường','FRAGGER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='SUPPORT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Hỗ trợ','SUPPORT','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='DRIVER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Lái xe','DRIVER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='SCOUTER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Trinh sát','SCOUTER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='HLV')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Huấn luyện viên trưởng','HLV','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='TACTICAL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'HLV Chiến thuật','TACTICAL','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='ASSISTANT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Trợ lý HLV','ASSISTANT','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaPUBG AND ky_hieu='MANAGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaPUBG,N'Quản lý','MANAGER','BanHuanLuyen');
END

-- ============================================================
-- 6. Battle Royale — Free Fire
-- ============================================================
IF @MaFF IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='TANKER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Đỡ đòn (Tanker)','TANKER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='RUSHER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Tiên phong (Rusher)','RUSHER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='SCOUTER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Trinh sát (Scouter)','SCOUTER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='SUPPORT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Hỗ trợ (Support)','SUPPORT','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='BOMBER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Chuyên gia nổ (Bomber)','BOMBER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='COVER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Yểm trợ (Cover)','COVER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='SNIPER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Bắn tỉa (Sniper)','SNIPER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='PLANKER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Bọc lót (Planker)','PLANKER','ChuyenMon');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='HLV')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Huấn luyện viên trưởng','HLV','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='TACTICAL')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'HLV Chiến thuật','TACTICAL','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='ASSISTANT')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Trợ lý HLV','ASSISTANT','BanHuanLuyen');
    IF NOT EXISTS (SELECT 1 FROM DANH_MUC_VI_TRI WHERE ma_tro_choi=@MaFF AND ky_hieu='MANAGER')
        INSERT INTO DANH_MUC_VI_TRI(ma_tro_choi,ten_vi_tri,ky_hieu,loai_vi_tri) VALUES(@MaFF,N'Quản lý','MANAGER','BanHuanLuyen');
END

SELECT tc.ten_game, vt.ten_vi_tri, vt.ky_hieu, vt.loai_vi_tri
FROM DANH_MUC_VI_TRI vt
JOIN TRO_CHOI tc ON vt.ma_tro_choi = tc.ma_tro_choi
ORDER BY tc.ten_game, vt.loai_vi_tri, vt.ten_vi_tri;
