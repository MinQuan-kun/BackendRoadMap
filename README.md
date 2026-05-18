# 🗺️ GameDev & IT Developer Roadmap - Career & Recruitment Platform

Chào mừng bạn đến với **GameDev & IT Developer Roadmap** — Hệ thống hỗ trợ xây dựng lộ trình học tập, hướng nghiệp và kết nối tuyển dụng dành cho lập trình viên Game & Công nghệ thông tin. Dự án được thiết kế chuyên nghiệp, linh hoạt với cấu trúc Graph-based Roadmap giúp học viên đi từ con số 0 đến khi có việc làm thành công.

---

## 🔗 Link Web Deploy & Live Demo

Hệ thống được cấu hình sẵn sàng để triển khai tự động lên **Render** thông qua Docker container.

*   **API Service (Backend):** `https://backendservice-sa6x.onrender.com` *(Có thể tùy chỉnh theo domain Render cá nhân của bạn)*
*   **Trạng thái Deployment:** Đã cấu hình tệp `render.yaml` và `Dockerfile` tối ưu hóa cho môi trường Production (Singapore region, Free plan).

---

## 🛠️ Công Nghệ Sử Dụng (Technology Stack)

Hệ thống sử dụng các công nghệ hiện đại bậc nhất để đảm bảo tính an toàn, bảo mật và khả năng mở rộng tối đa:

| Thành Phần | Công Nghệ | Mô Tả |
| :--- | :--- | :--- |
| **Backend Core** | **ASP.NET Core (.NET 10.0)** | Framework mạnh mẽ, tối ưu hiệu năng tốt nhất từ Microsoft |
| **Database** | **MongoDB Atlas** | Hệ quản trị cơ sở dữ liệu NoSQL dạng tài liệu cực kỳ linh hoạt |
| **Authentication** | **JWT Bearer / BCrypt.Net** | Xác thực phân quyền an toàn, mã hóa mật khẩu cực mạnh |
| **Validation** | **FluentValidation** | Kiểm tra dữ liệu đầu vào chuẩn chỉ, tự động hóa logic validation |
| **File Storage** | **Cloudinary** | Lưu trữ và xử lý hình ảnh, avatar, ảnh bìa tự động trên đám mây |
| **Email Service** | **SMTP MailKit** | Gửi email kích hoạt, khôi phục mật khẩu bảo mật qua Google SMTP |
| **Deployment** | **Docker & Render** | Đóng gói Container hóa và tự động hóa CI/CD lên Render |

---

## 🚀 Các Chức Năng Chính (Core Features)

Dự án là sự kết hợp hoàn hảo giữa **Học Tập (E-Learning)**, **Định Hướng (Career Guidance)** và **Tuyển Dụng (Recruitment Portal)**, bao gồm 3 phân hệ chính:

### 1. Phân Hệ Dành Cho Học Viên (Users)
*   🔑 **Đăng ký / Đăng nhập / Khôi phục mật khẩu**: Xác thực hai bước an toàn với mã OTP gửi trực tiếp qua Email.
*   🗺️ **Lộ trình học tập (Roadmap & Pathways)**:
    *   Xem danh sách các lộ trình chính thức (Official Roadmap) do hệ thống đề xuất hoặc các lộ trình cộng đồng (Community Roadmap).
    *   Theo dõi lộ trình dưới dạng đồ thị (Graph) trực quan với các Node (chủ đề học) và Edge (mối liên kết/tiền đề môn học).
    *   Bắt đầu học (Follow/Unfollow) các lộ trình mong muốn.
*   📈 **Theo dõi tiến độ học (Progress Tracking)**:
    *   Đánh dấu hoàn thành (Complete) hoặc bỏ qua (Skip) các chủ đề, bài học (Lessons) hay nhiệm vụ cụ thể (Tasks).
    *   Cập nhật tự động tiến độ học tập trực quan trên giao diện cá nhân.
*   📝 **Khảo sát hướng nghiệp (Career Quiz)**:
    *   Làm các bài test khảo sát năng lực và sở thích để hệ thống gợi ý lộ trình nghề nghiệp GameDev hoặc IT phù hợp nhất.
*   💼 **Tìm kiếm & Ứng tuyển việc làm**:
    *   Tìm kiếm việc làm (Jobs) phù hợp theo kỹ năng (Skills), mức lương, loại hình làm việc (Remote/Onsite).
    *   Nộp hồ sơ (Apply) trực tiếp cho nhà tuyển dụng, theo dõi trạng thái ứng tuyển (Pending, Approved, Rejected).

