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

### Battery

Lưu định danh, tình trạng kỹ thuật, thông số mới nhất và vị trí hiện tại của từng viên pin vật lý.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `BatteryId` | `BIGINT` | PK |
| `SerialNumber` | `NVARCHAR(100)` | NOT NULL, UNIQUE |
| `SOC` | `DECIMAL(5,2)` | NULL, CHECK (SOC BETWEEN 0 AND 100) |
| `SOH` | `DECIMAL(5,2)` | NULL, CHECK (SOH BETWEEN 0 AND 100) |
| `Temperature` | `DECIMAL(5,2)` | NULL |
| `CycleCount` | `INT` | NOT NULL, CHECK (CycleCount >= 0) |
| `OperationalStatus` | `VARCHAR(20)` | NOT NULL |
| `LocationType` | `VARCHAR(20)` | NOT NULL |
| `CurrentSlotId` | `BIGINT` | FK → Slot(SlotId), NULL; duy nhất khi khác NULL |
| `LastTelemetryAt` | `DATETIME2(3)` | NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- `OperationalStatus` phản ánh tình trạng kỹ thuật của pin, tách biệt với vị trí vật lý và hoạt động sạc.
- `LocationType` gồm `SLOT`, `VEHICLE`, `STORAGE`, `MAINTENANCE`, `IN_TRANSIT`, `UNKNOWN`.
- `LocationType = SLOT` yêu cầu `CurrentSlotId` khác NULL; với các loại vị trí khác, `CurrentSlotId` phải NULL. Áp dụng CHECK kết hợp hai trường.
- Tạo **filtered unique index** trên `CurrentSlotId` với điều kiện `CurrentSlotId IS NOT NULL`: một ô chỉ chứa tối đa một pin tại một thời điểm.
- Khi pin ở ngoài trạm, vị trí được phân loại bằng `LocationType`; quyền sử dụng pin trên xe được quản lý tại `BatteryAssignment`.
- `SOC`, `SOH`, `Temperature` biểu diễn giá trị đo mới nhất hợp lệ. `LastTelemetryAt` hỗ trợ chống ghi đè bởi dữ liệu đo cũ được gửi đến muộn.

### Station

Quản lý thông tin định danh, vị trí địa lý, mã QR và trạng thái hoạt động/kết nối của từng trạm đổi pin.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `StationId` | `BIGINT` | PK |
| `StationCode` | `VARCHAR(30)` | NOT NULL, UNIQUE |
| `StationName` | `NVARCHAR(150)` | NOT NULL |
| `Address` | `NVARCHAR(255)` | NOT NULL |
| `Latitude` | `DECIMAL(9,6)` | NULL, CHECK (Latitude BETWEEN -90 AND 90) |
| `Longitude` | `DECIMAL(9,6)` | NULL, CHECK (Longitude BETWEEN -180 AND 180) |
| `QRIdentifier` | `VARCHAR(100)` | NOT NULL, UNIQUE |
| `OperationalStatus` | `VARCHAR(20)` | NOT NULL |
| `ConnectionStatus` | `VARCHAR(20)` | NOT NULL |
| `LastSeenAt` | `DATETIME2(3)` | NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- `OperationalStatus` mô tả khả năng phục vụ; `ConnectionStatus` mô tả tình trạng kết nối với Simulator. Hai thông tin được quản lý độc lập.
- `LastSeenAt` hỗ trợ phát hiện trạm mất kết nối khi không còn tín hiệu trong khoảng thời gian cho phép.
- `QRIdentifier` chỉ dùng để nhận diện trạm; thao tác quét QR không đồng nghĩa được phép mở ô tủ. Backend phải kiểm tra quyền trước khi gửi lệnh.

### Slot

