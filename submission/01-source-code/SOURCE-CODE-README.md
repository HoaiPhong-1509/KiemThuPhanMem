# 01 - Source Code

## Thanh phan nop
- Selenium + NUnit UI tests: `VnExpressTests.cs`
- NUnit logic tests: `SearchLogicTests.cs`
- Search business logic: `SearchLogic.cs`
- Project file: `SeleniumVnExpress.csproj`
- JMeter script: `jmeter/NewsSites-Stress-500-1000.jmx`

## Cau hinh moi truong
- .NET SDK 6.0+
- Microsoft Edge (WebDriver tu package)
- Apache JMeter 5.6+

## Lenh chay Selenium + NUnit
```bash
dotnet restore
dotnet build
dotnet test
```

## Lenh chay rieng NUnit logic tests
```bash
dotnet test --filter "FullyQualifiedName~SearchLogicTests"
```

## Lenh chay JMeter (non-GUI)
```bash
jmeter -n -t jmeter/NewsSites-Stress-500-1000.jmx -l jmeter/results/all-results.jtl -e -o jmeter/results/html-report
```

## Ghi chu
- Script JMeter co 2 Thread Group: 500 users va 1000 users.
- Ket qua summary theo tung muc tai duoc ghi vao `jmeter/results/`.
- Artifact JMeter sau khi chay:
	- `jmeter/results/all-results.jtl`
	- `jmeter/results/stress-500-users.jtl`
	- `jmeter/results/stress-1000-users.jtl`
	- `jmeter/results/html-report/index.html`
- Ket qua NUnit da duoc luu tai `submission/01-source-code/test-results/submission-nunit-results.trx`.
- Ket qua chay thuc te hien tai: 16/16 test pass.
