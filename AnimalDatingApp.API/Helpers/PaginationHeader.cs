namespace AnimalDatingApp.API.Helpers
{
    public class PaginationHeader
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public PaginationHeader(int currentPage, int itermPerPage, int totalItems, int totalPages)
        {
            this.CurrentPage = currentPage;
            this.ItemsPerPage = itermPerPage;
            this.TotalItems = totalItems;
            this.TotalPages = totalPages;  
        }
    }
}