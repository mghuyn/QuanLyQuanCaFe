# ☕ Hệ thống Quản lý Quán Cà Phê

Hệ thống quản lý quán cà phê được xây dựng bằng **C# WPF**, áp dụng mô hình **MVVM**, kết hợp **SQL Server** để quản lý dữ liệu và **Crystal Reports** để xây dựng báo cáo.

Project được phát triển nhằm mô phỏng quy trình vận hành thực tế của một quán cà phê, từ quản lý bàn, gọi món, thanh toán, pha chế, quản lý kho đến báo cáo doanh thu.

---

## 📌 Giới thiệu

Hệ thống hỗ trợ nhân viên quản lý và vận hành quán cà phê thông qua giao diện desktop.

Các nghiệp vụ chính:

- Quản lý khu vực và bàn
- Quản lý sản phẩm và danh mục
- Quản lý biến thể sản phẩm
- Bán hàng / gọi món
- Quản lý hóa đơn
- Quản lý trạng thái bàn
- Xử lý đơn pha chế
- Quản lý nguyên liệu và kho
- Quản lý nhân viên
- Phân quyền người dùng
- Quản lý khách hàng
- Báo cáo doanh thu
- Xuất và xem báo cáo bằng Crystal Reports

---

## 🛠️ Công nghệ sử dụng

| Công nghệ | Mục đích |
|---|---|
| C# | Ngôn ngữ lập trình chính |
| WPF | Xây dựng giao diện Desktop |
| MVVM | Kiến trúc ứng dụng |
| SQL Server | Quản lý cơ sở dữ liệu |
| Entity Framework | Kết nối và thao tác dữ liệu |
| Crystal Reports | Xây dựng báo cáo |
| XAML | Thiết kế giao diện |
| Visual Studio | Môi trường phát triển |

---

## 🏗️ Kiến trúc

Project được tổ chức theo mô hình **MVVM (Model - View - ViewModel)**.

