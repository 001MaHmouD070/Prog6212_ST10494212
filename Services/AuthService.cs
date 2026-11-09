using Microsoft.EntityFrameworkCore;
using Raceday.API.DTOs;
using RacedayAPI.Data;
using RacedayAPI.Models;

namespace RacedayAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly RaceDayDbContext _context;

        public AuthService(RaceDayDbContext context)
        {
            _context = context;
        }

        // REGISTER
        public async Task<UserResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            //Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
            {
                return null; // Email already taken
            }

            // Validate role
            if (registerDto.Role != "Organiser" && registerDto.Role != "Participant")
            {
                return null; // Invalid role
            }

            // Hash the password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create the User
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = hashedPassword,
                FullName = registerDto.FullName,
                Role = registerDto.Role,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create the role-specific profile
            if (registerDto.Role == "Organiser")
            {
                var organiser = new Organiser
                {
                    UserID = user.UserID,
                    CompanyName = registerDto.CompanyName ?? "Unnamed Company",
                    PhoneNumber = registerDto.PhoneNumber ?? "Not provided",
                    IsVerified = false
                };
                _context.Organisers.Add(organiser);
            }
            else // Participant
            {
                var participant = new Participant
                {
                    UserID = user.UserID,
                    DateOfBirth = registerDto.DateOfBirth ?? DateTime.Now,
                    Gender = registerDto.Gender ?? "Not specified",
                    EmergencyContact = registerDto.EmergencyContact ?? "Not provided",
                    MedicalConditions = registerDto.MedicalConditions
                };
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            // Return the response (without password hash!)
            return new UserResponseDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        // LOGIN
        public async Task<UserResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // 1. Find the user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return null; // User not found
            }

            // 2. Verify the password with BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null; // Wrong password
            }

            // 3. Return the response
            return new UserResponseDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        // GET USER BY ID
        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return null;

            return new UserResponseDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}