# Thiết kế cơ sở dữ liệu – E-BatterySwap

> Tài liệu làm việc trên nhánh `dev`. Thiết kế dùng Microsoft SQL Server. Không coi những nội dung còn mở là ràng buộc đã duyệt.

## Phạm vi và tiến độ

- **Bước 1:** Chốt 14 thực thể.
- **Bước 2:** Rà soát quan hệ giữa các thực thể; các quan hệ cần kiểm chứng được ghi trong [ERD tổng quan](erd-diagram.md).
- **Bước 3:** Thiết kế thuộc tính theo bốn nhóm; **Nhóm 1 (Role, User, Vehicle) đã chốt các quyết định nghiệp vụ**. Những chi tiết chưa có bản đối chiếu đầy đủ được đánh dấu cần xác nhận thay vì tự suy diễn.
- **Bước 4:** Chuẩn hóa, rà soát và hoàn thiện ERD.

## Nhóm 1 – Tài khoản và phương tiện

### Role

| Thuộc tính | Vai trò | Ghi chú |
| --- | --- | --- |
| `RoleId` | Khóa chính (PK) | |
| `RoleCode` | Mã vai trò | |
| `RoleName` | Tên vai trò | |
| `Description` | Mô tả | |
| `IsActive` | Trạng thái hoạt động | |

### User

| Thuộc tính | Vai trò | Ghi chú |
| --- | --- | --- |
| `UserId` | Khóa chính (PK) | |
| `RoleId` | Khóa ngoại (FK) → Role | Một tài khoản chỉ có **một vai trò** |
| `FullName` | Họ tên | |
| `PhoneNumber` | Số điện thoại | |
| `Email` | Email đăng nhập | Cần bảo đảm tính duy nhất của định danh đăng nhập |
| `PasswordHash` | Mật khẩu đã băm | Không lưu mật khẩu dạng rõ |
| `Status` | Trạng thái tài khoản | |
| `CreatedAt` | Ngày tạo | |
| `UpdatedAt` | Ngày cập nhật | |

### Vehicle

| Thuộc tính | Vai trò | Ghi chú |
| --- | --- | --- |
| `VehicleId` | Khóa chính (PK) | |
| `UserId` | Khóa ngoại (FK) → User | |
| `LicensePlate` | Biển số | **Bắt buộc nhập** |
| `ModelName` | Tên dòng xe | |
| `BatteryCapacityCount` | Số pin sử dụng | |
| `Status` | Trạng thái xe | |
| `CreatedAt` | Ngày tạo | |
| `UpdatedAt` | Ngày cập nhật | |

**Quyết định đã xác nhận:** đăng nhập bằng `Email`; `Vehicle.LicensePlate` bắt buộc; **không lưu VIN** trong bảng `Vehicle`; mỗi `User` chỉ gắn một `Role`.

**Cần đối chiếu bản chốt chi tiết:** kiểu dữ liệu SQL Server, độ dài chuỗi, NULL/NOT NULL, UNIQUE và CHECK của từng cột. Các danh sách thuộc tính ở trên kế thừa bản tổng hợp Nhóm 1; không tự tuyên bố các kiểu dữ liệu và ràng buộc chưa được đối chiếu là đã duyệt.

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
