-- Fix trigger TRG_THANH_VIEN_DOI_RULES sau khi bo NHOM_DOI.
-- Loi cu: JOIN inserted theo ma_doi lam nhan ban dong doi_truong khi insert nhieu thanh vien cung luc.
-- Chay file nay truoc, sau do chay lai seed_aov_sample.sql.

USE QuanLy_Esports;
GO

CREATE OR ALTER TRIGGER dbo.TRG_THANH_VIEN_DOI_RULES
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
        WHERE i.trang_thai_duyet = N'da_duyet'
          AND i.trang_thai_hop_dong = N'dang_hieu_luc'
          AND tv.trang_thai_duyet = N'da_duyet'
          AND tv.trang_thai_hop_dong = N'dang_hieu_luc'
          AND di.ma_tro_choi = dtv.ma_tro_choi
          AND i.ma_thanh_vien <> tv.ma_thanh_vien
    )
    BEGIN
        RAISERROR (N'Mot nguoi dung chi duoc tham gia mot doi trong cung game.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.THANH_VIEN_DOI tv
        WHERE tv.ma_doi IN (SELECT DISTINCT ma_doi FROM inserted)
          AND tv.vai_tro_noi_bo = N'doi_truong'
          AND tv.trang_thai_duyet = N'da_duyet'
          AND tv.trang_thai_hop_dong = N'dang_hieu_luc'
        GROUP BY tv.ma_doi
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR (N'Moi doi chi duoc co mot doi truong.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO
