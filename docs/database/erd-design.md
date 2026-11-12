# Thiết kế cơ sở dữ liệu – E-BatterySwap

> Đặc tả dữ liệu phục vụ triển khai E-BatterySwap trên Microsoft SQL Server. Mỗi bảng gồm thuộc tính, kiểu dữ liệu và các ràng buộc tương ứng.

## Phạm vi đặc tả

Mô hình tổng quan gồm 14 thực thể; xem [ERD tổng quan](erd-diagram.md). Tài liệu này trình bày đặc tả chi tiết của các bảng theo nhóm nghiệp vụ. Cả 4 nhóm hiện có đặc tả chi tiết cho 14 thực thể. Script DDL SQL Server tương ứng nằm tại [schema.sql](schema.sql).

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

### Users

Lưu tài khoản người dùng, thông tin xác thực và vai trò được gán. Mỗi tài khoản tham chiếu đến một vai trò thông qua `RoleId`.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `UserId` | `BIGINT` | PK |
| `RoleId` | `BIGINT` | FK → Role(RoleId), NOT NULL |
| `FullName` | `NVARCHAR(150)` | NOT NULL |
| `PhoneNumber` | `VARCHAR(20)` | NULL |
| `Email` | `NVARCHAR(254)` | NOT NULL, UNIQUE |
| `PasswordHash` | `NVARCHAR(512)` | NOT NULL |
| `Status` | `VARCHAR(20)` | NOT NULL, CHECK (Status IN ('PENDING', 'ACTIVE', 'LOCKED', 'DISABLED')) |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |
| `UpdatedAt` | `DATETIME2(3)` | NULL |

Các trạng thái tài khoản: `PENDING`, `ACTIVE`, `LOCKED`, `DISABLED`; được bảo vệ bằng CHECK.

### Vehicle

Lưu phương tiện đăng ký sử dụng hệ thống và liên kết với tài khoản quản lý. Biển số là định danh bắt buộc, duy nhất trong hệ thống.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `VehicleId` | `BIGINT` | PK |
| `UserId` | `BIGINT` | FK → Users(UserId), NOT NULL |
| `LicensePlate` | `NVARCHAR(20)` | NOT NULL, UNIQUE |
| `ModelName` | `NVARCHAR(100)` | NOT NULL |
| `BatteryCapacityCount` | `TINYINT` | NOT NULL, CHECK (BatteryCapacityCount IN (1, 2)) |
| `Status` | `VARCHAR(20)` | NOT NULL, CHECK (Status IN ('ACTIVE', 'INACTIVE')) |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |
| `UpdatedAt` | `DATETIME2(3)` | NULL |

**Quy tắc dữ liệu:** Người dùng đăng nhập bằng email và mật khẩu; mỗi tài khoản trong `Users` được gán một `Role`; `Vehicle.LicensePlate` là bắt buộc và duy nhất; `BatteryCapacityCount` chỉ nhận giá trị 1 hoặc 2.

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
| `OperationalStatus` | `VARCHAR(20)` | NOT NULL, CHECK (OperationalStatus IN ('NORMAL', 'WARNING', 'FAULT', 'MAINTENANCE', 'RETIRED')) |
| `LocationType` | `VARCHAR(20)` | NOT NULL, CHECK (LocationType IN ('SLOT', 'VEHICLE', 'STORAGE', 'MAINTENANCE', 'IN_TRANSIT', 'UNKNOWN')) |
| `CurrentSlotId` | `BIGINT` | FK → Slot(SlotId), NULL; duy nhất khi khác NULL |
| `LastTelemetryAt` | `DATETIME2(3)` | NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- `OperationalStatus` nhận `NORMAL`, `WARNING`, `FAULT`, `MAINTENANCE`, `RETIRED`; phản ánh tình trạng kỹ thuật của pin, tách biệt với vị trí vật lý và hoạt động sạc.
- `LocationType` nhận `SLOT`, `VEHICLE`, `STORAGE`, `MAINTENANCE`, `IN_TRANSIT`, `UNKNOWN`.
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
| `OperationalStatus` | `VARCHAR(20)` | NOT NULL, CHECK (OperationalStatus IN ('ACTIVE', 'INACTIVE', 'MAINTENANCE')) |
| `ConnectionStatus` | `VARCHAR(20)` | NOT NULL, CHECK (ConnectionStatus IN ('ONLINE', 'OFFLINE')) |
| `LastSeenAt` | `DATETIME2(3)` | NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- `OperationalStatus` nhận `ACTIVE`, `INACTIVE`, `MAINTENANCE`; `ConnectionStatus` nhận `ONLINE`, `OFFLINE`. Hai thông tin được quản lý độc lập.
- `LastSeenAt` hỗ trợ phát hiện trạm mất kết nối khi không còn tín hiệu trong khoảng thời gian cho phép.
- `QRIdentifier` chỉ dùng để nhận diện trạm; thao tác quét QR không đồng nghĩa được phép mở ô tủ. Backend phải kiểm tra quyền trước khi gửi lệnh.

