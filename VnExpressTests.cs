using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VnExpressSeleniumTests
{
    /// <summary>
    /// Bài 4: Kiểm thử Website Tin tức - VnExpress
    /// Công cụ: Selenium WebDriver + NUnit (C#)
    /// Website: https://vnexpress.net
    /// </summary>
    [TestFixture]
    public class VnExpressTests
    {
        private IWebDriver? driver;
        private WebDriverWait? wait;
        private bool faultInjectionEnabled;
        private string stepScreenshotDir = string.Empty;
        private int stepScreenshotIndex;

        private static readonly By SearchToggleSelector = By.CssSelector("a.btn-search, .ic-search, [data-role='search-toggle']");
        private static readonly By SearchInputSelector = By.CssSelector("input[type='search'], input[name='q'], .search-input");
        private static readonly By SearchResultCardsSelector = By.CssSelector("article, .item-news, .list-news-subfolder h3");
        private static readonly By SearchResultLinksSelector = By.CssSelector("h3.title-news a, article h2 a, .item-news h3 a");
        private static readonly By HomeLogoSelector = By.CssSelector("a.logo, .logo-vne, header a[href='/']");
        private static readonly By ArticleTitleSelector = By.CssSelector("h1.title-detail, h1.heading-title, article h1, h1");
        private const string BASE_URL = "https://vnexpress.net";

        private IWebDriver Driver => driver ?? throw new InvalidOperationException("WebDriver has not been initialized.");
        private WebDriverWait Wait => wait ?? throw new InvalidOperationException("WebDriverWait has not been initialized.");

        // =============================================
        // SETUP & TEARDOWN
        // =============================================

        [SetUp]
        public void Setup()
        {
            faultInjectionEnabled = IsFaultInjectionEnabled();
            stepScreenshotIndex = 0;
            stepScreenshotDir = CreateStepScreenshotDirectory();

            // Khởi tạo EdgeDriver với các tùy chọn
            EdgeOptions options = new EdgeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            // Bỏ comment dòng dưới nếu muốn chạy headless (không hiển thị trình duyệt)
            // options.AddArgument("--headless");

            driver = new EdgeDriver(options);
            wait = CreateWait(15);
            LogInfo($"Test bắt đầu. FaultInjection={(faultInjectionEnabled ? "ON" : "OFF")}");

            NavigateTo(BASE_URL, "00_home_loaded");
        }

        [TearDown]
        public void TearDown()
        {
            // Nếu test fail thì chụp ảnh bug để làm bằng chứng
            if (driver != null && TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                try
                {
                    if (driver is ITakesScreenshot screenshotDriver)
                    {
                        var screenshot = screenshotDriver.GetScreenshot();
                        var safeTestName = string.Join("_", TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));
                        var outputDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts", "selenium-failures");
                        Directory.CreateDirectory(outputDir);
                        var screenshotPath = Path.Combine(outputDir, $"{safeTestName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                        screenshot.SaveAsFile(screenshotPath);
                        TestContext.WriteLine($"Screenshot saved: {screenshotPath}");
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Cannot save screenshot: {ex.Message}");
                }
            }

            // Đóng trình duyệt sau mỗi test
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }

        private static bool IsFaultInjectionEnabled()
        {
            string? env = Environment.GetEnvironmentVariable("SELENIUM_INJECT_FAULT");
            return string.Equals(env, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(env, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(env, "on", StringComparison.OrdinalIgnoreCase);
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

        private string CreateStepScreenshotDirectory()
        {
            string safeTestName = SanitizeFileName(TestContext.CurrentContext.Test.Name);
            string folder = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "artifacts",
                "selenium-steps",
                $"{safeTestName}_{DateTime.Now:yyyyMMdd_HHmmss}");

            Directory.CreateDirectory(folder);
            return folder;
        }

        private void LogInfo(string message)
        {
            TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss}] [INFO] {message}");
        }

        private void LogWarn(string message)
        {
            TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss}] [WARN] {message}");
        }

        private void CaptureStep(string stepName)
        {
            if (Driver is not ITakesScreenshot screenshotDriver)
            {
                return;
            }

            stepScreenshotIndex++;
            string fileName = $"{stepScreenshotIndex:D2}_{SanitizeFileName(stepName)}.png";
            string fullPath = Path.Combine(stepScreenshotDir, fileName);

            screenshotDriver.GetScreenshot().SaveAsFile(fullPath);
            LogInfo($"Step screenshot saved: {fullPath}");
        }

        private WebDriverWait CreateWait(int timeoutSeconds)
        {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
            {
                PollingInterval = TimeSpan.FromMilliseconds(250)
            };
        }

        private By ResolveSelector(string selectorName, By selector)
        {
            if (!faultInjectionEnabled)
            {
                return selector;
            }

            // Chủ động inject lỗi selector để xác nhận assert thất bại đúng kỳ vọng.
            if (selectorName.Equals("search-input", StringComparison.OrdinalIgnoreCase)
                || selectorName.Equals("article-title", StringComparison.OrdinalIgnoreCase))
            {
                LogWarn($"Fault injection: selector '{selectorName}' bị đổi sang selector không tồn tại.");
                return By.CssSelector("[data-fault-injected='missing-selector']");
            }

            return selector;
        }

        private void WaitForDocumentReady(string context, int timeoutSeconds = 15)
        {
            LogInfo($"Wait document ready: {context}");
            var localWait = CreateWait(timeoutSeconds);
            bool ready = localWait.Until(drv =>
            {
                try
                {
                    var readyState = ((IJavaScriptExecutor)drv).ExecuteScript("return document.readyState")?.ToString();
                    return string.Equals(readyState, "complete", StringComparison.OrdinalIgnoreCase);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                catch (WebDriverException)
                {
                    return false;
                }
            });

            Assert.That(ready, Is.True, $"Document không về trạng thái complete tại bước: {context}");
        }

        private void NavigateTo(string url, string screenshotStep)
        {
            LogInfo($"Navigate: {url}");
            Driver.Navigate().GoToUrl(url);
            WaitForDocumentReady($"navigate to {url}");
            CaptureStep(screenshotStep);
        }

        private IWebElement WaitForVisible(By selector, string selectorName, int timeoutSeconds = 15)
        {
            By resolved = ResolveSelector(selectorName, selector);
            LogInfo($"Wait visible [{selectorName}] => {resolved}");

            var localWait = CreateWait(timeoutSeconds);
            IWebElement? found = localWait.Until(drv =>
            {
                try
                {
                    return drv.FindElements(resolved).FirstOrDefault(e => e.Displayed);
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            return found ?? throw new WebDriverTimeoutException($"Không thấy element visible cho selector: {selectorName}");
        }

        private IWebElement WaitForClickable(By selector, string selectorName, int timeoutSeconds = 15)
        {
            By resolved = ResolveSelector(selectorName, selector);
            LogInfo($"Wait clickable [{selectorName}] => {resolved}");

            var localWait = CreateWait(timeoutSeconds);
            IWebElement? found = localWait.Until(drv =>
            {
                try
                {
                    return drv.FindElements(resolved).FirstOrDefault(e => e.Displayed && e.Enabled);
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            return found ?? throw new WebDriverTimeoutException($"Không thấy element clickable cho selector: {selectorName}");
        }

        private IReadOnlyCollection<IWebElement> WaitForElements(By selector, string selectorName, int minCount = 1, int timeoutSeconds = 15)
        {
            By resolved = ResolveSelector(selectorName, selector);
            LogInfo($"Wait elements [{selectorName}] => {resolved}, minCount={minCount}");

            var localWait = CreateWait(timeoutSeconds);
            IReadOnlyCollection<IWebElement>? elements = localWait.Until(drv =>
            {
                try
                {
                    var found = drv.FindElements(resolved);
                    return found.Count >= minCount ? found : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            return elements ?? Array.Empty<IWebElement>();
        }

        private void WaitForUrlContains(string expectedPart, string context, int timeoutSeconds = 15)
        {
            LogInfo($"Wait URL contains '{expectedPart}' ({context})");
            var localWait = CreateWait(timeoutSeconds);
            bool matched = localWait.Until(drv => drv.Url.Contains(expectedPart, StringComparison.OrdinalIgnoreCase));

            Assert.That(matched, Is.True, $"URL không chứa '{expectedPart}' tại bước: {context}. URL hiện tại: {Driver.Url}");
        }

        private void WaitForUrlToChange(string previousUrl, string context, int timeoutSeconds = 15)
        {
            LogInfo($"Wait URL change ({context})");
            var localWait = CreateWait(timeoutSeconds);
            bool changed = localWait.Until(drv => !string.Equals(drv.Url, previousUrl, StringComparison.OrdinalIgnoreCase));

            Assert.That(changed, Is.True, $"URL không đổi tại bước: {context}. URL: {Driver.Url}");
        }

        private void AssertSelectorExists(string selectorName, By selector, int timeoutSeconds = 8)
        {
            By resolved = ResolveSelector(selectorName, selector);
            LogInfo($"Validate selector exists [{selectorName}] => {resolved}");

            var localWait = CreateWait(timeoutSeconds);
            bool exists = localWait.Until(drv =>
            {
                try
                {
                    return drv.FindElements(resolved).Any();
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });

            Assert.That(exists, Is.True, $"Selector không tồn tại: {selectorName} => {resolved}");
        }

        private IWebElement OpenSearchAndGetInput(string keywordPreview)
        {
            LogInfo($"Mở search box cho keyword: {keywordPreview}");
            AssertSelectorExists("search-toggle", SearchToggleSelector);

            IWebElement searchIcon = WaitForClickable(SearchToggleSelector, "search-toggle");
            searchIcon.Click();
            CaptureStep("search_box_opened");

            AssertSelectorExists("search-input", SearchInputSelector);
            return WaitForVisible(SearchInputSelector, "search-input");
        }

        private IWebElement WaitForFirstNonEmptyElement(IEnumerable<By> selectors, string selectorGroupName, int timeoutSeconds = 12)
        {
            LogInfo($"Wait first non-empty element from selector group: {selectorGroupName}");
            var localWait = CreateWait(timeoutSeconds);

            IWebElement? found = localWait.Until(drv =>
            {
                foreach (var selector in selectors)
                {
                    var nodes = drv.FindElements(selector);
                    foreach (var node in nodes)
                    {
                        string text = (node.Text ?? string.Empty).Trim();
                        if (!string.IsNullOrWhiteSpace(text) && node.Displayed)
                        {
                            return node;
                        }
                    }
                }

                return null;
            });

            return found ?? throw new WebDriverTimeoutException($"Không tìm thấy phần tử có text cho nhóm selector: {selectorGroupName}");
        }

        // =============================================
        // TC01 - KIỂM TRA TRANG CHỦ TẢI THÀNH CÔNG
        // =============================================

        /// <summary>
        /// TC01: Kiểm tra trang chủ VnExpress tải thành công
        /// Input: Truy cập https://vnexpress.net
        /// Expected: Title chứa "VnExpress", logo hiển thị
        /// </summary>
        [Test]
        public void TC01_TrangChu_TaiThanhCong()
        {
            LogInfo("TC01 - Bắt đầu kiểm tra trang chủ");

            // Assert title trang
            Assert.That(Driver.Title, Does.Contain("VnExpress"),
                "Title trang chủ phải chứa 'VnExpress'");

            // Assert URL đúng
            Assert.That(Driver.Url, Does.Contain("vnexpress.net"),
                "URL phải chứa 'vnexpress.net'");

            AssertSelectorExists("home-logo", HomeLogoSelector);
            IWebElement logo = WaitForVisible(HomeLogoSelector, "home-logo");
            Assert.That(logo.Displayed, Is.True, "Logo VnExpress phải hiển thị");

            CaptureStep("tc01_home_verified");
            LogInfo($"[TC01 PASS] Trang chủ tải thành công. Title: {Driver.Title}");
        }

        // =============================================
        // TC02 - TÌM KIẾM BÀI VIẾT VỚI TỪ KHÓA HỢP LỆ
        // =============================================

        /// <summary>
        /// TC02: Tìm kiếm bài viết với từ khóa hợp lệ
        /// Input: Từ khóa "bóng đá"
        /// Expected: Kết quả tìm kiếm hiển thị, có bài viết liên quan
        /// </summary>
        [Test]
        public void TC02_TimKiem_TuKhoaHopLe()
        {
            LogInfo("TC02 - Bắt đầu tìm kiếm từ khóa hợp lệ");
            string tuKhoa = "bóng đá";

            IWebElement searchInput = OpenSearchAndGetInput(tuKhoa);
            searchInput.Clear();
            searchInput.SendKeys(tuKhoa);
            CaptureStep("tc02_keyword_typed");
            searchInput.SendKeys(Keys.Enter);

            WaitForDocumentReady("TC02 search submit");
            WaitForElements(SearchResultCardsSelector, "search-result-cards", 1, 15);

            var results = Driver.FindElements(SearchResultCardsSelector);
            Assert.That(results.Count, Is.GreaterThan(0),
                $"Phải có ít nhất 1 kết quả tìm kiếm cho từ khóa '{tuKhoa}'");

            var titles = Driver.FindElements(SearchResultLinksSelector)
                .Select(x => (x.Text ?? string.Empty).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            Assert.That(titles.Count, Is.GreaterThan(0), "Kết quả tìm kiếm phải có tiêu đề không rỗng");
            int relevantCount = titles.Count(title => SearchLogic.IsRelevantResult(title, tuKhoa));
            Assert.That(relevantCount, Is.GreaterThan(0), "Phải có ít nhất 1 tiêu đề liên quan trực tiếp đến từ khóa tìm kiếm");

            CaptureStep("tc02_search_results");
            LogInfo($"[TC02 PASS] Tìm kiếm '{tuKhoa}' trả về {results.Count} kết quả, relevant={relevantCount}");
        }

        // =============================================
        // TC03 - TÌM KIẾM VỚI KEYWORD RỖNG
        // =============================================

        /// <summary>
        /// TC03: Tìm kiếm với keyword rỗng (không nhập gì)
        /// Input: Keyword = "" (chuỗi rỗng)
        /// Expected: Hệ thống không crash, không redirect sai, hoặc hiện thông báo lỗi
        /// </summary>
        [Test]
        public void TC03_TimKiem_KeywordRong()
        {
            LogInfo("TC03 - Tìm kiếm keyword rỗng");

            IWebElement searchInput = OpenSearchAndGetInput("<empty>");
            searchInput.Clear();
            CaptureStep("tc03_empty_keyword_ready");
            searchInput.SendKeys(Keys.Enter);

            WaitForDocumentReady("TC03 search empty");

            // Assert trang không bị lỗi 500 hay crash
            string pageSource = Driver.PageSource;
            Assert.That(pageSource, Does.Not.Contain("500 Internal Server Error"),
                "Trang không được báo lỗi 500 khi tìm kiếm keyword rỗng");
            Assert.That(pageSource, Does.Not.Contain("Error 404"),
                "Trang không được báo lỗi 404 khi tìm kiếm keyword rỗng");
            Assert.That(Driver.Url, Does.Contain("vnexpress.net"), "URL phải vẫn ở domain VnExpress");

            CaptureStep("tc03_empty_keyword_result");
            LogInfo($"[TC03 PASS] Tìm kiếm keyword rỗng không gây crash. URL: {Driver.Url}");
        }

        // =============================================
        // TC04 - TÌM KIẾM VỚI KEYWORD DÀI
        // =============================================

        /// <summary>
        /// TC04: Tìm kiếm với keyword rất dài (200+ ký tự)
        /// Input: Chuỗi 200 ký tự
        /// Expected: Hệ thống xử lý bình thường, không crash
        /// </summary>
        [Test]
        public void TC04_TimKiem_KeywordDai()
        {
            LogInfo("TC04 - Tìm kiếm keyword dài");
            // Tạo keyword dài 200 ký tự
            string keywordDai = new string('a', 200);

            IWebElement searchInput = OpenSearchAndGetInput("<200 chars>");
            searchInput.Clear();
            searchInput.SendKeys(keywordDai);
            CaptureStep("tc04_long_keyword_typed");
            searchInput.SendKeys(Keys.Enter);

            WaitForDocumentReady("TC04 search long keyword");

            // Assert trang không crash
            string pageTitle = Driver.Title;
            Assert.That(pageTitle, Is.Not.Null.And.Not.Empty,
                "Title trang không được rỗng sau khi tìm với keyword dài");

            string pageSource = Driver.PageSource;
            Assert.That(pageSource, Does.Not.Contain("500 Internal Server Error"),
                "Trang không được báo lỗi 500 với keyword dài");
            Assert.That(Driver.Url, Does.Contain("vnexpress.net"), "URL phải thuộc domain VnExpress");

            CaptureStep("tc04_long_keyword_result");
            LogInfo($"[TC04 PASS] Keyword dài 200 ký tự xử lý bình thường. URL: {Driver.Url}");
        }

        // =============================================
        // TC05 - FILTER THEO CHUYÊN MỤC: THỜI SỰ
        // =============================================

        /// <summary>
        /// TC05: Click vào chuyên mục "Thời sự" và kiểm tra lọc đúng
        /// Input: Click menu "Thời sự"
        /// Expected: Trang thời sự hiển thị, URL chứa "thoi-su"
        /// </summary>
        [Test]
        public void TC05_Filter_ChuyenMucThoiSu()
        {
            LogInfo("TC05 - Filter chuyên mục Thời sự");
            By menuThoiSuSelector = By.XPath("//a[contains(@href, 'thoi-su') and (contains(text(),'Thời sự') or contains(text(),'thời sự'))]");
            AssertSelectorExists("menu-thoi-su", menuThoiSuSelector);

            IWebElement menuThoiSu = WaitForClickable(menuThoiSuSelector, "menu-thoi-su");
            menuThoiSu.Click();
            WaitForUrlContains("thoi-su", "TC05 click menu Thời sự");
            WaitForDocumentReady("TC05 page loaded");

            // Assert URL chứa "thoi-su"
            Assert.That(Driver.Url, Does.Contain("thoi-su"),
                "URL phải chứa 'thoi-su' sau khi click chuyên mục Thời sự");

            var articles = WaitForElements(By.CssSelector("article, .item-news, h3.title-news"), "thoi-su-articles", 1, 15);
            Assert.That(articles.Count, Is.GreaterThan(0),
                "Chuyên mục Thời sự phải hiển thị ít nhất 1 bài viết");

            IWebElement heading = WaitForFirstNonEmptyElement(
                new[] { By.CssSelector("h1.title-page"), By.CssSelector("h1") },
                "thoi-su-heading");
            Assert.That((heading.Text ?? string.Empty).Trim().Length, Is.GreaterThan(3),
                "Heading chuyên mục Thời sự phải có nội dung rõ ràng");

            CaptureStep("tc05_thoi_su_loaded");
            LogInfo($"[TC05 PASS] Chuyên mục Thời sự hiển thị {articles.Count} bài. URL: {Driver.Url}");
        }

        // =============================================
        // TC06 - FILTER THEO CHUYÊN MỤC: THỂ THAO
        // =============================================

        /// <summary>
        /// TC06: Click vào chuyên mục "Thể thao" và kiểm tra lọc đúng
        /// Input: Click menu "Thể thao"
        /// Expected: Trang thể thao hiển thị, URL chứa "the-thao"
        /// </summary>
        [Test]
        public void TC06_Filter_ChuyenMucTheThao()
        {
            LogInfo("TC06 - Filter chuyên mục Thể thao");
            By menuTheThaoSelector = By.XPath("//a[contains(@href, 'the-thao') and (contains(text(),'Thể thao') or contains(text(),'thể thao'))]");
            AssertSelectorExists("menu-the-thao", menuTheThaoSelector);

            IWebElement menuTheThao = WaitForClickable(menuTheThaoSelector, "menu-the-thao");
            menuTheThao.Click();
            WaitForUrlContains("the-thao", "TC06 click menu Thể thao");
            WaitForDocumentReady("TC06 page loaded");

            // Assert URL chứa "the-thao"
            Assert.That(Driver.Url, Does.Contain("the-thao"),
                "URL phải chứa 'the-thao' sau khi click chuyên mục Thể thao");

            var articles = WaitForElements(By.CssSelector("article, .item-news, h3.title-news"), "the-thao-articles", 1, 15);
            Assert.That(articles.Count, Is.GreaterThan(0),
                "Chuyên mục Thể thao phải hiển thị ít nhất 1 bài viết");

            IWebElement heading = WaitForFirstNonEmptyElement(
                new[] { By.CssSelector("h1.title-page"), By.CssSelector("h1") },
                "the-thao-heading");
            Assert.That((heading.Text ?? string.Empty).Trim().Length, Is.GreaterThan(3),
                "Heading chuyên mục Thể thao phải có nội dung rõ ràng");

            CaptureStep("tc06_the_thao_loaded");
            LogInfo($"[TC06 PASS] Chuyên mục Thể thao hiển thị {articles.Count} bài. URL: {Driver.Url}");
        }

        // =============================================
        // TC07 - CLICK ĐỌC BÀI - MỞ CHI TIẾT BÀI VIẾT
        // =============================================

        /// <summary>
        /// TC07: Click vào bài viết đầu tiên và đọc nội dung
        /// Input: Click bài viết đầu tiên trên trang chủ
        /// Expected: Trang chi tiết bài viết tải thành công, có tiêu đề và nội dung
        /// </summary>
        [Test]
        public void TC07_DocBai_ChiTietBaiViet()
        {
            LogInfo("TC07 - Click đọc bài chi tiết");

            // Thu thập nhiều link bài viết từ trang chủ, loại trừ link của Shorts/VnEGO.
            var candidates = new List<(string Href, string Text)>();
            var seenHrefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AssertSelectorExists("home-article-links", By.CssSelector("h1.title-news a[href], h2.title-news a[href], h3.title-news a[href], article h2 a[href], article h3 a[href]"));
            var homepageLinks = WaitForElements(
                By.CssSelector("h1.title-news a[href], h2.title-news a[href], h3.title-news a[href], article h2 a[href], article h3 a[href]"),
                "home-article-links",
                3,
                15);

            foreach (var link in homepageLinks)
            {
                string href = (link.GetAttribute("href") ?? string.Empty).Trim();
                string text = (link.Text ?? string.Empty).Trim();
                string hrefLower = href.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(text)) continue;
                if (!link.Displayed) continue;
                if (!hrefLower.Contains("vnexpress.net")) continue;
                if (!hrefLower.Contains(".html")) continue;
                if (hrefLower.Contains("/video") || hrefLower.Contains("/podcast") || hrefLower.Contains("vnego") || hrefLower.Contains("/shorts")) continue;

                bool insideShortsOrVnEGo = (bool)((IJavaScriptExecutor)Driver).ExecuteScript(@"
                    const el = arguments[0];
                    return !!el.closest('[class*=""short""], [id*=""short""], [class*=""vnego""], [id*=""vnego""]');
                ", link);
                if (insideShortsOrVnEGo) continue;

                if (seenHrefs.Add(href))
                {
                    candidates.Add((href, text));
                }

                if (candidates.Count >= 8) break;
            }

            Assert.That(candidates.Count, Is.GreaterThan(0),
                "Không tìm được link bài viết hợp lệ trên trang chủ để thực hiện TC07");

            IWebElement? tieuDe = null;
            IWebElement? noiDung = null;
            string tenBaiDuocMo = string.Empty;
            bool daMoBaiThanhCong = false;

            foreach (var candidate in candidates)
            {
                NavigateTo(BASE_URL, "tc07_back_home");

                tenBaiDuocMo = candidate.Text;
                LogInfo($"Đang thử click bài: {tenBaiDuocMo}");
                LogInfo($"Link bài viết: {candidate.Href}");

                try
                {
                    IWebElement? linkCanClick = null;
                    var linksByHref = Driver.FindElements(By.CssSelector($"a[href='{candidate.Href}']"));
                    if (linksByHref.Count > 0)
                    {
                        linkCanClick = linksByHref[0];
                    }

                    if (linkCanClick != null && linkCanClick.Displayed)
                    {
                        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", linkCanClick);

                        try
                        {
                            linkCanClick.Click();
                        }
                        catch (ElementClickInterceptedException)
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", linkCanClick);
                        }
                    }
                    else
                    {
                        LogWarn("Không click được link trong DOM hiện tại, fallback navigate trực tiếp bằng href.");
                        NavigateTo(candidate.Href, "tc07_fallback_navigate");
                    }

                    var shortWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(8));
                    shortWait.Until(drv => drv.Url.Contains(".html"));

                    string currentUrl = Driver.Url.ToLowerInvariant();
                    if (currentUrl.Contains("vnego") || currentUrl.Contains("shorts"))
                    {
                        LogWarn("Bài hiện tại thuộc luồng Shorts/VnEGO, thử bài khác.");
                        continue;
                    }

                    AssertSelectorExists("article-title", ArticleTitleSelector);
                    tieuDe = shortWait.Until(drv =>
                    {
                        var titleNodes = drv.FindElements(ResolveSelector("article-title", ArticleTitleSelector));
                        foreach (var node in titleNodes)
                        {
                            string nodeText = (node.Text ?? string.Empty).Trim();
                            if (!string.IsNullOrWhiteSpace(nodeText))
                            {
                                return node;
                            }
                        }

                        return null;
                    });

                    noiDung = shortWait.Until(drv =>
                    {
                        string[] selectors =
                        {
                            "article p",
                            ".fck_detail p",
                            ".content-detail p",
                            "p.description",
                            ".description",
                            ".lead",
                            ".sidebar-1 p"
                        };

                        foreach (var selector in selectors)
                        {
                            var nodes = drv.FindElements(By.CssSelector(selector));
                            foreach (var node in nodes)
                            {
                                string text = (node.Text ?? string.Empty).Trim();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    return node;
                                }
                            }
                        }

                        return null;
                    });

                    if (tieuDe != null && noiDung != null)
                    {
                        Assert.That((tieuDe.Text ?? string.Empty).Trim().Length, Is.GreaterThan(8),
                            "Tiêu đề bài viết phải có độ dài đủ rõ ràng");
                        Assert.That((noiDung.Text ?? string.Empty).Trim().Length, Is.GreaterThan(20),
                            "Nội dung bài viết phải đủ dài để xác nhận đã vào trang chi tiết");
                        Assert.That(Driver.Url, Does.Contain(".html"), "URL bài chi tiết phải chứa .html");
                        Assert.That(Driver.Url.ToLowerInvariant(), Does.Not.Contain("vnego").And.Not.Contain("shorts"),
                            "Không được điều hướng sang luồng Shorts/VnEGO");

                        CaptureStep("tc07_article_detail_loaded");
                        daMoBaiThanhCong = true;
                        break;
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    LogWarn("Timeout khi mở bài hiện tại, thử bài kế tiếp.");
                }
                catch (NoSuchElementException)
                {
                    LogWarn("Thiếu phần tử cần thiết trên bài hiện tại, thử bài kế tiếp.");
                }
            }

            Assert.That(daMoBaiThanhCong, Is.True,
                "Không mở được bài chi tiết chuẩn sau khi thử nhiều link bài viết trên trang chủ");
            Assert.That(tieuDe, Is.Not.Null, "Tiêu đề bài viết phải hiển thị");
            Assert.That(noiDung, Is.Not.Null, "Nội dung/đoạn mô tả bài viết phải hiển thị và có chữ");

            string titleText = tieuDe!.Text ?? string.Empty;
            LogInfo($"[TC07 PASS] Mở bài thành công. Bài: {tenBaiDuocMo}");
            LogInfo($"[TC07 PASS] Tiêu đề: {titleText.Substring(0, Math.Min(60, titleText.Length))}...");
        }

        // =============================================
        // TC08 - KIỂM TRA PHÂN TRANG
        // =============================================

        /// <summary>
        /// TC08: Kiểm tra chức năng phân trang trên trang chuyên mục
        /// Input: Vào chuyên mục Thời sự, click trang 2
        /// Expected: Tải được trang 2, URL thay đổi, nội dung mới
        /// </summary>
        [Test]
        public void TC08_PhanTrang_TrangTiepTheo()
        {
            LogInfo("TC08 - Kiểm tra phân trang");
            // Vào chuyên mục Thời sự
            NavigateTo($"{BASE_URL}/thoi-su", "tc08_thoi_su_page1");

            string url_trang1 = Driver.Url;

            // Tìm nút sang trang 2
            By nextPageSelector = By.CssSelector("a.next-page, li.next a, .pagination a[rel='next'], a[title='Trang kế']");
            AssertSelectorExists("pagination-next", nextPageSelector);

            IWebElement nextPage = WaitForClickable(nextPageSelector, "pagination-next");
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", nextPage);

            try
            {
                nextPage.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", nextPage);
            }

            WaitForUrlToChange(url_trang1, "TC08 chuyển trang 2");
            WaitForDocumentReady("TC08 page2 loaded");

            // Assert URL thay đổi (sang trang 2)
            Assert.That(Driver.Url, Is.Not.EqualTo(url_trang1),
                "URL phải thay đổi khi chuyển sang trang 2");
            Assert.That(Driver.Url.ToLowerInvariant(), Does.Contain("p2").Or.Contain("page=2"),
                "URL trang sau cần thể hiện đã sang page 2");

            // Assert có bài viết trên trang 2
            var articles = WaitForElements(By.CssSelector("article, .item-news, h3.title-news"), "page2-articles", 1, 15);
            Assert.That(articles.Count, Is.GreaterThan(0),
                "Trang 2 phải hiển thị bài viết");

            CaptureStep("tc08_page2_loaded");
            LogInfo($"[TC08 PASS] Phân trang hoạt động. URL trang 2: {Driver.Url}");
        }

        // =============================================
        // TC09 - KIỂM TRA TÌM KIẾM TRẢ VỀ KẾT QUẢ ĐÚNG CHỦ ĐỀ
        // =============================================

        /// <summary>
        /// TC09: Kết quả tìm kiếm phải liên quan đến từ khóa
        /// Input: Tìm kiếm "bóng đá"
        /// Expected: Tiêu đề các bài viết trong kết quả có chứa từ liên quan
        /// </summary>
        [Test]
        public void TC09_TimKiem_KetQuaDungChuDe()
        {
            LogInfo("TC09 - Kiểm tra tính liên quan kết quả tìm kiếm");
            string tuKhoa = "bóng đá";

            IWebElement searchInput = OpenSearchAndGetInput(tuKhoa);
            searchInput.Clear();
            searchInput.SendKeys(tuKhoa);
            CaptureStep("tc09_keyword_typed");
            searchInput.SendKeys(Keys.Enter);
            WaitForDocumentReady("TC09 search submit");
            WaitForElements(SearchResultLinksSelector, "search-result-links", 1, 15);

            Assert.That(Driver.Url, Does.Contain("vnexpress").IgnoreCase,
                "Sau khi tìm kiếm phải ở context trang thuộc VnExpress");

            // Lấy danh sách tiêu đề bài viết
            var titles = Driver.FindElements(SearchResultLinksSelector)
                .Select(t => (t.Text ?? string.Empty).Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            Assert.That(titles.Count, Is.GreaterThan(0),
                $"Phải có kết quả tìm kiếm cho từ khóa '{tuKhoa}'");

            int relevantCount = titles.Count(title => SearchLogic.IsRelevantResult(title, tuKhoa));
            Assert.That(relevantCount, Is.GreaterThan(0),
                "Kết quả tìm kiếm phải có ít nhất 1 tiêu đề liên quan từ khóa");

            CaptureStep("tc09_search_results");
            LogInfo($"[TC09 PASS] Tìm kiếm '{tuKhoa}' trả về {titles.Count} kết quả, relevant={relevantCount}");
            foreach (var title in titles.Take(10))
            {
                LogInfo($"  - {title}");
            }
        }

        // =============================================
        // TC10 - KIỂM TRA TÌM KIẾM KÝ TỰ ĐẶC BIỆT
        // =============================================

        /// <summary>
        /// TC10: Tìm kiếm với ký tự đặc biệt (SQL Injection, XSS cơ bản)
        /// Input: Từ khóa "<script>alert(1)</script>"
        /// Expected: Trang không bị XSS, xử lý an toàn
        /// </summary>
        [Test]
        public void TC10_TimKiem_KyTuDacBiet()
        {
            LogInfo("TC10 - Kiểm tra tìm kiếm ký tự đặc biệt");
            string kyTuDacBiet = "<script>alert(1)</script>";

            IWebElement searchInput = OpenSearchAndGetInput("<script>alert(1)</script>");
            searchInput.Clear();
            searchInput.SendKeys(kyTuDacBiet);
            CaptureStep("tc10_special_keyword_typed");
            searchInput.SendKeys(Keys.Enter);
            WaitForDocumentReady("TC10 special chars search submit");

            // Assert không xuất hiện popup alert (XSS)
            bool alertAppeared = false;
            try
            {
                var shortWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(2));
                alertAppeared = shortWait.Until(drv =>
                {
                    try
                    {
                        drv.SwitchTo().Alert();
                        return true;
                    }
                    catch (NoAlertPresentException)
                    {
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                // Không có alert trong timeout ngắn - đúng kỳ vọng.
                alertAppeared = false;
            }

            Assert.That(alertAppeared, Is.False, "Trang bị lỗi XSS - xuất hiện alert popup!");

            // Assert trang không crash
            Assert.That(Driver.Title, Is.Not.Null.And.Not.Empty,
                "Trang phải vẫn hoạt động sau khi tìm kiếm ký tự đặc biệt");
            Assert.That(Driver.PageSource, Does.Not.Contain("500 Internal Server Error"),
                "Trang không được báo lỗi 500 khi tìm ký tự đặc biệt");
            Assert.That(Driver.Url, Does.Contain("vnexpress.net"), "URL phải thuộc domain VnExpress");

            CaptureStep("tc10_special_keyword_result");
            LogInfo($"[TC10 PASS] Ký tự đặc biệt được xử lý an toàn. URL: {Driver.Url}");
        }
    }
}
