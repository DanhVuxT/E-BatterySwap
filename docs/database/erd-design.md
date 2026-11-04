# Thiết kế cơ sở dữ liệu – E-BatterySwap

> Đặc tả dữ liệu phục vụ triển khai E-BatterySwap trên Microsoft SQL Server. Mỗi bảng gồm thuộc tính, kiểu dữ liệu và các ràng buộc tương ứng.

## Phạm vi đặc tả

Mô hình tổng quan gồm 14 thực thể; xem [ERD tổng quan](erd-diagram.md). Tài liệu này trình bày đặc tả chi tiết của các bảng theo nhóm nghiệp vụ. Hiện có đặc tả cho `Role`, `User` và `Vehicle`.

## Nhóm 1 – Tài khoản và phương tiện

### Role

Lưu danh mục vai trò dùng trong phân quyền tài khoản.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `RoleId` | `BIGINT` | PK |
| `RoleCode` | `VARCHAR(30)` | NOT NULL, UNIQUE |
| `RoleName` | `NVARCHAR(100)` | NOT NULL |
| `Description` | `NVARCHAR(255)` | NULL |
| `IsActive` | `BIT` | NOT NULL |

Các mã vai trò: `ADMIN`, `STAFF`, `CUSTOMER`.

### User

Lưu tài khoản người dùng, thông tin xác thực và vai trò được gán. Mỗi tài khoản tham chiếu đến một vai trò thông qua `RoleId`.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `UserId` | `BIGINT` | PK |
| `RoleId` | `BIGINT` | FK → Role(RoleId), NOT NULL |
| `FullName` | `NVARCHAR(150)` | NOT NULL |
| `PhoneNumber` | `VARCHAR(20)` | NULL |
| `Email` | `NVARCHAR(254)` | NOT NULL, UNIQUE |
| `PasswordHash` | `NVARCHAR(512)` | NOT NULL |
| `Status` | `VARCHAR(20)` | NOT NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |
| `UpdatedAt` | `DATETIME2(3)` | NULL |

Các trạng thái tài khoản: `PENDING`, `ACTIVE`, `LOCKED`, `DISABLED`.

### Vehicle

Lưu phương tiện đăng ký sử dụng hệ thống và liên kết với tài khoản quản lý. Biển số là định danh bắt buộc, duy nhất trong hệ thống.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `VehicleId` | `BIGINT` | PK |
| `UserId` | `BIGINT` | FK → User(UserId), NOT NULL |
| `LicensePlate` | `NVARCHAR(20)` | NOT NULL, UNIQUE |
| `ModelName` | `NVARCHAR(100)` | NOT NULL |
| `BatteryCapacityCount` | `TINYINT` | NOT NULL, CHECK (BatteryCapacityCount IN (1, 2)) |
| `Status` | `VARCHAR(20)` | NOT NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |
| `UpdatedAt` | `DATETIME2(3)` | NULL |

**Quy tắc dữ liệu:** Người dùng đăng nhập bằng email và mật khẩu; mỗi `User` được gán một `Role`; `Vehicle.LicensePlate` là bắt buộc và duy nhất; `BatteryCapacityCount` chỉ nhận giá trị 1 hoặc 2.

## Nhóm 2 – Pin và trạm

`Battery`, `Station`, `Slot`, `BatteryAssignment`, `BatteryTelemetry`, `ChargingSession`.

*Chưa bổ sung thiết kế thuộc tính.*

## Nhóm 3 – Giao dịch đổi pin

`SwapSession`, `SwapItem`.

*Chưa bổ sung thiết kế thuộc tính.*

## Nhóm 4 – Mô phỏng và giám sát

`StationCommand`, `StationEvent`, `Alert`.

*Chưa bổ sung thiết kế thuộc tính.*

