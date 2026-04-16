# 02 - Test Case Document

## UI Test (Selenium)

| ID | Mo ta | Input | Expected Result |
|---|---|---|---|
| UI-01 | Mo trang chu VnExpress | URL: https://vnexpress.net/ | Trang tai thanh cong, title hop le, co bai viet |
| UI-02 | Search bai viet VnExpress | Keyword: "bong da" | Co danh sach ket qua, khong crash |
| UI-03 | Filter theo chuyen muc | Click menu "Thoi su" | URL chua `thoi-su`, hien thi danh sach bai |
| UI-04 | Click doc bai chi tiet | Click bai dau tien | Mo trang chi tiet, co tieu de va noi dung |
| UI-05 | Search bai viet Zing News | URL: https://zingnews.vn/ + keyword "the gioi" | Tra ket qua tim kiem, khong loi trang |

## API/Logic Test (NUnit)

| ID | Mo ta | Input | Expected Result |
|---|---|---|---|
| LG-01 | Validate keyword rong | keyword = "" | Tra ve `SearchKeywordStatus.Empty` |
| LG-02 | Validate keyword qua dai | keyword dai > 120 ky tu | Tra ve `SearchKeywordStatus.TooLong` |
| LG-03 | Validate keyword hop le | keyword = "chung khoan" | Tra ve `SearchKeywordStatus.Valid` |
| LG-04 | Validate do lien quan ket qua | title co token keyword | `IsRelevantResult` tra ve `true` |
| LG-05 | Loc danh sach ket qua hop le | list title + keyword | `FilterRelevantResults` tra ve danh sach da loc dung |

## Performance Test (JMeter)

| ID | Mo ta | Input | Expected Result |
|---|---|---|---|
| PF-01 | Stress homepage 500 users | 500 threads, ramp-up 120s, loop 10 | Error rate duy tri thap, response time trong nguong chap nhan |
| PF-02 | Stress homepage 1000 users | 1000 threads, ramp-up 180s, loop 8 | He thong van phan hoi, throughput tang, ghi nhan bottleneck |

## Tieu chi danh gia pass/fail
- UI: khong xuat hien exception Selenium, assert pass.
- Logic: tat ca test case NUnit pass.
- Performance: khong co loi hang loat (error burst), p95 response time on dinh theo nguong nhom dat ra.
