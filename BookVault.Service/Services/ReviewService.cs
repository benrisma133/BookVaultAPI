// BookVault.Service/Services/ReviewService.cs
using BookVault.Repository.Models.ReviewModels;
using BookVault.Repository.Repositories;
using BookVault.Service.Enums.Review;

namespace BookVault.Service.Services
{
    public class ReviewService
    {
        // ─── enMode ────────────────────────────────────────────────────────
        public enum enMode { AddNew, Update }
        private enMode _Mode;

        // ─── Properties ────────────────────────────────────────────────────
        public int ReviewID { get; private set; }
        public int UserID { get; set; }
        public string UserName { get; private set; } = null!;
        public int BookID { get; set; }
        public string BookTitle { get; private set; } = null!;
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; private set; }
        public int CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public int? UpdatedBy { get; private set; }

        // ─── Constructor: from existing Review (Update mode) ───────────────
        public ReviewService(Review review, enMode mode = enMode.Update)
        {
            ReviewID = review.ReviewID;
            UserID = review.UserID;
            UserName = review.UserName;
            BookID = review.BookID;
            BookTitle = review.BookTitle;
            Rating = review.Rating;
            Comment = review.Comment;
            CreatedAt = review.CreatedAt;
            CreatedBy = review.CreatedBy;
            UpdatedAt = review.UpdatedAt;
            UpdatedBy = review.UpdatedBy;
            _Mode = mode;
        }

        // ─── Constructor: empty (AddNew mode) ──────────────────────────────
        public ReviewService()
        {
            _Mode = enMode.AddNew;
        }

        // ─── Private: AddNew ───────────────────────────────────────────────
        private enReviewSaveResult _AddNew(int createdBy)
        {
            try
            {
                var model = new CreateReviewModel
                {
                    UserID = UserID,
                    BookID = BookID,
                    Rating = Rating,
                    Comment = Comment
                };

                var (result, newReviewID) = ReviewRepository.CreateReview(model, createdBy);

                return result switch
                {
                    "CREATED" => _OnCreated(newReviewID),
                    "BOOK_NOT_FOUND" => enReviewSaveResult.BookNotFound,
                    "USER_NOT_FOUND" => enReviewSaveResult.UserNotFound,
                    "NOT_BORROWED" => enReviewSaveResult.NotBorrowed,
                    "ALREADY_REVIEWED" => enReviewSaveResult.AlreadyReviewed,
                    _ => enReviewSaveResult.Failed
                };
            }
            catch
            {
                return enReviewSaveResult.Failed;
            }
        }

        private enReviewSaveResult _OnCreated(int newReviewID)
        {
            ReviewID = newReviewID;
            _Mode = enMode.Update;
            return enReviewSaveResult.Saved;
        }

        // ─── Private: Update ───────────────────────────────────────────────
        private enReviewSaveResult _Update(int updatedBy)
        {
            try
            {
                var model = new UpdateReviewModel
                {
                    Rating = Rating,
                    Comment = Comment
                };

                string result = ReviewRepository.UpdateReview(ReviewID, model, updatedBy);

                return result switch
                {
                    "UPDATED" => enReviewSaveResult.Saved,
                    "NOT_FOUND" => enReviewSaveResult.NotFound,
                    _ => enReviewSaveResult.Failed
                };
            }
            catch
            {
                return enReviewSaveResult.Failed;
            }
        }

        // ─── Public: Save ──────────────────────────────────────────────────
        public enReviewSaveResult Save(int callerUserID)
        {
            return _Mode switch
            {
                enMode.AddNew => _AddNew(callerUserID),
                enMode.Update => _Update(callerUserID),
                _ => enReviewSaveResult.Failed
            };
        }

        // ─── Static: Delete ────────────────────────────────────────────────
        public static enReviewDeleteResult Delete(int reviewID)
        {
            try
            {
                string result = ReviewRepository.DeleteReview(reviewID);

                return result switch
                {
                    "DELETED" => enReviewDeleteResult.Deleted,
                    "NOT_FOUND" => enReviewDeleteResult.NotFound,
                    _ => enReviewDeleteResult.Failed
                };
            }
            catch
            {
                return enReviewDeleteResult.Failed;
            }
        }

        // ─── Static: Find ──────────────────────────────────────────────────
        public static (enReviewRetrieveResult result, ReviewService? service) Find(int reviewID)
        {
            try
            {
                Review? review = ReviewRepository.GetReviewByID(reviewID);

                if (review is null)
                    return (enReviewRetrieveResult.NotFound, null);

                return (enReviewRetrieveResult.Found, new ReviewService(review, enMode.Update));
            }
            catch
            {
                return (enReviewRetrieveResult.Failed, null);
            }
        }

        // ─── Static: GetReviewsByBookID ────────────────────────────────────
        public static (enReviewRetrieveResult result, List<Review> reviews) GetReviewsByBookID(int bookID)
        {
            try
            {
                List<Review> list = ReviewRepository.GetReviewsByBookID(bookID);
                return (enReviewRetrieveResult.Found, list);
            }
            catch
            {
                return (enReviewRetrieveResult.Failed, new List<Review>());
            }
        }

        // ─── Static: GetMyReviews ──────────────────────────────────────────
        public static (enReviewRetrieveResult result, List<Review> reviews) GetMyReviews(int userID)
        {
            try
            {
                List<Review> list = ReviewRepository.GetMyReviews(userID);
                return (enReviewRetrieveResult.Found, list);
            }
            catch
            {
                return (enReviewRetrieveResult.Failed, new List<Review>());
            }
        }
    }
}