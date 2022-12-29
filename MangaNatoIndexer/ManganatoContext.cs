using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MangaNatoIndexer
{
    internal class ManganatoContext : DbContext
    {
        public DbSet<Manga> Mangas { get; set; }
        public DbSet<MangaDetail> MangaDetails { get; set; }

        public ManganatoContext(DbContextOptions<ManganatoContext> contextOptions) : base(contextOptions) { }
    }

    internal class Manga
    {
        public int MangaId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public List<MangaDetail> MangaDetails { get; set; } = new List<MangaDetail>();
    }

    internal class MangaDetail
    {
        public int MangaDetailId { get; set; }
        public int ChapterCount { get; set; }

        public string Tags { get; set; }
        public DateTimeOffset LastChapterUpdatedOn { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }

}
