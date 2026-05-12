USE QuanLy_Esports;
GO

-- Dong bo CHECK constraint GIAI_DAU.trang_thai voi state machine hien tai cua app.
-- Luu y: backend/frontend trong du an nay dang dung trang thai dang snake_case
-- (vd: 'sap_dien_ra', 'bi_tu_choi'), khong phai bo ten tieng Anh
-- (vd: 'Registration_Open', 'Rejected').
-- Loi can sua: khi Admin phe duyet, code cap nhat trang_thai = 'sap_dien_ra'
-- nhung database cu co the van giu CHK_GD_TRANGTHAI khong chap nhan gia tri nay.

UPDATE dbo.GIAI_DAU
SET trang_thai = CASE trang_thai
    WHEN 'ban_nhap' THEN 'nhap'
    WHEN 'cho_phe_duyet' THEN 'cho_xet_duyet'
    WHEN 'chuan_bi_dien_ra' THEN 'sap_dien_ra'
    WHEN 'tong_ket' THEN 'ket_thuc'
    WHEN 'tam_hoan' THEN 'da_huy'
    WHEN 'khoa' THEN 'khoa_dang_ky'
    WHEN 'Draft' THEN 'nhap'
    WHEN 'Pending_Approval' THEN 'cho_xet_duyet'
    WHEN 'Rejected' THEN 'bi_tu_choi'
    WHEN 'Registration_Open' THEN 'mo_dang_ky'
    WHEN 'Registration_Closed' THEN 'khoa_dang_ky'
    WHEN 'Live' THEN 'dang_dien_ra'
    WHEN 'Completed' THEN 'ket_thuc'
    WHEN 'Cancelled' THEN 'da_huy'
    WHEN 'Upcoming' THEN 'sap_dien_ra'
    ELSE trang_thai
END
WHERE trang_thai IN (
    'ban_nhap',
    'cho_phe_duyet',
    'chuan_bi_dien_ra',
    'tong_ket',
    'tam_hoan',
    'khoa',
    'Draft',
    'Pending_Approval',
    'Rejected',
    'Registration_Open',
    'Registration_Closed',
    'Live',
    'Completed',
    'Cancelled',
    'Upcoming'
);
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CHK_GD_TRANGTHAI'
      AND parent_object_id = OBJECT_ID('dbo.GIAI_DAU')
)
BEGIN
    ALTER TABLE dbo.GIAI_DAU DROP CONSTRAINT CHK_GD_TRANGTHAI;
END
GO

ALTER TABLE dbo.GIAI_DAU ADD CONSTRAINT CHK_GD_TRANGTHAI CHECK (trang_thai IN (
    'nhap',
    'cho_xet_duyet',
    'bi_tu_choi',
    'sap_dien_ra',
    'mo_dang_ky',
    'khoa_dang_ky',
    'dang_dien_ra',
    'ket_thuc',
    'da_huy'
));
GO

PRINT N'Fixed CHK_GD_TRANGTHAI for GIAI_DAU.trang_thai.';
GO
