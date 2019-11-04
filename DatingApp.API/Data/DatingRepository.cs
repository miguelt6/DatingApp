using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain == true);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p=>p.Photos).FirstOrDefaultAsync(u=>u.Id==id);
            return user;
        }
        
        //userParams is a class to store pagination, filtering and sorting
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            //asQueryable so we can apply Where (Linq)
            var users = _context.Users.Include(p=>p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            // don't show themselves on the list of users:
            users = users.Where(u => u.Id != userParams.UserId);

            // by default show the opposite gender 
            //unless something else is specified on userParams:
            users = users.Where(u => u.Gender == userParams.Gender);

            // if sprecified on userParams, we filter by age:
            if(userParams.MinAge != 18 || userParams.MaxAge !=99){
                 // the minimum date of birth depends on the maximum age preferred:
                 var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);

                 // the maximum date of birth depends on the minimum age preferred:
                 var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                 users= users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            // Check if there's a sorting preference:
            if(!string.IsNullOrEmpty(userParams.OrderBy)){
                switch (userParams.OrderBy){
                    case "created":
                        users=users.OrderByDescending(u=>u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }


            return await PagedList<User>.CreateAsync(users,userParams.PageNumber,userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}