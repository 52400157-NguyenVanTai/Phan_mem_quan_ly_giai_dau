# AI Project Handbook

## 1. Muc dich tai lieu

Tai lieu nay dung de onboarding nhanh cho AI hoac lap trinh vien moi khi tiep tuc phat trien du an `Phan_mem_quan_ly_giai_dau`. Muc tieu la giu cho cac thay doi moi:

- di dung kien truc hien co
- giu dung giao dien va tinh cach visual cua web
- khong pha vo quy uoc data flow, phan quyen va cach to chuc man hinh
- uu tien mo rong tren nen code san co thay vi viet lai theo style moi

## 2. Tong quan du an

Du an la nen tang quan ly giai dau esports, tap trung vao cac nghiep vu:

- dang ky va dang nhap nguoi dung
- tao doi, tao nhom thi dau, moi thanh vien, duyet xin gia nhap
- tao va quan ly giai dau
- duyet yeu cau giai dau
- tao giai doan thi dau, seeding, dong bo roster
- tao lich thi dau, cap nhat ket qua, xu ly khieu nai
- cong khai thong tin giai dau cho nguoi xem
- thong bao, theo doi, like, tim kiem

He thong dang co 2 dau giao dien:

- `GUI_HTML`: web app chinh, day la phan can uu tien khi phat trien tiep
- `Phan_mem_quan_ly_esport`: dau WinForms cu, hien chi thay file thiet ke con sot lai, khong phai trung tam cua workflow hien tai

## 3. Cong nghe va nen tang

- Backend web: ASP.NET MVC 5
- Framework: .NET Framework 4.7.2
- Frontend: Razor Views + jQuery + JavaScript thuan + Bootstrap
- CSS chinh: `GUI_HTML/Content/esport.css`
- Database: SQL Server, database `QuanLy_Esports`
- Connection string duoc khai bao trong `GUI_HTML/Web.config` voi key `MyDbConn`

Luu y:

- ung dung dang su dung session cho xac thuc va phan quyen
- API tra ve JSON theo mot format chung `ServiceResultDTO`
- giao dien frontend goi API truc tiep bang `fetch`

## 4. Kien truc tong the

He thong duoc chia thanh 4 tang ro rang:

1. `GUI_HTML`
2. `BUS`
3. `DAL`
4. `database.sql`

Y nghia tung tang:

- `GUI_HTML`: controller, view, js, css, filter phan quyen
- `BUS`: xu ly nghiep vu
- `DAL`: truy van SQL, thao tac `DataTable`, `SqlConnection`, `SqlParameter`
- `DTO`: cac object du lieu trao doi giua cac tang

Nguyen tac can giu:

- Controller khong nen nhan qua nhieu nghiep vu phuc tap
- Nghiep vu dat o `BUS`
- Truy cap database dat o `DAL`
- DTO la hop dong du lieu giua frontend, controller, bus, dal
- Khong nen truyen linh tinh object anonymous phuc tap neu da co DTO tuong ung

## 5. Cau truc thu muc quan trong

### Thu muc backend

- `BUS/`
- `DAL/`
- `DTO/`
- `database.sql`

### Thu muc web app

- `GUI_HTML/Controllers`
- `GUI_HTML/Views`
- `GUI_HTML/Scripts`
- `GUI_HTML/Content`
- `GUI_HTML/Filters`
- `GUI_HTML/App_Start`

### File frontend cot song

- `GUI_HTML/Content/esport.css`
- `GUI_HTML/Scripts/app.js`
- `GUI_HTML/Scripts/tournament-public.js`
- `GUI_HTML/Views/Portal/Index.cshtml`
- `GUI_HTML/Views/Home/Index.cshtml`
- `GUI_HTML/Views/TournamentPublic/Index.cshtml`

## 6. Route va entry points chinh

Tu `RouteConfig.cs`, cac duong vao chinh hien tai la:

