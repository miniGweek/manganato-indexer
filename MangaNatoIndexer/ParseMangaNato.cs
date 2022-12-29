using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaNatoIndexer
{
    internal class ParseMangaNato
    {
        private ILogger<ParseMangaNato> _logger;

        public ParseMangaNato(ILogger<ParseMangaNato> logger)
        {
            _logger = logger;
        }
        public async Task Index()
        {
            _logger.LogInformation("Initialization Manganato Indexer.");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var context = await browser.NewContextAsync();
            // Create a new page inside context.
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://manganato.com/genre-all");
            _logger.LogInformation("Loaded https://manganato.com/genre-all");

            var lastPageUrl = await page.Locator("div.group-page>a.page-blue.page-last").GetAttributeAsync("href");
            var pagesToIndexCount = int.Parse(lastPageUrl.Split("/").Last());

            _logger.LogInformation($"Total pages to parse: {pagesToIndexCount}");
            Console.ReadKey();
        }
    }
}