Quản lý từng ô chứa của trạm cùng trạng thái cửa, khóa và khả năng vận hành.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `SlotId` | `BIGINT` | PK |
| `StationId` | `BIGINT` | FK → Station(StationId), NOT NULL |
| `SlotNumber` | `SMALLINT` | NOT NULL |
| `DoorStatus` | `VARCHAR(20)` | NOT NULL |
| `LockStatus` | `VARCHAR(20)` | NOT NULL |
| `OperationalStatus` | `VARCHAR(20)` | NOT NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- `UNIQUE(StationId, SlotNumber)`: một trạm không có hai ô cùng số thứ tự.
- `DoorStatus` và `LockStatus` độc lập: cửa đóng không mặc nhiên đồng nghĩa đã khóa.
- Không lưu `BatteryId` trong `Slot`. Pin đang nằm trong ô được xác định qua `Battery.CurrentSlotId`; không duy trì hai chiều dữ liệu vị trí trùng lặp.
- Phiên sạc của ô được quản lý bởi `ChargingSession`, thay vì tạo thêm thuộc tính trùng về lịch sử sạc ở `Slot`.

### BatteryAssignment

Lưu lịch sử cấp và kết thúc quyền sử dụng từng viên pin cho phương tiện, bao gồm bàn giao ban đầu và cấp pin qua trạm.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `AssignmentId` | `BIGINT` | PK |
| `BatteryId` | `BIGINT` | FK → Battery(BatteryId), NOT NULL |
| `VehicleId` | `BIGINT` | FK → Vehicle(VehicleId), NOT NULL |
| `AssignedAt` | `DATETIME2(3)` | NOT NULL |
| `EndedAt` | `DATETIME2(3)` | NULL |
| `AssignmentStatus` | `VARCHAR(30)` | NOT NULL |
| `AssignmentSource` | `VARCHAR(30)` | NOT NULL |
| `RecordedByUserId` | `BIGINT` | FK → User(UserId), NULL |

**Quy tắc dữ liệu:**

- `AssignmentSource` phân biệt cấp ban đầu bởi nhân viên, cấp pin trong phiên đổi và điều chỉnh nghiệp vụ.
- `RecordedByUserId` lưu tài khoản ghi nhận thao tác thủ công; có thể NULL với thao tác tự động.
- `EndedAt` khi có giá trị phải lớn hơn hoặc bằng `AssignedAt`.
- Một pin có tối đa một quyền sử dụng đang hiệu lực. Một xe không được có số quyền sử dụng pin hiệu lực vượt quá `Vehicle.BatteryCapacityCount`; kiểm tra đồng thời với cập nhật giao dịch.
- Phân biệt quyền sử dụng tại `BatteryAssignment` với vị trí vật lý tại `Battery`; không sử dụng hai thông tin này thay thế cho nhau.

### BatteryTelemetry

Lưu lịch sử phép đo SOC, SOH, nhiệt độ của pin, bao gồm thời điểm đo và thời điểm Backend nhận dữ liệu.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `TelemetryId` | `BIGINT` | PK |
| `BatteryId` | `BIGINT` | FK → Battery(BatteryId), NOT NULL |
| `SOC` | `DECIMAL(5,2)` | NOT NULL, CHECK (SOC BETWEEN 0 AND 100) |
| `SOH` | `DECIMAL(5,2)` | NULL, CHECK (SOH BETWEEN 0 AND 100) |
| `Temperature` | `DECIMAL(5,2)` | NULL |
| `MeasuredAt` | `DATETIME2(3)` | NOT NULL |
| `ReceivedAt` | `DATETIME2(3)` | NOT NULL |
| `Source` | `VARCHAR(20)` | NOT NULL |

**Quy tắc dữ liệu:**

- `MeasuredAt` thể hiện thời điểm đo; `ReceivedAt` thể hiện thời điểm Backend tiếp nhận. Dữ liệu đến muộn vẫn được ghi nhận theo thời gian đo thực tế.
- Bản ghi Telemetry mới hợp lệ được sử dụng để cập nhật trạng thái hiện tại trong `Battery`, tránh ghi đè giá trị mới bằng phép đo cũ.
- Không có `ChargingSessionId` trong `BatteryTelemetry`. Khi cần hiển thị dữ liệu đo theo phiên sạc, truy vấn các bản ghi cùng `BatteryId` có `MeasuredAt` nằm trong khoảng thời gian của `ChargingSession`.
- Tạo chỉ mục phù hợp trên `(BatteryId, MeasuredAt)` để truy vấn lịch sử đo và đo theo khoảng thời gian.

