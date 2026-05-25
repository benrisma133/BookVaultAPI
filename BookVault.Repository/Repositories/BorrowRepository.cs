// BookVault.Repository/Repositories/BorrowRepository.cs
using BookVault.Repository.Data;
using BookVault.Repository.Loggers;
using BookVault.Repository.Models.BorrowModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BookVault.Repository.Repositories
{
    public static class BorrowRepository
    {
        private static string ConnectionString => DatabaseHelper.ConnectionString;

        // ======================== [ GET ALL BORROWS ] ========================
        public static List<Borrow> GetAllBorrows()
        {
            var list = new List<Borrow>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetAllBorrows", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapBorrow(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetAllBorrows), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetAllBorrows), ex);
                throw;
            }

            return list;
        }

        // ======================== [ GET BORROW BY ID ] ========================
        public static Borrow? GetBorrowByID(int borrowID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetBorrowByID", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BorrowID", borrowID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapBorrow(reader) : null;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetBorrowByID), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetBorrowByID), ex);
                throw;
            }
        }

        // ======================== [ GET MY BORROWS ] ========================
        public static List<Borrow> GetMyBorrows(int userID)
        {
            var list = new List<Borrow>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetMyBorrows", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapBorrow(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetMyBorrows), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetMyBorrows), ex);
                throw;
            }

            return list;
        }

        // ======================== [ GET ACTIVE BORROWS ] ========================
        public static List<Borrow> GetActiveBorrows()
        {
            var list = new List<Borrow>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetActiveBorrows", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapBorrow(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetActiveBorrows), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetActiveBorrows), ex);
                throw;
            }

            return list;
        }

        // ======================== [ GET OVERDUE BORROWS ] ========================
        public static List<Borrow> GetOverdueBorrows()
        {
            var list = new List<Borrow>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetOverdueBorrows", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapBorrow(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetOverdueBorrows), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(GetOverdueBorrows), ex);
                throw;
            }

            return list;
        }

        // ======================== [ CREATE BORROW ] ========================
        public static (string Result, int NewBorrowID) CreateBorrow(CreateBorrowModel model, int createdBy)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_CreateBorrow", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", model.UserID);
                cmd.Parameters.AddWithValue("@BookID", model.BookID);
                cmd.Parameters.AddWithValue("@DueDate", model.DueDate);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string result = reader.GetString(reader.GetOrdinal("Result"));
                    int newBorrowID = reader.IsDBNull(reader.GetOrdinal("NewBorrowID"))
                                            ? -1
                                            : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("NewBorrowID")));
                    return (result, newBorrowID);
                }

                return ("UNKNOWN", -1);
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(CreateBorrow), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(CreateBorrow), ex);
                throw;
            }
        }

        // ======================== [ RETURN BORROW ] ========================
        public static string ReturnBorrow(int borrowID, int userID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_ReturnBorrow", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BorrowID", borrowID);
                cmd.Parameters.AddWithValue("@UserID", userID);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(ReturnBorrow), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(BorrowRepository), nameof(ReturnBorrow), ex);
                throw;
            }
        }

        // ======================== [ MAPPER ] ========================
        private static Borrow MapBorrow(SqlDataReader reader) => new Borrow
        {
            BorrowID = reader.GetInt32(reader.GetOrdinal("BorrowID")),
            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            BookID = reader.GetInt32(reader.GetOrdinal("BookID")),
            BookTitle = reader.GetString(reader.GetOrdinal("BookTitle")),
            BorrowedAt = reader.GetDateTime(reader.GetOrdinal("BorrowedAt")),
            DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
            ReturnedAt = reader.IsDBNull(reader.GetOrdinal("ReturnedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ReturnedAt")),
            Status = reader.GetByte(reader.GetOrdinal("Status")),
            CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy"))
        };
    }
}