### Slot

Quản lý từng ô chứa của trạm cùng trạng thái cửa, khóa và khả năng vận hành.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `SlotId` | `BIGINT` | PK |
| `StationId` | `BIGINT` | FK → Station(StationId), NOT NULL |
| `SlotNumber` | `SMALLINT` | NOT NULL |
| `DoorStatus` | `VARCHAR(20)` | NOT NULL, CHECK (DoorStatus IN ('OPEN', 'CLOSED', 'UNKNOWN')) |
| `LockStatus` | `VARCHAR(20)` | NOT NULL, CHECK (LockStatus IN ('LOCKED', 'UNLOCKED', 'UNKNOWN')) |
| `OperationalStatus` | `VARCHAR(20)` | NOT NULL, CHECK (OperationalStatus IN ('AVAILABLE', 'DISABLED', 'FAULT', 'MAINTENANCE')) |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- Tạo UNIQUE constraint trên (`StationId`, `SlotNumber`): một trạm không có hai ô cùng số thứ tự.
- `DoorStatus` nhận `OPEN`, `CLOSED`, `UNKNOWN`; `LockStatus` nhận `LOCKED`, `UNLOCKED`, `UNKNOWN`; `OperationalStatus` nhận `AVAILABLE`, `DISABLED`, `FAULT`, `MAINTENANCE`. Ba trạng thái phản ánh các khía cạnh riêng; cửa đóng không mặc nhiên đồng nghĩa đã khóa.
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
| `AssignmentSource` | `VARCHAR(30)` | NOT NULL, CHECK (AssignmentSource IN ('INITIAL_HANDOVER', 'SWAP', 'MANUAL_ADJUSTMENT')) |
| `RecordedByUserId` | `BIGINT` | FK → Users(UserId), NULL |

**Quy tắc dữ liệu:**

- `AssignmentSource` nhận `INITIAL_HANDOVER`, `SWAP`, `MANUAL_ADJUSTMENT`, phân biệt cấp ban đầu bởi nhân viên, cấp pin trong phiên đổi và điều chỉnh nghiệp vụ.
- `RecordedByUserId` lưu tài khoản ghi nhận thao tác thủ công; có thể NULL với thao tác tự động.
- `EndedAt` là nguồn dữ liệu xác định hiệu lực: NULL nghĩa là quyền sử dụng đang hiệu lực; khác NULL nghĩa là đã kết thúc. Không lưu `AssignmentStatus` để tránh hai nguồn sự thật.
- `EndedAt` khi có giá trị phải lớn hơn hoặc bằng `AssignedAt`; áp dụng CHECK (`EndedAt IS NULL OR EndedAt >= AssignedAt`).
- Tạo filtered unique index trên `BatteryId` với điều kiện `EndedAt IS NULL`: một pin chỉ có tối đa một quyền sử dụng đang hiệu lực. Một xe không được có số quyền sử dụng pin hiệu lực vượt quá `Vehicle.BatteryCapacityCount`; Backend kiểm tra trong transaction.
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
| `Source` | `VARCHAR(20)` | NOT NULL, CHECK (Source IN ('SIMULATOR', 'STATION', 'MANUAL')) |

**Quy tắc dữ liệu:**

- `Source` nhận `SIMULATOR`, `STATION`, `MANUAL` để xác định nguồn dữ liệu.
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
| `Status` | `VARCHAR(20)` | NOT NULL, CHECK (Status IN ('CHARGING', 'COMPLETED', 'INTERRUPTED', 'FAILED')) |
| `StopReason` | `NVARCHAR(255)` | NULL |

