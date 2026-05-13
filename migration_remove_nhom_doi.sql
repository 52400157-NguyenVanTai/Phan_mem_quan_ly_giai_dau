-- Chuyen doi: bo NHOM_DOI, gan game va thanh vien truc tiep vao DOI.
-- Chay sau database.sql tren database hien co.

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.DOI', 'ma_tro_choi') IS NULL
BEGIN
    ALTER TABLE dbo.DOI ADD ma_tro_choi INT NULL;
END;

IF OBJECT_ID('dbo.NHOM_DOI', 'U') IS NOT NULL
BEGIN
    UPDATE d
    SET d.ma_tro_choi = n.ma_tro_choi
    FROM dbo.DOI d
    INNER JOIN (
        SELECT ma_doi, MIN(ma_tro_choi) AS ma_tro_choi
        FROM dbo.NHOM_DOI
        WHERE ma_tro_choi IS NOT NULL
        GROUP BY ma_doi
    ) n ON d.ma_doi = n.ma_doi
    WHERE d.ma_tro_choi IS NULL;
END;

IF EXISTS (SELECT 1 FROM dbo.DOI WHERE ma_tro_choi IS NULL)
BEGIN
    RAISERROR(N'Con DOI chua co ma_tro_choi. Hay gan game cho tat ca doi truoc khi chay migration.', 16, 1);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DOI_TROCHOI' AND parent_object_id = OBJECT_ID('dbo.DOI'))
BEGIN
    ALTER TABLE dbo.DOI ADD CONSTRAINT FK_DOI_TROCHOI FOREIGN KEY (ma_tro_choi) REFERENCES dbo.DANH_MUC_TRO_CHOI(ma_tro_choi);
END;

IF COL_LENGTH('dbo.THANH_VIEN_DOI', 'ma_doi') IS NULL
BEGIN
    ALTER TABLE dbo.THANH_VIEN_DOI ADD ma_doi INT NULL;
END;

IF OBJECT_ID('dbo.NHOM_DOI', 'U') IS NOT NULL
BEGIN
    UPDATE tv
    SET tv.ma_doi = n.ma_doi
    FROM dbo.THANH_VIEN_DOI tv
    INNER JOIN dbo.NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
    WHERE tv.ma_doi IS NULL;
END;

IF COL_LENGTH('dbo.XIN_GIA_NHAP', 'ma_doi') IS NOT NULL AND OBJECT_ID('dbo.NHOM_DOI', 'U') IS NOT NULL
BEGIN
    UPDATE xg
    SET xg.ma_doi = n.ma_doi
    FROM dbo.XIN_GIA_NHAP xg
    INNER JOIN dbo.NHOM_DOI n ON xg.ma_nhom = n.ma_nhom
    WHERE xg.ma_doi IS NULL;
END;

IF COL_LENGTH('dbo.THAM_GIA_GIAI', 'ma_doi') IS NULL
BEGIN
    ALTER TABLE dbo.THAM_GIA_GIAI ADD ma_doi INT NULL;
END;

IF OBJECT_ID('dbo.NHOM_DOI', 'U') IS NOT NULL
BEGIN
    UPDATE tg
    SET tg.ma_doi = n.ma_doi
    FROM dbo.THAM_GIA_GIAI tg
    INNER JOIN dbo.NHOM_DOI n ON tg.ma_nhom = n.ma_nhom
    WHERE tg.ma_doi IS NULL;
END;

IF COL_LENGTH('dbo.CHI_TIET_TRAN_DAU', 'ma_doi') IS NULL
BEGIN
    ALTER TABLE dbo.CHI_TIET_TRAN_DAU ADD ma_doi INT NULL;
END;

IF OBJECT_ID('dbo.NHOM_DOI', 'U') IS NOT NULL
BEGIN
    UPDATE ctd
    SET ctd.ma_doi = n.ma_doi
    FROM dbo.CHI_TIET_TRAN_DAU ctd
    INNER JOIN dbo.NHOM_DOI n ON ctd.ma_nhom = n.ma_nhom
    WHERE ctd.ma_doi IS NULL;
END;

