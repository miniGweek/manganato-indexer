namespace MangaNatoIndexer
{
    internal class ParseMangaNato
    {
        private ILogger<ParseMangaNato> _logger;
        private readonly IRepository _repository;
        private const float Timeout = 15000;
        private IPlaywright? playwright;

        public ParseMangaNato(ILogger<ParseMangaNato> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
        public async Task Index()
        {
            _logger.LogInformation("Initialization Manganato Indexer.");

            playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var context = await browser.NewContextAsync();
            context.SetDefaultNavigationTimeout(Timeout);
            context.SetDefaultTimeout(Timeout);

            var page = await context.NewPageAsync();
            await page.GoToAsync2("https://manganato.com/genre-all", _logger);
            _logger.LogInformation("Loaded https://manganato.com/genre-all");

            var lastPageUrl = await page.Locator("div.group-page>a.page-blue.page-last").GetAttributeAsync("href");
            var pagesToIndexCount = int.Parse(lastPageUrl.Split("/").Last());
            await page.CloseAsync();
            await context.CloseAsync();

            _logger.LogInformation($"Total pages to parse: {pagesToIndexCount}");

            var existingTags = (await _repository.GetListAsync<Tag>()).ToList();
            var parallelProcessMangaPages = 3;
            var parallelProcessMangasInAPage = 4;

            for (var i = 1; i <= pagesToIndexCount; i += parallelProcessMangaPages)
            {
                var concurrentBagOfManga = new ConcurrentBag<Manga>();
                var concurrentBagOfMangaAndTags = new ConcurrentBag<MangaAndTags>();

                var tasks = new List<Task>();
                var pIndexUpper = i + parallelProcessMangaPages > pagesToIndexCount ? pagesToIndexCount : i + parallelProcessMangaPages - 1;

                for (int pIndex = i; pIndex <= pIndexUpper; pIndex++)
                {
                    tasks.Add(ParseNewMangaPage(pIndex, parallelProcessMangasInAPage, existingTags, concurrentBagOfManga, concurrentBagOfMangaAndTags));

                }
                await Task.WhenAll(tasks);

                await SaveMangaInformation(concurrentBagOfManga, concurrentBagOfMangaAndTags);
            }
        }

        private async Task ParseNewMangaPage( int mangaPageIndex, int parallelProcessMangaInAPage, List<Tag> existingTags, ConcurrentBag<Manga> concurrentBagOfManga, ConcurrentBag<MangaAndTags> concurrentBagOfMangaAndTags)
        {
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var context = await browser.NewContextAsync();
            var mangaPage = await context.NewPageAsync();
            await mangaPage.GoToAsync2($"https://manganato.com/genre-all/{mangaPageIndex}", _logger);

            // _logger.LogInformation($"Begin parsing page {mangaPageIndex}");

            var mangaTiles = mangaPage.Locator("div.content-genres-item");
            var mangaTilesCount = await mangaTiles.CountAsync();
            _logger.LogInformation($"To queue - PageIndex:{mangaPageIndex} - MangaTitlesInAPage:{mangaTilesCount}");
            for (var index = 0; index < mangaTilesCount; index += parallelProcessMangaInAPage)
            {
                var tasks = new List<Task>();
                var pIndexUpper = index + parallelProcessMangaInAPage >= mangaTilesCount
                    ? mangaTilesCount
                    : index + parallelProcessMangaInAPage;
                for (int pIndex = index; pIndex < pIndexUpper; pIndex++)
                {
                    tasks.Add(ParseMangasInAPage(mangaTiles, mangaPageIndex, pIndex, context, existingTags, concurrentBagOfManga,
                        concurrentBagOfMangaAndTags));
                }

                await Task.WhenAll(tasks);
            }
            _logger.LogInformation($"End parsing page {mangaPageIndex}");
            await mangaPage.CloseAsync();
        }

        private async Task ParseMangasInAPage(ILocator mangaTiles, int mangaPageIndex, int index, IBrowserContext context, List<Tag> existingTags,
            ConcurrentBag<Manga> concurrentBagOfManga, ConcurrentBag<MangaAndTags> concurrentBagOfMangaAndTags)
        {
            var mangaCard = mangaTiles.Nth(index);
            var mangaInfoCard = mangaCard.Locator("div.genres-item-info>h3>a.genres-item-name.text-nowrap.a-h");
            var titleText = await mangaInfoCard.InnerTextHandleExceptionAsync(_logger);//await mangaInfoCard.InnerTextHandleExceptionAsync(_logger);
            var url = await mangaInfoCard.GetAttributeAsync("href");
            try
            {
                // _logger.LogInformation($"Begin - Page:{mangaPageIndex} - Manga:{index} - {titleText}");

                var lastUpdated = await mangaCard
                    .Locator("div.genres-item-info>p.genres-item-view-time.text-nowrap>span.genres-item-time")
                    .InnerTextHandleExceptionAsync(_logger);
                var lastUpdatedDateTimeOffset =
                    DateTimeOffset.ParseExact(lastUpdated, "MMM dd,yy",
                        System.Globalization.CultureInfo.InvariantCulture);
                var lastUpdatedDateString = lastUpdatedDateTimeOffset.ToString("dd/MM/yyyy");

                var author = await mangaCard
                    .Locator("div.genres-item-info>p.genres-item-view-time.text-nowrap>span.genres-item-author")
                    .InnerTextHandleExceptionAsync(_logger);
                var description = await mangaCard.Locator("div.genres-item-info>div.genres-item-description")
                    .InnerTextHandleExceptionAsync(_logger);

                var ratingText = await mangaCard.Locator("a.genres-item-img.bookmark_check>em.genres-item-rate")
                    .InnerTextHandleExceptionAsync(_logger);
                var rating = float.Parse(ratingText);

                string chapterCountText = string.Empty;

                int chapterCount = 0;
                try
                {
                    chapterCountText =
                        await mangaCard.Locator("div.genres-item-info>a.genres-item-chap.text-nowrap.a-h").InnerTextHandleExceptionAsync(_logger);
                }
                catch (TimeoutException tex)
                {
                    _logger.LogError(tex, $"Didn't find chapter count for Title:{titleText} - Url:{url} - MangaPage:{mangaPageIndex} -  MangaIndexInPage:{index}");
                }

                if (!string.IsNullOrWhiteSpace(chapterCountText))
                {
                    var chapterCountParsMatch = Regex.Match(chapterCountText, "Chapter ([\\d]{1,5})");
                    if (chapterCountParsMatch != null && chapterCountParsMatch.Success)
                    {
                        chapterCount = int.Parse(chapterCountParsMatch.Groups[1].Value);
                    }
                }


                var mangaDetailsPage = await context.NewPageAsync();
                await mangaDetailsPage.GoToAsync2(url, _logger);
                var mangaDetailCard = mangaDetailsPage.Locator("div.story-info-right");

                var authorsTableRow =
                    mangaDetailCard.Locator(
                        "table.variations-tableInfo>tbody>tr:has(td.table-label>i.info-author)");
                var authors = await authorsTableRow.Locator("td.table-value").InnerTextHandleExceptionAsync(_logger);

                var statusTableRow = mangaDetailCard.Locator(
                    "table.variations-tableInfo>tbody>tr:has(td.table-label>i.info-status)");
                var status = await statusTableRow.Locator("td.table-value").InnerTextHandleExceptionAsync(_logger);

                var tagsTableRow = mangaDetailCard.Locator(
                    "table.variations-tableInfo>tbody>tr:has(td.table-label>i.info-genres)");
                var tags = await tagsTableRow.Locator("td.table-value").InnerTextHandleExceptionAsync(_logger);
                List<Tag> matchedTags = null;
                var listOfTags = tags.GetListOfTags();
                if (listOfTags != null)
                {
                    matchedTags = existingTags.Where(e => listOfTags.Contains(e.TagName)).ToList();
                }

                var moreMangaDetailCard = mangaDetailsPage.Locator("div.story-info-right-extent");
                var viewCountParagraph = moreMangaDetailCard.Locator("p:has(span.stre-label>i.info-view)");
                var viewCount = await viewCountParagraph.Locator("span.stre-value").InnerTextHandleExceptionAsync(_logger);

                await mangaDetailsPage.CloseAsync();

                var mangaEntity = new Manga()
                {
                    Name = titleText,
                    Url = url,
                    ChapterCount = chapterCount,
                    LastChapterUpdatedOn = lastUpdatedDateTimeOffset,
                    Description = description,
                    RecordCreatedOn = DateTimeOffset.Now,
                    Rating = rating,
                    Status = status,
                    Authors = authors,
                    ViewCount = viewCount.GetViewCountNumber()
                };

                concurrentBagOfManga.Add(mangaEntity);

                if (matchedTags != null && matchedTags.Count > 0)
                {
                    foreach (var tag in matchedTags)
                    {
                        concurrentBagOfMangaAndTags.Add(new MangaAndTags() { Manga = mangaEntity, Tag = tag });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errored while parsing Title:{titleText} - Url:{url} - MangaPage:{mangaPageIndex} -  MangaIndexInPage:{index}");
                throw;
            }
            // _logger.LogInformation($"End {index} - {titleText}");
        }

        private async Task SaveMangaInformation(ConcurrentBag<Manga> concurrentBagOfManga, ConcurrentBag<MangaAndTags> concurrentBagOfMangaAndTags)
        {
            foreach (var manga in concurrentBagOfManga)
            {
                await _repository.AddAsync(manga);
            }

            foreach (var mangaAndTag in concurrentBagOfMangaAndTags)
            {
                await _repository.AddAsync(mangaAndTag);
            }

            await _repository.SaveChangesAsync();
        }
    }
}
