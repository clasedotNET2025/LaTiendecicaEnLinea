using System.Text.Json.Serialization;

namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Common
{
    /// <summary>
    /// Represents a paginated response with metadata for consistent API responses across all microservices
    /// </summary>
    /// <typeparam name="T">The type of items contained in the response</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// The collection of items for the current page
        /// </summary>
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// The current page number (1-based)
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of items per page
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// The total number of pages available
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

        /// <summary>
        /// Indicates if there is a previous page available
        /// </summary>
        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Indicates if there is a next page available
        /// </summary>
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Optional link to the first page
        /// </summary>
        [JsonPropertyName("firstPageUrl")]
        public string? FirstPageUrl { get; set; }

        /// <summary>
        /// Optional link to the previous page
        /// </summary>
        [JsonPropertyName("previousPageUrl")]
        public string? PreviousPageUrl { get; set; }

        /// <summary>
        /// Optional link to the next page
        /// </summary>
        [JsonPropertyName("nextPageUrl")]
        public string? NextPageUrl { get; set; }

        /// <summary>
        /// Optional link to the last page
        /// </summary>
        [JsonPropertyName("lastPageUrl")]
        public string? LastPageUrl { get; set; }

        /// <summary>
        /// Creates a new empty paginated response
        /// </summary>
        public PaginatedResponse() { }

        /// <summary>
        /// Creates a new paginated response with the specified items and metadata
        /// </summary>
        /// <param name="items">The items for the current page</param>
        /// <param name="page">The current page number</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="totalCount">The total number of items</param>
        public PaginatedResponse(List<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }

    /// <summary>
    /// Represents parameters for paginated requests
    /// </summary>
    public class PaginationParams
    {
        /// <summary>
        /// The page number to retrieve (default: 1)
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of items per page (default: 20, max: 100)
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Optional sort field
        /// </summary>
        [JsonPropertyName("sortBy")]
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc or desc)
        /// </summary>
        [JsonPropertyName("sortDirection")]
        public string? SortDirection { get; set; } = "asc";

        /// <summary>
        /// Validates and normalizes pagination parameters
        /// </summary>
        public void Normalize()
        {
            Page = Math.Max(1, Page);
            PageSize = Math.Min(Math.Max(1, PageSize), 100);

            if (!string.IsNullOrEmpty(SortDirection))
            {
                SortDirection = SortDirection.ToLower() == "desc" ? "desc" : "asc";
            }
        }
    }
}