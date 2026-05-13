IF OBJECT_ID('dbo.KET_QUA_VAN_DAU', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KET_QUA_VAN_DAU(
        ma_tran INT NOT NULL,
        so_van INT NOT NULL,
        ma_doi INT NOT NULL,
        ket_qua NVARCHAR(20) NULL,
        thu_hang INT NULL,
        so_kill INT NOT NULL DEFAULT 0,
        diem_so FLOAT NOT NULL DEFAULT 0,
        ngay_cap_nhat DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_KET_QUA_VAN_DAU PRIMARY KEY(ma_tran, so_van, ma_doi)
    );
END
