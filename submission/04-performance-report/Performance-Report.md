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
jmeter -n -t jmeter/NewsSites-Stress-500-1000.jmx -l jmeter/results/all-results.jtl -e -o jmeter/results/html-report
```

## Trang thai thuc thi hien tai
- Da chay thanh cong ngay 2026-04-15 bang JMeter 5.6.3.
- File ket qua tong: `jmeter/results/all-results.jtl`
- File ket qua theo muc tai:
  - `jmeter/results/stress-500-users.jtl`
  - `jmeter/results/stress-1000-users.jtl`
- Dashboard HTML: `jmeter/results/html-report/index.html`

## Screenshot bat buoc
- Summary Report: `submission/04-performance-report/screenshots/summary-report.png`
- Graph Results: `submission/04-performance-report/screenshots/graph-results.png`

## Phan tich ket qua

### 1) Response Time
- 500 users:
  - Average: 4474.35 ms
  - p90: 10002 ms
  - p95: 10007 ms
  - Max: 30705 ms
- 1000 users:
  - Average: 4689.40 ms
  - p90: 10007 ms
  - p95: 10012 ms
  - Max: 30749 ms

Nhan xet: Response time tang cao va sat nguong timeout 10s o ca 2 muc tai, cho thay he thong bi qua tai o mot so endpoint.

### 2) Throughput
- Throughput trung binh:
  - 500 users: 45.05 req/s
  - 1000 users: 61.30 req/s
- Throughput theo host (all-results):
  - vnexpress.net: 51.79 req/s
  - zingnews.vn: 50.00 req/s
  - znews.vn (redirect host): 100.00 req/s

Nhan xet: Throughput tang khi tang user, nhung hieu nang thuc te cua VnExpress khong on dinh do error/timed out cao.

### 3) Error Rate
- Error % tong:
  - 500 users: 47.63%
  - 1000 users: 48.32%
  - all-results (tong hop): 24.03% (do co mau redirect thanh cong tu Zing)
- Nhom loi chinh:
  - Loi timeout/qua thoi gian dap ung o nhom request `GET VnExpress Home` va `GET VnExpress Home - 1000`
  - Zing News khong ghi nhan loi trong lan chay nay (0%)

Nhan xet: VnExpress co ty le loi rat cao o muc stress 500-1000 users; Zing News on dinh hon trong cung dieu kien test.

## Ket luan performance
- Muc tai on dinh nhat: 500 users on dinh hon 1000 users nhung van co error rate cao.
- Gioi han he thong quan sat duoc: o 500-1000 users, endpoint VnExpress khong dap ung on dinh (error ~48%).
- De xuat toi uu:
  1. Tang kha nang chiu tai backend/cache cho trang chu.
  2. Toi uu CDN va static asset loading.
  3. Tach bo test redirect/embedded request khi so sanh cong bang giua 2 website.
