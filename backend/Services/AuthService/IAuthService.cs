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
        Task<DocumentReference> RegisterUserAsync(string userId, string firstName, string lastName, string email, string salt, string hashedPassword);
        Task<List<Dictionary<string, object>>> GetAllUsers();
        Task<bool> UpdateUserAsync(string id, UpdateUser updateUser);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> DeleteAllUsersAsync();
    }
}