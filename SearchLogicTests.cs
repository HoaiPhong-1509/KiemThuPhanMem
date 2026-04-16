using NUnit.Framework;
using System.Collections.Generic;

namespace VnExpressSeleniumTests;

[TestFixture]
public class SearchLogicTests
{
    [Test]
    public void ValidateKeyword_ReturnsEmpty_WhenKeywordIsBlank()
    {
        var result = SearchLogic.ValidateKeyword("   ");

        Assert.That(result, Is.EqualTo(SearchKeywordStatus.Empty));
    }

    [Test]
    public void ValidateKeyword_ReturnsTooLong_WhenKeywordExceedsMaxLength()
    {
        var longKeyword = new string('x', SearchLogic.MaxKeywordLength + 1);

        var result = SearchLogic.ValidateKeyword(longKeyword);

        Assert.That(result, Is.EqualTo(SearchKeywordStatus.TooLong));
    }

    [Test]
    public void ValidateKeyword_ReturnsValid_WhenKeywordWithinLimit()
    {
        var result = SearchLogic.ValidateKeyword("chung khoan");

        Assert.That(result, Is.EqualTo(SearchKeywordStatus.Valid));
    }

    [Test]
    public void IsRelevantResult_ReturnsTrue_WhenTitleContainsKeywordToken()
    {
        var isRelevant = SearchLogic.IsRelevantResult(
            "VN-Index tang manh nho nhom co phieu ngan hang",
            "co phieu ngan hang");

        Assert.That(isRelevant, Is.True);
    }

    [Test]
    public void IsRelevantResult_ReturnsFalse_WhenTitleDoesNotContainKeywordToken()
    {
        var isRelevant = SearchLogic.IsRelevantResult(
            "Du bao thoi tiet mien Bac co mua dong",
            "chung khoan");

        Assert.That(isRelevant, Is.False);
    }

    [Test]
    public void FilterRelevantResults_ReturnsOnlyMatchingTitles()
    {
        var titles = new List<string>
        {
            "Gia vang hom nay giam nhe",
            "Thi truong bat dong san phuc hoi",
            "Du bao gia vang cuoi nam"
        };

        var filtered = SearchLogic.FilterRelevantResults(titles, "vang");

        Assert.That(filtered, Has.Count.EqualTo(2));
    }
}