**Quy tắc dữ liệu:**

- Các trạng thái phiên sạc: `CHARGING`, `COMPLETED`, `INTERRUPTED`, `FAILED`; miền giá trị được bảo vệ bằng CHECK.
- CHECK trạng thái/thời gian: `CHARGING` yêu cầu `EndedAt IS NULL`; `COMPLETED`, `INTERRUPTED`, `FAILED` yêu cầu `EndedAt IS NOT NULL`.
- `EndedAt` khi có giá trị phải lớn hơn hoặc bằng `StartedAt`; áp dụng CHECK (`EndedAt IS NULL OR EndedAt >= StartedAt`).
- Tạo hai filtered unique index riêng trên `BatteryId` và `SlotId`, mỗi index có điều kiện `Status = 'CHARGING'`.
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
| `Users` | 1–N (tùy chọn ở bản ghi con) | `BatteryAssignment` | Tài khoản thực hiện ghi nhận bàn giao |

**Tổng số thuộc tính Nhóm 2:** 53 (`Battery`: 11, `Station`: 11, `Slot`: 7, `BatteryAssignment`: 7, `BatteryTelemetry`: 8, `ChargingSession`: 9).

## Nhóm 3 – Giao dịch đổi pin

### SwapSession

Lưu thông tin tổng thể của một phiên đổi pin, gắn với phương tiện và trạm thực hiện. Một phiên có thể gồm một hoặc hai lượt đổi pin tùy sức chứa pin của phương tiện.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `SwapSessionId` | `BIGINT` | PK |
| `VehicleId` | `BIGINT` | FK → Vehicle(VehicleId), NOT NULL |
| `StationId` | `BIGINT` | FK → Station(StationId), NOT NULL |
| `RequestedBatteryCount` | `TINYINT` | NOT NULL, CHECK (RequestedBatteryCount IN (1, 2)) |
| `Status` | `VARCHAR(30)` | NOT NULL, CHECK (Status IN ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'PARTIALLY_COMPLETED', 'FAILED', 'CANCELLED')) |
| `StartedAt` | `DATETIME2(3)` | NOT NULL |
| `EndedAt` | `DATETIME2(3)` | NULL |
| `FailureReason` | `NVARCHAR(255)` | NULL |

### SwapItem

Lưu chi tiết từng lượt trả pin và nhận pin trong một phiên đổi. Mỗi bản ghi tương ứng với một viên pin được trả và/hoặc một viên pin được cấp; hỗ trợ trường hợp phiên đổi chỉ hoàn thành một phần.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `SwapItemId` | `BIGINT` | PK |
| `SwapSessionId` | `BIGINT` | FK → SwapSession(SwapSessionId), NOT NULL |
| `SequenceNumber` | `TINYINT` | NOT NULL, CHECK (SequenceNumber IN (1, 2)) |
| `ReturnedBatteryId` | `BIGINT` | FK → Battery(BatteryId), NULL |
| `IssuedBatteryId` | `BIGINT` | FK → Battery(BatteryId), NULL |
| `ReturnSlotId` | `BIGINT` | FK → Slot(SlotId), NULL |
| `IssueSlotId` | `BIGINT` | FK → Slot(SlotId), NULL |
| `Status` | `VARCHAR(30)` | NOT NULL, CHECK (Status IN ('PENDING', 'RETURN_CONFIRMED', 'ISSUE_CONFIRMED', 'COMPLETED', 'FAILED', 'CANCELLED')) |
| `ReturnedAt` | `DATETIME2(3)` | NULL |
| `IssuedAt` | `DATETIME2(3)` | NULL |
| `FailureReason` | `NVARCHAR(255)` | NULL |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |

**Quy tắc dữ liệu:**

