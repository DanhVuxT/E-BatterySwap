# E-BatterySwap

**E-BatterySwap** là đồ án xây dựng **hệ thống quản lý và mô phỏng trạm đổi pin thông minh cho xe máy điện**. Hệ thống gồm ứng dụng khách hàng, ứng dụng web quản lý/giám sát và IoT Simulator mô phỏng hoạt động của trạm mà không cần chế tạo phần cứng thật.

> **Trạng thái dự án:** Đang trong giai đoạn thiết kế. Các chức năng dưới đây mô tả phạm vi hệ thống, không phải các tính năng đã triển khai. Phiên bản yêu cầu hiện hành tập trung vào vận hành trạm đổi pin và không bao gồm quản lý thuê bao hoặc thanh toán.

## 1. Mục tiêu và phạm vi

- Xây dựng quy trình đổi pin tự động: khách hàng dùng Mobile App quét QR trạm, yêu cầu đổi **1 hoặc 2 viên pin** và theo dõi kết quả.
- Quản lý người dùng, phương tiện, quyền sử dụng pin, từng viên pin, trạm và ô chứa.
- Mô phỏng tiếp nhận pin cũ, kiểm tra hợp lệ, cấp pin mới, điều khiển cửa ô tủ và xử lý các tình huống bất thường.
- Theo dõi thông số **SOC** (State of Charge – mức sạc), **SOH** (State of Health – sức khỏe pin), nhiệt độ; lưu lịch sử đo để phục vụ giám sát.
- Mô phỏng quá trình sạc tự động và ghi nhận các phiên sạc, bao gồm hoàn thành, gián đoạn hoặc lỗi.
- Giám sát lệnh điều khiển, sự kiện phản hồi, cảnh báo và giao dịch đổi pin không hoàn tất hoặc hoàn thành một phần.

**Ngoài phạm vi triển khai:** quản lý gói thuê pin, thuê bao, tính phí, thanh toán, bán xe và chế tạo phần cứng trạm thật. Kiểm tra quyền sử dụng pin vẫn thuộc phạm vi hệ thống.

## 2. Thành phần hệ thống

| Thành phần | Trách nhiệm |
| --- | --- |
| **Mobile App** | Khách hàng đăng nhập, quản lý thông tin và xe, tìm trạm, quét QR, yêu cầu đổi pin, theo dõi tiến trình và xem lịch sử. |
| **Web Application (Customer Portal + Admin Dashboard)** | Một ứng dụng web chung; Customer Portal phục vụ tra cứu tài khoản/xe/lịch sử, Admin Dashboard dành cho nhân viên và quản trị viên vận hành, giám sát và xử lý sự cố. Phân quyền theo vai trò. |
| **Backend API** | Xác thực, phân quyền, kiểm tra quyền đổi pin, điều phối giao dịch, quản lý trạng thái và giao tiếp với Simulator. |
| **Database (Microsoft SQL Server)** | Lưu dữ liệu người dùng, xe, quyền sử dụng pin, pin, trạm, ô tủ, giao dịch, thông số đo, phiên sạc, lệnh, sự kiện và cảnh báo. |
| **IoT Simulator** | Mô phỏng ô chứa, khóa/cửa, nhận diện pin, cảm biến, quá trình sạc, phản hồi lệnh và các lỗi vận hành. |

Mobile App và Web Application dùng chung Backend API, không truy cập trực tiếp cơ sở dữ liệu.

## 3. Chức năng dự kiến

### 3.1. Khách hàng

- Đăng ký, đăng nhập và quản lý tài khoản, phương tiện.
- Xem danh sách trạm và trạng thái phục vụ.
- Quét mã QR bằng Mobile App để xác định trạm; Backend tiếp tục xác thực quyền đổi pin.
- Yêu cầu đổi **1 hoặc 2 viên pin**, theo dõi tiến trình và xem lịch sử giao dịch.

### 3.2. Nhân viên và quản trị viên

- Ghi nhận bàn giao pin lần đầu cho xe/khách hàng qua Admin Dashboard; không triển khai quy trình bán xe.
- Quản lý người dùng, phương tiện, pin, trạm và ô chứa.
- Theo dõi trạng thái hiện tại và lịch sử **SOC, SOH, nhiệt độ** của pin.
- Theo dõi quá trình sạc, tình trạng kết nối trạm, lệnh điều khiển và phản hồi của Simulator.
- Giám sát các phiên đổi pin, cảnh báo và hỗ trợ xử lý giao dịch bất thường.

### 3.3. IoT Simulator

