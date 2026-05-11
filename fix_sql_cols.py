import re

def fix_yeucau_dal():
    filepath = r'DAL\YeuCauDAL.cs'
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Fix ho_ten -> ten_dang_nhap
    content = content.replace('u.ho_ten as ten_nguoi_gui', 'u.ten_dang_nhap as ten_nguoi_gui')
    content = content.replace('u.ho_ten as ten_chu_tich', 'u.ten_dang_nhap as ten_chu_tich')
    
    # Fix g.ngay_bat_dau -> GETDATE() as ngay_bat_dau
    content = content.replace('g.ngay_bat_dau, g.the_thuc', 'GETDATE() as ngay_bat_dau, g.the_thuc')

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_doidal():
    filepath = r'DAL\DoiDAL.cs'
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Remove gd.ngay_bat_dau, gd.ngay_ket_thuc from query
    content = content.replace('tgg.trang_thai_tham_gia, gd.ngay_bat_dau, gd.ngay_ket_thuc', 'tgg.trang_thai_tham_gia')
    # Change ORDER BY gd.ngay_bat_dau DESC -> ORDER BY gd.ma_giai_dau DESC
    content = content.replace('ORDER BY gd.ngay_bat_dau DESC', 'ORDER BY gd.ma_giai_dau DESC')
    
    # We still have r["ngay_bat_dau"] in C# code, but since it's no longer in SELECT, we should just assign null.
    # reader[\"ngay_bat_dau\"] will throw an error if the column is missing in reader.
    # So we need to comment out the reading or change it to not use the reader.
    content = re.sub(r'ngay_bat_dau = reader\["ngay_bat_dau"\][^,]*,', 'ngay_bat_dau = null,', content)
    content = re.sub(r'ngay_ket_thuc = reader\["ngay_ket_thuc"\][^,]*,', 'ngay_ket_thuc = null,', content)

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_giaidaudal():
    filepath = r'DAL\GiaiDauDAL.cs'
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # The query is SELECT gd.*
    # Since GIAI_DAU no longer has ngay_bat_dau, r["ngay_bat_dau"] will throw IndexOutOfRangeException.
    # We must replace r["ngay_bat_dau"] with DBNull.Value logic or just null.
    # Let's just assign null to ngay_bat_dau and created_at.
    content = re.sub(r'ngay_bat_dau\s*=\s*r\["ngay_bat_dau"\][^,]*,', 'ngay_bat_dau = null,', content)
    content = re.sub(r'created_at\s*=\s*r\["ngay_bat_dau"\][^,]*,', 'created_at = null,', content)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

fix_yeucau_dal()
fix_doidal()
fix_giaidaudal()
print('Fixes applied.')
