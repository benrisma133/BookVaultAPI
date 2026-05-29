// BookVault.Presentation/Requests/Borrow/CreateBorrowRequest.cs
namespace BookVault.Presentation.Requests.Borrow
{
    public class CreateBorrowRequest
    {
        public int BookID { get; set; }
        public DateTime DueDate { get; set; }
    }
}