- `RequestedBatteryCount` chỉ nhận 1 hoặc 2. Tạo UNIQUE constraint trên (`SwapSessionId`, `SequenceNumber`) để bảo đảm thứ tự lượt đổi không bị trùng. Backend kiểm tra số `SwapItem` không vượt `RequestedBatteryCount` trong transaction.
- `ReturnedBatteryId` và `IssuedBatteryId` ghi nhận pin thực tế được trạm xác nhận, không phải pin dự kiến. Hai trường có thể NULL khi thao tác tương ứng chưa hoàn thành.
- Khi thao tác trả pin được xác nhận, `ReturnedBatteryId`, `ReturnSlotId` và `ReturnedAt` được ghi nhận. Khi thao tác cấp pin được xác nhận, `IssuedBatteryId`, `IssueSlotId` và `IssuedAt` được ghi nhận.
- `ReturnSlotId` và `IssueSlotId` lưu vị trí ô chứa thực tế tại thời điểm giao dịch; hai ô phải thuộc trạm được tham chiếu bởi `SwapSession.StationId`. Backend kiểm tra tính nhất quán này khi xử lý giao dịch.
- Một pin không thể đồng thời là pin trả và pin cấp trong cùng một `SwapItem`; áp dụng CHECK (`ReturnedBatteryId IS NULL OR IssuedBatteryId IS NULL OR ReturnedBatteryId <> IssuedBatteryId`).
- Áp dụng CHECK để bộ ba (`ReturnedBatteryId`, `ReturnSlotId`, `ReturnedAt`) cùng NULL hoặc cùng khác NULL: `((ReturnedBatteryId IS NULL AND ReturnSlotId IS NULL AND ReturnedAt IS NULL) OR (ReturnedBatteryId IS NOT NULL AND ReturnSlotId IS NOT NULL AND ReturnedAt IS NOT NULL))`. Áp dụng CHECK tương tự cho (`IssuedBatteryId`, `IssueSlotId`, `IssuedAt`).
- `EndedAt` khi có giá trị phải lớn hơn hoặc bằng `StartedAt`; áp dụng CHECK (`EndedAt IS NULL OR EndedAt >= StartedAt`).
- Mỗi `VehicleId` chỉ có một phiên đang mở. Tạo filtered unique index trên `VehicleId` với điều kiện `EndedAt IS NULL`.
- Áp dụng CHECK: `((Status IN ('PENDING', 'IN_PROGRESS') AND EndedAt IS NULL) OR (Status IN ('COMPLETED', 'PARTIALLY_COMPLETED', 'FAILED', 'CANCELLED') AND EndedAt IS NOT NULL))`.
- Backend kiểm tra `RequestedBatteryCount <= Vehicle.BatteryCapacityCount` trong transaction.
- Khi xác nhận pin trả, Backend phải kiểm tra pin đang có `BatteryAssignment` hiệu lực thuộc đúng `VehicleId` của `SwapSession`; thao tác kết thúc quyền cũ và ghi nhận cấp pin mới phải nằm trong cùng transaction.
- `FailureReason` ở `SwapSession` mô tả lỗi hoặc nguyên nhân thất bại ở cấp toàn phiên; `FailureReason` ở `SwapItem` mô tả lỗi của từng lượt đổi. Chi tiết sự kiện kỹ thuật được lưu tại `StationEvent`.
- `SwapSession.Status` nhận `PENDING`, `IN_PROGRESS`, `COMPLETED`, `PARTIALLY_COMPLETED`, `FAILED`, `CANCELLED`. `SwapItem.Status` nhận `PENDING`, `RETURN_CONFIRMED`, `ISSUE_CONFIRMED`, `COMPLETED`, `FAILED`, `CANCELLED`; miền giá trị được bảo vệ bằng CHECK.
- `Status` của phiên và từng lượt đổi được quản lý theo luồng trạng thái nghiệp vụ tại Backend. Trạng thái phiên phải phản ánh kết quả tổng thể, bao gồm trường hợp hoàn thành một phần.
- Khi thao tác đổi pin được xác nhận, Backend cập nhật `BatteryAssignment` để phản ánh quyền sử dụng pin của phương tiện và cập nhật vị trí hiện tại trong `Battery`. `SwapItem` lưu lịch sử giao dịch, không thay thế hai nguồn dữ liệu này.
- Không lưu `InitiatedByUserId` trong `SwapSession`; tài khoản quản lý phương tiện có thể được xác định qua `Vehicle.UserId`. Bảng không lưu người thao tác cụ thể tại thời điểm bắt đầu phiên.

### Quan hệ

