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
- Apache JMeter
- .NET 6 SDK

## 4. Ket qua test

### 4.1 Pass/Fail
- UI Test: 10 pass / 0 fail
- Logic Test: 6 pass / 0 fail
- Performance Test: Da thuc thi thanh cong script JMeter 500-1000 users

### 4.2 Bug summary
- So bug tong: 4
- High: 0
- Medium: 2
- Low: 2

## 5. Danh gia he thong
- Muc do on dinh: On dinh tot o muc chuc nang UI va logic trong lan chay hien tai (16/16 pass).
- Diem yeu chinh:
  - Ket qua stress test cho thay endpoint VnExpress co error rate cao (~48% o 500-1000 users).
  - Selector UI phu thuoc layout website cong khai, co rui ro flaky khi giao dien thay doi.

## 7. Ket qua performance bo sung
- 500 users: Throughput 45.05 req/s, Error rate 47.63%, p95 10007 ms.
- 1000 users: Throughput 61.30 req/s, Error rate 48.32%, p95 10012 ms.

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
- Ket qua test NUnit (TRX): `submission/01-source-code/test-results/submission-nunit-results.trx`
