// BookVault.Repository/Models/ReviewModels/Review.cs
namespace BookVault.Repository.Models.ReviewModels
{
    public class Review
    {
        public int ReviewID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public int BookID { get; set; }
        public string BookTitle { get; set; } = null!;
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class CreateReviewModel
    {
        public int UserID { get; set; }
        public int BookID { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReviewModel
    {
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }
}