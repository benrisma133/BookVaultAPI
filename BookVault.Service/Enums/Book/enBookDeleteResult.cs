// BookVault.Service/Enums/Book/enBookDeleteResult.cs
namespace BookVault.Service.Enums.Book
{
    public enum enBookDeleteResult
    {
        Deleted,
        NotFound,
        HasActiveBorrows,
        Failed
    }
}