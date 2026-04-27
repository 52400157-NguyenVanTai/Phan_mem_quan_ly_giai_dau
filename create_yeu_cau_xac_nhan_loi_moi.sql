-- Tạo bảng YEU_CAU_XAC_NHAN_LOI_MOI để lưu trữ yêu cầu xác nhận lời mời từ Đội trưởng
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'YEU_CAU_XAC_NHAN_LOI_MOI')
BEGIN
    CREATE TABLE YEU_CAU_XAC_NHAN_LOI_MOI (
        ma_yeu_cau INT IDENTITY(1,1) PRIMARY KEY,
        ma_nguoi_gui INT NOT NULL,  -- Đội trưởng gửi yêu cầu
        ma_doi INT NOT NULL,
        ma_nhom INT NULL,  -- NULL nếu mời vào đội (chưa phân nhóm), có giá trị nếu mời vào nhóm cụ thể
        ma_nguoi_nhan INT NOT NULL,  -- Người được mời
        trang_thai NVARCHAR(50) NOT NULL DEFAULT 'cho_xac_nhan',  -- cho_xac_nhan, da_xac_nhan, tu_choi
        ngay_tao DATETIME NOT NULL DEFAULT GETDATE(),
        ngay_xac_nhan DATETIME NULL,
        ma_nguoi_xac_nhan INT NULL  -- Chủ tịch hoặc Ban điều hành xác nhận
    );
    
    -- Add indexes
    CREATE INDEX IX_YCXNLM_NguoiGui ON YEU_CAU_XAC_NHAN_LOI_MOI(ma_nguoi_gui);
    CREATE INDEX IX_YCXNLM_Doi ON YEU_CAU_XAC_NHAN_LOI_MOI(ma_doi);
    CREATE INDEX IX_YCXNLM_NguoiNhan ON YEU_CAU_XAC_NHAN_LOI_MOI(ma_nguoi_nhan);
    CREATE INDEX IX_YCXNLM_TrangThai ON YEU_CAU_XAC_NHAN_LOI_MOI(trang_thai);
END
GO
