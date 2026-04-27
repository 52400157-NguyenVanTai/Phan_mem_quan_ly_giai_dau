-- ================================================================
-- DELETE TEAM BY ID (Alternative method)
-- Purpose: Delete a team using ma_doi directly (no name conversion)
-- ================================================================

USE QuanLy_Esports;
GO

-- First, list all teams to find the ma_doi
SELECT ma_doi, ten_doi, trang_thai 
FROM DOI 
ORDER BY ma_doi;
GO

-- Then run the deletion with the ma_doi found above
DECLARE @MaDoi INT = 1;  -- <-- Thay bằng ma_doi thực tế

PRINT 'Đang xóa đội có ma_doi = ' + CAST(@MaDoi AS VARCHAR(10));

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
