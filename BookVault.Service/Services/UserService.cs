// BookVault.Service/Services/UserService.cs
using BCrypt.Net;
using BookVault.Repository.Models.UserModels;
using BookVault.Repository.Repositories;
using BookVault.Service.Enums.User;

namespace BookVault.Service.Services
{
    public class UserService
    {
        // ─── enMode ────────────────────────────────────────────────────────
        public enum enMode { AddNew, Update }
        private enMode _Mode;

        // ─── Properties ────────────────────────────────────────────────────
        public int UserID { get; private set; }
        public string Name { get; set; } = null!;
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public byte Role { get; private set; }
        public int Permissions { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // ─── Constructor: from existing User (Update mode) ─────────────────
        public UserService(User user, enMode mode = enMode.Update)
        {
            UserID = user.UserID;
            Name = user.Name;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
            Role = user.Role;
            Permissions = user.Permissions;
            CreatedAt = user.CreatedAt;
            _Mode = mode;
        }

        // ─── Constructor: empty (AddNew mode) ──────────────────────────────
        public UserService()
        {
            _Mode = enMode.AddNew;
        }

        // ─── Static: Register ──────────────────────────────────────────────
        public static enUserRegisterResult Register(RegisterModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                    return enUserRegisterResult.Failed;

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var (result, _) = UserRepository.Register(model, passwordHash);

                return result switch
                {
                    "CREATED" => enUserRegisterResult.Registered,
                    "EMAIL_TAKEN" => enUserRegisterResult.EmailTaken,
                    _ => enUserRegisterResult.Failed
                };
            }
            catch
            {
                return enUserRegisterResult.Failed;
            }
        }

        // ─── Static: Login ─────────────────────────────────────────────────
        public static (enUserLoginResult result, UserService? service) Login(LoginModel model)
        {
            try
            {
                User? user = UserRepository.GetUserByEmail(model.Email);

                if (user is null)
                    return (enUserLoginResult.InvalidCredentials, null);

                bool passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

                if (!passwordValid)
                    return (enUserLoginResult.InvalidCredentials, null);

                return (enUserLoginResult.Success, new UserService(user, enMode.Update));
            }
            catch
            {
                return (enUserLoginResult.Failed, null);
            }
        }

        // ─── Private: Update Name ──────────────────────────────────────────
        private enUserSaveResult _Update()
        {
            try
            {
                var model = new UpdateUserModel { Name = Name };

                string result = UserRepository.UpdateUser(UserID, model);

                return result switch
                {
                    "UPDATED" => enUserSaveResult.Saved,
                    "NOT_FOUND" => enUserSaveResult.NotFound,
                    _ => enUserSaveResult.Failed
                };
            }
            catch
            {
                return enUserSaveResult.Failed;
            }
        }

        // ─── Public: Save ──────────────────────────────────────────────────
        public enUserSaveResult Save()
        {
            return _Mode switch
            {
                enMode.Update => _Update(),
                _ => enUserSaveResult.Failed
            };
        }

        // ─── Public: UpdateEmail ───────────────────────────────────────────
        public enUserSaveResult UpdateEmail(string newEmail)
        {
            try
            {
                string result = UserRepository.UpdateEmail(UserID, newEmail);

                return result switch
                {
                    "UPDATED" => enUserSaveResult.Saved,
                    "NOT_FOUND" => enUserSaveResult.NotFound,
                    "EMAIL_TAKEN" => enUserSaveResult.EmailTaken,
                    _ => enUserSaveResult.Failed
                };
            }
            catch
            {
                return enUserSaveResult.Failed;
            }
        }

        // ─── Public: UpdatePassword ────────────────────────────────────────
        public enUserPasswordResult UpdatePassword(string currentPassword, string newPassword)
        {
            try
            {
                // Verify current password in C# before touching the database
                bool currentPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, PasswordHash);

                if (!currentPasswordValid)
                    return enUserPasswordResult.InvalidCurrentPassword;

                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                string result = UserRepository.UpdatePassword(UserID, newPasswordHash);

                return result switch
                {
                    "UPDATED" => enUserPasswordResult.Updated,
                    "NOT_FOUND" => enUserPasswordResult.NotFound,
                    _ => enUserPasswordResult.Failed
                };
            }
            catch
            {
                return enUserPasswordResult.Failed;
            }
        }

        // ─── Static: PromoteToAdmin ────────────────────────────────────────
        public static enUserRoleResult PromoteToAdmin(int userID)
        {
            try
            {
                string result = UserRepository.PromoteToAdmin(userID);

                return result switch
                {
                    "PROMOTED" => enUserRoleResult.Promoted,
                    "NOT_FOUND" => enUserRoleResult.NotFound,
                    "ALREADY_ADMIN" => enUserRoleResult.AlreadyAdmin,
                    _ => enUserRoleResult.Failed
                };
            }
            catch
            {
                return enUserRoleResult.Failed;
            }
        }

