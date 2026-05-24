namespace BookVault.Repository.Models.BookModels
{
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalStock { get; set; }
        public int AvailableStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class AddBookModel
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalStock { get; set; }
    }

    public class UpdateBookModel
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalStock { get; set; }
    }
}
