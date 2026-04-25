# 05 - Bao Cao Tong Ket

## 1. Gioi thieu he thong test
- Website test:
  - VnExpress: https://vnexpress.net/
  - Zing News: https://zingnews.vn/
- Muc tieu: kiem thu chuc nang tim kiem, loc chuyen muc, doc bai viet va danh gia kha nang chiu tai.

## 2. Pham vi kiem thu
- UI Test (Selenium): search bai viet, filter chuyen muc, click doc bai.
- Logic Test (NUnit): validate keyword rong, keyword dai, validate lien quan ket qua.
- Performance Test (JMeter): stress homepage tai 500-1000 users.

## 3. Cong cu su dung
- Selenium WebDriver (C#)
- NUnit
- Playwright
- Apache JMeter
- .NET 6 SDK

## 4. Ket qua test

### 4.1 Pass/Fail
- UI Test (Selenium): 10 pass / 0 fail
- Logic Test: 6 pass / 0 fail
- Bug Scan (Playwright): 0 pass / 2 fail
- Performance Test (JMeter): Da thuc thi thanh cong script stress 500-1000 users

### 4.2 Bug summary
- So bug tong: 3
- High: 1
- Medium: 2
- Low: 0

## 5. Danh gia he thong
- Muc do on dinh: On dinh o muc Selenium + logic trong lan chay hien tai (16/16 pass).
- Diem yeu chinh:
  - Playwright bug scan phat hien console errors tren VnExpress va broken images/tracking responses tren Zing.
  - Ket qua stress test cho thay endpoint VnExpress co error rate rat cao (89-91%).
  - EdgeDriver 146 dang canh bao chua verify voi Edge 147 (rui ro on dinh moi truong).

## 7. Ket qua performance bo sung
- Total: Throughput 96.90 req/s, Error rate 34.08%, p95 51521 ms.
- GET VnExpress Home: Error rate 89.18%.
- GET VnExpress Home - 1000: Error rate 91.19%.

## 6. De xuat cai tien
1. Toi uu co che cache va CDN cho trang chu.
2. Giam do phuc tap DOM de UI test on dinh hon.
3. Bo sung rate-limit va timeout handling cho cao tai.
4. Chuan hoa selector test (data-testid) de giam flaky.

## Phu luc
- Source code: `submission/01-source-code/`
- Test case: `submission/02-test-case-document/Test-Cases.md`
- Bug report: `submission/03-bug-report/Bug-Report.md`
- Performance report: `submission/04-performance-report/Performance-Report.md`
- Ket qua test NUnit (TRX): `submission/01-source-code/test-results/results-after-screenshot-fix.trx`
- Ket qua JMeter moi nhat: `submission/01-source-code/test-results/all-results-latest.jtl`
