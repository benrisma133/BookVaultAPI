// BookVault.Presentation/Requests/Review/UpdateReviewRequest.cs
namespace BookVault.Presentation.Requests.Review
{
    public class UpdateReviewRequest
    {
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }
}