        // ─── Static: DemoteToMember ────────────────────────────────────────
        public static enUserRoleResult DemoteToMember(int userID, int callerUserID)
        {
            try
            {
                string result = UserRepository.DemoteToMember(userID, callerUserID);

                return result switch
                {
                    "DEMOTED" => enUserRoleResult.Demoted,
                    "NOT_FOUND" => enUserRoleResult.NotFound,
                    "ALREADY_MEMBER" => enUserRoleResult.AlreadyMember,
                    "CANNOT_DEMOTE_SELF" => enUserRoleResult.CannotDemoteSelf,
                    _ => enUserRoleResult.Failed
                };
            }
            catch
            {
                return enUserRoleResult.Failed;
            }
        }

        // ─── Static: UpdatePermissions ─────────────────────────────────────
        public static enUserSaveResult UpdatePermissions(int userID, int permissions)
        {
            try
            {
                string result = UserRepository.UpdatePermissions(userID, permissions);

                return result switch
                {
                    "UPDATED" => enUserSaveResult.Saved,
                    "NOT_FOUND" => enUserSaveResult.NotFound,
                    _ => enUserSaveResult.Failed
                };
            }
            catch
            {
                return enUserSaveResult.Failed;
            }
        }

        // ─── Static: Delete ────────────────────────────────────────────────
        public static enUserDeleteResult Delete(int userID, int callerUserID)
        {
            try
            {
                string result = UserRepository.DeleteUser(userID, callerUserID);

                return result switch
                {
                    "DELETED" => enUserDeleteResult.Deleted,
                    "NOT_FOUND" => enUserDeleteResult.NotFound,
                    "CANNOT_DELETE_SELF" => enUserDeleteResult.CannotDeleteSelf,
                    _ => enUserDeleteResult.Failed
                };
            }
            catch
            {
                return enUserDeleteResult.Failed;
            }
        }

        // ─── Static: Find ──────────────────────────────────────────────────
        public static (enUserRetrieveResult result, UserService? service) Find(int userID)
        {
            try
            {
                User? user = UserRepository.GetUserByID(userID);

                if (user is null)
                    return (enUserRetrieveResult.NotFound, null);

                return (enUserRetrieveResult.Found, new UserService(user, enMode.Update));
            }
            catch
            {
                return (enUserRetrieveResult.Failed, null);
            }
        }

        // ─── Static: GetAll ────────────────────────────────────────────────
        public static (enUserRetrieveResult result, List<User> users) GetAll()
        {
            try
            {
                List<User> list = UserRepository.GetAllUsers();
                return (enUserRetrieveResult.Found, list);
            }
            catch
            {
                return (enUserRetrieveResult.Failed, new List<User>());
            }
        }

        // ─── Static: SaveRefreshToken ──────────────────────────────────────
        public static enRefreshTokenResult SaveRefreshToken(SaveRefreshTokenModel model)
        {
            try
            {
                string result = UserRepository.SaveRefreshToken(model);

                return result switch
                {
                    "SAVED" => enRefreshTokenResult.Saved,
                    _ => enRefreshTokenResult.Failed
                };
            }
            catch
            {
                return enRefreshTokenResult.Failed;
            }
        }

        // ─── Static: GetRefreshToken ───────────────────────────────────────
        public static (enRefreshTokenResult result, RefreshToken? token) GetRefreshToken(string token)
        {
            try
            {
                RefreshToken? refreshToken = UserRepository.GetRefreshToken(token);

                if (refreshToken is null)
                    return (enRefreshTokenResult.NotFound, null);

                if (refreshToken.IsRevoked)
                    return (enRefreshTokenResult.AlreadyRevoked, null);

                if (refreshToken.ExpiresAt < DateTime.UtcNow)
                    return (enRefreshTokenResult.Expired, null);

                return (enRefreshTokenResult.Saved, refreshToken);
            }
            catch
            {
                return (enRefreshTokenResult.Failed, null);
            }
        }

        // ─── Static: RevokeRefreshToken ────────────────────────────────────
        public static enRefreshTokenResult RevokeRefreshToken(string token)
        {
            try
            {
                string result = UserRepository.RevokeRefreshToken(token);

                return result switch
                {
                    "REVOKED" => enRefreshTokenResult.Revoked,
                    "NOT_FOUND" => enRefreshTokenResult.NotFound,
                    _ => enRefreshTokenResult.Failed
                };
            }
            catch
            {
                return enRefreshTokenResult.Failed;
            }
        }

        // ─── Static: RevokeAllUserRefreshTokens ────────────────────────────
        public static enRefreshTokenResult RevokeAllUserRefreshTokens(int userID)
        {
            try
            {
                string result = UserRepository.RevokeAllUserRefreshTokens(userID);

                return result switch
                {
                    "REVOKED" => enRefreshTokenResult.Revoked,
                    _ => enRefreshTokenResult.Failed
                };
            }
            catch
            {
                return enRefreshTokenResult.Failed;
            }
        }
    }
}