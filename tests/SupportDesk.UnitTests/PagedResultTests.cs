using SupportDesk.Web.Contracts;

namespace SupportDesk.UnitTests;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(3, 2, 2)]
    public void TotalPages_rounds_up(int totalCount, int pageSize, int expected)
    {
        var page = new PagedResult<string>([], Page: 1, PageSize: pageSize, TotalCount: totalCount);

        Assert.Equal(expected, page.TotalPages);
    }
}
