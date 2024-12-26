using System.Threading.Tasks;
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
    }
}