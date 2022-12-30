namespace MangaNatoIndexer
{
    internal class ManganatoDbContext : DbContext
    {
        public DbSet<Manga> Mangas { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<MangaAndTags> MangaAndTags { get; set; }
        public ManganatoDbContext(DbContextOptions<ManganatoDbContext> contextOptions) : base(contextOptions) { }

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var tags = new List<string>()
            {
                "Action",
                "Adult",
                "Adventure",
                "Comedy",
                "Cooking",
                "Doujinshi",
                "Drama",
                "Ecchi",
                "Erotica",
                "Fantasy",
                "Gender bender",
                "Harem",
                "Historical",
                "Horror",
                "Isekai",
                "Josei",
                "Manhua",
                "Manhwa",
                "Martial arts",
                "Mature",
                "Mecha",
                "Medical",
                "Mystery",
                "One shot",
                "Pornographic",
                "Psychological",
                "Romance",
                "School life",
                "Sci fi",
                "Seinen",
                "Shoujo",
                "Shoujo ai",
                "Shounen",
                "Shounen ai",
                "Slice of life",
                "Smut",
                "Sports",
                "Supernatural",
                "Tragedy",
                "Webtoons",
                "Yaoi",
                "Yuri"

            };
            int tagIndex = 1;
            foreach (var tag in tags)
            {
                modelBuilder.Entity<Tag>()
                    .HasData(new Tag() { TagId = tagIndex,TagName = tag });
                tagIndex++;
            }
           
        }
        #endregion
    }
}