- Mô phỏng mở/đóng cửa, nhận diện pin được đưa vào hoặc lấy ra và phản hồi lệnh điều khiển.
- Mô phỏng kiểm tra pin cũ, cấp pin mới khi đủ điều kiện và cập nhật trạng thái thực tế.
- Mô phỏng tự động sạc pin; ghi nhận lịch sử thông số đo và phiên sạc.
- Mô phỏng lỗi pin không hợp lệ, lỗi cửa, mất kết nối, gián đoạn sạc và giao dịch đổi pin hoàn thành một phần.

## 4. Quy tắc nghiệp vụ cốt lõi

- **Pin thuộc sở hữu đơn vị vận hành**; hệ thống ghi nhận quyền sử dụng pin đã bàn giao cho khách hàng/xe.
- Pin ban đầu được nhân viên ghi nhận bàn giao qua Admin Dashboard.
- Sử dụng **một chuẩn pin chung** cho các dòng xe được hỗ trợ; mỗi xe dùng 1 hoặc 2 viên.
- Cho phép đổi pin ở **mọi mức SOC hợp lệ**, miễn thỏa điều kiện về quyền sử dụng pin, trạng thái trạm và an toàn.
- **QR chỉ dùng để xác định trạm**, không tự động mở khóa. Backend phải kiểm tra quyền đổi pin trước khi điều khiển trạm.
- Hệ thống chỉ coi hành động vật lý là đã thực hiện khi nhận được sự kiện/phản hồi phù hợp từ Simulator; gửi lệnh không đồng nghĩa với thành công.
- Một phiên đổi hai viên có thể **hoàn thành một phần**; phải ghi nhận kết quả của từng viên, không tự động coi toàn phiên thành công.
- Cần phân biệt **quyền sử dụng pin** với **vị trí vật lý hiện tại** của pin.

> **Các quy tắc vận hành cần đặc tả ở giai đoạn tiếp theo:** thứ tự đổi hai viên, tiêu chí an toàn khi cấp pin, thời gian chờ cửa, xử lý pin trả không hợp lệ và cơ chế phục hồi sau mất kết nối. Tham số mô phỏng không thay thế quy tắc nghiệp vụ.

## 5. Cơ sở dữ liệu (Database)

Hệ thống dùng **Microsoft SQL Server**. Thiết kế hiện gồm **14 thực thể** phục vụ quản lý tài khoản, phương tiện, pin, trạm, giao dịch đổi pin và giám sát Simulator.

- [ERD tổng quan – danh sách thực thể và quan hệ](docs/database/erd-diagram.md)
- [Đặc tả CSDL – bảng, thuộc tính và ràng buộc](docs/database/erd-design.md)

**Phạm vi tài liệu:** ERD tổng quan mô tả 14 thực thể. Đặc tả chi tiết hiện có các bảng `Role`, `User`, `Vehicle`; các nhóm bảng còn lại được bổ sung theo tiến độ thiết kế.

## 6. Công nghệ sử dụng

| Hạng mục | Công nghệ / định hướng | Trạng thái |
| --- | --- | --- |
| Ngôn ngữ Backend | **C#** | Sử dụng |
| Backend Framework | **ASP.NET Core Web API** | Sử dụng |
| Giao tiếp ứng dụng | **REST API** dùng chung Mobile/Web | Định hướng |
| Mobile App | Chưa chọn framework | Đang xác định |
| Web Application | **Một ứng dụng gồm Customer Portal và Admin Dashboard**; framework đang xác định | Thiết kế |
| Database | **Microsoft SQL Server** | Sử dụng |
| IoT Simulator | Chưa chọn công nghệ cụ thể | Đang xác định |
| Giao tiếp thời gian thực | Đang xác định giao thức/công nghệ | Đang xác định |

## 7. Trạng thái phát triển và tài liệu

- **Phân tích yêu cầu:** SRS v0.1 là phiên bản cơ sở; tài liệu yêu cầu cần cập nhật để phản ánh phạm vi hệ thống trạm đổi pin thông minh hiện hành.
- **Thiết kế CSDL:** ERD tổng quan gồm 14 thực thể; đặc tả chi tiết hiện có bảng `Role`, `User`, `Vehicle`. Chưa triển khai schema SQL Server.
- **Phân tích và thiết kế hệ thống:** Use Case, Activity Diagram và Sequence Diagram nằm trong phạm vi tài liệu thiết kế hệ thống.
- **Phát triển và kiểm thử:** Chưa triển khai chức năng; cấu trúc mã nguồn, hướng dẫn chạy, tài liệu API và kết quả kiểm thử sẽ được bổ sung trong giai đoạn phát triển.

> README mô tả **phạm vi thiết kế hiện hành**, không thay thế SRS. Khi có thay đổi nghiệp vụ, cần đồng bộ tài liệu yêu cầu và kế hoạch trước khi triển khai.