- `/` -> `Home/Index`: landing public
- `/login` -> `Portal/Login`: trang auth
- `/dashboard` -> `Portal/Index`: dashboard rieng sau dang nhap
- `/giai/{id}` -> `TournamentPublic/Index`: trang cong khai cua giai dau

Quy tac:

- public landing va private dashboard la 2 trai nghiem khac nhau
- khong duoc tron layout public va private vao cung mot shell
- neu them mot man hinh private lon, nen dua vao dashboard app shell thay vi tao mot layout rieng lech tong the

## 7. Cac module nghiep vu hien co

Du an da chia module kha ro trong `Controllers` va `BUS/DAL`.

### Auth va profile

- `AuthApiController`
- `ProfileApiController`
- `UploadApiController`

Chuc nang:

- dang ky
- dang nhap
- doi mat khau
- cap nhat thong tin co ban
- tao ho so in-game
- upload avatar

### Team va recruitment

- `TeamApiController`
- `RecruitmentApiController`

Chuc nang:

- tao doi
- tao nhom
- them thanh vien
- xin gia nhap
- duyet xin gia nhap
- gui loi moi
- bat tat che do dang tuyen
- thong bao team
- tim kiem doi cong khai

### Tournament va tournament builder

- `TournamentApiController`
- `TournamentBuilderApiController`

Chuc nang:

- gui yeu cau tao giai
- tao ban nhap giai dau
- gui xet duyet
- phe duyet, tu choi, khoa, mo khoa
- them giai doan
- duyet dang ky doi
- cap nhat hat giong
- dong bo roster
- bat dau giai

### Matchmaking va public tournament

- `MatchmakingApiController`
- `TournamentPublicController`

Chuc nang:

- tao lich
- tao vong tiep theo
- xem danh sach tran
- bang xep hang
- vinh danh
- live snapshot
- cong khai giai dau

### Referee

- `RefereeApiController`

Chuc nang:

- gan trong tai
- xem tran cua toi
- chi tiet tran
- nhap ket qua
- sua ket qua
- admin sua ket qua
- tao va xu ly khieu nai
- xem lich su sua ket qua

### Admin

- `AdminApiController`

Chuc nang:

- dashboard thong ke
- tim kiem user
- ban, unban user
- ban doi
- quan ly game
- hard wipe giai dau

### Tuong tac

- `TuongTacApiController`

Chuc nang:

- like
- follow
- trang thai tuong tac
- danh sach theo doi

## 8. Mo hinh du lieu tong quat

Tu `database.sql`, cac nhom bang quan trong gom:

### Danh muc game va vi tri

- `TRO_CHOI`
- `DANH_MUC_VI_TRI`

### Nguoi dung va ho so

- `NGUOI_DUNG`
- `HO_SO_IN_GAME`

### Doi va tuyen dung

- `DOI`
- `NHOM_DOI`
- `THANH_VIEN_DOI`
- `BAI_DANG_TUYEN_DUNG`
- `DON_UNG_TUYEN`
- `LOI_MOI_GIA_NHAP`
- `XIN_GIA_NHAP`

### Giai dau

- `GIAI_DAU`
- `QUAN_TRI_GIAI_DAU`
- `YEU_CAU_TAO_GIAI_DAU`
- `THAM_GIA_GIAI`
- `DOI_HINH_THI_DAU`
- `GIAI_THUONG`
- `GIAI_DOAN`
- `TRAN_DAU`
- `CHI_TIET_TRAN_DAU`
- `KET_QUA_TRAN`
- `CHI_TIET_NGUOI_CHOI_TRAN`

### Giam sat va tranh chap

- `LICH_SU_SUA_KET_QUA`
- `KHIEU_NAI_KET_QUA`

### Bang xep hang va thong bao

- `BANG_XEP_HANG`
- `BANG_XEP_HANG_CA_NHAN`
- `THONG_BAO`
- `TUONG_TAC_GIAI_DAU`

Nhan dinh:

