-- Sửa hệ thống role mới theo yêu cầu
-- Role mới: Chủ tịch, Thành viên, Ban điều hành, Đội trưởng
-- Hủy bỏ: leader, coach
-- Quy tắc: Ban điều hành không có đội trưởng, chỉ các nhóm thi đấu (có ma_tro_choi) mới có đội trưởng

-- 1. Cập nhật dữ liệu hiện có trước khi thay đổi constraint
UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = 'chu_tich' WHERE vai_tro_noi_bo = 'chairman';
UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = 'thanh_vien' WHERE vai_tro_noi_bo = 'member';
UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = 'ban_dieu_hanh' WHERE vai_tro_noi_bo = 'leader';
UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = 'ban_dieu_hanh' WHERE vai_tro_noi_bo = 'coach';
UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = 'doi_truong' WHERE vai_tro_noi_bo = 'captain';
GO

-- 2. Xóa đội trưởng khỏi nhóm Ban điều hành (ma_tro_choi IS NULL)
UPDATE THANH_VIEN_DOI
SET vai_tro_noi_bo = 'thanh_vien'
WHERE vai_tro_noi_bo = 'doi_truong'
  AND ma_nhom IN (SELECT ma_nhom FROM NHOM_DOI WHERE ma_tro_choi IS NULL);
GO

-- 3. Cập nhật constraint CHK_TV_VAITRO cho role mới
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_TV_VAITRO')
BEGIN
    ALTER TABLE THANH_VIEN_DOI DROP CONSTRAINT CHK_TV_VAITRO;
END
GO

ALTER TABLE THANH_VIEN_DOI ADD CONSTRAINT CHK_TV_VAITRO
CHECK (vai_tro_noi_bo IN ('chu_tich','thanh_vien','ban_dieu_hanh','doi_truong'));
GO

PRINT 'Đã cập nhật hệ thống role mới thành công!';
