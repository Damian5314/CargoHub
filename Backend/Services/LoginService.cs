using StarterKit.Models;
using System;

namespace StarterKit.Services
{
    public class LoginService : ILoginService
    {
        private readonly DatabaseContext _context;

        public LoginService(DatabaseContext context)
        {
            _context = context;
        }

        // Check Password for both User and Admin
        public LoginStatus CheckPassword(string email, string password)
        {
            Console.WriteLine($"[CheckPassword] Validating user with email: {email}");

            var admin = _context.Admin.FirstOrDefault(a => a.Email == email);
            if (admin != null)
            {
                return admin.Password == password ? LoginStatus.Success : LoginStatus.IncorrectPassword;
            }

            var user = _context.User.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                return user.Password == password ? LoginStatus.Success : LoginStatus.IncorrectPassword;
            }

            Console.WriteLine($"[CheckPassword] Email '{email}' not found.");
            return LoginStatus.IncorrectEmail;
        }

        // Register User
        public bool RegisterUser(string firstName, string lastName, string email, string password, string recuringDays)
        {
            try
            {
                Console.WriteLine($"[RegisterUser] Attempting to register user: {email}");

                if (_context.User.Any(u => u.Email == email))
                {
                    Console.WriteLine($"[RegisterUser] Email '{email}' already exists.");
                    return false;
                }

                var user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password, // Store plain-text password
                    Role = "User",
                    RecuringDays = recuringDays
                };

                _context.User.Add(user);
                _context.SaveChanges();

                Console.WriteLine($"[RegisterUser] User '{email}' registered successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterUser] Error: {ex.Message}");
                return false;
            }
        }

        // Register Admin
        public bool RegisterAdmin(string userName, string email, string password)
        {
            try
            {
                Console.WriteLine($"[RegisterAdmin] Attempting to register admin: {email}");

                if (_context.Admin.Any(a => a.UserName == userName))
                {
                    throw new InvalidOperationException("Admin username already exists.");
                }

                if (_context.Admin.Any(a => a.Email == email))
                {
                    throw new InvalidOperationException("Admin email already exists.");
                }

                var newAdmin = new Admin
                {
                    UserName = userName,
                    Email = email,
                    Password = password // Store plain-text password
                };

                _context.Admin.Add(newAdmin);
                _context.SaveChanges();

                Console.WriteLine($"[RegisterAdmin] Admin '{email}' registered successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterAdmin] Error: {ex.Message}");
                return false;
            }
        }

        // Admin Login
        public (bool isValid, string role) AdminLogin(string email, string password)
        {
            Console.WriteLine($"[AdminLogin] Checking login for admin: {email}");

            var admin = _context.Admin.FirstOrDefault(a => a.Email == email && a.Password == password);
            if (admin != null)
            {
                Console.WriteLine($"[AdminLogin] Admin '{email}' logged in successfully.");
                return (true, "Admin");
            }

            Console.WriteLine($"[AdminLogin] Invalid credentials for admin: {email}");
            return (false, string.Empty);
        }

        // Get User ID by Email
        public int GetUserIdByEmail(string email)
        {
            Console.WriteLine($"[GetUserIdByEmail] Fetching user ID for email: {email}");
            var user = _context.User.FirstOrDefault(u => u.Email == email);
            return user?.UserId ?? 0;
        }
        public int GetAdminIdByEmail(string email)
        {
            Console.WriteLine($"[GetUserIdByEmail] Fetching user ID for email: {email}");
            var user = _context.Admin.FirstOrDefault(u => u.Email == email);
            return user?.AdminId ?? 0;
        }

        // Get User Name by ID
        public string GetFirstNameByEmail(string email)
        {
            Console.WriteLine($"[GetUserIdByEmail] Fetching user name for Email: {email}");
            var user = _context.User.FirstOrDefault(u => u.Email == email);
            return user?.FirstName ?? "null";
        }

        public string GetUserNameByEmailAdmin(string email)
        {
            Console.WriteLine($"[GetUserIdByEmail] Fetching user name for Email: {email}");
            var user = _context.Admin.FirstOrDefault(u => u.Email == email);
            return user?.UserName ?? "null";
        }

        // Generate Password Reset Token
        public bool GeneratePasswordResetToken(string email)
        {
            try
            {
                Console.WriteLine($"[GeneratePasswordResetToken] Generating reset token for email: {email}");

                var user = _context.User.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    Console.WriteLine($"[GeneratePasswordResetToken] Email '{email}' not found.");
                    return false;
                }

                user.PasswordResetToken = Guid.NewGuid().ToString();
                user.TokenExpiry = DateTime.UtcNow.AddMinutes(30);

                _context.SaveChanges();
                Console.WriteLine($"[GeneratePasswordResetToken] Token generated and saved for '{email}'.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeneratePasswordResetToken] Error: {ex.Message}");
                return false;
            }
        }

        // Reset Password
        public bool ResetPassword(string token, string newPassword)
        {
            try
            {
                Console.WriteLine($"[ResetPassword] Resetting password for token: {token}");

                var user = _context.User.FirstOrDefault(u => u.PasswordResetToken == token && u.TokenExpiry > DateTime.UtcNow);
                if (user == null)
                {
                    Console.WriteLine($"[ResetPassword] Invalid or expired token.");
                    return false;
                }

                user.Password = newPassword; // Store plain-text password
                user.PasswordResetToken = null;
                user.TokenExpiry = null;

                _context.SaveChanges();
                Console.WriteLine($"[ResetPassword] Password reset successfully for user: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ResetPassword] Error: {ex.Message}");
                return false;
            }
        }

        // Validate Reset Token
        public bool ValidateResetToken(string token)
        {
            Console.WriteLine($"[ValidateResetToken] Validating token: {token}");
            return _context.User.Any(u => u.PasswordResetToken == token && u.TokenExpiry > DateTime.UtcNow);
        }
    }
}
