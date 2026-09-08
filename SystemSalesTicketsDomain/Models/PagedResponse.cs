namespace MyApp.Application.Common.Models;

public record PagedResponse<T>(
    IEnumerable<T> Data,
    int PageNumber,
    int PageSize,
    int TotalRecords)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