| Thực thể A | Bội số nghiệp vụ | Thực thể B | Ý nghĩa |
| --- | --- | --- | --- |
| `Vehicle` | 1–N | `SwapSession` | Một phương tiện có nhiều phiên đổi pin theo thời gian |
| `Station` | 1–N | `SwapSession` | Một trạm thực hiện nhiều phiên đổi pin |
| `SwapSession` | 1–N (tối đa 2) | `SwapItem` | Một phiên gồm một hoặc hai lượt đổi pin |
| `Battery` | 1–N (vai trò pin trả) | `SwapItem` | Một pin có thể được trả trong nhiều giao dịch theo thời gian |
| `Battery` | 1–N (vai trò pin cấp) | `SwapItem` | Một pin có thể được cấp trong nhiều giao dịch theo thời gian |
| `Slot` | 1–N (vai trò ô nhận pin trả) | `SwapItem` | Một ô có thể tiếp nhận pin trả trong nhiều giao dịch |
| `Slot` | 1–N (vai trò ô cấp pin) | `SwapItem` | Một ô có thể cấp pin trong nhiều giao dịch |

**Tổng số thuộc tính Nhóm 3:** 20 (`SwapSession`: 8, `SwapItem`: 12).

## Nhóm 4 – Mô phỏng và giám sát

### StationCommand

Lưu các lệnh Backend gửi đến trạm hoặc Simulator và theo dõi trạng thái thực hiện của từng lệnh.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `CommandId` | `BIGINT` | PK |
| `StationId` | `BIGINT` | FK → Station(StationId), NOT NULL |
| `SlotId` | `BIGINT` | FK → Slot(SlotId), NULL |
| `SwapItemId` | `BIGINT` | FK → SwapItem(SwapItemId), NULL |
| `ChargingSessionId` | `BIGINT` | FK → ChargingSession(ChargingSessionId), NULL |
| `CommandType` | `VARCHAR(30)` | NOT NULL |
| `Payload` | `NVARCHAR(MAX)` | NULL |
| `Status` | `VARCHAR(20)` | NOT NULL, CHECK (Status IN ('PENDING', 'SENT', 'ACKNOWLEDGED', 'SUCCEEDED', 'FAILED', 'TIMEOUT', 'CANCELLED')) |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |
| `SentAt` | `DATETIME2(3)` | NULL |
| `CompletedAt` | `DATETIME2(3)` | NULL |
| `RetryCount` | `TINYINT` | NOT NULL, DEFAULT (0) |

**Quy tắc dữ liệu:**

- `CommandType` xác định loại thao tác, ví dụ: `UNLOCK_DOOR`, `LOCK_DOOR`, `START_CHARGING`, `STOP_CHARGING`.
- `Payload` lưu tham số bổ sung của lệnh theo định dạng JSON; không dùng thay thế các khóa ngoại cần truy vấn trực tiếp.
- `SlotId` có thể NULL với lệnh áp dụng cho toàn trạm. Khi khác NULL, ô phải thuộc `StationId` của lệnh.
- `SwapItemId` liên kết lệnh với lượt đổi pin; `ChargingSessionId` liên kết lệnh với phiên sạc. Hai trường có thể NULL nếu lệnh không thuộc các luồng nghiệp vụ này.
- Các trạng thái lệnh: `PENDING`, `SENT`, `ACKNOWLEDGED`, `SUCCEEDED`, `FAILED`, `TIMEOUT`, `CANCELLED`.
- `RetryCount` không âm; áp dụng CHECK (`RetryCount >= 0`). `SentAt` và `CompletedAt` chỉ được ghi khi lệnh đạt đến các mốc tương ứng. Áp dụng CHECK bảo đảm `SentAt >= CreatedAt`, `CompletedAt >= CreatedAt` và nếu cả hai có giá trị thì `CompletedAt >= SentAt`.
- Backend quản lý timeout, gửi lại lệnh và chống thực thi lặp; Simulator cần nhận diện lệnh bằng `CommandId` để xử lý idempotent.

### StationEvent

