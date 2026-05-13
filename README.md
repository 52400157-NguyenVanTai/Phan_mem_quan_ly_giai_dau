# Phan Mem Quan Ly Giai Dau Esports

Du an ASP.NET MVC quan ly doi tuyen, giai dau esports, yeu cau tham gia, loi moi, trong tai, lich dau va ket qua tran dau.

## Cau Truc Du An

- `DTO`: lop du lieu dung de truyen qua cac tang.
- `DAL`: truy cap SQL Server.
- `BUS`: xu ly nghiep vu.
- `GUI`: ung dung web ASP.NET MVC.
- `database.sql`: script tong hop tao lai toan bo database.
- `du_lieu_mau.sql`: script tong hop du lieu mau de test.

## Moi Truong Can Co

- Windows
- Visual Studio 2022 hoac moi hon
- .NET Framework 4.7.2 Developer Pack
- SQL Server, SQL Server Express hoac LocalDB
- IIS Express

Khi cai Visual Studio, nen chon workload `ASP.NET and web development`.

## 1. Cau Hinh Ket Noi Database

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

Mac dinh app ket noi database:

- Server: `localhost`
- Database: `QuanLy_Esports`
- Dang nhap: Windows Authentication

Neu dung SQL Express, sua `Data Source` thanh:

```txt
.\SQLEXPRESS
```

Neu dung LocalDB, sua `Data Source` thanh:

```txt
(localdb)\MSSQLLocalDB
```

Neu dung SQL Authentication, sua connection string theo mau:

```xml
Data Source=localhost;Initial Catalog=QuanLy_Esports;User ID=sa;Password=your_password;Encrypt=True;TrustServerCertificate=True;
```

## 2. Tao Database

Mo SQL Server Management Studio hoac Azure Data Studio, ket noi vao SQL Server, sau do chay file:

```txt
database.sql
```

Luu y: file nay se `DROP` va tao lai database `QuanLy_Esports`, nen du lieu cu se bi xoa.

File `database.sql` da duoc tong hop tu schema chinh va cac migration, vi vay khong can chay tung file `migration_*.sql` nua.

## 3. Them Du Lieu Mau

Sau khi chay xong `database.sql`, neu muon co tai khoan, doi, giai dau va tran dau mau de test, chay tiep:

```txt
du_lieu_mau.sql
```

Mat khau chung cho cac tai khoan mau:

```txt
123456
```

Mot so tai khoan mau de dang nhap:

```txt
demo_admin / demo_admin@example.com
demo_btc / demo_btc@example.com
demo_referee_1 / demo_referee_1@example.com
aov_phoenix_president / aov_phoenix_president@example.com
admin_seed_1 / admin_seed_1@example.com
```

## 4. Chay Du An Bang Visual Studio

1. Mo [GUI.slnx](GUI.slnx) bang Visual Studio.
2. Neu Visual Studio hoi restore NuGet packages, chon restore.
3. Dat project `GUI` lam Startup Project.
4. Kiem tra lai connection string trong [GUI/Web.config](GUI/Web.config).
5. Build solution bang `Ctrl + Shift + B`.
6. Bam `F5` hoac `Ctrl + F5` de chay bang IIS Express.

## 5. Build Bang Command Line

Neu muon build bang PowerShell:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' GUI.slnx /p:Configuration=Debug /v:minimal
```

Neu Visual Studio cai o duong dan khac, tim file `MSBuild.exe` trong thu muc Visual Studio cua may.

## Cac Trang Chinh

- `/Home/Index`: trang chu
- `/Home/DangNhap`: dang nhap
- `/Home/DangKy`: dang ky
- `/Doi`: danh sach va quan ly doi
- `/DoiCuaToi`: cac doi cua nguoi dung dang nhap
- `/GiaiDau`: tao, xem va quan ly giai dau
- `/YeuCau`: phe duyet giai, duyet doi tham gia, nhan loi moi
- `/TrongTai`: khu vuc trong tai

## Luu Y Khi Test Giai Dau

- Giai dau can duoc admin phe duyet truoc khi mo dang ky.
- Giai dau phai o trang thai `mo_dang_ky` thi doi moi dang ky duoc.
- Doi tham gia phai cung game voi giai dau.
- Nguoi dang ky doi vao giai phai la chu tich, doi truong hoac ban dieu hanh cua doi.
- Doi vua dang ky se co trang thai `cho_duyet`.
- BTC vao `/YeuCau` de duyet doi; sau khi duyet, doi moi thanh `da_duyet`.
- Khi khoi tranh, he thong chi lay cac doi `da_duyet`.

## Loi Thuong Gap

### Khong ket noi duoc database

- Kiem tra SQL Server dang chay.
- Kiem tra `Data Source` trong [GUI/Web.config](GUI/Web.config).
- Kiem tra database `QuanLy_Esports` da duoc tao.
- Neu dung SQL Authentication, kiem tra `User ID` va `Password`.

### Build loi do thieu .NET Framework

- Cai `.NET Framework 4.7.2 Developer Pack`.
- Cai workload `ASP.NET and web development` trong Visual Studio.

### Thieu NuGet package

- Mo solution bang Visual Studio va restore NuGet packages.
- Kiem tra thu muc `packages` trong project.

### Chay du lieu mau thay hanh vi khac luc test don gian

Du lieu mau co nhieu user, nhieu doi, nhieu game, loi moi va trang thai duyet. Vi vay cac dieu kien nhu dung game, dung quyen quan ly doi, `cho_duyet`, `da_duyet`, `bi_tu_choi` se anh huong truc tiep den ket qua test.

