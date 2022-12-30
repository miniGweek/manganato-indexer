-- use manga;
drop table MangaAndTags
drop table Tags;
drop table Mangas;
drop table dbo.__EFMigrationsHistory;

use manga;
select * from Mangas
select * from Tags
select * from MangaAndTags

select * from (
select M1.Name,M1.Url,M1.ChapterCount,
       M1.Rating,M1.Status,M1.Description,
       M1.LastChapterUpdatedOn, M1.ViewCount,
       Stuff((SELECT ',' + T2.TagName
from Mangas as M2
join MangaAndTags as MT2 on M2.MangaId = MT2.MangaId
join Tags as T2 on MT2.TagId = T2.TagId
    where M2.MangaId = M1.MangaId
    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') as Tags
from Mangas as M1) as Data
where ChapterCount > 100
Order by LastChapterUpdatedOn asc


select Name,Count(Name) from Mangas
Group By Name