# 03 - Bug Report

## Tong quan
Bao cao nay ghi nhan cac van de tim thay trong qua trinh test UI, logic va performance.

## Danh sach bug

| Bug ID | Mo ta loi | Steps to Reproduce | Severity | Trang thai |
|---|---|---|---|---|
| BUG-01 | O tim kiem co the tra ve ket qua khong lien quan cao khi keyword qua ngan (1 ky tu) | 1. Mo trang tim kiem 2. Nhap keyword 1 ky tu 3. Quan sat ket qua | Low | Open |
| BUG-02 | O muc tai cao (1000 users), mot so request co the timeout | 1. Chay TG-1000-Users 2. Xem Summary Report 3. Kiem tra Error % va timeout message | Medium | Open |
| BUG-03 | Mot so selector menu co the thay doi theo giao dien moi, gay flaky test | 1. Chay UI filter chuyen muc 2. Website doi layout 3. Test fail do khong tim thay element | Medium | Open |
| BUG-04 | EdgeDriver 146 canh bao chua verify voi Edge 147 (rui ro flaky khi nang cap trinh duyet) | 1. Chay `dotnet test` 2. Quan sat log WebDriver warning 3. Kiem tra version Edge tren may | Low | Open |

## Minh chung screenshot
- Dat screenshot tai thu muc: `submission/03-bug-report/screenshots/`
- Quy uoc ten file:
  - `BUG-01.png`
  - `BUG-02.png`
  - `BUG-03.png`
  - `BUG-04.png`
