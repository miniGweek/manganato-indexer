namespace MangaNatoIndexer;

internal class Manga
{
    public int MangaId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public int ChapterCount { get; set; }
    public float Rating { get; set; }
    public string Authors { get; set; }
    public string Status { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset LastChapterUpdatedOn { get; set; }
    public DateTimeOffset RecordCreatedOn { get; set; }

}

internal class MangaAndTags
{
    public int MangaAndTagsId { get; set; }
    public Manga Manga { get; set; }
    public Tag Tag { get; set; }
}

internal class Tag
{
    public int TagId { get; set; }
    public string TagName { get; set; }
}