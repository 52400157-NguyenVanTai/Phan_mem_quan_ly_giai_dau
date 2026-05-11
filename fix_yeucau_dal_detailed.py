import re

filepath = r'DAL\YeuCauDAL.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. sqlLoiMoiDoi
content = content.replace('SELECT y.ma_yeu_cau, y.ma_nguoi_gui, y.ma_doi, d.ten_doi, y.mo_ta, y.thoi_gian_tao', 'SELECT y.ma_yeu_cau, y.ma_nguoi_gui, y.ma_doi, d.ten_doi, y.mo_ta, y.ngay_tao as thoi_gian_tao')
content = content.replace('WHERE y.ma_nguoi_nhan = @UserId AND y.trang_thai = \'cho_xac_nhan\'', 'WHERE y.ma_nguoi_duoc_moi = @UserId AND y.trang_thai = \'cho_duyet\'')

# 2. sqlXinVaoDoi
content = content.replace('SELECT y.ma_yeu_cau, y.ma_nguoi_gui, u.ten_dang_nhap as ten_nguoi_gui, y.ma_nhom, d.ten_doi, y.loi_nhan, y.thoi_gian_tao', 'SELECT y.ma_yeu_cau, y.ma_nguoi_dung as ma_nguoi_gui, u.ten_dang_nhap as ten_nguoi_gui, y.ma_nhom, d.ten_doi, NULL as loi_nhan, y.ngay_tao as thoi_gian_tao')
content = content.replace('JOIN NGUOI_DUNG u ON y.ma_nguoi_gui = u.ma_nguoi_dung', 'JOIN NGUOI_DUNG u ON y.ma_nguoi_dung = u.ma_nguoi_dung')
content = content.replace('WHERE d.ma_doi_truong = @UserId AND y.trang_thai = \'cho_xac_nhan\'', 'WHERE d.ma_doi_truong = @UserId AND y.trang_thai = \'cho_duyet\'')

# 3. sqlAddMem
content = content.replace('SELECT ma_doi, ma_nguoi_nhan, \'thanh_vien\' FROM YEU_CAU_MOI_THANH_VIEN_DOI', 'SELECT ma_doi, ma_nguoi_duoc_moi, \'thanh_vien\' FROM YEU_CAU_MOI_THANH_VIEN_DOI')
content = content.replace('SELECT ma_nhom, ma_nguoi_gui, \'thanh_vien\' FROM YEU_CAU_THAM_GIA_NHOM', 'SELECT ma_nhom, ma_nguoi_dung, \'thanh_vien\' FROM YEU_CAU_THAM_GIA_NHOM')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print('Applied detailed fixes to YeuCauDAL.cs')
