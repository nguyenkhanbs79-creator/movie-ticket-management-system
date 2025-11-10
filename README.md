# CinemaSolution

## Giới thiệu
- Bài tập lớn môn Lập trình hướng đối tượng C# xây dựng hệ thống quản lý bán vé xem phim.
- Ứng dụng Windows Forms kết hợp ADO.NET theo mô hình kiến trúc 3 tầng để củng cố kiến thức học phần.

## Kiến trúc thư mục
```
CinemaSolution
 ├─ Cinema.Entities
 ├─ Cinema.DAL
 ├─ Cinema.BLL
 ├─ Cinema.WinForms
 └─ sql/001_create_db.sql
```

## Công nghệ sử dụng
- .NET 8
- Windows Forms
- ADO.NET
- SQL Server
- Kiến trúc 3 tầng (Entities, DAL, BLL, UI)

## Yêu cầu môi trường
- Hệ điều hành Windows.
- .NET 8 SDK.
- SQL Server (LocalDB hoặc Express).
- Quyền tạo và chỉnh sửa cơ sở dữ liệu.

## Cài đặt CSDL
- Mở SQL Server Management Studio hoặc công cụ tương đương.
- Chạy script `sql/001_create_db.sql`.
- Script sẽ tự tạo database, bảng, view, stored procedure và seed tài khoản admin cùng dữ liệu mẫu.

## Cấu hình kết nối
- Chỉnh sửa `connectionStrings["CinemaDb"]` trong App.config của các project `Cinema.DAL`, `Cinema.BLL`, `Cinema.WinForms`.
- Ví dụ: `Server=.;Database=CinemaDb;Trusted_Connection=True;TrustServerCertificate=True`.

## Build & Run
- Mở `CinemaSolution.sln` bằng Visual Studio.
- Restore NuGet packages nếu được yêu cầu.
- Chọn `Cinema.WinForms` làm Startup Project.
- Build và chạy ứng dụng.

## Tài khoản demo
- Username: `admin`
- Password: `admin` (hash MD5 demo, không dùng cho môi trường thật).

## Tính năng
- Đăng nhập xác thực người dùng.
- Quản lý phim (CRUD) và suất chiếu (CRUD).
- Bán vé với kiểm tra và khóa ghế theo từng suất chiếu.
- Tìm kiếm phim, suất chiếu theo tiêu chí.
- Báo cáo doanh thu theo ngày và thống kê Top phim qua view/proc SQL.

## Ảnh chụp màn hình
- ![Login](docs/login.png) *(sẽ bổ sung ảnh sau)*
- ![Main](docs/main.png) *(sẽ bổ sung ảnh sau)*
- ![Report](docs/report.png) *(sẽ bổ sung ảnh sau)*

## Khắc phục sự cố
- Không kết nối được cơ sở dữ liệu: kiểm tra connection string và quyền truy cập SQL Server.
- Lỗi build: đảm bảo đã cài .NET 8 SDK và restore đầy đủ packages.
- Lỗi đăng nhập: chắc chắn đã chạy script SQL để seed tài khoản.

## Ghi chú bảo mật
- Mật khẩu lưu dưới dạng MD5 chỉ nhằm mục đích minh họa, không sử dụng trong sản phẩm thực tế.

## License
- MIT (có thể bổ sung chi tiết sau).
