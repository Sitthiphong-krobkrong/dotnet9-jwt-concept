using dotnet9_jwt_concept.Helper;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Microsoft.AspNetCore.Authorization;
using dotnet9_jwt_concept.Models.Core;

namespace dotnet9_jwt_concept.Core
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<(User,bool,string)> AddAsync(User user);
        Task<(bool,string)> UpdateAsync(User user);
        Task<(bool, string)> DeleteAsync(int id);
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

        public async Task<(User,bool,string)> AddAsync(User user)
        {
            var userProfile = _jwtHelper.DecodeUserFromToken();
            if (userProfile == null)
            {
                return (null, false, "Token invalid or expired");
            }

            user.record_status = "A";
            user.create_user_date = DateTime.UtcNow;
            user.create_user_id = userProfile.user_id;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return (user,true,"เพิ่มข้อมูลสำเร็จ");
        }

        public async Task<(bool,string)> DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return (false,"ไม่พบข้อมูลผู้ใช้งาน");

            var userProfile = _jwtHelper.DecodeUserFromToken();
            if (userProfile == null)
            {
                return (false, "Token invalid or expired");
            }

            user.record_status = "I";
            user.update_user_date = DateTime.UtcNow;
            user.update_user_id = userProfile.user_id;
            await _db.SaveChangesAsync();
            return (true,"ลบข้อมูลสำเร็จ");
        }

        public async Task<(bool,string)> UpdateAsync(User user)
        {
            // 1. เช็คว่ามี user นี้จริงไหม
            var dbUser = await _db.Users.FindAsync(user.user_id);
            if (dbUser == null) 
            {
                return (false, "ไม่พบผู้ใช้งาน");
            }

            var checkDuplicateUserName = await GetByUserNameAsync(user.user_name);
            if (checkDuplicateUserName != null 
                && checkDuplicateUserName.user_id != user.user_id)
            { 
                return (false, "user_name ซ้ำ กรุณาใช้ user_name อื่น"); 
            }

            var userProfile = _jwtHelper.DecodeUserFromToken();
            if (userProfile == null)
            {
                return (false, "Token invalid or expired");
            }

            // 2. อัปเดตเฉพาะ field ที่ควรแก้ไข
            dbUser.user_name = user.user_name;
            dbUser.user_pass = user.user_pass;
            dbUser.user_fname = user.user_fname;
            dbUser.user_lname = user.user_lname;
            dbUser.update_user_date = DateTime.UtcNow;
            dbUser.update_user_id = userProfile.user_id;
            await _db.SaveChangesAsync();
            return (true, "อัพเดทข้อมูลสำเร็จ"); 
        }

        public async Task<User?> GetByUserNameAsync(string user_name)
        {
            string tempUserName = user_name?.Trim()?.ToLower();
            return await _db.Users.FirstOrDefaultAsync(u => u.user_name == tempUserName);
        }

        public async Task<User?> CheckUserLoginAsync(string user_name, string user_pass)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.user_name == user_name && u.user_pass == user_pass);
        }
    }
}