Lưu các sự kiện thực tế do trạm hoặc Simulator gửi về Backend, bao gồm phản hồi lệnh và sự kiện phát sinh độc lập.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `EventId` | `BIGINT` | PK |
| `StationId` | `BIGINT` | FK → Station(StationId), NOT NULL |
| `CommandId` | `BIGINT` | FK → StationCommand(CommandId), NULL |
| `SlotId` | `BIGINT` | FK → Slot(SlotId), NULL |
| `BatteryId` | `BIGINT` | FK → Battery(BatteryId), NULL |
| `EventIdentifier` | `VARCHAR(100)` | NOT NULL |
| `EventType` | `VARCHAR(40)` | NOT NULL |
| `Payload` | `NVARCHAR(MAX)` | NULL |
| `OccurredAt` | `DATETIME2(3)` | NOT NULL |
| `ReceivedAt` | `DATETIME2(3)` | NOT NULL |
| `ProcessingStatus` | `VARCHAR(20)` | NOT NULL, CHECK (ProcessingStatus IN ('PENDING', 'PROCESSED', 'FAILED')) |
| `ProcessedAt` | `DATETIME2(3)` | NULL |
| `ProcessingError` | `NVARCHAR(255)` | NULL |

**Quy tắc dữ liệu:**

- `EventType` mô tả loại sự kiện, ví dụ: `COMMAND_ACCEPTED`, `COMMAND_SUCCEEDED`, `COMMAND_FAILED`, `DOOR_OPENED`, `DOOR_CLOSED`, `BATTERY_DETECTED`, `CHARGING_STARTED`, `CHARGING_STOPPED`, `STATION_OFFLINE`, `FAULT_DETECTED`.
- `CommandId` có thể NULL nếu sự kiện không phải phản hồi của một lệnh cụ thể. Một lệnh có thể phát sinh nhiều sự kiện trong quá trình tiếp nhận và thực hiện.
- `SlotId` có thể NULL với sự kiện ở cấp trạm. Khi khác NULL, ô phải thuộc `StationId` của sự kiện.
- `BatteryId` có thể NULL vì không phải sự kiện nào cũng liên quan đến pin. Khi xác định được pin liên quan, lưu khóa ngoại trực tiếp để truy vấn lịch sử sự kiện theo pin.
- `Payload` lưu dữ liệu chi tiết bổ sung theo định dạng JSON; các định danh dùng để liên kết và truy vấn vẫn được lưu ở các cột riêng.
- `EventIdentifier` là mã sự kiện do Simulator cung cấp. Tạo UNIQUE constraint trên (`StationId`, `EventIdentifier`) để chống ghi nhận trùng sự kiện từ cùng một trạm.
- `OccurredAt` là thời điểm sự kiện xảy ra tại trạm; `ReceivedAt` là thời điểm Backend nhận được sự kiện. Hai thời điểm có thể khác nhau do độ trễ hoặc mất kết nối.
- `ProcessingStatus` nhận `PENDING`, `PROCESSED`, `FAILED`. Áp dụng CHECK: `((ProcessingStatus = 'PENDING' AND ProcessedAt IS NULL) OR (ProcessingStatus IN ('PROCESSED', 'FAILED') AND ProcessedAt IS NOT NULL))`. `ProcessingError` ghi nhận lỗi xử lý khi có.
- Backend cần xử lý sự kiện theo hướng idempotent để sự kiện gửi lại không làm lặp cập nhật trạng thái pin, ô hoặc phiên nghiệp vụ.

### Alert

Lưu các cảnh báo do hệ thống tạo ra khi phát hiện lỗi hoặc tình trạng bất thường tại trạm, phục vụ Admin/Staff theo dõi và xử lý.

| Thuộc tính | Kiểu SQL Server | Ràng buộc |
| --- | --- | --- |
| `AlertId` | `BIGINT` | PK |
| `StationId` | `BIGINT` | FK → Station(StationId), NOT NULL |
| `EventId` | `BIGINT` | FK → StationEvent(EventId), NULL |
| `AlertType` | `VARCHAR(40)` | NOT NULL |
| `Severity` | `VARCHAR(20)` | NOT NULL, CHECK (Severity IN ('INFO', 'WARNING', 'ERROR', 'CRITICAL')) |
| `Title` | `NVARCHAR(150)` | NOT NULL |
| `Description` | `NVARCHAR(500)` | NULL |
| `Status` | `VARCHAR(20)` | NOT NULL, CHECK (Status IN ('OPEN', 'ACKNOWLEDGED', 'RESOLVED')) |
| `CreatedAt` | `DATETIME2(3)` | NOT NULL |
| `AcknowledgedAt` | `DATETIME2(3)` | NULL |
| `AcknowledgedByUserId` | `BIGINT` | FK → Users(UserId), NULL |
| `ResolvedAt` | `DATETIME2(3)` | NULL |
| `ResolvedByUserId` | `BIGINT` | FK → Users(UserId), NULL |

