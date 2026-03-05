# User

## User Registration Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Page["Trang đăng ký"]
    Page --> Input["Nhập: Email, MK, Tên, SĐT, Địa chỉ, Vai trò"]
    Input --> CheckUnique{"Email đã tồn tại?"}
    CheckUnique -- Có --> ErrExist["Lỗi: Email đã dùng"]
    ErrExist --> Input
    CheckUnique -- Không --> SendEmail["Gửi email xác thực<br>IsVerified = false"]
    SendEmail --> Verify{"Xác thực email?"}
    Verify -- Không --> Wait["Chờ người dùng click link"]
    Verify -- Có --> Create["Tạo Users record<br>CreatedAt = now()"]
    Create --> Success["Đăng ký thành công<br>Chuyển đến đăng nhập"]
    Success --> End((Kết thúc))
```

## User Login Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Page["Trang đăng nhập"]
    Page --> Input["Nhập Email & Password<br>hoặc OAuth"]
    Input --> Check{"Kiểm tra PasswordHash<br>hoặc OAuth token"}
    Check -- Sai --> Err["Lỗi: Sai thông tin"]
    Err --> Input
    Check -- Đúng --> GenJWT["Tạo JWT token"]
    GenJWT --> Sync["Đồng bộ AvatarUrl, Role"]
    Sync --> Redirect["Chuyển hướng Dashboard<br>theo Role"]
    Redirect --> End((Kết thúc))

    Page -. Quên MK .-> Forgot["Gửi link reset password"]
    Forgot --> End
```

## Payment Processing Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Role{ Vai trò? }

    Role -- Tenant --> View["Xem danh sách hóa đơn"]
    View --> Detail["Xem chi tiết Invoice"]
    Detail --> Pay{"Chọn phương thức thanh toán<br>Cash / BankTransfer / Online"}
    Pay --> Submit["Gửi yêu cầu thanh toán<br>Amount (toàn bộ hoặc một phần)"]
    Submit --> WaitLandlord["Chờ Landlord xác nhận"]

    Role -- Landlord --> Confirm["Xem yêu cầu thanh toán"]
    Confirm --> Action{ Hành động? }
    Action -- Xác nhận --> Record["Tạo Payments record<br>PaymentDate = now()"]
    Action -- Từ chối --> Reject["Ghi chú từ chối"]
    Record --> Update["Cập nhật Invoices.Status<br>→ Paid / Partial"]
    Update --> Notify["SignalR thông báo Tenant"]
    Reject --> Notify
    Notify --> End((Kết thúc))
```

# Landlord

## Create Area Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Đã đăng nhập<br>Role = Landlord?"}
    Auth -- Không --> Deny["Từ chối truy cập"]
    Auth -- Có --> Dashboard["Dashboard Landlord"]
    Dashboard --> Form["Form tạo khu trọ"]
    Form --> Input["Nhập: Tên, Địa chỉ, Lat/Lng,<br>Mô tả, RoomCount"]
    Input --> Save["Lưu vào Areas<br>LandlordId = currentUser.Id<br>CreatedAt = now()"]
    Save --> Success["Thông báo thành công"]
    Success --> List["Cập nhật danh sách khu trọ"]
    List --> End((Kết thúc))
```

## Manage Property Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Đăng nhập<br>Role = Landlord?"}
    Auth -- Không --> Deny["Từ chối truy cập"]
    Auth -- Có --> Dashboard["Dashboard Landlord → Quản lý phòng"]

    Dashboard --> Action{ Hành động? }
    Action -- Tạo mới --> Form["Form tạo phòng mới"]
    Action -- Sửa --> EditForm["Form sửa phòng (chỉ nếu Approved hoặc PendingApproval)"]
    Action -- Xóa --> Delete["Xóa phòng (nếu chưa có Contract)"]

    Form --> Input["Nhập: Tên, Diện tích, Giá, Mô tả, Địa chỉ/Lat/Lng,<br>Area (nếu có), Upload ảnh, Thêm tiện ích"]
    Input --> Save["Lưu vào Properties<br>Status = 'PendingApproval'<br>CreatedAt/UpdatedAt = now()"]
    Save --> NotifyAdmin["SignalR / Notification gửi cho Admin:<br>'Có phòng mới chờ duyệt' + link detail"]
    NotifyAdmin --> WaitAdmin["Chờ Admin xử lý (PendingApproval)"]

    subgraph "Admin duyệt (Dashboard Admin)"
        WaitAdmin --> AdminView["Admin xem danh sách PendingApproval<br>(filter theo Landlord, khu vực)"]
        AdminView --> Review["Xem chi tiết: ảnh, tiện ích, vị trí, giá, mô tả"]
        Review --> Decision{ Quyết định? }

        Decision -- Duyệt --> Approve["Set Status = 'Approved'<br>UpdatedAt = now()<br>Ghi log: ApprovedBy = AdminId"]
        Decision -- Từ chối --> Reject["Set Status = 'Rejected'<br>Nhập RejectionReason<br>UpdatedAt = now()"]
        Decision -- Yêu cầu sửa --> RequestEdit["Gửi Notification cho Landlord:<br>'Yêu cầu chỉnh sửa: [lý do]'<br>Status vẫn PendingApproval hoặc set 'NeedsRevision' (nếu thêm enum)"]

        Approve --> NotifyLandlordApprove["SignalR thông báo Landlord:<br>'Phòng đã được duyệt, bạn có thể set Available'"]
        Reject --> NotifyLandlordReject["SignalR thông báo Landlord:<br>'Phòng bị từ chối: [RejectionReason]'"]
        RequestEdit --> NotifyLandlordEdit["SignalR thông báo Landlord:<br>'Yêu cầu chỉnh sửa phòng'"]
    end

    NotifyLandlordApprove --> LandlordNext["Landlord set Status = 'Available'<br>→ Phòng hiển thị công khai cho Tenant tìm kiếm"]
    LandlordNext --> Success["Quy trình tạo phòng hoàn tất"]
    Success --> End((Kết thúc))

    EditForm --> Update["Cập nhật Properties<br>Nếu đang Approved → có thể cần duyệt lại (tùy logic)<br>Status = 'PendingApproval' nếu thay đổi lớn"]
    Delete --> ConfirmDelete["Xác nhận xóa (cascade ảnh, tiện ích nếu cần)"]
    ConfirmDelete --> SuccessDelete["Xóa thành công + cập nhật RoomCount ở Area"]
```

## Create Contract Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Landlord?"}
    Auth -- Có --> Select["Chọn phòng (Properties.Id)<br>Chọn người thuê (Users.Id)"]
    Select --> Form["Nhập: StartDate, EndDate,<br>Deposit, Terms"]
    Form --> Upload["Upload hợp đồng PDF → ContractFileUrl"]
    Upload --> Create["Tạo Contracts record<br>Status = Active"]
    Create --> UpdateRoom["Properties.Status = 'Rented'"]
    UpdateRoom --> Notify["Gửi thông báo cho Tenant"]
    Notify --> Success["Hợp đồng tạo thành công"]
    Success --> End((Kết thúc))
```

## Manage Invoice Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Đăng nhập Landlord?"}
    Auth -- Không --> Deny["Từ chối truy cập"]
    Auth -- Có --> Select["Chọn hợp đồng (Contracts.Id)"]
    Select --> Form["Nhập: Kỳ (Period),<br>Điện (số + tiền), Nước (số + tiền),<br>Phí khác, Phạt (nếu có)"]
    Form --> Calc["Tính Total = Rent + Điện + Nước + Other + Penalty"]
    Calc --> Generate["Tạo PDF → InvoiceUrl<br>(sử dụng QuestPDF hoặc iText)"]
    Generate --> Save["Lưu Invoices record<br>Status = Pending<br>DueDate = ..."]
    Save --> Notify["SignalR gửi thông báo cho Tenant"]
    Notify --> Success["Hóa đơn tạo thành công"]
    Success --> End((Kết thúc))
```

## Revenue Statistics Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{ Vai trò? }
    Auth -- Landlord --> Scope["Chọn phạm vi: khu trọ / tất cả phòng của mình"]
    Auth -- Admin --> Scope["Toàn hệ thống"]

    Scope --> Time["Chọn khoảng thời gian<br>tháng / quý / năm"]
    Time --> Query["Truy vấn:<br>Invoices + Payments<br>Properties.Status"]
    Query --> Calc["Tính:<br>- Doanh thu thực tế<br>- Tỷ lệ lấp đầy (%)<br>- Công nợ còn lại"]
    Calc --> Chart["Tạo dữ liệu cho biểu đồ<br>(Recharts / Chart.js)"]
    Chart --> Display["Hiển thị: Line chart doanh thu,<br>Pie chart tỷ lệ lấp đầy"]
    Display --> Export{ Xuất báo cáo? }
    Export -- Có --> PDF["Xuất PDF / Excel"]
    Export -- Không --> End
    PDF --> End((Kết thúc))
```

# Tenant

