# README_AI

## Muc tieu

Doc file nay truoc khi sua du an. Day la ban tom tat rat ngan cua `AI_PROJECT_HANDBOOK.md`.

## Du an nay la gi

Nen tang quan ly giai dau esports, gom:

- auth va profile
- doi, nhom, tuyen dung
- tao va quan ly giai dau
- trong tai nhap ket qua
- admin duyet va quan tri
- public tournament page

## Cong nghe

- ASP.NET MVC 5
- .NET Framework 4.7.2
- SQL Server
- jQuery + JavaScript thuan
- Bootstrap
- CSS chinh: `GUI_HTML/Content/esport.css`

## Kien truc phai giu

- `GUI_HTML` -> giao dien, controller, js, css
- `BUS` -> nghiep vu
- `DAL` -> truy cap SQL
- `DTO` -> object trao doi du lieu

Khong dua logic SQL vao view.
Khong day nghiep vu lon vao controller.

## Theme giao dien

Phong cach hien tai:

- dark gaming dashboard
- accent tim + cyan
- card toi mau, border mo, glow nhe

Mau cot loi:

- bg dark: `#0d0f14`
- bg card: `#161b27`
- bg sidebar: `#111520`
- accent: `#6c63ff`
- accent2: `#00d4ff`
- danger: `#ff4757`
- success: `#2ed573`
- warning: `#ffa502`
- text primary: `#e8eaf6`
- text muted: `#7b8db0`

Khong doi sang theme sang.
Khong pha palette tim-cyan neu khong that su can.

## Layout web

### Private app

Private dashboard phai theo shell:

- `#app-shell`
- `#sidebar`
- `#main-area`
- `#topbar`
- `#content-area`

Neu them man hinh private moi, uu tien them vao `page-section` va dieu huong bang `navigateTo(...)`.

### Public app

- `/` la public landing
- `/giai/{id}` la trang cong khai giai dau
- public page van phai giu dark esport tone

## Route chinh

- `/` -> `Home/Index`
- `/login` -> `Portal/Login`
- `/dashboard` -> `Portal/Index`
- `/giai/{id}` -> `TournamentPublic/Index`

## Quy tac frontend

- frontend hien tai dua nhieu vao `app.js`
- goi API bang `fetch`
- nhieu ham duoc expose qua `window.*`
- dashboard van hanh kieu SPA noi bo

Khong chen React/Vue vao giua du an.
Khong viet mot he thong routing frontend moi.

## Quy tac API

API dang tra ve theo format:

- `Success`
- `Message`
- `Data`

Neu them API moi, phai giu format `ServiceResultDTO`.

## Session va phan quyen

Du an dang dung session:

- `CurrentUserId`
- `SystemRole`

Backend phai giu filter phan quyen.
Khong duoc chi an nut o frontend ma bo check backend.

## Component nen tai su dung

- `btn-primary-glow`
- `btn-outline-glow`
- `btn-save`
- `form-control-dark`
- `select-dark`
- `profile-card`
- `tab-bar`
- `tab-btn`
- `empty-state`

Neu co the, dung lai class cu truoc khi tao class moi.

## Cach mo rong dung

1. Doc `README_AI.md`
2. Doc `AI_PROJECT_HANDBOOK.md`
3. Doc `esport.css`
4. Doc `Portal/Index.cshtml`
5. Doc `app.js`
6. Sua controller/BUS/DAL theo domain lien quan

## Nguyen tac cuoi

Mo rong tren duong ray co san.
Khong viet lai theo style moi.
Khong pha theme.
Khong pha app shell.
Khong pha format API.
