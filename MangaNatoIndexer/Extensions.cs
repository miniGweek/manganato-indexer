namespace MangaNatoIndexer
{
    internal static class Extensions
    {
        internal static List<string>? GetMatches(this string source, string regex)
        {
            var matches = Regex.Matches(source, regex);
            if (matches.Count > 0)
            {
                return matches
                    // flatten to single list
                    .SelectMany(o =>
                        // linq-ify
                        o.Groups.Cast<Capture>()
                            // don't need the pattern
                            .Skip(1)
                            // select what you wanted
                            .Select(c => c.Value))
                    .ToList();
            }

            return null;
        }

        internal static List<string>? GetListOfTags(this string source)
        {
            return source
                .Split("-")
                .Select(tags => tags.Trim())
                .ToList();
        }

        internal static int GetViewCountNumber(this string viewCount)
        {
            var parsed = viewCount.GetMatches("([\\d]+.?[\\d])(\\w)");
            if (parsed == null)
                return 0;
            var viewCountNumberPart = float.Parse(parsed[0]);
            if ("K".Equals(parsed[1], StringComparison.InvariantCultureIgnoreCase))
                return (int)(viewCountNumberPart * 1000);
            if ("M".Equals(parsed[1], StringComparison.InvariantCultureIgnoreCase))
                return (int)(viewCountNumberPart * 1000000);
            return 0;
        }
    }
}
