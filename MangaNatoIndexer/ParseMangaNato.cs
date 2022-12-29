namespace MangaNatoIndexer
{
    internal class ParseMangaNato
    {
        private ILogger<ParseMangaNato> _logger;
        private readonly IRepository _repository;

        public ParseMangaNato(ILogger<ParseMangaNato> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
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

            for (int i = 1; i <= pagesToIndexCount; i++)
            {
                var mangaTiles = page.Locator("div.content-genres-item");
                var mangaTilesCount = await mangaTiles.CountAsync();
                for (int index = 0; index < mangaTilesCount; index++)
                {
                    var mangaCard = mangaTiles.Nth(index);
                    var mangaTitle = mangaCard.Locator("div.genres-item-info>h3>a.genres-item-name.text-nowrap.a-h");
                    var titleText = await mangaTitle.InnerTextAsync();
                    var url = await mangaTitle.GetAttributeAsync("href");

                    var lastUpdated = await mangaCard.Locator("div.genres-item-info>p.genres-item-view-time.text-nowrap>span.genres-item-time").InnerTextAsync();
                    var lastUpdatedDateTimeOffset = DateTimeOffset.ParseExact(lastUpdated, "MMM dd,yy", System.Globalization.CultureInfo.InvariantCulture);
                    var lastUpdatedDateString = lastUpdatedDateTimeOffset.ToString("dd/MM/yyyy");

                    var author = await mangaCard.Locator("div.genres-item-info>p.genres-item-view-time.text-nowrap>span.genres-item-author").InnerTextAsync();
                    var description = await mangaCard.Locator("div.genres-item-info>div.genres-item-description").InnerTextAsync();
                    _logger.LogInformation($"{index + 1}. {titleText} - {url} - {lastUpdatedDateString} - {author} - {description}");
                    var mangaEntity = new Manga()
                    {
                        Name = titleText,
                        Url = url,
                        MangaDetails = new List<MangaDetail>()
                        {
                            new MangaDetail()
                            {
                                ChapterCount = 0,
                                LastChapterUpdatedOn = lastUpdatedDateTimeOffset,
                                Description = description,
                                CreatedOn = DateTimeOffset.Now,
                                Tags = "empty"
                            }
                        }

                    };
                    await _repository.AddAsync(mangaEntity);
                }

                await _repository.SaveChangesAsync();
                Console.ReadKey();
            }
            Console.ReadKey();
        }
    }
}
