param(
    [ValidateSet("normal", "demo", "fault")]
    [string]$Mode = "normal"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$project = ".\SeleniumVnExpress.csproj"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

function Run-Test {
    param(
        [string]$Filter,
        [string]$LogFile
    )

    Write-Host "[INFO] Running mode: $Mode" -ForegroundColor Cyan
    Write-Host "[INFO] Filter: $Filter" -ForegroundColor Cyan

    dotnet test $project --filter "$Filter" --logger "trx;LogFileName=$LogFile"
}

switch ($Mode) {
    "normal" {
        $logFile = "results-vnexpress-normal-$timestamp.trx"
        $filter = "FullyQualifiedName~VnExpressSeleniumTests.VnExpressTests"
        Run-Test -Filter $filter -LogFile $logFile
    }
    "demo" {
        $logFile = "results-vnexpress-demo-$timestamp.trx"
        $filter = "FullyQualifiedName~TC02_TimKiem_TuKhoaHopLe|FullyQualifiedName~TC05_Filter_ChuyenMucThoiSu|FullyQualifiedName~TC07_DocBai_ChiTietBaiViet"
        Run-Test -Filter $filter -LogFile $logFile
    }
    "fault" {
        $logFile = "results-vnexpress-fault-$timestamp.trx"
        $filter = "FullyQualifiedName~TC02_TimKiem_TuKhoaHopLe"
        $env:SELENIUM_INJECT_FAULT = "1"

        try {
            Run-Test -Filter $filter -LogFile $logFile
        }
        finally {
            Remove-Item Env:SELENIUM_INJECT_FAULT -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "" 
Write-Host "[DONE] TRX saved under .\TestResults\$logFile" -ForegroundColor Green
Write-Host "[DONE] Step screenshots in .\bin\Debug\net6.0\artifacts\selenium-steps" -ForegroundColor Green
