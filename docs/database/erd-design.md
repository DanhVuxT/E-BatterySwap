# Thiết kế cơ sở dữ liệu – E-BatterySwap

> Tài liệu làm việc trên nhánh `dev`. Thiết kế dùng Microsoft SQL Server. Không coi những nội dung còn mở là ràng buộc đã duyệt.

## Phạm vi và tiến độ

- **Bước 1:** Chốt 14 thực thể.
- **Bước 2:** Rà soát quan hệ giữa các thực thể; các quan hệ cần kiểm chứng được ghi trong [ERD tổng quan](erd-diagram.md).
- **Bước 3:** Thiết kế thuộc tính theo bốn nhóm; **Nhóm 1 (Role, User, Vehicle) đã chốt**.
- **Bước 4:** Chuẩn hóa, rà soát và hoàn thiện ERD.

## Nhóm 1 – Tài khoản và phương tiện (ĐÃ CHỐT)

### Role

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `RoleId` | `BIGINT` | PK |
| `RoleCode` | `VARCHAR(30)` | NOT NULL, UNIQUE |
| `RoleName` | `NVARCHAR(100)` | NOT NULL |
| `Description` | `NVARCHAR(255)` | NULL |
| `IsActive` | `BIT` | NOT NULL |

Các mã vai trò đã thống nhất: `ADMIN`, `STAFF`, `CUSTOMER`.

### User

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

Các trạng thái tài khoản được nêu trong thiết kế: `PENDING`, `ACTIVE`, `LOCKED`, `DISABLED`.

### Vehicle

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

**Quyết định đã chốt:** đăng nhập bằng `Email` + mật khẩu; một `User` chỉ gắn một `Role`; `LicensePlate` bắt buộc và duy nhất; không có thuộc tính VIN trong `Vehicle`; mỗi xe sử dụng 1 hoặc 2 viên pin.

## Nhóm 2 – Pin và trạm

`Battery`, `Station`, `Slot`, `BatteryAssignment`, `BatteryTelemetry`, `ChargingSession`.

*Chưa bổ sung thiết kế thuộc tính.*

## Nhóm 3 – Giao dịch đổi pin

`SwapSession`, `SwapItem`.

*Chưa bổ sung thiết kế thuộc tính.*

## Nhóm 4 – Mô phỏng và giám sát

`StationCommand`, `StationEvent`, `Alert`.

*Chưa bổ sung thiết kế thuộc tính.*

## Nguyên tắc bảo trì

- Chỉ thay đổi phần đã chốt khi có quyết định mới, và ghi rõ lý do.
- Đồng bộ tài liệu này với [ERD tổng quan](erd-diagram.md) khi các mối quan hệ thay đổi.
- Khi triển khai EF Core, đối chiếu thiết kế với entities và migrations; migration là nguồn phản ánh cấu trúc triển khai.
