-- ================================================================
-- DELETE TEAM SCRIPT
-- Purpose: Delete a team and all its members/squads from database
-- Usage: Replace @TenDoi with the team name you want to delete
-- ================================================================

USE QuanLy_Esports;
GO

DECLARE @TenDoi NVARCHAR(150) = N'FPT';  -- <-- Thay tên đội ở đây
DECLARE @MaDoi INT;

-- Debug: First check what teams exist
PRINT 'Đang tìm đội: ' + @TenDoi;
SELECT ma_doi, ten_doi FROM DOI WHERE ten_doi = @TenDoi;

-- Get the team ID
SELECT TOP 1 @MaDoi = ma_doi 
FROM DOI 
WHERE ten_doi = @TenDoi;

PRINT 'ma_doi nhận được: ' + ISNULL(CAST(@MaDoi AS VARCHAR(10)), 'NULL');

IF @MaDoi IS NULL
BEGIN
    PRINT 'Không tìm thấy đội: ' + @TenDoi;
    RETURN;
END

PRINT 'Đang xóa đội: ' + @TenDoi + ' (ma_doi = ' + CAST(@MaDoi AS VARCHAR(10)) + ')';

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Delete team members from THANH_VIEN_DOI
    DELETE FROM THANH_VIEN_DOI 
    WHERE ma_nhom IN (SELECT ma_nhom FROM NHOM_DOI WHERE ma_doi = @MaDoi);
    PRINT 'Đã xóa thành viên đội.';

    -- 2. Delete squad join requests
    DELETE FROM YEU_CAU_THAM_GIA_NHOM 
    WHERE ma_nhom IN (SELECT ma_nhom FROM NHOM_DOI WHERE ma_doi = @MaDoi);
    PRINT 'Đã xóa yêu cầu tham gia nhóm.';

    -- 3. Delete recruitment posts
    DELETE FROM BAI_DANG_TUYEN_DUNG 
    WHERE ma_nhom IN (SELECT ma_nhom FROM NHOM_DOI WHERE ma_doi = @MaDoi);
    PRINT 'Đã xóa bài đăng tuyển dụng.';

    -- 4. Delete invitations
    DELETE FROM LOI_MOI_GIA_NHAP 
    WHERE ma_nhom IN (SELECT ma_nhom FROM NHOM_DOI WHERE ma_doi = @MaDoi);
    PRINT 'Đã xóa lời mời gia nhập.';

    -- 5. Delete squads from NHOM_DOI
    DELETE FROM NHOM_DOI WHERE ma_doi = @MaDoi;
    PRINT 'Đã xóa nhóm (squads).';

    -- 6. Delete team from DOI
    DELETE FROM DOI WHERE ma_doi = @MaDoi;
    PRINT 'Đã xóa đội thành công!';

    COMMIT TRANSACTION;
    PRINT 'Xóa đội hoàn tất.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Lỗi khi xóa đội: ' + ERROR_MESSAGE();
END CATCH
GO