**Quy tắc dữ liệu:**

- `AlertType` xác định nhóm cảnh báo, ví dụ: `STATION_OFFLINE`, `COMMAND_TIMEOUT`, `DOOR_FAULT`, `LOCK_FAULT`, `CHARGING_FAULT`, `OVER_TEMPERATURE`.
- `Severity` nhận một trong các mức `INFO`, `WARNING`, `ERROR`, `CRITICAL`.
- `Status` nhận `OPEN`, `ACKNOWLEDGED`, `RESOLVED`. Cảnh báo mới tạo có trạng thái `OPEN`; nhân viên tiếp nhận chuyển sang `ACKNOWLEDGED`; sau khi xử lý xong chuyển sang `RESOLVED`.
- `EventId` có thể NULL nếu cảnh báo được Backend phát hiện độc lập, ví dụ lệnh timeout hoặc trạm không gửi heartbeat trong thời gian quy định.
- `AcknowledgedAt` và `AcknowledgedByUserId` ghi nhận thời điểm và tài khoản tiếp nhận cảnh báo. `ResolvedAt` và `ResolvedByUserId` ghi nhận thời điểm và tài khoản xác nhận xử lý xong.
- Các trường thời gian và người xử lý phải nhất quán với trạng thái cảnh báo; áp dụng CHECK theo ba trường hợp: `OPEN` thì `AcknowledgedAt`, `AcknowledgedByUserId`, `ResolvedAt`, `ResolvedByUserId` đều NULL; `ACKNOWLEDGED` thì cặp tiếp nhận khác NULL và cặp xử lý NULL; `RESOLVED` thì cả hai cặp đều khác NULL. Thời điểm tiếp nhận và xử lý không được nhỏ hơn `CreatedAt`, thời điểm xử lý không được nhỏ hơn thời điểm tiếp nhận.
- Chi tiết sự kiện kỹ thuật được lưu trong `StationEvent`; `Alert` lưu thông tin cô đọng để hiển thị và theo dõi xử lý.

### Quan hệ

| Thực thể A | Bội số nghiệp vụ | Thực thể B | Ý nghĩa |
| --- | --- | --- | --- |
| `Station` | 1–N | `StationCommand` | Một trạm nhận nhiều lệnh điều khiển |
| `Slot` | 0..1 – 0..N | `StationCommand` | Một lệnh có thể nhắm đến một ô; một ô nhận nhiều lệnh theo thời gian |
| `SwapItem` | 0..1 – 0..N | `StationCommand` | Một lệnh có thể gắn với một lượt đổi pin; một lượt đổi có thể phát sinh nhiều lệnh |
| `ChargingSession` | 0..1 – 0..N | `StationCommand` | Một lệnh có thể gắn với một phiên sạc; một phiên sạc có thể phát sinh nhiều lệnh |
| `Station` | 1–N | `StationEvent` | Một trạm phát sinh nhiều sự kiện |
| `StationCommand` | 0..1 – 0..N | `StationEvent` | Một sự kiện phản hồi tối đa một lệnh; một lệnh có thể phát sinh nhiều sự kiện |
| `Slot` | 0..1 – 0..N | `StationEvent` | Sự kiện có thể gắn với một ô; một ô phát sinh nhiều sự kiện |
| `Battery` | 0..1 – 0..N | `StationEvent` | Sự kiện có thể gắn với một pin; một pin có nhiều sự kiện |
| `Station` | 1–N | `Alert` | Một trạm có nhiều cảnh báo |
| `StationEvent` | 0..1 – 0..N | `Alert` | Một cảnh báo có thể bắt nguồn từ một sự kiện; một sự kiện có thể tạo nhiều cảnh báo |
| `Users` | 0..1 – 0..N (vai trò tiếp nhận) | `Alert` | Một tài khoản có thể tiếp nhận nhiều cảnh báo |
| `Users` | 0..1 – 0..N (vai trò xử lý) | `Alert` | Một tài khoản có thể xử lý nhiều cảnh báo |

**Tổng số thuộc tính Nhóm 4:** 38 (`StationCommand`: 12, `StationEvent`: 13, `Alert`: 13).

