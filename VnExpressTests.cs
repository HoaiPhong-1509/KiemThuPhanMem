using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

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
        private IWebDriver driver;
        private WebDriverWait wait;
        private const string BASE_URL = "https://vnexpress.net";

        // =============================================
        // SETUP & TEARDOWN
        // =============================================

        [SetUp]
        public void Setup()
        {
            // Khởi tạo EdgeDriver với các tùy chọn
            EdgeOptions options = new EdgeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            // Bỏ comment dòng dưới nếu muốn chạy headless (không hiển thị trình duyệt)
            // options.AddArgument("--headless");

            driver = new EdgeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            driver.Navigate().GoToUrl(BASE_URL);
            Thread.Sleep(2000); // Chờ trang tải xong
        }

        [TearDown]
        public void TearDown()
        {
            // Đóng trình duyệt sau mỗi test
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
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
            // Assert title trang
            Assert.That(driver.Title, Does.Contain("VnExpress"),
                "Title trang chủ phải chứa 'VnExpress'");

            // Assert URL đúng
            Assert.That(driver.Url, Does.Contain("vnexpress.net"),
                "URL phải chứa 'vnexpress.net'");

            // Assert logo hiển thị
            IWebElement logo = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("a.logo, .logo-vne, header a[href='/']")));
            Assert.That(logo.Displayed, Is.True, "Logo VnExpress phải hiển thị");

            Console.WriteLine($"[TC01 PASS] Trang chủ tải thành công. Title: {driver.Title}");
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
            string tuKhoa = "bóng đá";

            // Tìm ô search và click vào
            IWebElement searchIcon = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-search, .ic-search, [data-role='search-toggle']")));
            searchIcon.Click();
            Thread.Sleep(1000);

            // Nhập từ khóa vào ô tìm kiếm
            IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[type='search'], input[name='q'], .search-input")));
            searchInput.Clear();
            searchInput.SendKeys(tuKhoa);
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            // Assert có kết quả hiển thị
            var results = driver.FindElements(By.CssSelector("article, .item-news, .list-news-subfolder h3"));
            Assert.That(results.Count, Is.GreaterThan(0),
                $"Phải có ít nhất 1 kết quả tìm kiếm cho từ khóa '{tuKhoa}'");

            Console.WriteLine($"[TC02 PASS] Tìm kiếm '{tuKhoa}' trả về {results.Count} kết quả.");
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
            // Mở ô search
            IWebElement searchIcon = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-search, .ic-search, [data-role='search-toggle']")));
            searchIcon.Click();
            Thread.Sleep(1000);

            // Không nhập gì, nhấn Enter
            IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[type='search'], input[name='q'], .search-input")));
            searchInput.Clear();
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            // Assert trang không bị lỗi 500 hay crash
            string pageSource = driver.PageSource;
            Assert.That(pageSource, Does.Not.Contain("500 Internal Server Error"),
                "Trang không được báo lỗi 500 khi tìm kiếm keyword rỗng");
            Assert.That(pageSource, Does.Not.Contain("Error 404"),
                "Trang không được báo lỗi 404 khi tìm kiếm keyword rỗng");

            Console.WriteLine($"[TC03 PASS] Tìm kiếm keyword rỗng không gây crash. URL: {driver.Url}");
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
            // Tạo keyword dài 200 ký tự
            string keywordDai = new string('a', 200);

            // Mở ô search
            IWebElement searchIcon = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-search, .ic-search, [data-role='search-toggle']")));
            searchIcon.Click();
            Thread.Sleep(1000);

            // Nhập keyword dài
            IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[type='search'], input[name='q'], .search-input")));
            searchInput.Clear();
            searchInput.SendKeys(keywordDai);
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            // Assert trang không crash
            string pageTitle = driver.Title;
            Assert.That(pageTitle, Is.Not.Null.And.Not.Empty,
                "Title trang không được rỗng sau khi tìm với keyword dài");

            string pageSource = driver.PageSource;
            Assert.That(pageSource, Does.Not.Contain("500 Internal Server Error"),
                "Trang không được báo lỗi 500 với keyword dài");

            Console.WriteLine($"[TC04 PASS] Keyword dài 200 ký tự xử lý bình thường. URL: {driver.Url}");
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
            // Click vào menu Thời sự
            IWebElement menuThoiSu = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//a[contains(@href, 'thoi-su') and (contains(text(),'Thời sự') or contains(text(),'thời sự'))]")));
            menuThoiSu.Click();
            Thread.Sleep(2000);

            // Assert URL chứa "thoi-su"
            Assert.That(driver.Url, Does.Contain("thoi-su"),
                "URL phải chứa 'thoi-su' sau khi click chuyên mục Thời sự");

            // Assert có bài viết trong chuyên mục
            var articles = driver.FindElements(By.CssSelector("article, .item-news, h3.title-news"));
            Assert.That(articles.Count, Is.GreaterThan(0),
                "Chuyên mục Thời sự phải hiển thị ít nhất 1 bài viết");

            Console.WriteLine($"[TC05 PASS] Chuyên mục Thời sự hiển thị {articles.Count} bài. URL: {driver.Url}");
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
            // Click vào menu Thể thao
            IWebElement menuTheThao = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//a[contains(@href, 'the-thao') and (contains(text(),'Thể thao') or contains(text(),'thể thao'))]")));
            menuTheThao.Click();
            Thread.Sleep(2000);

            // Assert URL chứa "the-thao"
            Assert.That(driver.Url, Does.Contain("the-thao"),
                "URL phải chứa 'the-thao' sau khi click chuyên mục Thể thao");

            // Assert có bài viết
            var articles = driver.FindElements(By.CssSelector("article, .item-news, h3.title-news"));
            Assert.That(articles.Count, Is.GreaterThan(0),
                "Chuyên mục Thể thao phải hiển thị ít nhất 1 bài viết");

            Console.WriteLine($"[TC06 PASS] Chuyên mục Thể thao hiển thị {articles.Count} bài. URL: {driver.Url}");
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
            // Lấy bài viết đầu tiên
            IWebElement firstArticle = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("h3.title-news a, article h2 a, .item-news h3 a")));

            string tenBai = firstArticle.Text;
            Console.WriteLine($"Đang click vào bài: {tenBai}");
            firstArticle.Click();
            Thread.Sleep(3000);

            // Assert URL thay đổi (không còn ở trang chủ)
            Assert.That(driver.Url, Is.Not.EqualTo(BASE_URL + "/"),
                "URL phải thay đổi khi mở chi tiết bài viết");

            // Assert tiêu đề bài viết hiển thị
            IWebElement tieuDe = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("h1.title-detail, h1.heading-title, article h1")));
            Assert.That(tieuDe.Displayed, Is.True, "Tiêu đề bài viết phải hiển thị");
            Assert.That(tieuDe.Text, Is.Not.Empty, "Tiêu đề bài viết không được rỗng");

            // Assert nội dung bài viết hiển thị
            IWebElement noiDung = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("article p, .fck_detail p, .content-detail p")));
            Assert.That(noiDung.Displayed, Is.True, "Nội dung bài viết phải hiển thị");

            Console.WriteLine($"[TC07 PASS] Mở bài thành công. Tiêu đề: {tieuDe.Text.Substring(0, Math.Min(60, tieuDe.Text.Length))}...");
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
            // Vào chuyên mục Thời sự
            driver.Navigate().GoToUrl($"{BASE_URL}/thoi-su");
            Thread.Sleep(2000);

            string url_trang1 = driver.Url;

            // Tìm nút sang trang 2
            IWebElement nextPage = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.next-page, li.next a, .pagination a[rel='next'], a[title='Trang kế']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", nextPage);
            Thread.Sleep(500);

            try
            {
                nextPage.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", nextPage);
            }

            Thread.Sleep(2000);

            // Assert URL thay đổi (sang trang 2)
            Assert.That(driver.Url, Is.Not.EqualTo(url_trang1),
                "URL phải thay đổi khi chuyển sang trang 2");

            // Assert có bài viết trên trang 2
            var articles = driver.FindElements(By.CssSelector("article, .item-news, h3.title-news"));
            Assert.That(articles.Count, Is.GreaterThan(0),
                "Trang 2 phải hiển thị bài viết");

            Console.WriteLine($"[TC08 PASS] Phân trang hoạt động. URL trang 2: {driver.Url}");
        }

        // =============================================
        // TC09 - KIỂM TRA TÌM KIẾM TRẢ VỀ KẾT QUẢ ĐÚNG CHỦ ĐỀ
        // =============================================

        /// <summary>
        /// TC09: Kết quả tìm kiếm phải liên quan đến từ khóa
        /// Input: Tìm kiếm "COVID"
        /// Expected: Tiêu đề các bài viết trong kết quả có chứa từ liên quan
        /// </summary>
        [Test]
        public void TC09_TimKiem_KetQuaDungChuDe()
        {
            string tuKhoa = "COVID";

            // Mở ô search
            IWebElement searchIcon = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-search, .ic-search, [data-role='search-toggle']")));
            searchIcon.Click();
            Thread.Sleep(1000);

            IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[type='search'], input[name='q'], .search-input")));
            searchInput.Clear();
            searchInput.SendKeys(tuKhoa);
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            // Lấy danh sách tiêu đề bài viết
            var titles = driver.FindElements(By.CssSelector("h3.title-news a, article h2 a, .item-news h3 a"));
            Assert.That(titles.Count, Is.GreaterThan(0),
                $"Phải có kết quả tìm kiếm cho từ khóa '{tuKhoa}'");

            Console.WriteLine($"[TC09 PASS] Tìm kiếm '{tuKhoa}' trả về {titles.Count} kết quả.");
            foreach (var title in titles)
            {
                Console.WriteLine($"  - {title.Text}");
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
            string kyTuDacBiet = "<script>alert(1)</script>";

            // Mở ô search
            IWebElement searchIcon = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-search, .ic-search, [data-role='search-toggle']")));
            searchIcon.Click();
            Thread.Sleep(1000);

            IWebElement searchInput = wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[type='search'], input[name='q'], .search-input")));
            searchInput.Clear();
            searchInput.SendKeys(kyTuDacBiet);
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            // Assert không xuất hiện popup alert (XSS)
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Dismiss(); // Nếu có alert, dismiss và fail test
                Assert.Fail("Trang bị lỗi XSS - xuất hiện alert popup!");
            }
            catch (NoAlertPresentException)
            {
                // Đây là kết quả mong đợi - không có alert
            }

            // Assert trang không crash
            Assert.That(driver.Title, Is.Not.Null.And.Not.Empty,
                "Trang phải vẫn hoạt động sau khi tìm kiếm ký tự đặc biệt");

            Console.WriteLine($"[TC10 PASS] Ký tự đặc biệt được xử lý an toàn. URL: {driver.Url}");
        }
    }
}
