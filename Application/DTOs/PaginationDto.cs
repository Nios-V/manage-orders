namespace Application.DTOs
{
    public class PaginationDto
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;
        public int Size
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    public class PaginatedDto<T>
    {
        public IEnumerable<T> Data { get; set; } = [];
        public int ActualPage { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        public bool HasNext => ActualPage < TotalPages;
        public bool HasPrevious => ActualPage > 1;
    }
}
