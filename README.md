# Phan Mem Quan Ly Giai Dau Esports

Ung dung ASP.NET MVC dung mo hinh 3 lop:

- `DTO`: cac lop truyen du lieu
- `DAL`: truy cap SQL Server
- `BUS`: xu ly nghiep vu
- `GUI`: web ASP.NET MVC

## Yeu Cau Moi Truong

- Windows
- Visual Studio 2022 hoac moi hon
- .NET Framework 4.7.2 Developer Pack
- SQL Server hoac SQL Server Express/LocalDB
- IIS Express, thuong co san khi cai Visual Studio

## Cau Hinh Database

Connection string nam trong [GUI/Web.config](GUI/Web.config):

```xml
<add name="MyDbConn"
     connectionString="Data Source=localhost;
       Initial Catalog=QuanLy_Esports;
       Integrated Security=True;
       Encrypt=True;
       TrustServerCertificate=True;
       Connect Timeout=60;"
     providerName="System.Data.SqlClient" />
```

Mac dinh ung dung ket noi SQL Server tai `localhost`, database `QuanLy_Esports`, dung Windows Authentication.

Neu may ban dung SQL Express, sua `Data Source` thanh:

```txt
.\SQLEXPRESS
```

Neu dung LocalDB, co the sua thanh:

```txt
(localdb)\MSSQLLocalDB
```

## Tao Database

1. Mo SQL Server Management Studio hoac Azure Data Studio.
2. Ket noi vao SQL Server cua ban.
3. Tao database:

```sql
CREATE DATABASE QuanLy_Esports;
GO
USE QuanLy_Esports;
GO
```

4. Chay script chinh:

```txt
database.sql
```

5. Neu can cap nhat them cac tinh nang moi, chay tiep cac migration theo thu tu gan dung sau:

```txt
migration_doi.sql
migration_phase2.sql
migration_tournament_engine.sql
migration_20260507_ho_so_thi_dau.sql
migration_fix_gd_trangthai.sql
migration_add_thongbao_ma_doi.sql
migration_remove_nhom_doi.sql
fix_trigger_thanh_vien_doi.sql
migration_fix_performance.sql
migration_van_hanh_giai_dau.sql
migration_ket_qua_van_dau.sql
```

6. Neu muon co du lieu mau de test, chay mot trong cac file seed:

```txt
seed_demo_full.sql
seed_test_tournament_8teams.sql
seed_aov_sample.sql
```

Khuyen nghi dung `seed_demo_full.sql` neu muon test tong the nhieu chuc nang.

## Chay Du An Bang Visual Studio

1. Mo file [GUI.slnx](GUI.slnx) bang Visual Studio.
2. Cho Visual Studio restore NuGet packages neu duoc hoi.
3. Dat `GUI` lam startup project.
4. Kiem tra connection string trong [GUI/Web.config](GUI/Web.config).
5. Build solution voi `Ctrl + Shift + B`.
6. Bam `F5` hoac `Ctrl + F5` de chay bang IIS Express.

## Build Bang Command Line

Neu co MSBuild cua Visual Studio:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' GUI.slnx /p:Configuration=Debug /v:minimal
```

Neu duong dan Visual Studio khac, tim `MSBuild.exe` trong thu muc cai Visual Studio cua may ban.

## Cac Trang Chinh

- `/Home/Index`: trang chu
- `/Home/DangNhap`: dang nhap
- `/Home/DangKy`: dang ky
- `/Doi`: quan ly doi
- `/DoiCuaToi`: cac doi cua nguoi dung
- `/GiaiDau`: danh sach, tao va quan ly giai dau
- `/YeuCau`: xu ly loi moi, yeu cau tham gia, phe duyet
- `/TrongTai`: khu vuc trong tai

## Luu Y Khi Test Giai Dau

- Giai dau phai duoc phe duyet truoc khi mo dang ky.
- Doi tham gia giai phai cung game voi giai dau.
- User dang ky doi vao giai phai la chu tich, doi truong hoac ban dieu hanh cua doi.
- Doi moi dang ky se o trang thai `cho_duyet`; BTC can vao `/YeuCau` de duyet.
- Khi BTC duyet, doi moi tinh la `da_duyet` va duoc dung khi khoi tranh/sinh tran.

## Loi Thuong Gap

### Khong ket noi duoc database

- Kiem tra SQL Server dang chay.
- Kiem tra `Data Source` trong [GUI/Web.config](GUI/Web.config).
- Kiem tra database `QuanLy_Esports` da ton tai.
- Neu dung SQL Authentication, sua connection string de them `User ID` va `Password`.

### Loi thieu package

- Mo Visual Studio va restore NuGet packages.
- Dam bao thu muc `packages` ton tai hoac cho Visual Studio tai lai package.

### Build loi do .NET Framework

- Cai `.NET Framework 4.7.2 Developer Pack`.
- Kiem tra Visual Studio co workload `ASP.NET and web development`.

### Du lieu mau lam hanh vi khac luc test ban dau

Du lieu mau co nhieu user, doi, game, trang thai duyet va loi moi. Vi vay cac dieu kien nhu dung game, dung quyen quan ly doi, trang thai `cho_duyet/da_duyet/bi_tu_choi` se anh huong truc tiep den ket qua test.

