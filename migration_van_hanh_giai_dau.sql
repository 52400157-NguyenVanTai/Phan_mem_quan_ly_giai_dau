USE QuanLy_Esports;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_TD_TRANGTHAI' AND parent_object_id = OBJECT_ID('dbo.TRAN_DAU'))
BEGIN
    ALTER TABLE dbo.TRAN_DAU DROP CONSTRAINT CHK_TD_TRANGTHAI;
END
GO

ALTER TABLE dbo.TRAN_DAU ADD CONSTRAINT CHK_TD_TRANGTHAI
CHECK (trang_thai IN (
    'chua_dau',
    'chuan_bi',
    'san_sang',
    'dang_dau',
    'dang_thi_dau',
    'cho_ket_qua',
    'da_hoan_thanh',
    'huy_bo',
    'bye'
));
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_TD_THETHUCTRAN' AND parent_object_id = OBJECT_ID('dbo.TRAN_DAU'))
BEGIN
    ALTER TABLE dbo.TRAN_DAU DROP CONSTRAINT CHK_TD_THETHUCTRAN;
END
GO

ALTER TABLE dbo.TRAN_DAU ADD CONSTRAINT CHK_TD_THETHUCTRAN
CHECK (the_thuc_tran IN ('BO1','BO3','BO5','BO7','SinhTon'));
GO

PRINT N'Migration van hanh giai dau completed.';
GO
