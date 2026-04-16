namespace SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

public record PaginationParams
{
    private const int MaxPageSize = 50;

    public int Page { get; init; } = 1;

    public int PageSize
    {
        get;
        init => field = (value > MaxPageSize) ? MaxPageSize : value;
    } = 10;
}