### 2. Phân Hệ Dành Cho Nhà Tuyển Dụng (Recruiters)
*   ✅ **Xác minh tài khoản**: Được quản trị viên duyệt tài khoản để đảm bảo uy tín của các bài đăng tuyển.
*   📮 **Đăng tin tuyển dụng (Job Board)**:
    *   Tạo, cập nhật và xóa các tin tuyển dụng một cách linh hoạt.
    *   Yêu cầu các kỹ năng bắt buộc (Required Skill Tags) và các khóa học học viên cần hoàn thành để tăng tỷ lệ trúng tuyển.
*   👁️‍🗨️ **Quản lý hồ sơ ứng viên (Applicant Management)**:
    *   Xem danh sách chi tiết các ứng viên đã nộp hồ sơ vào từng tin tuyển dụng.
    *   Cập nhật trạng thái duyệt hồ sơ của ứng viên kèm theo email thông báo tự động.

### 3. Phân Hệ Dành Cho Quản Trị Viên (Admin)
*   👤 **Quản lý người dùng**: Kích hoạt, tạm khóa hoặc cấm (Ban) các tài khoản vi phạm chính sách hệ thống.
*   🏗️ **Quản lý Đồ thị Lộ trình (Roadmap Builder)**:
    *   Xây dựng, chỉnh sửa cấu trúc các Node bài học, các liên kết Edge của đồ thị lộ trình.
    *   Duyệt lộ trình học từ cộng đồng đóng góp lên thành lộ trình chính thức.
*   📊 **Quản lý Câu hỏi Khảo sát (Quiz Management)**:
    *   Tạo lập ngân hàng câu hỏi khảo sát hướng nghiệp và phân chia theo các nhóm ngành Game/IT.

---

## 💻 Cấu Hình Và Chạy Cục Bộ (Local Installation)

### 📌 Yêu Cầu Hệ Thống
*   [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) hoặc phiên bản mới hơn.
*   Tài khoản [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) (hoặc MongoDB cài đặt cục bộ).
*   Tài khoản [Cloudinary](https://cloudinary.com/) (để lưu trữ ảnh).
*   Tài khoản Gmail cấp [App Password](https://support.google.com/accounts/answer/185833) để gửi email.

### 🏃 Chạy Ứng Dụng

1.  **Clone dự án về máy:**
    ```bash
    git clone https://github.com/MinQuan-kun/BackendRoadMap.git
    cd BackendRoadMap/BackendService
    ```

2.  **Cấu hình tệp `appsettings.json`:**
    Mở tệp [appsettings.json](file:///d:/BT/năm%203%20k%C3%AC%202/Thực%20tập%201/BackendRoadMap-review2/BackendService/appsettings.json) và cập nhật các thông số bảo mật của bạn:
    ```json
    {
      "ConnectionStrings": {
        "GameDevDB": "your_mongodb_connection_string"
      },
      "Jwt": {
        "Key": "your_strong_jwt_secret_key"
      },
      "Cloudinary": {
        "CloudName": "your_cloud_name",
        "ApiKey": "your_api_key",
        "ApiSecret": "your_api_secret"
      },
      "EmailOptions": {
        "Sender": {
          "Name": "GameDev Support",
          "Email": "your_email@gmail.com"
        },
        "Credential": {
          "SmtpServer": "smtp.gmail.com",
          "Port": 587,
          "Username": "your_email@gmail.com",
          "Password": "your_app_password"
        }
      }
    }
    ```

3.  **Restore các thư viện và chạy ứng dụng:**
    ```bash
    dotnet restore
    dotnet run
    ```
    *Mặc định backend sẽ chạy tại địa chỉ:* `http://localhost:7111` hoặc `https://localhost:7070`

---

## 🐳 Triển Khai Với Docker (Docker Deployment)

Nếu bạn muốn build và chạy ứng dụng thông qua Docker Container cục bộ hoặc deploy lên VPS/Render:

1.  **Build Docker Image:**
    ```bash
    docker build -t backend-roadmap .
    ```

2.  **Chạy Docker Container:**
    ```bash
    docker run -d -p 8080:8080 --name gamedev-backend backend-roadmap
    ```

Ứng dụng sẽ được khởi tạo tại cổng `8080`.

---

## 👥 Nhóm Tác Giả & Bản Quyền
Dự án được xây dựng và phát triển trong khuôn khổ chương trình **Thực tập tốt nghiệp 1** bởi nhóm tác giả tại **MinQuan-kun/BackendRoadMap**.

Nếu bạn thấy dự án hữu ích, hãy tặng cho chúng tôi **1 ⭐ Star** tại repository này nhé! Chúc các bạn học tập và lập trình vui vẻ! 🚀
