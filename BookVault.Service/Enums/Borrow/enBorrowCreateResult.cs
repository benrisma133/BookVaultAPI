// BookVault.Service/Enums/Borrow/enBorrowCreateResult.cs
namespace BookVault.Service.Enums.Borrow
{
    public enum enBorrowCreateResult
    {
        Created,
        BookNotFound,
        UserNotFound,
        OutOfStock,
        AlreadyBorrowed,
        Failed
    }
}