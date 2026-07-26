# Android Debloater Studio

**Android Debloater Studio** là một ứng dụng giao diện đồ họa (GUI) hiện đại được phát triển bằng WPF (.NET 10), giúp người dùng dễ dàng quản lý, gỡ bỏ, vô hiệu hóa hoặc khôi phục các ứng dụng rác (bloatware) được cài đặt sẵn trên các thiết bị Android thông qua ADB.

---

## Tính năng chính (Features)

- **Giao diện hiện đại (Modern UI)**: Giao diện trực quan với dạng danh sách (ListView) dạng thẻ. Các nút hành động được đính kèm trực tiếp vào từng thẻ phần mềm.
- **Tự động nhận diện trạng thái**: Ứng dụng quét toàn diện thiết bị để đánh dấu chính xác trạng thái thực tế của từng gói (Màu xanh lá: `Installed`, Màu cam: `Disabled`, Màu đỏ: `Uninstalled`).
- **Phân loại rủi ro thông minh**: 
  - Các gói phần mềm được phân cấp mức độ nguy hiểm: `Safe`, `Advanced`, `Expert`, `Unsafe`.
  - Hiển thị hộp thoại cảnh báo an toàn (Safety Warning) khi người dùng cố gắng can thiệp vào các gói thuộc cấp độ `Expert` hoặc `Unsafe` để tránh gây lỗi hệ thống (Bootloop).
- **Bộ lọc mạnh mẽ**: Tìm kiếm phần mềm theo từ khóa, lọc theo Hãng sản xuất (OEM) hoặc theo Mức độ rủi ro (Risk Level).
- **Tích hợp ADB tự động**: 
  - Tự động kiểm tra môi trường chạy `adb`. Nếu không tìm thấy, hệ thống sẽ gợi ý tải xuống công cụ *Android SDK Platform-Tools*.
  - Hỗ trợ đóng gói chung bộ công cụ ADB trong thư mục `Assets/platform-tools` giúp phần mềm có thể chạy Portable (Không cần cài đặt rườm rà).
- **Console Log Trực tiếp**: Khung console tích hợp sẵn giúp theo dõi trực tiếp kết quả các câu lệnh ADB trả về, và tự động cuộn xuống dòng mới nhất.

---

## Yêu cầu hệ thống (Requirements)

- **Hệ điều hành**: Windows 10 / 11.
- **Môi trường chạy**: [.NET 10 SDK](https://dotnet.microsoft.com/) (để build mã nguồn).
- **Thiết bị Android**: Đã kích hoạt chế độ **Gỡ lỗi USB (USB Debugging)** trong Tùy chọn nhà phát triển (Developer Options).
- **Công cụ dòng lệnh**: [Android SDK Platform-Tools](https://developer.android.com/tools/releases/platform-tools).

---

## Hướng dẫn cài đặt và sử dụng (Getting Started)

### 1. Chuẩn bị thư mục Platform-Tools (Tùy chọn nhưng Khuyến nghị)
Phần mềm cần `adb` để giao tiếp với điện thoại. Có 2 cách để thiết lập:
- **Cách 1 (Portable)**: Tải bộ công cụ [Platform-Tools](https://developer.android.com/tools/releases/platform-tools), giải nén và chép các file (`adb.exe`, `AdbWinApi.dll`, v.v.) vào thư mục:
  `Assets/platform-tools/`
- **Cách 2 (System PATH)**: Cài đặt ADB và khai báo biến môi trường (Environment Variable) `PATH` trên Windows.

### 2. Biên dịch (Build) và Chạy (Run)
Mở Terminal/Command Prompt tại thư mục dự án và chạy các lệnh sau:

```bash
# Build dự án
dotnet build

# Chạy ứng dụng
dotnet run
```

### 3. Kết nối thiết bị
1. Cắm cáp kết nối điện thoại Android vào máy tính.
2. Cho phép (Allow) quyền USB Debugging khi trên màn hình điện thoại xuất hiện thông báo cấp quyền.
3. Phần mềm sẽ tự động nhận diện thiết bị và tải lên danh sách ứng dụng.

---

## Cấu trúc dữ liệu (Data Source)
Ứng dụng sử dụng cơ sở dữ liệu package được lưu tại `Assets/uad_lists.json`. File này quy định định danh gói (`id`), mô tả (`description`), hãng (`list`), các mức độ an toàn (`removal`), và các phụ thuộc (`dependencies`).

---

## Cảnh báo trách nhiệm (Disclaimer)
Việc gỡ bỏ các ứng dụng hệ thống luốn ẩn chứa rủi ro. 
- Hãy cẩn trọng khi gỡ cài đặt các thành phần cốt lõi của Android hoặc các dịch vụ từ nhà sản xuất (nhóm Expert / Unsafe). 
- Tác giả không chịu trách nhiệm cho các vấn đề hỏng hóc thiết bị, lỗi phần mềm, mất mát dữ liệu hoặc bootloop phát sinh trong quá trình sử dụng phần mềm.
