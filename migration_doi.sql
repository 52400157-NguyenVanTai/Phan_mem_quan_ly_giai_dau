USE QuanLy_Esports;
GO

IF COL_LENGTH('LOI_MOI_GIA_NHAP', 'ma_vi_tri') IS NULL
BEGIN
    ALTER TABLE LOI_MOI_GIA_NHAP ADD ma_vi_tri INT NULL;
    ALTER TABLE LOI_MOI_GIA_NHAP ADD CONSTRAINT FK_LM_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri);
END
GO

IF COL_LENGTH('LOI_MOI_GIA_NHAP', 'mo_ta') IS NULL
BEGIN
    ALTER TABLE LOI_MOI_GIA_NHAP ADD mo_ta NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'YEU_CAU_MOI_THANH_VIEN_DOI')
BEGIN
    CREATE TABLE YEU_CAU_MOI_THANH_VIEN_DOI (
        ma_yeu_cau INT IDENTITY PRIMARY KEY,
        ma_doi INT NOT NULL,
        ma_nhom INT NOT NULL,
        ma_nguoi_duoc_moi INT NOT NULL,
        ma_nguoi_gui INT NOT NULL,
        ma_vi_tri INT NULL,
        mo_ta NVARCHAR(500) NULL,
        trang_thai NVARCHAR(20) NOT NULL CONSTRAINT DF_YCMTV_TRANGTHAI DEFAULT 'cho_duyet',
        ngay_tao DATETIME NOT NULL CONSTRAINT DF_YCMTV_NGAYTAO DEFAULT GETDATE(),
        ngay_duyet DATETIME NULL,
        ma_nguoi_duyet INT NULL,
        CONSTRAINT FK_YCMTV_DOI FOREIGN KEY (ma_doi) REFERENCES DOI(ma_doi),
        CONSTRAINT FK_YCMTV_NHOM FOREIGN KEY (ma_nhom) REFERENCES NHOM_DOI(ma_nhom),
        CONSTRAINT FK_YCMTV_NGUOINHAN FOREIGN KEY (ma_nguoi_duoc_moi) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_YCMTV_NGUOIGUI FOREIGN KEY (ma_nguoi_gui) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT FK_YCMTV_VITRI FOREIGN KEY (ma_vi_tri) REFERENCES DANH_MUC_VI_TRI(ma_vi_tri),
        CONSTRAINT FK_YCMTV_NGUOIDUYET FOREIGN KEY (ma_nguoi_duyet) REFERENCES NGUOI_DUNG(ma_nguoi_dung),
        CONSTRAINT CHK_YCMTV_TRANGTHAI CHECK (trang_thai IN ('cho_duyet','chap_nhan','tu_choi'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TRG_THANH_VIEN_DOI_RULES')
BEGIN
    EXEC('
    CREATE TRIGGER TRG_THANH_VIEN_DOI_RULES
    ON THANH_VIEN_DOI
    AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (
            SELECT 1
            FROM inserted i
            INNER JOIN THANH_VIEN_DOI tv ON i.ma_nguoi_dung = tv.ma_nguoi_dung
            INNER JOIN NHOM_DOI ni ON i.ma_nhom = ni.ma_nhom
            INNER JOIN NHOM_DOI ntv ON tv.ma_nhom = ntv.ma_nhom
            WHERE i.trang_thai_duyet = ''da_duyet''
              AND i.trang_thai_hop_dong = ''dang_hieu_luc''
              AND tv.trang_thai_duyet = ''da_duyet''
              AND tv.trang_thai_hop_dong = ''dang_hieu_luc''
              AND ni.ma_tro_choi = ntv.ma_tro_choi
              AND i.ma_thanh_vien <> tv.ma_thanh_vien
        )
        BEGIN
            RAISERROR (N''Một người dùng chỉ được tham gia một đội trong cùng một game.'', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        IF EXISTS (
            SELECT 1
            FROM THANH_VIEN_DOI tv
            INNER JOIN inserted i ON tv.ma_nhom = i.ma_nhom
            WHERE tv.vai_tro_noi_bo = ''doi_truong''
              AND tv.trang_thai_duyet = ''da_duyet''
              AND tv.trang_thai_hop_dong = ''dang_hieu_luc''
            GROUP BY tv.ma_nhom
            HAVING COUNT(*) > 1
        )
        BEGIN
            RAISERROR (N''Mỗi đội chỉ được có một đội trưởng.'', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;
    END
    ');
END
GO

PRINT N'Migration đội đã chạy xong.';
GO
