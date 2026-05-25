// BookVault.Service/Services/BorrowService.cs
using BookVault.Repository.Models.BorrowModels;
using BookVault.Repository.Repositories;
using BookVault.Service.Enums.Borrow;

namespace BookVault.Service.Services
{
    public class BorrowService
    {
        // ─── Properties ────────────────────────────────────────────────────
        public int BorrowID { get; private set; }
        public int UserID { get; private set; }
        public string UserName { get; private set; } = null!;
        public int BookID { get; private set; }
        public string BookTitle { get; private set; } = null!;
        public DateTime BorrowedAt { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime? ReturnedAt { get; private set; }
        public byte Status { get; private set; }
        public int CreatedBy { get; private set; }

        // ─── Constructor ───────────────────────────────────────────────────
        public BorrowService(Borrow borrow)
        {
            BorrowID = borrow.BorrowID;
            UserID = borrow.UserID;
            UserName = borrow.UserName;
            BookID = borrow.BookID;
            BookTitle = borrow.BookTitle;
            BorrowedAt = borrow.BorrowedAt;
            DueDate = borrow.DueDate;
            ReturnedAt = borrow.ReturnedAt;
            Status = borrow.Status;
            CreatedBy = borrow.CreatedBy;
        }

        // ─── Static: Create ────────────────────────────────────────────────
        public static (enBorrowCreateResult result, int NewBorrowID) Create(CreateBorrowModel model, int createdBy)
        {
            try
            {
                var (result, newBorrowID) = BorrowRepository.CreateBorrow(model, createdBy);

                return result switch
                {
                    "CREATED" => (enBorrowCreateResult.Created, newBorrowID),
                    "BOOK_NOT_FOUND" => (enBorrowCreateResult.BookNotFound, -1),
                    "USER_NOT_FOUND" => (enBorrowCreateResult.UserNotFound, -1),
                    "OUT_OF_STOCK" => (enBorrowCreateResult.OutOfStock, -1),
                    "ALREADY_BORROWED" => (enBorrowCreateResult.AlreadyBorrowed, -1),
                    _ => (enBorrowCreateResult.Failed, -1)
                };
            }
            catch
            {
                return (enBorrowCreateResult.Failed, -1);
            }
        }

        // ─── Static: Return ────────────────────────────────────────────────
        public static enBorrowReturnResult Return(int borrowID, int userID)
        {
            try
            {
                string result = BorrowRepository.ReturnBorrow(borrowID, userID);

                return result switch
                {
                    "RETURNED" => enBorrowReturnResult.Returned,
                    "NOT_FOUND" => enBorrowReturnResult.NotFound,
                    "ALREADY_RETURNED" => enBorrowReturnResult.AlreadyReturned,
                    _ => enBorrowReturnResult.Failed
                };
            }
            catch
            {
                return enBorrowReturnResult.Failed;
            }
        }

        // ─── Static: Find ──────────────────────────────────────────────────
        public static (enBorrowRetrieveResult result, BorrowService? service) Find(int borrowID)
        {
            try
            {
                Borrow? borrow = BorrowRepository.GetBorrowByID(borrowID);

                if (borrow is null)
                    return (enBorrowRetrieveResult.NotFound, null);

                return (enBorrowRetrieveResult.Found, new BorrowService(borrow));
            }
            catch
            {
                return (enBorrowRetrieveResult.Failed, null);
            }
        }

        // ─── Static: GetAll ────────────────────────────────────────────────
        public static (enBorrowRetrieveResult result, List<Borrow> borrows) GetAll()
        {
            try
            {
                List<Borrow> list = BorrowRepository.GetAllBorrows();
                return (enBorrowRetrieveResult.Found, list);
            }
            catch
            {
                return (enBorrowRetrieveResult.Failed, new List<Borrow>());
            }
        }

        // ─── Static: GetMyBorrows ──────────────────────────────────────────
        public static (enBorrowRetrieveResult result, List<Borrow> borrows) GetMyBorrows(int userID)
        {
            try
            {
                List<Borrow> list = BorrowRepository.GetMyBorrows(userID);
                return (enBorrowRetrieveResult.Found, list);
            }
            catch
            {
                return (enBorrowRetrieveResult.Failed, new List<Borrow>());
            }
        }

        // ─── Static: GetActiveBorrows ──────────────────────────────────────
        public static (enBorrowRetrieveResult result, List<Borrow> borrows) GetActiveBorrows()
        {
            try
            {
                List<Borrow> list = BorrowRepository.GetActiveBorrows();
                return (enBorrowRetrieveResult.Found, list);
            }
            catch
            {
                return (enBorrowRetrieveResult.Failed, new List<Borrow>());
            }
        }

        // ─── Static: GetOverdueBorrows ─────────────────────────────────────
        public static (enBorrowRetrieveResult result, List<Borrow> borrows) GetOverdueBorrows()
        {
            try
            {
                List<Borrow> list = BorrowRepository.GetOverdueBorrows();
                return (enBorrowRetrieveResult.Found, list);
            }
            catch
            {
                return (enBorrowRetrieveResult.Failed, new List<Borrow>());
            }
        }
    }
}