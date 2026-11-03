# E-BatterySwap

**E-BatterySwap** là dự án xây dựng **hệ thống quản lý và mô phỏng trạm đổi pin cho xe máy điện**, gồm ứng dụng dành cho khách hàng, hệ thống quản trị và phần mềm mô phỏng hoạt động của trạm đổi pin tự động.

Dự án được thực hiện trong khuôn khổ **Đồ án 1**, nhằm nghiên cứu, phân tích, thiết kế và triển khai một quy trình đổi pin có thể vận hành và trình diễn trên phần mềm mà không cần chế tạo trạm vật lý.

> **Trạng thái:** Đang trong giai đoạn phân tích và thiết kế hệ thống. Tài liệu SRS v0.1 đã được phê duyệt; các chức năng dưới đây mô tả **phạm vi dự kiến**, chưa có nghĩa là đã lập trình hoàn tất.

## 1. Mục đích dự án

- Xây dựng quy trình đổi pin xe máy điện thuận tiện cho khách hàng thông qua ứng dụng di động và mã QR tại trạm.
- Quản lý tập trung khách hàng, xe, quyền sử dụng pin, thuê bao, trạm đổi pin và các giao dịch.
- Giám sát trạng thái pin như **SOC** (State of Charge – mức sạc), **SOH** (State of Health – tình trạng sức khỏe pin) và nhiệt độ.
- Mô phỏng quá trình nhận pin cũ, cấp pin mới, sạc pin và xử lý các sự cố vận hành mà không cần phần cứng thật.
- Ứng dụng kiến thức phân tích yêu cầu, thiết kế hệ thống, phát triển API, giao diện và kiểm thử phần mềm vào một bài toán thực tế.

## 2. Các thành phần chính

| Thành phần | Vai trò |
| --- | --- |
| **Mobile App** | Khách hàng tìm trạm, quét QR, yêu cầu đổi pin và xem lịch sử. |
| **Web Application (Customer Portal + Admin Dashboard)** | Một ứng dụng web gồm khu vực khách hàng để quản lý tài khoản, phương tiện, thuê bao và khu vực nhân viên/quản trị viên để vận hành, giám sát hệ thống; giao diện và quyền truy cập được phân tách theo vai trò. |
| **Backend API** | Xác thực, phân quyền, áp dụng quy tắc nghiệp vụ, quản lý trạng thái pin và giao dịch, cung cấp API dùng chung. |
| **Database (Microsoft SQL Server)** | Lưu trữ tài khoản, phương tiện, pin, trạm, thuê bao, giao dịch, thanh toán và nhật ký sự kiện. |
| **IoT Simulator** | Mô phỏng ô tủ, khóa cửa, cảm biến, quá trình sạc và các sự kiện từ trạm đổi pin. |

**Tổ chức Web Application:** Customer Portal và Admin Dashboard được phát triển trong cùng một ứng dụng web, nhưng phân quyền riêng. Mobile App và Web Application dùng chung Backend API; các giao diện không truy cập trực tiếp cơ sở dữ liệu.

## 3. Chức năng chính (dự kiến)

### Dành cho khách hàng

- Đăng ký, đăng nhập và quản lý tài khoản, phương tiện.
- Xem thông tin trạm đổi pin và tình trạng phục vụ.
- Quản lý dịch vụ thuê pin theo tháng.
- Quét mã QR bằng Mobile App để xác định trạm và bắt đầu yêu cầu đổi pin.
- Lựa chọn đổi **1 hoặc 2 viên pin** trong một phiên, theo dõi tiến trình và xem lịch sử giao dịch.
- Xem phí thuê pin, phí đổi pin và lịch sử thanh toán.

### Dành cho nhân viên và quản trị viên

- Ghi nhận việc **bàn giao pin lần đầu** cho khách hàng tại bên bán xe.
- Quản lý thông tin khách hàng, phương tiện, pin, trạm và ô chứa.
- Theo dõi SOC, SOH, nhiệt độ, trạng thái sạc và tình trạng kết nối của trạm.
- Giám sát phiên đổi pin, lịch sử hoạt động và cảnh báo sự cố.
- Hỗ trợ kiểm tra, xử lý những giao dịch bất thường.

### Mô phỏng trạm đổi pin

- Mô phỏng mở/đóng cửa, nhận diện pin được đưa vào hoặc lấy ra.
- Mô phỏng kiểm tra pin cũ và cấp pin mới khi đủ điều kiện.
- Mô phỏng sạc pin tự động và cập nhật thông số pin.
- Mô phỏng các lỗi như pin không hợp lệ, lỗi cửa hoặc mất kết nối trạm.

## 4. Quy tắc nghiệp vụ cốt lõi

- **Pin thuộc sở hữu đơn vị vận hành**; khách hàng được cấp quyền sử dụng theo dịch vụ thuê pin.
- Khách hàng trả **phí thuê pin hằng tháng** và **phí cố định cho mỗi viên pin được đổi**. Mức giá cụ thể chưa được chốt.
- Pin ban đầu được nhân viên bên bán xe bàn giao và ghi nhận qua Admin Dashboard; dự án không đi sâu vào quy trình bán xe.
- Hệ thống sử dụng **một chuẩn pin chung** cho các dòng xe được hỗ trợ; mỗi xe có thể sử dụng 1 hoặc 2 viên.
- Khách hàng có thể yêu cầu đổi pin ở **bất kỳ mức SOC hợp lệ** nào, miễn đáp ứng các điều kiện dịch vụ và an toàn.
- Quét QR chỉ để xác định trạm; Backend phải xác thực quyền đổi pin trước khi cấp lệnh cho trạm.

> Những chi tiết như trình tự đổi hai viên, ngưỡng an toàn, thời điểm thu phí và quy trình khôi phục lỗi vẫn đang được phân tích và sẽ được bổ sung sau.

## 5. Công nghệ sử dụng

| Hạng mục | Công nghệ / định hướng | Trạng thái |
| --- | --- | --- |
| Ngôn ngữ Backend | **C#** | Đã chọn |
| Backend Framework | **ASP.NET Core Web API** | Đã chọn |
| Giao tiếp ứng dụng | **REST API** dùng chung cho Mobile/Web/Admin | Định hướng thiết kế |
| Mobile App | Chưa chọn framework | Chưa chốt |
| Web Application (Customer Portal + Admin Dashboard) | Một ứng dụng web chung; framework chưa chọn | **Đã chốt mô hình gộp**, chưa chốt framework |
| Database | **Microsoft SQL Server** | **Đã chọn** |
| IoT Simulator | Chưa chọn công nghệ triển khai cụ thể | Chưa chốt |
| Giao tiếp thời gian thực | Chưa chốt giao thức/công nghệ | Chưa chốt |

## 6. Trạng thái phát triển

Dự án hiện đang thực hiện theo quy trình phân tích – thiết kế – phát triển – kiểm thử. **Bước tìm hiểu yêu cầu đã hoàn thành** với SRS v0.1 được phê duyệt. Bước tiếp theo là xây dựng **Use Case Diagram**, sau đó là Activity Diagram, Sequence Diagram, thiết kế kiến trúc, cơ sở dữ liệu và triển khai các thành phần.

Các nội dung như hướng dẫn cài đặt, cấu trúc thư mục, API documentation, hình ảnh demo, kiểm thử và triển khai sẽ được bổ sung khi dự án phát triển.
