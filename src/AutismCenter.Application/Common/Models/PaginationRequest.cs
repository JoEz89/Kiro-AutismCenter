namespace AutismCenter.Application.Common.Models;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public PaginationRequest()
    {
    }

    public PaginationRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
    }

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}