- schema duoc thiet ke theo huong domain-rich, nghia la nghiep vu giai dau kha day du
- AI moi nen tai su dung thuc the hien co truoc khi tao bang moi
- neu can them tinh nang, uu tien mo rong bang hien co hoac them bang phu hop domain, tranh dat ten chung chung

## 9. Quy tac response API

`DTO/CoreDtos.cs` cho thay API hien tai su dung `ServiceResultDTO`:

- `Success`
- `Message`
- `Data`

Quy tac bat buoc:

- API moi nen tra ve cung format nay
- frontend hien tai da ky vong `result.Success`, `result.Message`, `result.Data`
- khong nen tra ve raw object khong co vo boc `ServiceResultDTO`

## 10. Session va phan quyen

Web dang dung session-based auth.

### Dau moc session quan trong

- `CurrentUserId`
- `SystemRole`

### Filter dang co

- `RequireLoginAttribute`
- `RequireSystemRoleAttribute`
- ngoai ra con cac filter theo role doi va trong tai

Nguyen tac:

- action private phai check dang nhap
- action admin phai di qua filter role he thong
- khong hard-code phan quyen o frontend roi bo qua backend
- frontend co the an hien nut, nhung backend van la lop chan chinh

## 11. Theme giao dien web

Day la phan rat quan trong. AI khac can bam sat theme nay, khong duoc tu y doi style sang mot ngon ngu thiet ke khac.

### Ban sac tong the

Visual hien tai la:

- dark gaming dashboard
- futuristic / esports
- nhan manh bang glow, gradient, card toi mau
- pha tron giua utility cua dashboard va cam giac san dau

### Mau chu dao

Lay truc tiep tu `:root` trong `esport.css`:

- `--bg-dark: #0d0f14`
- `--bg-card: #161b27`
- `--bg-sidebar: #111520`
- `--accent: #6c63ff`
- `--accent2: #00d4ff`
- `--danger: #ff4757`
- `--success: #2ed573`
- `--warning: #ffa502`
- `--text-primary: #e8eaf6`
- `--text-muted: #7b8db0`
- `--border: rgba(255, 255, 255, 0.07)`

### Y nghia su dung mau

- nen chinh: rat toi, xanh den
- card/sidebar/topbar: toi hon nen nhung tach lop nhe
- accent tim `#6c63ff`: mau dieu huong chinh, nut primary, tab active, glow
- accent cyan `#00d4ff`: mau phu cho thong tin, rank, emphasis thu cap
- do `#ff4757`: danger, live, xoa, canh bao
- xanh la `#2ed573`: thanh cong, chi so tot
- vang/cam `#ffa502`: giai thuong, huy hieu, game dot mau nong

### Quy tac khong duoc pha

- khong doi sang theme sang
- khong thay accent tim-cyan bang palette moi neu khong co ly do rat manh
- khong dua card trang, form trang, modal trang vao private dashboard
- khong them nhieu gradient mau la lam dut visual language

## 12. Typography va visual language

He thong dang dung 2 font:

- `Inter`: text UI chinh
- `Orbitron`: brand, hero stat, tieu de esports

Quy tac:

- title thuong dung dam, gon, ro
- brand / hero / stat quan trong moi dung `Orbitron`
- body text, label, input, button nen giu `Inter`
- letter spacing co su dung o logo, eyebrow, label uppercase

## 13. Layout chuan cho web

### A. Private dashboard shell

Trang private mac dinh phai theo cau truc:

- `#app-shell`
- `#sidebar`
- `#main-area`
- `#topbar`
- `#content-area`

Y nghia:

- `sidebar` chua dieu huong chinh va game categories
- `topbar` chua search, thong bao, avatar, menu nhanh
- `content-area` chua cac `page-section`

Neu tao man hinh private moi:

- khong tao full page moi tach biet neu no la mot module thuoc dashboard
- nen them mot `page-section` moi va dieu huong bang `navigateTo(...)`
- can giu mot trai nghiem SPA-like ben trong dashboard