```text
QuanLyQuanCaFe
│
├── Models
│   └── Các model và Entity Framework
│
├── Views
│   ├── Pages
│   └── Các giao diện WPF
│
├── ViewModels
│   └── Logic trung gian giữa View và Model
│
├── Services
│   └── Xử lý nghiệp vụ và truy cập dữ liệu
│
├── Reports
│   └── Crystal Reports
│
└── Database
    └── SQL Scripts
MVVM

Model

Đại diện cho dữ liệu và các entity trong hệ thống.

Ví dụ:

Sản phẩm
Bàn
Hóa đơn
Nhân viên
Khách hàng
Nguyên liệu

View

Được xây dựng bằng XAML và WPF, chịu trách nhiệm hiển thị giao diện.

ViewModel

Xử lý logic giao diện, Binding dữ liệu và Command.

Ví dụ:

View
 ↓
Data Binding
 ↓
ViewModel
 ↓
Service
 ↓
Database
✨ Chức năng chính
1. 🔐 Đăng nhập và phân quyền

Hệ thống hỗ trợ đăng nhập và phân quyền theo vai trò nhân viên.

Ví dụ:

Quản lý
Thu ngân
Nhân viên pha chế

Các chức năng được hiển thị dựa trên quyền của tài khoản.

2. 🪑 Quản lý bàn

Cho phép quản lý bàn theo từng khu vực.

Ví dụ:

Tầng trệt A
├── A1
├── A2
├── A3
└── ...

Tầng trệt B
├── B1
├── B2
├── B3
└── ...

Lầu 1C
├── 1C-1
├── 1C-2
└── ...

Các trạng thái bàn:

Trống
Đang phục vụ
Cần dọn

Quy trình trạng thái:

Trống
  ↓
Đang phục vụ
  ↓
Thanh toán
  ↓
Cần dọn
  ↓
Nhân viên dọn bàn
  ↓
Trống

Sau khi thanh toán, bàn không chuyển trực tiếp về trạng thái Trống mà chuyển sang Cần dọn.

3. 🛒 Bán hàng

Nhân viên thu ngân có thể:

Chọn bàn
Chọn danh mục sản phẩm
Tìm kiếm sản phẩm
Chọn biến thể sản phẩm
Thêm sản phẩm vào hóa đơn
Thay đổi số lượng
Tính tổng tiền
Thanh toán hóa đơn
4. 🔎 Tìm kiếm

Chức năng tìm kiếm sử dụng Data Binding trong MVVM.

Ví dụ:

TextBox
   ↓
TuKhoa
   ↓
ViewModel
   ↓
LoadSanPham()
   ↓
Danh sách sản phẩm

Khi người dùng nhập từ khóa, dữ liệu được cập nhật thông qua Binding.

5. 👨‍🍳 Pha chế

Đơn hàng sau khi được thu ngân gửi đi sẽ xuất hiện tại khu vực pha chế.

Nhân viên pha chế có thể theo dõi và xử lý đơn.

Quy trình:

Thu ngân
   ↓
Tạo đơn hàng
   ↓
Gửi pha chế
   ↓
Nhân viên pha chế nhận đơn
   ↓
Đang pha chế
   ↓
Hoàn tất
6. 📦 Quản lý kho

Hệ thống hỗ trợ quản lý nguyên liệu và nhập kho.

Các nghiệp vụ gồm:

Quản lý nguyên liệu
Nhập nguyên liệu
Theo dõi số lượng tồn
Kiểm tra nguyên liệu
Cập nhật tồn kho
7. 👥 Quản lý nhân viên

Cho phép quản lý:

Thông tin nhân viên
Tài khoản
Vai trò
Trạng thái hoạt động
Phân quyền
8. 🧾 Quản lý hóa đơn

Hệ thống lưu lại lịch sử bán hàng và hóa đơn.

Có thể:

Tìm kiếm hóa đơn
Xem chi tiết hóa đơn
Xem thông tin thanh toán
In hóa đơn
Xem lịch sử bán hàng
9. 📊 Báo cáo

Hệ thống sử dụng Crystal Reports để xây dựng báo cáo.

Report có thể hiển thị:

Danh sách hóa đơn
Doanh thu
Tổng số hóa đơn
Doanh thu theo ngày
Tổng doanh thu trong khoảng thời gian

Ví dụ:

BÁO CÁO DOANH THU

Ngày bán: 28/08/2026

Mã hóa đơn    Khách hàng     Tổng tiền
HD00001       Khách lẻ       120.000
HD00002       Nguyễn Văn A   185.000
HD00003       Khách lẻ        95.000

----------------------------------------
Tổng doanh thu ngày: 400.000đ
🗄️ Database

Database sử dụng Microsoft SQL Server.

Các nhóm dữ liệu chính:

Nhân viên
    ↓
Tài khoản / Phân quyền

Khu vực
    ↓
Bàn

Danh mục
    ↓
Sản phẩm
    ↓
Biến thể sản phẩm

Hóa đơn
    ↓
Chi tiết hóa đơn

Nguyên liệu
    ↓
Kho / Nhập kho

Database script được cung cấp trong project để có thể triển khai lại hệ thống.

🚀 Cài đặt và chạy project
1. Clone repository
git clone https://github.com/mghuyn/QuanLyQuanCaFe.git
2. Mở project

Mở file:

QuanLyQuanCaFe.sln bằng Visual Studio.

3. Tạo database

Mở SQL Server Management Studio (SSMS).

Chạy file SQL database được cung cấp trong project.

Sau khi database được tạo thành công, kiểm tra tên database.

4. Cấu hình Connection String

Mở: App.config
Kiểm tra connection string và thay đổi:

Server
Database
Authentication

phù hợp với SQL Server trên máy.

Ví dụ:

<connectionStrings>
    <add name="QuanLyQuanCaPheDbEntities1"
         connectionString="..."
         providerName="System.Data.EntityClient" />
</connectionStrings>
5. Build project

Trong Visual Studio:
Build
→ Rebuild Solution
Sau đó chạy:
F5
📊 Crystal Reports
Project sử dụng SAP Crystal Reports để hiển thị báo cáo.
Các thành phần chính:
.rpt
 ↓
CrystalReportsViewer
 ↓
Report

Viewer được cấu hình để hiển thị:

Toolbar
Status bar
Report preview
In báo cáo
Export báo cáo
🎯 Mục tiêu của project

Project được xây dựng nhằm áp dụng các kiến thức:

Lập trình hướng đối tượng
C# / WPF
MVVM
Data Binding
Entity Framework
SQL Server
CRUD
Phân quyền
Quản lý trạng thái
Xử lý nghiệp vụ bán hàng
Crystal Reports
Thiết kế cơ sở dữ liệu



## GitHub

🔗 [github.com/mghuyn](https://github.com/mghuyn)

📄 License

Project được thực hiện với mục đích học tập và nghiên cứu.
