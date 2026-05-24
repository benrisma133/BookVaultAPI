// BookVault.Service/Services/BookService.cs
using BookVault.Repository.Models.BookModels;
using BookVault.Repository.Repositories;
using BookVault.Service.Enums.Book;

namespace BookVault.Service.Services
{
    public class BookService
    {
        // ─── enMode ────────────────────────────────────────────────────────
        public enum enMode { AddNew, Update }
        private enMode _Mode;

        // ─── Properties ────────────────────────────────────────────────────
        public int BookID { get; private set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalStock { get; set; }
        public int AvailableStock { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public int CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public int? UpdatedBy { get; private set; }

        // ─── Constructor: from existing Book (Update mode) ─────────────────
        public BookService(Book book, enMode mode = enMode.Update)
        {
            BookID = book.BookID;
            Title = book.Title;
            Author = book.Author;
            Genre = book.Genre;
            Description = book.Description;
            TotalStock = book.TotalStock;
            AvailableStock = book.AvailableStock;
            CreatedAt = book.CreatedAt;
            CreatedBy = book.CreatedBy;
            UpdatedAt = book.UpdatedAt;
            UpdatedBy = book.UpdatedBy;
            _Mode = mode;
        }

        // ─── Constructor: empty (AddNew mode) ──────────────────────────────
        public BookService()
        {
            _Mode = enMode.AddNew;
        }

        // ─── Private: AddNew ───────────────────────────────────────────────
        private enBookSaveResult _AddNew(int createdBy)
        {
            try
            {
                var model = new AddBookModel
                {
                    Title = Title,
                    Author = Author,
                    Genre = Genre,
                    Description = Description,
                    TotalStock = TotalStock
                };

                var (result, newBookID) = BookRepository.AddBook(model, createdBy);

                return result switch
                {
                    "CREATED" => _OnCreated(newBookID),
                    "DUPLICATE_TITLE" => enBookSaveResult.DuplicateTitle,
                    _ => enBookSaveResult.Failed
                };
            }
            catch
            {
                return enBookSaveResult.Failed;
            }
        }

        private enBookSaveResult _OnCreated(int newBookID)
        {
            BookID = newBookID;
            _Mode = enMode.Update;
            return enBookSaveResult.Saved;
        }

        // ─── Private: Update ───────────────────────────────────────────────
        private enBookSaveResult _Update(int updatedBy)
        {
            try
            {
                var model = new UpdateBookModel
                {
                    Title = Title,
                    Author = Author,
                    Genre = Genre,
                    Description = Description,
                    TotalStock = TotalStock
                };

                string result = BookRepository.UpdateBook(BookID, model, updatedBy);

                return result switch
                {
                    "UPDATED" => enBookSaveResult.Saved,
                    "NOT_FOUND" => enBookSaveResult.NotFound,
                    "DUPLICATE_TITLE" => enBookSaveResult.DuplicateTitle,
                    _ => enBookSaveResult.Failed
                };
            }
            catch
            {
                return enBookSaveResult.Failed;
            }
        }

        // ─── Public: Save ──────────────────────────────────────────────────
        public enBookSaveResult Save(int callerUserID)
        {
            return _Mode switch
            {
                enMode.AddNew => _AddNew(callerUserID),
                enMode.Update => _Update(callerUserID),
                _ => enBookSaveResult.Failed
            };
        }

        // ─── Static: Delete ────────────────────────────────────────────────
        public static enBookDeleteResult Delete(int bookID)
        {
            try
            {
                string result = BookRepository.DeleteBook(bookID);

                return result switch
                {
                    "DELETED" => enBookDeleteResult.Deleted,
                    "NOT_FOUND" => enBookDeleteResult.NotFound,
                    "HAS_ACTIVE_BORROWS" => enBookDeleteResult.HasActiveBorrows,
                    _ => enBookDeleteResult.Failed
                };
            }
            catch
            {
                return enBookDeleteResult.Failed;
            }
        }

        // ─── Static: Find ──────────────────────────────────────────────────
        public static (enBookRetrieveResult result, BookService? service) Find(int bookID)
        {
            try
            {
                Book? book = BookRepository.GetBookByID(bookID);

                if (book is null)
                    return (enBookRetrieveResult.NotFound, null);

                return (enBookRetrieveResult.Found, new BookService(book, enMode.Update));
            }
            catch
            {
                return (enBookRetrieveResult.Failed, null);
            }
        }

        // ─── Static: GetAll ────────────────────────────────────────────────
        public static (enBookRetrieveResult result, List<Book> books) GetAll()
        {
            try
            {
                List<Book> list = BookRepository.GetAllBooks();
                return (enBookRetrieveResult.Found, list);
            }
            catch
            {
                return (enBookRetrieveResult.Failed, new List<Book>());
            }
        }
    }
}