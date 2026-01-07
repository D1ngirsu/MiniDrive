using System.Collections.Generic;
using Xunit;
using MiniDrive.Common;

namespace MiniDrive.UnitTests;

public class PaginationTests
{
    [Fact]
    public void Default_pagination_uses_page_1_and_default_size()
    {
        var p = new Pagination();
        Assert.Equal(1, p.PageNumber);
        Assert.Equal(Pagination.DefaultPageSize, p.PageSize);
    }

    [Fact]
    public void Custom_page_number_and_size_are_set()
    {
        var p = new Pagination(3, 50);
        Assert.Equal(3, p.PageNumber);
        Assert.Equal(50, p.PageSize);
    }

    [Fact]
    public void Negative_page_number_defaults_to_1()
    {
        var p = new Pagination(-5);
        Assert.Equal(1, p.PageNumber);
    }

    [Fact]
    public void Zero_page_number_defaults_to_1()
    {
        var p = new Pagination(0);
        Assert.Equal(1, p.PageNumber);
    }

    [Fact]
    public void Negative_page_size_defaults_to_1()
    {
        var p = new Pagination(1, -10);
        Assert.Equal(1, p.PageSize);
    }

    [Fact]
    public void Page_size_exceeding_max_is_capped()
    {
        var p = new Pagination(1, 500);
        Assert.Equal(Pagination.MaxPageSize, p.PageSize);
    }

    [Fact]
    public void Skip_calculation_correct()
    {
        var p = new Pagination(3, 20);
        Assert.Equal(40, p.Skip); // (3-1) * 20 = 40
    }

    [Fact]
    public void Take_equals_page_size()
    {
        var p = new Pagination(1, 25);
        Assert.Equal(25, p.Take);
    }

    [Fact]
    public void NextPage_increments_page_number()
    {
        var p = new Pagination(2, 20);
        var next = p.NextPage();
        Assert.Equal(3, next.PageNumber);
        Assert.Equal(20, next.PageSize);
    }

    [Fact]
    public void FirstPage_skip_is_zero()
    {
        var p = new Pagination(1, 20);
        Assert.Equal(0, p.Skip);
    }
}

public class PagedResultTests
{
    [Fact]
    public void TotalPages_calculated_correctly()
    {
        var result = new PagedResult<int>(
            Items: new[] { 1, 2, 3 },
            PageNumber: 1,
            PageSize: 10,
            TotalCount: 35);
        
        Assert.Equal(4, result.TotalPages); // ceil(35/10) = 4
    }

    [Fact]
    public void HasNextPage_true_when_not_on_last_page()
    {
        var result = new PagedResult<int>(
            Items: new[] { 1, 2 },
            PageNumber: 1,
            PageSize: 10,
            TotalCount: 25);
        
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_false_when_on_last_page()
    {
        var result = new PagedResult<int>(
            Items: new[] { 21, 22, 23, 24, 25 },
            PageNumber: 3,
            PageSize: 10,
            TotalCount: 25);
        
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasPreviousPage_false_on_first_page()
    {
        var result = new PagedResult<int>(
            Items: new[] { 1, 2 },
            PageNumber: 1,
            PageSize: 10,
            TotalCount: 25);
        
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_true_on_later_pages()
    {
        var result = new PagedResult<int>(
            Items: new[] { 11, 12 },
            PageNumber: 2,
            PageSize: 10,
            TotalCount: 25);
        
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void Empty_items_collection_still_calculates_pages()
    {
        var result = new PagedResult<string>(
            Items: new List<string>(),
            PageNumber: 1,
            PageSize: 10,
            TotalCount: 0);
        
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages); // ceil(0/10) = 0
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
}
