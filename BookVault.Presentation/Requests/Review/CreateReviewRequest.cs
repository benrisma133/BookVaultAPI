// BookVault.Presentation/Requests/Review/CreateReviewRequest.cs
namespace BookVault.Presentation.Requests.Review
{
    public class CreateReviewRequest
    {
        public int BookID { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }
}