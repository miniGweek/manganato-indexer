-- use manga;
drop table dbo.__EFMigrationsHistory;
drop table Tags;
drop table MangaAndTags
drop table Mangas;

use manga;
select * from Mangas
select * from Tags
select * from MangaAndTags

select M1.Name,M1.ChapterCount,M1.Rating,M1.Status,
       Tags = Stuff((SELECT ',' + T2.TagName
from Mangas as M2
join MangaAndTags as MT2 on M2.MangaId = MT2.MangaId
join Tags as T2 on MT2.TagId = T2.TagId
    where M2.MangaId = M1.MangaId
    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')
from Mangas as M1