using dotnet9_jwt_concept.Helper;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Microsoft.AspNetCore.Authorization;

namespace dotnet9_jwt_concept.Core
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> AddAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id,int userId);
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> CheckUserLoginAsync(string userName, string passWord);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly JwtHelper _jwtHelper;
        public UserService(AppDbContext db, JwtHelper jwtHelper)
        {
            _db = db;
            _jwtHelper = jwtHelper;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _db.Users.AsNoTracking().ToListAsync();

        public async Task<User?> GetByIdAsync(int id)
            => await _db.Users.FindAsync(id);

        public async Task<User> AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return false;

            user.record_status = "I";
            user.update_user_date = DateTime.UtcNow;
            user.update_user_id = userId;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            // 1. เช็คว่ามี user นี้จริงไหม
            var dbUser = await _db.Users.FindAsync(user.user_id);
            if (dbUser == null) return false;

            // 2. อัปเดตเฉพาะ field ที่ควรแก้ไข
            dbUser.user_fname = user.user_fname;
            dbUser.user_lname = user.user_lname;
            dbUser.update_user_date = DateTime.UtcNow;
            //dbUser.update_user_id = userId;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetByUserNameAsync(string user_name)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.user_name == user_name);
        }

        public async Task<User?> CheckUserLoginAsync(string user_name, string user_pass)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.user_name == user_name && u.user_pass == user_pass);
        }
    }
}

