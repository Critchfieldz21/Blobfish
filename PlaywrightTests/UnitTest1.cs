using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public class UploadTests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserType _browserType;

        // Parameterize browser type
        [TestCase("chromium")]   // Chrome/Edge
        [TestCase("firefox")]    // Firefox
        public async Task UploadSingleShopTicket_ShouldNavigateToProcessing(string browserName)
        {
            _playwright = await Playwright.CreateAsync();
            _browserType = browserName switch
            {
                "chromium" => _playwright.Chromium,
                "firefox"  => _playwright.Firefox,
                _          => _playwright.Chromium
            };

            _browser = await _browserType.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 250,
            });
            var page = await _browser.NewPageAsync();
            await page.GotoAsync("http://localhost:5283/");
            await page.EvaluateAsync("document.body.style.zoom = '80%'");

            await page.SetInputFilesAsync("#uploadFiles", "TestFiles/25-NE1203.01-W019_P2.pdf");
            await Task.Delay(1000);

            await Task.WhenAll(
                page.WaitForURLAsync("**/processing"),
                page.ClickAsync("button.processbtn")
            );

            Assert.That(page.Url, Does.EndWith("/processing"));
            await Task.Delay(5000);
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [TestCase("chromium")]
        [TestCase("firefox")]
        public async Task UploadMultipleShopTickets_ShouldNavigateToProcessing(string browserName)
        {
            _playwright = await Playwright.CreateAsync();
            _browserType = browserName switch
            {
                "chromium" => _playwright.Chromium,
                "firefox"  => _playwright.Firefox,
                _          => _playwright.Chromium
            };

            _browser = await _browserType.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 250,
            });
            var page = await _browser.NewPageAsync();

            await page.GotoAsync("http://localhost:5283/");
            await page.EvaluateAsync("document.body.style.zoom = '80%'");

            await page.SetInputFilesAsync("#uploadFiles", new[]
            {
                "TestFiles/25-NE1203.01-W040_P2.pdf",
                "TestFiles/25-NE1203.01-W049_P2.pdf",
                "TestFiles/25-NE1203.01-W063_P2.pdf",
                "TestFiles/25-NE1203.02-W014_P2.pdf",
                "TestFiles/25-NE1203.02-W040_P2.pdf"
            });

            await Task.Delay(1000);

            await Task.WhenAll(
                page.WaitForURLAsync("**/processing"),
                page.ClickAsync("button.processbtn")
            );

            Assert.That(page.Url, Does.EndWith("/processing"));
            await Task.Delay(5000);
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
        [TestCase("chromium")]
        [TestCase("firefox")]
        public async Task UploadBadPdf_ShouldShowErrorModal(string browserName)
        {
            using var playwright = await Playwright.CreateAsync();
            var browserType = browserName switch
            {
                "chromium" => playwright.Chromium,
                "firefox"  => playwright.Firefox,
                _          => playwright.Chromium
            };

            _browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 250
            });

            var page = await _browser.NewPageAsync();
            await page.GotoAsync("http://localhost:5283/");
            await page.EvaluateAsync("document.body.style.zoom = '80%'");

            // Upload a deliberately bad PDF
            await page.SetInputFilesAsync("#uploadFiles", "TestFiles/20-NE0881-W001_P2.pdf");

            // Click Process
            await page.ClickAsync("button.processbtn");

            // Wait for modal to appear
            var modal = page.Locator("div[role='dialog']");
            await modal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

            // Assert it is visible
            Assert.That(await modal.IsVisibleAsync(), Is.True);
            await Task.Delay(5000);
        }

    }
}
