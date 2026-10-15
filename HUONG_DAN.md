# Hướng dẫn: chạy riêng phần Hạnh kiểm & ghép vào code chung của nhóm

Phiên bản này KHÔNG dùng stored procedure: form gọi trực tiếp các câu SQL có tham số
(`@MaHS`, `@MaLop`, ...) thông qua `DatabaseHelper.LayBang(...)` và `DatabaseHelper.ThucThi(...)`.

## 1. Cấu trúc project
```
HanhKiemApp/
├── HanhKiemApp.csproj
├── Program.cs                    <- Kiểm tra kết nối rồi mở frmHanhKiem
├── DatabaseHelper.cs             <- Kết nối MySQL, đọc appsettings.json
├── appsettings.example.json      <- File mẫu (copy thành appsettings.json)
├── .gitignore
├── Forms/
│   └── frmHanhKiem.cs            <- Màn hình Quản lý hạnh kiểm (phần bạn gửi nhóm)
└── database/
    └── 01_schema_test.sql        <- CHỈ để test riêng: bảng tối thiểu + dữ liệu mẫu
```

## 2. Chạy thử trên máy bạn (Windows + .NET 8 SDK + MySQL 8.0+)

**Bước 1 – Tạo CSDL test** (CSDL riêng `QuanLyDiemTHCS_Test`, không đụng CSDL chung):
```bash
cd database
mysql -u root -p < 01_schema_test.sql
```
Kiểm tra nhanh: `SELECT * FROM QuanLyDiemTHCS_Test.tblHanhKiem;` phải ra 8 dòng.

**Bước 2 – Cấu hình kết nối:** copy `appsettings.example.json` thành `appsettings.json`
(cùng thư mục với `HanhKiemApp.csproj`), sửa `Uid` / `Pwd` đúng với MySQL của bạn.

**Bước 3 – Chạy:**
```bash
dotnet restore
dotnet run
```

### Nếu muốn tự tạo project từ đầu (thay vì dùng thư mục này)
```bash
dotnet new winforms -n HanhKiemApp -f net8.0
cd HanhKiemApp
dotnet add package MySql.Data --version 8.4.0
```
Sau đó thêm các file `DatabaseHelper.cs`, `Program.cs`, `Forms/frmHanhKiem.cs` vào,
và thêm đoạn sau vào `.csproj` để file `appsettings.json` được copy sang thư mục chạy:
```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## 3. Ghép vào project chung của nhóm
1. **Chỉ cần gửi `Forms/frmHanhKiem.cs`** (không cần gửi file SQL nào, không gửi `appsettings.json`).
2. Các bảng `tblHocSinh`, `tblLopHoc`, `tblHocKy`, `tblPhanLop`, `tblHanhKiem` trong CSDL chung phải có
   đúng tên bảng/cột như sơ đồ chung (form đang dùng chính các tên đó).
3. Chép `frmHanhKiem.cs` vào thư mục `Forms/` của nhóm và đổi dòng
   `namespace HanhKiemApp.Forms` cho trùng namespace dự án nhóm.
4. **DatabaseHelper:** form cần 2 hàm `LayBang(sql, params MySqlParameter[])` (trả về DataTable) và
   `ThucThi(sql, params MySqlParameter[])` (INSERT/UPDATE/DELETE). Nếu `DatabaseHelper` của nhóm chưa có,
   hãy chép 2 hàm này từ file `DatabaseHelper.cs` của bạn sang; nhớ đổi `using HanhKiemApp;` nếu cần.
5. **Mở màn hình** từ menu chính của nhóm: `new frmHanhKiem().ShowDialog();`

## 4. Lưu ý
- Form tự chặn nhập trùng (1 học sinh – 1 lớp – 1 học kỳ chỉ có 1 dòng hạnh kiểm) bằng một câu SELECT COUNT trước khi thêm/sửa.
  Muốn chặn chắc chắn ở tầng CSDL, nhờ nhóm thêm: `ALTER TABLE tblHanhKiem ADD UNIQUE (MaHS, MaLop, MaHocKy);`
- Chọn học sinh ở khung nhập thì lớp tự nhảy theo lớp gần nhất trong `tblPhanLop`.
- Không commit `appsettings.json` (đã có trong `.gitignore`).
