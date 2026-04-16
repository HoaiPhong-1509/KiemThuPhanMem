using System;
using System.Collections.Generic;
using System.Linq;

namespace VnExpressSeleniumTests;

public enum SearchKeywordStatus
{
    Valid,
    Empty,
    TooLong
}

public static class SearchLogic
{
    public const int MaxKeywordLength = 120;

    public static SearchKeywordStatus ValidateKeyword(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return SearchKeywordStatus.Empty;
        }

        var trimmed = keyword.Trim();
        if (trimmed.Length > MaxKeywordLength)
        {
            return SearchKeywordStatus.TooLong;
        }

        return SearchKeywordStatus.Valid;
    }

    public static bool IsRelevantResult(string title, string keyword)
    {
        if (string.IsNullOrWhiteSpace(title) || ValidateKeyword(keyword) != SearchKeywordStatus.Valid)
        {
            return false;
        }

        var normalizedTitle = title.Trim().ToLowerInvariant();
        var tokens = keyword
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length >= 2)
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            return false;
        }

        return tokens.Any(normalizedTitle.Contains);
    }

    public static IReadOnlyList<string> FilterRelevantResults(IEnumerable<string> titles, string keyword)
    {
        return titles
            .Where(title => IsRelevantResult(title, keyword))
            .ToList();
    }
}
