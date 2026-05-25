// BookVault.Repository/Models/BorrowModels/Borrow.cs
namespace BookVault.Repository.Models.BorrowModels
{
    public class Borrow
    {
        public int BorrowID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public int BookID { get; set; }
        public string BookTitle { get; set; } = null!;
        public DateTime BorrowedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public byte Status { get; set; }
        public int CreatedBy { get; set; }
    }

    public class CreateBorrowModel
    {
        public int UserID { get; set; }
        public int BookID { get; set; }
        public DateTime DueDate { get; set; }
    }
}