### B. Public page

Public page dang tach rieng:

- `public-topbar`
- `public-body`
- trang chi tiet giai dau dung Bootstrap container + script rieng

Quy tac:

- public page cho phep don gian hon private dashboard
- van phai giu dark theme esport
- khong bien public page thanh mot marketing site mau sac lac tong

## 14. Pattern component can tai su dung

Trong `esport.css`, nhieu component da co san. Uu tien dung lai class cu.

### Nut

- `btn-primary-glow`
- `btn-outline-glow`
- `btn-save`
- `btn-detail`
- `btn-auth`

### Form

- `form-group-dark`
- `form-control-dark`
- `select-dark`
- `auth-input`

### Card

- `profile-card`
- `tournament-card`
- `team card` theo cac class `team-grid`, `mt-*`
- `notif-item`

### Navigation / Tabs

- `sidebar-item`
- `tab-bar`
- `tab-btn`
- `game-profile-tabs`
- `mt-tabs`

### Status / Badge

- `tc-status`
- `t-status-badge`
- `mt-game-badge`
- `mt-badge-role`
- `mt-badge-pos`

Quy tac:

- them tinh nang moi thi uu tien ghep tu class san co
- chi tao class moi khi component thuc su khac ban chat
- tranh duplicate style cung nghia nhung ten class moi

## 15. Quy tac to chuc man hinh

### Dashboard page-section

Moi man hinh trong private dashboard nen co:

- `page-title`
- `page-sub`
- 1 action area neu can
- 1 hoac nhieu card / grid / tab ro rang

### Card va grid

- giai dau: su dung grid card
- thong bao: danh sach doc theo chieu dung
- profile: card tach theo nhom thong tin
- my teams: layout 2 cot `main + sidebar`

### Search va filter

Neu mot man hinh co danh sach lon:

- uu tien co `filter-bar`
- neu co search input, style theo `form-control-dark` hoac `search-wrap`

### Empty state

Phai co empty state ro rang, dung class:

- `empty-state`
- `empty-state-icon`

## 16. Quy tac frontend logic

Tu `app.js` co the thay mot so nguyen tac rat ro:

- JavaScript thuan va jQuery cung ton tai, nhung fetch dang la huong chinh cho API
- nhieu ham duoc gan vao `window` de view goi truc tiep bang `onclick`
- private dashboard hoat dong gan giong SPA noi bo
- dieu huong man hinh bang `navigateTo(page)`

AI moi can tuan thu:

- neu sua tinh nang hien co, ton trong co che `window.functionName = ...`
- neu them page moi trong dashboard, noi vao `navigateTo`
- neu them API call moi, giu style `api(...)`, `postForm(...)` hoac `fetch` dang co
- khong pha cach to chuc do de chen framework frontend moi vao giua du an

Khong nen:

- dua React, Vue, Angular vao mot man hinh rieng
- viet song song 2 he thong routing frontend
- thay het `onclick` inline bang mot pattern moi nua mua roi bo do

## 17. Quy tac dat ten va mo rong backend

### Controllers

Pattern hien co:

- `XxxApiController` cho JSON endpoint
- controller thuong cho page render view

Neu them tinh nang moi:

- neu la giao dien render page, dat o controller page
- neu la data endpoint, dua vao `XxxApiController` dung domain

### BUS / DAL

Pattern hien co:

- `GameBUS` di voi `GameDAL`
- `TeamBUS` di voi `TeamDAL`
- `TournamentBuilderBUS` di voi `TournamentBuilderDAL`

Quy tac:

- them nghiep vu team -> sua `TeamBUS`, `TeamDAL`
- them nghiep vu tournament builder -> sua `TournamentBuilderBUS`, `TournamentBuilderDAL`
- tranh dat logic dung chung vao file `Class1.cs`

### DTO

Khi API co payload ro rang:

- tao DTO moi trong `CoreDtos.cs` hoac tach file neu sau nay qua lon
- dat ten theo nghiep vu, vi du `TaoLichGiaiDoanDTO`, `RefereeSubmitResultDTO`

## 18. Quy tac phat trien giao dien cho AI khac

Neu AI khac duoc giao tao man hinh moi, phai tuan thu:

1. bat dau bang dark background va dung bien mau san co
2. neu la private feature thi dat trong dashboard shell
3. dung `page-title`, `page-sub`, card toi mau, border mo, radius mem
4. nut primary dung tim glow, nut secondary dung outline accent
5. text phu dung `--text-muted`, text chinh dung `--text-primary`
6. trang thai thanh cong, loi, canh bao phai theo mau success, danger, warning san co
7. neu co tab, filter, badge thi uu tien dung pattern da co trong `esport.css`
8. neu co danh sach giai dau, doi, thong bao thi uu tien render bang card/grid thay vi table tho
9. neu la public detail du lieu ky thuat nhu bang xep hang, bracket, match list thi co the dung Bootstrap table/card, nhung van giu dark context neu sua them

## 19. Nhung diem can luu y khi sua code

### Khong nen lam

- khong xoa theme dark
- khong thay toan bo `esport.css` bang framework style khac
- khong doi ten API response format
- khong bo qua filter phan quyen o backend
- khong tron nghiep vu controller va SQL truc tiep trong view
- khong tao mot style moi khac hoan toan cho 1 man hinh private

### Nen lam

- tai su dung class CSS san co
- them ham vao `app.js` theo domain va theo flow hien co
- giu page shell on dinh
- them DTO ro rang khi payload phuc tap
- giu ten bien va ten action bang nghia tieng Viet khong dau theo phong cach du an hien tai

## 20. Cac thuc the va use case cot loi can hieu truoc khi mo rong

AI moi nen hieu cac quan he sau:

- 1 nguoi dung co the co nhieu ho so in-game
- 1 doi co the co nhieu nhom thi dau
- 1 nhom gan voi 1 game cu the
- 1 giai dau co nhieu giai doan
- 1 giai doan sinh tran dau, bang xep hang, vinh danh
- trong tai nhap va sua ket qua
- admin duyet, ban, hard wipe, quan ly game

Neu mot tinh nang moi dong vao 1 trong cac truc nay, can mo rong dung domain cua no thay vi tao mot luong song song.

## 21. Danh gia nhanh hien trang du an

### Diem manh

- domain kha day du
- da co chia tang ro rang
- da co private dashboard va public flow
- da co bo CSS rieng rat ro visual language
- da co he API phan theo module kha tot

### Diem can than trong

- `app.js` dang lon va gom rat nhieu nhiem vu
- mot so text file dang co dau hieu loi encoding
- public landing `Home/Index` con rat so khai
- code frontend dang co kha nhieu inline onclick, can sua theo cach ton trong he thong cu

## 22. Huong de xuat khi phat trien tiep

Neu co AI khac tiep tuc du an, uu tien theo thu tu:

1. doc file nay
2. doc `GUI_HTML/Content/esport.css`
3. doc `GUI_HTML/Views/Portal/Index.cshtml`
4. doc `GUI_HTML/Scripts/app.js`
5. doc controller domain lien quan
6. doc `DTO/CoreDtos.cs`
7. chi sau do moi sua `BUS` va `DAL`

## 23. Ket luan quy chuan ngan gon

Tom tat mot cau:

> Day la mot nen tang quan ly giai dau esports theo phong cach dark gaming dashboard, su dung ASP.NET MVC + BUS/DAL/DTO + SQL Server, trong do AI moi phai ton trong app shell hien co, palette tim-cyan tren nen toi, response API `ServiceResultDTO`, va cach chia module theo domain nghiep vu.

Neu can mo rong, hay mo rong tren duong ray san co, khong dung mot ngon ngu thiet ke hay kien truc moi chen ngang vao du an.