## Search Properties Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Page["Trang tìm kiếm"]
    Page --> Filter["Chọn bộ lọc: Khu vực, Giá,<br>Diện tích, Tiện ích"]
    Filter --> Map["Hiển thị bản đồ + danh sách"]
    Map --> View["Xem chi tiết phòng"]
    View --> Log{"Đã đăng nhập?"}
    Log -- Có --> SaveView["Lưu ViewHistories"]
    Log -- Không --> NoLog["Không lưu lịch sử"]
    SaveView --> Recommend["Cập nhật gợi ý"]
    NoLog --> Recommend
    Recommend --> End((Kết thúc))
```

## Schedule Appointment Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Đăng nhập Tenant?"}
    Auth -- Không --> Login["Yêu cầu đăng nhập"]
    Auth -- Có --> Select["Chọn phòng từ kết quả tìm kiếm"]
    Select --> Form["Chọn ngày giờ<br>Nhập ghi chú"]
    Form --> Submit["Gửi yêu cầu → Appointments<br>Status = Pending"]
    Submit --> Notify["SignalR thông báo Landlord"]
    Notify --> LandlordAction{ Landlord: }
    LandlordAction -- Xác nhận --> Confirm["Status = Confirmed"]
    LandlordAction -- Từ chối --> Reject["Status = Rejected"]
    Confirm --> NotifyTenant["Thông báo Tenant"]
    Reject --> NotifyTenant
    NotifyTenant --> End((Kết thúc))
```

# System

## Recommendation Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Đăng nhập?"}
    Auth -- Không --> LocationOnly["Chỉ dùng vị trí hiện tại<br>(nếu có)"]
    Auth -- Có --> History["Lấy SearchHistories<br>+ ViewHistories<br>của user hiện tại"]

    LocationOnly --> CalcDist[Tính khoảng cách<br>Haversine từ vị trí<br> người dùng → Properties]
    History --> Content["Content-based: so sánh<br>bộ lọc cũ<br>(giá, diện tích, tiện ích)"]
    History --> Collab["Collaborative: tìm user<br>tương đồng<br>dựa trên ViewHistories"]

    CalcDist --> Rank["Sắp xếp theo khoảng cách<br>gần nhất"]
    Content --> Score["Tính điểm tương đồng<br>(cosine similarity đơn giản)"]
    Collab --> Score

    Rank --> Combine["Kết hợp điểm: vị trí + content + collab"]
    Score --> Combine
    Combine --> List["Trả về top 8-12 phòng gợi ý"]
    List --> Display["Hiển thị danh sách"]
    Display --> End((Kết thúc))
```

## Notification Flow (System + Admin)
```mermaid
graph TD
    Start((Bắt đầu)) --> Trigger{ Sự kiện trigger? }

    Trigger -- Hệ thống --> Event["Ví dụ: Invoice created,<br>Appointment confirmed,<br>Contract near expire"]
    Trigger -- Admin --> Manual["Admin soạn thông báo hệ thống<br>hoặc gửi cá nhân"]

    Event --> Create["Tạo record Notifications<br>Type, Content, RelatedId, UserId"]
    Manual --> Create

    Create --> Push["SignalR Hub push realtime<br>đến UserId tương ứng"]
    Push --> Frontend["NotificationBell nhấp nháy / popup"]
    Frontend --> Read{ Người dùng đọc? }
    Read -- Có --> Mark["IsRead = true"]
    Read -- Không --> Keep["Giữ nguyên IsRead = false"]
    Mark --> End
    Keep --> End((Kết thúc))
```

# Admin

## Admin Manage Users Flow
```mermaid
graph TD
    Start((Bắt đầu)) --> Auth{"Role = Admin?"}
    Auth -- Không --> Deny["Từ chối truy cập"]
    Auth -- Có --> Dashboard["Admin Dashboard → Quản lý người dùng"]
    Dashboard --> Search["Tìm kiếm theo Email / Tên / Role"]
    Search --> List["Hiển thị danh sách Users"]
    List --> Action{ Hành động? }

    Action -- Xem chi tiết --> Detail["Xem FullName, Phone, Address,<br>Avatar, CreatedAt, IsVerified"]
    Action -- Sửa --> Edit["Chỉnh sửa thông tin / Role / IsVerified"]
    Action -- Khóa --> Block["Set IsActive = false<br>Ghi log lý do"]
    Action -- Xem khiếu nại --> Complaints["Xem Messages / Notifications liên quan"]

    Edit --> Save["Cập nhật Users record<br>UpdatedAt = now()"]
    Block --> Save
    Save --> Notify["(Tùy chọn) Gửi thông báo cho user"]
    Notify --> Refresh["Làm mới danh sách"]
    Refresh --> End((Kết thúc))
```