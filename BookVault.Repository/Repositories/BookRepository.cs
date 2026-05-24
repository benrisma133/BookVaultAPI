using BookVault.Repository.Data;
using BookVault.Repository.Loggers;
using BookVault.Repository.Models.BookModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BookVault.Repository.Repositories
{
    public static class BookRepository
    {
        private static string ConnectionString => DatabaseHelper.ConnectionString;

        // ======================== [ GET ALL BOOKS ] ========================
        public static List<Book> GetAllBooks()
        {
            var list = new List<Book>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetAllBooks", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapBook(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(GetAllBooks), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(GetAllBooks), ex);
                throw;
            }

            return list;
        }

        // ======================== [ GET BOOK BY ID ] ========================
        public static Book? GetBookByID(int bookID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetBookByID", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BookID", bookID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapBook(reader) : null;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(GetBookByID), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(GetBookByID), ex);
                throw;
            }
        }

        // ======================== [ ADD BOOK ] ========================
        public static (string Result, int NewBookID) AddBook(AddBookModel model, int createdBy)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_CreateBook", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Author", model.Author);
                cmd.Parameters.AddWithValue("@Genre", model.Genre);
                cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalStock", model.TotalStock);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string result = reader.GetString(reader.GetOrdinal("Result"));
                    int newBookID = reader.IsDBNull(reader.GetOrdinal("NewBookID"))
                                        ? -1
                                        : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("NewBookID")));
                    return (result, newBookID);
                }

                return ("UNKNOWN", -1);
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(AddBook), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(AddBook), ex);
                throw;
            }
        }

        // ======================== [ UPDATE BOOK ] ========================
        public static string UpdateBook(int bookID, UpdateBookModel model, int updatedBy)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_UpdateBook", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BookID", bookID);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Author", model.Author);
                cmd.Parameters.AddWithValue("@Genre", model.Genre);
                cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalStock", model.TotalStock);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(UpdateBook), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(UpdateBook), ex);
                throw;
            }
        }

        // ======================== [ DELETE BOOK ] ========================
        public static string DeleteBook(int bookID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_DeleteBook", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BookID", bookID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(DeleteBook), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BookRepository), nameof(DeleteBook), ex);
                throw;
            }
        }

        // ======================== [ MAPPER ] ========================
        private static Book MapBook(SqlDataReader reader) => new Book
        {
            BookID = reader.GetInt32(reader.GetOrdinal("BookID")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Author = reader.GetString(reader.GetOrdinal("Author")),
            Genre = reader.GetString(reader.GetOrdinal("Genre")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            TotalStock = reader.GetInt32(reader.GetOrdinal("TotalStock")),
            AvailableStock = reader.GetInt32(reader.GetOrdinal("AvailableStock")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy"))
        };
    }
}