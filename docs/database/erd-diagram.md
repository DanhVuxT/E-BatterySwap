# ERD tổng quan – E-BatterySwap

> Tài liệu thiết kế CSDL. Nội dung được chuyển từ mục 5 của README trên nhánh `dev` nhằm giữ nguyên lịch sử đề xuất; những nhãn trạng thái trong bản gốc có thể chưa phản ánh các quyết định mới nhất.

## Các thực thể và quan hệ tổng quan

Danh sách **14 thực thể** đã được chốt ở Bước 1. Các mối quan hệ ở sơ đồ tổng quan bên dưới đang được đề xuất tại Bước 2; chưa chốt thuộc tính, khóa hoặc cấu trúc bảng SQL cụ thể.

| Nhóm nghiệp vụ | Thực thể | Trách nhiệm chính |
| --- | --- | --- |
| Người dùng và phương tiện | **User** | Tài khoản người dùng |
| | **Role** | Vai trò và phân quyền |
| | **Vehicle** | Phương tiện sử dụng dịch vụ đổi pin |
| Pin và trạm | **Battery** | Viên pin và tình trạng hiện tại |
| | **Station** | Trạm đổi pin |
| | **Slot** | Ô chứa của trạm |
| | **BatteryAssignment** | Lịch sử cấp/thu hồi quyền sử dụng pin |
| | **BatteryTelemetry** | Lịch sử đo SOC, SOH, nhiệt độ |
| | **ChargingSession** | Lịch sử từng phiên sạc |
| Giao dịch đổi pin | **SwapSession** | Phiên đổi pin |
| | **SwapItem** | Kết quả đổi từng viên pin trong phiên |
| Mô phỏng và giám sát | **StationCommand** | Lệnh Backend gửi Simulator |
| | **StationEvent** | Sự kiện/phản hồi từ Simulator |
| | **Alert** | Cảnh báo và trạng thái xử lý |

## ERD tổng quan – Bước 2 (bản đề xuất, chưa duyệt)

Sơ đồ dưới đây thể hiện đủ **14 thực thể** và các mối quan hệ chính bằng ký hiệu Crow's Foot. Mức 1–N mô tả **lịch sử nhiều bản ghi**, không mặc định rằng mọi thực thể đều bắt buộc có dữ liệu liên quan ngay khi được tạo.

```mermaid
erDiagram
    ROLE ||--o{ USER : "phan quyen"
    USER ||--o{ VEHICLE : "quan ly"

    VEHICLE ||--o{ BATTERY_ASSIGNMENT : "duoc cap pin"
    BATTERY ||--o{ BATTERY_ASSIGNMENT : "lich su ban giao"

    STATION ||--o{ SLOT : "gom o chua"
    BATTERY ||--o{ BATTERY_TELEMETRY : "lich su do"
    BATTERY ||--o{ CHARGING_SESSION : "lich su sac"
    SLOT ||--o{ CHARGING_SESSION : "noi sac"

    VEHICLE ||--o{ SWAP_SESSION : "yeu cau doi"
    STATION ||--o{ SWAP_SESSION : "phuc vu"
    SWAP_SESSION ||--o{ SWAP_ITEM : "gom luot doi"
    BATTERY |o--o{ SWAP_ITEM : "pin tra"
    BATTERY |o--o{ SWAP_ITEM : "pin cap"

    STATION ||--o{ STATION_COMMAND : "nhan lenh"
    STATION ||--o{ STATION_EVENT : "phat sinh"
    STATION ||--o{ ALERT : "canh bao"

    STATION_COMMAND |o--o{ STATION_EVENT : "phan hoi lenh"
    STATION_EVENT |o--o{ ALERT : "nguon canh bao"
    SLOT |o--o| BATTERY : "vi tri hien tai"
```

**Lưu ý khi đọc sơ đồ:**

- `VEHICLE` và `BATTERY` có quan hệ **N–N theo lịch sử**, được thể hiện qua `BATTERY_ASSIGNMENT`. Quan hệ hiện tại còn cần ràng buộc riêng.
- `SWAP_ITEM` có hai vai trò pin khác nhau: **pin trả** và **pin cấp**. Khi giao dịch đang xử lý hoặc gặp lỗi, một trong hai vai trò có thể chưa được xác định.
- Quy tắc **mỗi phiên đổi 1 hoặc 2 viên** là ràng buộc nghiệp vụ; ký hiệu Crow's Foot 1–N chưa thể hiện giới hạn trên là 2.
- Các quan hệ **SLOT–BATTERY (vị trí hiện tại)**, **STATION_COMMAND–STATION_EVENT (phản hồi lệnh)**, **STATION_EVENT–ALERT (nguồn cảnh báo)** là **đề xuất cần kiểm tra**. Không mặc định mọi sự kiện đều phát sinh từ một lệnh hoặc mọi sự kiện đều tạo cảnh báo.
- Vị trí vật lý của pin, quyền sử dụng khi giao dịch gián đoạn và cách cập nhật dữ liệu từ Simulator sẽ được làm rõ trước khi duyệt ERD logic chi tiết.

**Chưa đưa vào phạm vi:** ServicePlan, Subscription, Charge, Payment. **Chưa cần tách thành thực thể riêng:** BatteryMovement, MaintenanceRecord, AuditLog. Các đối tượng này có thể được xem xét lại nếu có yêu cầu mới.

**Tiến độ ERD:** Bước 1 (xác định thực thể) **đã chốt**; Bước 2 (quan hệ), Bước 3 (thuộc tính/ràng buộc) và ERD hoàn chỉnh **chưa chốt**.


Xem [thiết kế bảng và thuộc tính](erd-design.md).
