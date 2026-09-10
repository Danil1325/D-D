using DnDGame.BusinessLayer.Dtos.Common;

namespace DnDGame.Tests.Infrastructure;

public class PagedRequestAndResultTests
{
    [Fact]
    public void Normalize_NegativePage_ClampsToOne()
    {
        var request = new PagedRequest { Page = -5, PageSize = 20 };

        var normalized = request.Normalize();

        Assert.Equal(1, normalized.Page);
    }

    [Fact]
    public void Normalize_PageSizeTooLarge_ClampsToMax()
    {
        var request = new PagedRequest { Page = 1, PageSize = 9999 };

        var normalized = request.Normalize();

        Assert.Equal(PagedRequest.MaxPageSize, normalized.PageSize);
    }

    [Fact]
    public void Normalize_ZeroPageSize_FallsBackToDefault()
    {
        var request = new PagedRequest { Page = 1, PageSize = 0 };

        var normalized = request.Normalize();

        Assert.Equal(PagedRequest.DefaultPageSize, normalized.PageSize);
    }

    [Fact]
    public void Create_ComputesTotalPagesCorrectly()
    {
        var result = PagedResult<string>.Create(new[] { "a", "b" }, page: 1, pageSize: 2, totalCount: 5);

        Assert.Equal(3, result.TotalPages); // ceil(5 / 2)
    }
}
