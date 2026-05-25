using BookVault.Repository.Data;
using BookVault.Repository.Loggers;
using BookVault.Repository.Models.ReviewModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookVault.Repository.Repositories
{
    public static class ReviewRepository
    {
        private static string ConnectionString => DatabaseHelper.ConnectionString;

        // ======================== [ GET REVIEW BY ID ] ========================
        public static Review? GetReviewByID(int reviewID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetReviewByID", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ReviewID", reviewID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReview(reader) : null;

            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(GetReviewByID), ex);
                throw;
            }
            catch(Exception ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(GetReviewByID), ex);
                throw;
            }
        }

        // ======================== [ GET REVIEWS BY BOOK ID ] ========================
        public static List<Review> GetReviewsByBookID(int bookID)
        {
            var list = new List<Review>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetReviewsByBookID", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BookID", bookID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapReview(reader));

            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(GetReviewsByBookID), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(GetReviewsByBookID), ex);
                throw;
            }

            return list;
        }


        // ======================== [ GET MY REVIEWS ] ========================
        public static List<Review> GetMyReviews(int userID)
        {
            var list = new List<Review>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetMyReviews", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapReview(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(GetMyReviews), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(GetMyReviews), ex);
                throw;
            }

            return list;
        }

        // ======================== [ CREATE REVIEW ] ========================
        public static (string Result ,int NewReviewID) CreateReview(CreateReviewModel model ,int createdBy)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_CreateReview", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", model.UserID);
                cmd.Parameters.AddWithValue("@BookID", model.BookID);
                cmd.Parameters.AddWithValue("@Rating", model.Rating);
                cmd.Parameters.AddWithValue("@Comment", model.Comment ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string result = reader.GetString(reader.GetOrdinal("Result"));
                    int newReviewID = reader.IsDBNull(reader.GetOrdinal("NewReviewID"))
                                            ? -1
                                            : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("NewReviewID")));
                    return (result, newReviewID);
                }

                return ("UNKNOWN", -1);
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(CreateReview), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(CreateReview), ex);
                throw;
            }
        }

        // ======================== [ UPDATE REVIEW ] ========================
        public static string UpdateReview(int reviewID, UpdateReviewModel model, int updatedBy)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_UpdateReview", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ReviewID", reviewID);
                cmd.Parameters.AddWithValue("@Rating", model.Rating);
                cmd.Parameters.AddWithValue("@Comment", model.Comment ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(UpdateReview), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(UpdateReview), ex);
                throw;
            }
        }

        // ======================== [ DELETE REVIEW ] ========================
        public static string DeleteReview(int reviewID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_DeleteReview", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ReviewID", reviewID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(DeleteReview), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(ReviewRepository), nameof(DeleteReview), ex);
                throw;
            }
        }

        // ======================== [ MAPPER ] ========================
        private static Review MapReview(SqlDataReader reader) => new Review
        {
            ReviewID = reader.GetInt32(reader.GetOrdinal("ReviewID")),
            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            BookID = reader.GetInt32(reader.GetOrdinal("BookID")),
            BookTitle = reader.GetString(reader.GetOrdinal("BookTitle")),
            Rating = reader.GetByte(reader.GetOrdinal("Rating")),
            Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy"))
        };

    }
}
