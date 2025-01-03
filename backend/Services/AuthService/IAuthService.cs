using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;

namespace backend.Services.AuthService
{
    public interface IAuthService
    {
        string GenerateSalt();
        string HashPassword(string password, string salt);
        bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt);
        Task<bool> IsEmailRegistered(string email);
        Task<string> GetNextUserId();
        Task<DocumentSnapshot> GetUserByEmail(string email);
        Task<DocumentSnapshot> GetUserById(string userId);
        Task<DocumentReference> RegisterUserAsync(string userId, string firstName, string lastName, string email, string salt, string hashedPassword, IList<Role> roles);
        Task<List<Dictionary<string, object>>> GetAllUsers();
        Task<bool> UpdateUserAsync(string userId, User updatedUser);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> DeleteAllUsersAsync();
    }
}