IF EXISTS (SELECT 1 FROM dbo.THANH_VIEN_DOI WHERE ma_doi IS NULL)
    RAISERROR(N'Con THANH_VIEN_DOI chua gan duoc ma_doi.', 16, 1);
IF EXISTS (SELECT 1 FROM dbo.THAM_GIA_GIAI WHERE ma_doi IS NULL)
    RAISERROR(N'Con THAM_GIA_GIAI chua gan duoc ma_doi.', 16, 1);
IF EXISTS (SELECT 1 FROM dbo.CHI_TIET_TRAN_DAU WHERE ma_doi IS NULL)
    RAISERROR(N'Con CHI_TIET_TRAN_DAU chua gan duoc ma_doi.', 16, 1);

IF COL_LENGTH('dbo.THANH_VIEN_DOI', 'ma_doi') IS NOT NULL
BEGIN
    ALTER TABLE dbo.THANH_VIEN_DOI ALTER COLUMN ma_doi INT NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TV_DOI' AND parent_object_id = OBJECT_ID('dbo.THANH_VIEN_DOI'))
        ALTER TABLE dbo.THANH_VIEN_DOI ADD CONSTRAINT FK_TV_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi);
END;

IF COL_LENGTH('dbo.THAM_GIA_GIAI', 'ma_doi') IS NOT NULL
BEGIN
    ALTER TABLE dbo.THAM_GIA_GIAI ALTER COLUMN ma_doi INT NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TGG_DOI' AND parent_object_id = OBJECT_ID('dbo.THAM_GIA_GIAI'))
        ALTER TABLE dbo.THAM_GIA_GIAI ADD CONSTRAINT FK_TGG_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TGG_GD_DOI' AND object_id = OBJECT_ID('dbo.THAM_GIA_GIAI'))
        ALTER TABLE dbo.THAM_GIA_GIAI ADD CONSTRAINT UQ_TGG_GD_DOI UNIQUE (ma_giai_dau, ma_doi);
END;

IF COL_LENGTH('dbo.CHI_TIET_TRAN_DAU', 'ma_doi') IS NOT NULL
BEGIN
    ALTER TABLE dbo.CHI_TIET_TRAN_DAU ALTER COLUMN ma_doi INT NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CTTD_DOI' AND parent_object_id = OBJECT_ID('dbo.CHI_TIET_TRAN_DAU'))
        ALTER TABLE dbo.CHI_TIET_TRAN_DAU ADD CONSTRAINT FK_CTTD_DOI FOREIGN KEY (ma_doi) REFERENCES dbo.DOI(ma_doi);
END;

IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TRG_THANH_VIEN_DOI_RULES')
    DROP TRIGGER dbo.TRG_THANH_VIEN_DOI_RULES;

EXEC('
CREATE TRIGGER dbo.TRG_THANH_VIEN_DOI_RULES
ON dbo.THANH_VIEN_DOI
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.THANH_VIEN_DOI tv ON i.ma_nguoi_dung = tv.ma_nguoi_dung
        INNER JOIN dbo.DOI di ON i.ma_doi = di.ma_doi
        INNER JOIN dbo.DOI dtv ON tv.ma_doi = dtv.ma_doi
        WHERE i.trang_thai_duyet = ''da_duyet''
          AND i.trang_thai_hop_dong = ''dang_hieu_luc''
          AND tv.trang_thai_duyet = ''da_duyet''
          AND tv.trang_thai_hop_dong = ''dang_hieu_luc''
          AND di.ma_tro_choi = dtv.ma_tro_choi
          AND i.ma_thanh_vien <> tv.ma_thanh_vien
    )
    BEGIN
        RAISERROR (N''Mot nguoi dung chi duoc tham gia mot doi trong cung game.'', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.THANH_VIEN_DOI tv
        WHERE tv.ma_doi IN (SELECT DISTINCT ma_doi FROM inserted)
          AND tv.vai_tro_noi_bo = ''doi_truong''
          AND tv.trang_thai_duyet = ''da_duyet''
          AND tv.trang_thai_hop_dong = ''dang_hieu_luc''
        GROUP BY tv.ma_doi
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR (N''Moi doi chi duoc co mot doi truong.'', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END
');

COMMIT TRANSACTION;
