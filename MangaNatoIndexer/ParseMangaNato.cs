using System.Text.RegularExpressions;

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

            var page = await context.NewPageAsync();
            await page.GotoAsync("https://manganato.com/genre-all");
            _logger.LogInformation("Loaded https://manganato.com/genre-all");

            var lastPageUrl = await page.Locator("div.group-page>a.page-blue.page-last").GetAttributeAsync("href");
            var pagesToIndexCount = int.Parse(lastPageUrl.Split("/").Last());

            _logger.LogInformation($"Total pages to parse: {pagesToIndexCount}");

            var existingTags = (await _repository.GetListAsync<Tag>()).ToList();

            for (var i = 1; i <= pagesToIndexCount; i++)
            {
                var mangaTiles = page.Locator("div.content-genres-item");
                var mangaTilesCount = await mangaTiles.CountAsync();
                for (var index = 0; index < mangaTilesCount; index++)
                {
                    var mangaCard = mangaTiles.Nth(index);
                    var mangaInfoCard = mangaCard.Locator("div.genres-item-info>h3>a.genres-item-name.text-nowrap.a-h");
                    var titleText = await mangaInfoCard.InnerTextAsync();
                    var url = await mangaInfoCard.GetAttributeAsync("href");

                    var lastUpdated = await mangaCard.Locator("div.genres-item-info>p.genres-item-view-time.text-nowrap>span.genres-item-time").InnerTextAsync();
                    var lastUpdatedDateTimeOffset = DateTimeOffset.ParseExact(lastUpdated, "MMM dd,yy", System.Globalization.CultureInfo.InvariantCulture);
                    var lastUpdatedDateString = lastUpdatedDateTimeOffset.ToString("dd/MM/yyyy");

                    var author = await mangaCard.Locator("div.genres-item-info>p.genres-item-view-time.text-nowrap>span.genres-item-author").InnerTextAsync();
                    var description = await mangaCard.Locator("div.genres-item-info>div.genres-item-description").InnerTextAsync();

                    var ratingText = await mangaCard.Locator("a.genres-item-img.bookmark_check>em.genres-item-rate").InnerTextAsync();
                    var rating = float.Parse(ratingText);
                    var chapterCountText = await mangaCard.Locator("div.genres-item-info>a.genres-item-chap.text-nowrap.a-h").InnerTextAsync();
                    int chapterCount = 0;
                    var chapterCountParsMatch = Regex.Match(chapterCountText, "Chapter ([\\d]{1,5})");
                    if (chapterCountParsMatch.Success)
                    {
                        chapterCount = int.Parse(chapterCountParsMatch.Groups[1].Value);
                    }

                    var mangaDetailsPage = await context.NewPageAsync();
                    await mangaDetailsPage.GotoAsync(url);
                    var mangaDetailCard = mangaDetailsPage.Locator("div.story-info-right");

                    var authorsTableRow =
                        mangaDetailCard.Locator(
                            "table.variations-tableInfo>tbody>tr:has(td.table-label>i.info-author)");
                    var authors = await authorsTableRow.Locator("td.table-value").InnerTextAsync();

                    var statusTableRow = mangaDetailCard.Locator(
                        "table.variations-tableInfo>tbody>tr:has(td.table-label>i.info-status)");
                    var status = await statusTableRow.Locator("td.table-value").InnerTextAsync();

                    var tagsTableRow = mangaDetailCard.Locator(
                        "table.variations-tableInfo>tbody>tr:has(td.table-label>i.info-genres)");
                    var tags = await tagsTableRow.Locator("td.table-value").InnerTextAsync();
                    List<Tag> matchedTags = null;
                    var listOfTags = tags.GetListOfTags();
                    if (listOfTags != null)
                    {
                        matchedTags = existingTags.Where(e => listOfTags.Contains(e.TagName)).ToList();
                    }

                    var moreMangaDetailCard = mangaDetailsPage.Locator("div.story-info-right-extent");
                    var viewCountParagraph = moreMangaDetailCard.Locator("p:has(span.stre-label>i.info-view)");
                    var viewCount = await viewCountParagraph.Locator("span.stre-value").InnerTextAsync();

                    _logger.LogInformation($"{index + 1}. {titleText} - {rating} - {chapterCount} - {authors} - {status} - {tags} - {url} - {lastUpdatedDateString} - {author} - {description}");

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
                    if (matchedTags != null && matchedTags.Count > 0)
                    {
                        foreach (var tag in matchedTags)
                        {
                            await _repository.AddAsync(new MangaAndTags(){Manga = mangaEntity,Tag = tag});
                        }
                    }
                    await _repository.AddAsync(mangaEntity);
                    
                }

                await _repository.SaveChangesAsync();
                Console.ReadKey();
            }
            Console.ReadKey();
        }
    }
}