### ChargingSession

Lưu từng quá trình sạc của một viên pin tại một ô chứa, bao gồm thời gian, mức SOC đầu/cuối và kết quả sạc.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `ChargingSessionId` | `BIGINT` | PK |
| `BatteryId` | `BIGINT` | FK → Battery(BatteryId), NOT NULL |
| `SlotId` | `BIGINT` | FK → Slot(SlotId), NOT NULL |
| `StartedAt` | `DATETIME2(3)` | NOT NULL |
| `EndedAt` | `DATETIME2(3)` | NULL |
| `StartSOC` | `DECIMAL(5,2)` | NULL, CHECK (StartSOC BETWEEN 0 AND 100) |
| `EndSOC` | `DECIMAL(5,2)` | NULL, CHECK (EndSOC BETWEEN 0 AND 100) |
| `Status` | `VARCHAR(20)` | NOT NULL |
| `StopReason` | `NVARCHAR(255)` | NULL |

**Quy tắc dữ liệu:**

- Các trạng thái phiên sạc: `CHARGING`, `COMPLETED`, `INTERRUPTED`, `FAILED`.
- `EndedAt` khi có giá trị phải lớn hơn hoặc bằng `StartedAt`.
- Chỉ cho phép bắt đầu sạc khi pin đang nằm đúng ô (`Battery.CurrentSlotId = ChargingSession.SlotId`) và đáp ứng điều kiện vận hành.
- Một viên pin và một ô chứa đều chỉ được có tối đa một phiên sạc `CHARGING` tại một thời điểm. Có thể bảo vệ bằng hai filtered unique index riêng trên `BatteryId` và `SlotId` với điều kiện `Status = 'CHARGING'`.
- `ChargingSession` mô tả vòng đời phiên sạc; dữ liệu thông số theo thời gian lưu trong `BatteryTelemetry`.

### Quan hệ và ràng buộc liên nhóm

| Thực thể A | Bội số nghiệp vụ | Thực thể B | Ý nghĩa |
| --- | --- | --- | --- |
| `Station` | 1–N | `Slot` | Một trạm có nhiều ô chứa |
| `Slot` | 0..1 – 0..1 (hiện tại) | `Battery` | Một ô chứa tối đa một pin và pin có thể ở ngoài trạm |
| `Battery` | 1–N (lịch sử) | `BatteryTelemetry` | Một pin có nhiều bản ghi thông số |
| `Battery` | 1–N (lịch sử) | `ChargingSession` | Một pin có nhiều phiên sạc |
| `Slot` | 1–N (lịch sử) | `ChargingSession` | Một ô có nhiều phiên sạc qua thời gian |
| `Battery` | 1–N (lịch sử) | `BatteryAssignment` | Một pin được cấp nhiều lần qua thời gian |
| `Vehicle` | 1–N (lịch sử) | `BatteryAssignment` | Một xe nhận nhiều pin qua thời gian |
| `User` | 1–N (tùy chọn ở bản ghi con) | `BatteryAssignment` | Tài khoản thực hiện ghi nhận bàn giao |

**Tổng số thuộc tính Nhóm 2:** 54 (`Battery`: 11, `Station`: 11, `Slot`: 7, `BatteryAssignment`: 8, `BatteryTelemetry`: 8, `ChargingSession`: 9).

## Nhóm 3 – Giao dịch đổi pin

`SwapSession`, `SwapItem`.

*Chưa bổ sung thiết kế thuộc tính.*

## Nhóm 4 – Mô phỏng và giám sát

`StationCommand`, `StationEvent`, `Alert`.

*Chưa bổ sung thiết kế thuộc tính.*

