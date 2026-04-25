# 04 - Performance Report (JMeter)

## Muc tieu test
- Stress test trang chu:
  - https://vnexpress.net/
  - https://zingnews.vn/
- Muc tai: 500 users va 1000 users.

## Cau hinh test
- Tool: Apache JMeter 5.6+
- Script: `jmeter/NewsSites-Stress-500-1000.jmx`
- Thread Group 1: 500 users, ramp-up 120s, loop 10
- Thread Group 2: 1000 users, ramp-up 180s, loop 8
- Timeout: connect 10s, response 15s

## Lenh chay
```bash
jmeter -n -t jmeter/NewsSites-Stress-500-1000.jmx -l jmeter/results/all-results-latest.jtl -e -o jmeter/results/html-report-latest
```

## Trang thai thuc thi hien tai
- Da chay thanh cong ngay 2026-04-16 bang JMeter 5.6.3.
- File ket qua tong: `jmeter/results/all-results-latest.jtl`
- File ket qua theo muc tai:
  - `jmeter/results/stress-500-users.jtl`
  - `jmeter/results/stress-1000-users.jtl`
- Dashboard HTML: `jmeter/results/html-report-latest/index.html`

## Screenshot bat buoc
- Summary Report: `submission/04-performance-report/screenshots/summary-report.png`
- Graph Results (Response Time): `submission/04-performance-report/screenshots/graph-response-times.png`
- Graph Results (Throughput): `submission/04-performance-report/screenshots/graph-throughput.png`

## Phan tich ket qua

### 1) Response Time
- Tong (tat ca sample):
  - Average: 17635.48 ms
  - Median: 10003 ms
  - p90: 38915 ms
  - p95: 51521 ms
  - p99: 74960 ms
  - Max: 150368 ms
- Transaction VnExpress:
  - GET VnExpress Home: avg 12662.21 ms, error 89.18%
  - GET VnExpress Home - 1000: avg 12252.82 ms, error 91.19%

Nhan xet: Response time vuot xa nguong timeout o nhieu sample, dac biet voi VnExpress trong tai cao.

### 2) Throughput
- Tong throughput: 96.90 req/s
- Theo transaction chinh:
  - GET VnExpress Home: 9.68 req/s
  - GET VnExpress Home - 1000: 15.98 req/s
  - GET Zing News Home: 9.70 req/s
  - GET Zing News Home - 1000: 15.91 req/s

Nhan xet: Throughput dat muc cao, nhung chat luong phan hoi khong on dinh do error rate lon.

### 3) Error Rate
- Tong Error %: 34.08% (17058/50048 samples)
- Error % theo transaction:
  - GET VnExpress Home: 89.18%
  - GET VnExpress Home - 1000: 91.19%
  - GET Zing News Home: 21.28%
  - GET Zing News Home - 1000: 25.95%

Nhan xet: Loi tap trung rat cao o transaction VnExpress, la diem nghen chinh cua he thong duoc test.

## Ket luan performance
- O tai 500-1000 users, he thong khong on dinh cho endpoint VnExpress (error gan 90%+).
- Zing News co ket qua tot hon nhung van co error rate 21-26% trong stress test.
- De xuat toi uu:
  1. Tang kha nang chiu tai backend/cache cho trang chu.
  2. Toi uu CDN va static asset loading.
  3. Tach bo test redirect/embedded request khi so sanh cong bang giua 2 website.
