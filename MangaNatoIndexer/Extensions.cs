
using Azure;
using System;
using static System.Net.Mime.MediaTypeNames;

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

        internal static async Task<IResponse?> GoToAsync2(this IPage? page, string uri, ILogger<ParseMangaNato> logger)
        {
            int maxRetryCount = 5;
            double retryInterval = 3;
            double retryIntervalBackOffCoefficient = 2;

            int retryCount = 0;
            while (retryCount <= maxRetryCount)
            {
                
                try
                {
                    return await page.GotoAsync(uri);
                }
                catch (TimeoutException tex)
                {
                    logger.LogError(tex, $"Timeout Exception. Failed to load {uri}. Retrying - RetryCount:{retryCount} - RetryInterval:{retryInterval}:");
                    logger.LogInformation($"{uri} RetryCount:{retryCount} - RetryInterval:{retryInterval}:");
                    await Task.Delay(TimeSpan.FromSeconds(retryInterval));
                    logger.LogInformation($"Ready for next attempt. {uri}");
                    retryCount++;
                    retryInterval = Math.Pow(retryIntervalBackOffCoefficient, retryCount+1);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Unknown exception. Failed to load {uri}. Retrying - RetryCount:{retryCount} - RetryInterval:{retryInterval}:");
                    logger.LogInformation($"{uri} RetryCount:{retryCount} - RetryInterval:{retryInterval}:");
                    await Task.Delay(TimeSpan.FromSeconds(retryInterval));
                    logger.LogInformation($"Ready for next attempt. {uri}");
                    retryCount++;
                    retryInterval = Math.Pow(retryIntervalBackOffCoefficient, retryCount + 1);
                }

            }
            return null;
        }

        internal static async Task<string> InnerTextHandleExceptionAsync(this ILocator locator,
            ILogger<ParseMangaNato> logger)
        {
            try
            {
                return await locator.InnerTextAsync();
            }
            catch (TimeoutException tex)
            {
                logger.LogError(tex, $"Locator failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unknown exception while trying to locate, moving on the next one");
            }
            return "";
        }
    }
}
