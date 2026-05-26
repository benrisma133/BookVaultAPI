// BookVault.Repository/Repositories/UserRepository.cs
using BookVault.Repository.Data;
using BookVault.Repository.Loggers;
using BookVault.Repository.Models.UserModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BookVault.Repository.Repositories
{
    public static class UserRepository
    {
        private static string ConnectionString => DatabaseHelper.ConnectionString;

        // ======================== [ GET ALL USERS ] ========================
        public static List<User> GetAllUsers()
        {
            var list = new List<User>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetAllUsers", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapUser(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetAllUsers), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetAllUsers), ex);
                throw;
            }

            return list;
        }

        // ======================== [ GET USER BY ID ] ========================
        public static User? GetUserByID(int userID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetUserByID", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetUserByID), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetUserByID), ex);
                throw;
            }
        }

        // ======================== [ GET USER BY EMAIL ] ========================
        public static User? GetUserByEmail(string email)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetUserByEmail", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetUserByEmail), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetUserByEmail), ex);
                throw;
            }
        }

        // ======================== [ REGISTER ] ========================
        public static (string Result, int NewUserID) Register(RegisterModel model, string passwordHash)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_Register", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string result = reader.GetString(reader.GetOrdinal("Result"));
                    int newUserID = reader.IsDBNull(reader.GetOrdinal("NewUserID"))
                                          ? -1
                                          : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("NewUserID")));
                    return (result, newUserID);
                }

                return ("UNKNOWN", -1);
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(Register), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(Register), ex);
                throw;
            }
        }

        // ======================== [ UPDATE USER ] ========================
        public static string UpdateUser(int userID, UpdateUserModel model)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_UpdateUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@Name", model.Name);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdateUser), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdateUser), ex);
                throw;
            }
        }

        // ======================== [ UPDATE EMAIL ] ========================
        public static string UpdateEmail(int userID, string newEmail)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_UpdateEmail", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@NewEmail", newEmail);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdateEmail), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdateEmail), ex);
                throw;
            }
        }

        // ======================== [ UPDATE PASSWORD ] ========================
        public static string UpdatePassword(int userID, string newPasswordHash)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_UpdatePassword", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);

                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdatePassword), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdatePassword), ex);
                throw;
            }
        }

        // ======================== [ PROMOTE TO ADMIN ] ========================
        public static string PromoteToAdmin(int userID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_PromoteToAdmin", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(PromoteToAdmin), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(PromoteToAdmin), ex);
                throw;
            }
        }

        // ======================== [ DEMOTE TO MEMBER ] ========================
        public static string DemoteToMember(int userID, int callerUserID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_DemoteToMember", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@CallerUserID", callerUserID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(DemoteToMember), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(DemoteToMember), ex);
                throw;
            }
        }

        // ======================== [ UPDATE PERMISSIONS ] ========================
        public static string UpdatePermissions(int userID, int permissions)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_UpdatePermissions", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@Permissions", permissions);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdatePermissions), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(UpdatePermissions), ex);
                throw;
            }
        }

        // ======================== [ DELETE USER ] ========================
        public static string DeleteUser(int userID, int callerUserID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_DeleteUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@CallerUserID", callerUserID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(DeleteUser), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(DeleteUser), ex);
                throw;
            }
        }

        // ======================== [ SAVE REFRESH TOKEN ] ========================
        public static string SaveRefreshToken(SaveRefreshTokenModel model)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_SaveRefreshToken", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", model.UserID);
                cmd.Parameters.AddWithValue("@Token", model.Token);
                cmd.Parameters.AddWithValue("@ExpiresAt", model.ExpiresAt);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(SaveRefreshToken), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(SaveRefreshToken), ex);
                throw;
            }
        }

        // ======================== [ GET REFRESH TOKEN ] ========================
        public static RefreshToken? GetRefreshToken(string token)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetRefreshToken", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Token", token);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapRefreshToken(reader) : null;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetRefreshToken), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(GetRefreshToken), ex);
                throw;
            }
        }

        // ======================== [ REVOKE REFRESH TOKEN ] ========================
        public static string RevokeRefreshToken(string token)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_RevokeRefreshToken", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Token", token);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(RevokeRefreshToken), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(RevokeRefreshToken), ex);
                throw;
            }
        }

        // ======================== [ REVOKE ALL USER REFRESH TOKENS ] ========================
        public static string RevokeAllUserRefreshTokens(int userID)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_RevokeAllUserRefreshTokens", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return reader.GetString(reader.GetOrdinal("Result"));

                return "UNKNOWN";
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(RevokeAllUserRefreshTokens), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(UserRepository), nameof(RevokeAllUserRefreshTokens), ex);
                throw;
            }
        }

        // ======================== [ MAPPERS ] ========================
        private static User MapUser(SqlDataReader reader) => new User
        {
            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Role = reader.GetByte(reader.GetOrdinal("Role")),
            Permissions = reader.GetInt32(reader.GetOrdinal("Permissions")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };

        private static RefreshToken MapRefreshToken(SqlDataReader reader) => new RefreshToken
        {
            RefreshTokenID = reader.GetInt32(reader.GetOrdinal("RefreshTokenID")),
            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
            Token = reader.GetString(reader.GetOrdinal("Token")),
            ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            RevokedAt = reader.IsDBNull(reader.GetOrdinal("RevokedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("RevokedAt")),
            IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked"))
        };
    }
}