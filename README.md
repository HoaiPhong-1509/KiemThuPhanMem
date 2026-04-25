# Selenium Test – VnExpress (Bài 4: Website Tin tức)

## Yêu cầu cài đặt

- .NET 6.0 SDK: https://dotnet.microsoft.com/download
- Google Chrome (phiên bản mới nhất)
- Visual Studio 2022 hoặc VS Code + C# extension

## Cài đặt & Chạy

### Chạy nhanh (không cần nhớ nhiều lệnh)

Chỉ cần 1 lệnh theo mục đích:

```bash
npm run selenium:normal
```

- Chạy full 10 test Selenium VnExpress để lấy kết quả chính thức nộp giảng viên.

```bash
npm run selenium:demo
```

- Chạy nhanh 3 case chính để demo: Search, Filter, Click đọc bài.

```bash
npm run selenium:fault
```

- Chạy chế độ inject lỗi có kiểm soát để chứng minh assert bắt lỗi đúng.

Sau khi chạy, xem:
- File TRX trong thư mục `TestResults`
- Screenshot từng bước trong thư mục `bin/Debug/net6.0/artifacts/selenium-steps`

### Bước 1: Restore packages
```bash
dotnet restore
```

### Bước 2: Build project
```bash
dotnet build
```

### Bước 3: Chạy tất cả test
```bash
dotnet test
```

### Bước 4: Chạy từng test cụ thể
```bash
dotnet test --filter "TC01"
dotnet test --filter "TC05_Filter_ChuyenMucThoiSu"
```

---

## Danh sách Test Cases

| ID | Tên Test | Mô tả |
|----|----------|-------|
| TC01 | TrangChu_TaiThanhCong | Kiểm tra trang chủ tải đúng, có logo |
| TC02 | TimKiem_TuKhoaHopLe | Tìm kiếm với từ khóa "bóng đá" |
| TC03 | TimKiem_KeywordRong | Tìm kiếm không nhập gì – không crash |
| TC04 | TimKiem_KeywordDai | Tìm kiếm chuỗi 200 ký tự – không crash |
| TC05 | Filter_ChuyenMucThoiSu | Click menu Thời sự, URL đúng |
| TC06 | Filter_ChuyenMucTheThao | Click menu Thể thao, URL đúng |
| TC07 | DocBai_ChiTietBaiViet | Click bài đầu tiên, đọc chi tiết |
| TC08 | PhanTrang_TrangTiepTheo | Chuyển sang trang 2 |
| TC09 | TimKiem_KetQuaDungChuDe | Tìm "COVID", có kết quả trả về |
| TC10 | TimKiem_KyTuDacBiet | Nhập script XSS – trang xử lý an toàn |

---

## Lưu ý

- Nếu Chrome bị update lên version mới, cập nhật package `Selenium.WebDriver.ChromeDriver` cho khớp version Chrome của máy.
- Nếu website VnExpress thay đổi giao diện, cần cập nhật các CSS Selector trong code.
- Để chạy không hiện trình duyệt (headless), bỏ comment dòng `options.AddArgument("--headless")` trong Setup().
