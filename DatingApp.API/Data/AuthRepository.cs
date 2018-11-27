using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.Username == username);
            if(user == null){
                return null;
            }
            if(!VerfifyPassHash(password, user.PasswordHash, user.PasswordSalt)){
                return null;
            }
            return user;
        }

        private bool VerfifyPassHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
           using( var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){
               var ComputeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
               for(int i= 0; i < ComputeHash.Length; i++){
                   if(ComputeHash[i] != passwordHash[i]) return false;
               }
           }
           return true;
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
           byte[] passHash, passSalt;
           createPasswordHash(password,out passHash,out passSalt);
           user.PasswordHash = passHash;
           user.PasswordSalt = passSalt;
           await _context.Users.AddAsync(user);
           await _context.SaveChangesAsync();

            return user;
        }

        private void createPasswordHash(string password, out byte[] passHash, out byte[] passSalt)
        {
           using( var hmac = new System.Security.Cryptography.HMACSHA512()){
               passSalt = hmac.Key;
               passHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
           }
        }

        public async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(x=>x.Username == username)) return true;

            return false;
        }
    }
}