// BookVault.Service/Enums/Review/enReviewSaveResult.cs
namespace BookVault.Service.Enums.Review
{
    public enum enReviewSaveResult
    {
        Saved,
        BookNotFound,
        UserNotFound,
        NotBorrowed,
        AlreadyReviewed,
        NotFound,
        Failed
    }
}