﻿using SportFieldBooking.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using SportFieldBooking.Biz.Model.User;
using Microsoft.EntityFrameworkCore;
using SportFieldBooking.Helper;
using SportFieldBooking.Helper.Pagination;
using SportFieldBooking.Helper.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using static SportFieldBooking.Helper.ObjectHelper;

namespace SportFieldBooking.Biz.User
{
    public class Repository : IRepository
    {
        private readonly DomainDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IRepositoryWrapper> _logger;
        private readonly IMapper _mapper;

        public Repository(DomainDbContext context, IConfiguration configuration, ILogger<RepositoryWrapper> logger, IMapper mapper)
        {
            _dbContext = context;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        // Read from jwt token and return the neccessary information
        //private CurrentUser GetCurrentUser(HttpContext httpContext)
        //{
            
        //}

        /// <summary>
        /// Auth: Hung
        /// Created: 18/04/2022
        /// Method tao moi mot user (async)
        /// </summary>
        /// <param name="model">Biz model cho tao moi user</param>
        /// <returns>Biz model khi view mot user</returns>
        public async Task<View> CreateAsync(New model)
        {
            // Do data tu biz model New vao data model User thong qua AutoMapper
            var newUser = _mapper.Map<Data.Model.User>(model);

            // Add data model user entity itemData vao database roi save changes
            _dbContext.Users?.Add(newUser);
            await _dbContext.SaveChangesAsync();

            // Tra ve view
                // Do data tu biz model New vao biz model View thong qua AutoMapper 
            var userView = _mapper.Map<View>(newUser);
                // return view
            return userView;
        }

        /// <summary>
        /// Auth: Hung
        /// Created: 19/04/2022
        /// Method lay nhung thong tin ve mot user (async)
        /// </summary>
        /// <param name="id">id cua user trong database</param>
        /// <returns>Biz model cho view mot user</returns>
        /// <exception cref="Exception">Khi user voi id da nhap khong ton tai, xu ly o controller</exception>
        public async Task<View> GetAsync(long id)
        {
            var user = await _dbContext.Users.FindAsync(id);    
            if (user != null)
            {
                var userView = _mapper.Map<View>(user);
                return userView;
            }
            else
            {
                // Throw expception cho controller xu ly
                throw new Exception($"There's no user with the id {id}");
            }
        }

        /// <summary>
        /// Auth: Hung
        /// Created: 25/04/2022
        /// Method lay thong tin cua nguoi dung ve theo trang
        /// </summary>
        /// <param name="pageIndex"> So thu tu trang </param>
        /// <param name="pageSize"> So ban ghi trong mot trang </param>
        /// <returns> Page object - mot trang chua cac thong tin cua nguoi dung kem voi mot so thong tin khac</returns>
        public async Task<Page<List>> GetListAsync(HttpContext httpContext, long pageIndex, int pageSize)
        {
            #region using claims in jwt
            //var identity = httpContext.User.Identity as ClaimsIdentity;
            //if (identity != null)
            //{
            //    //IEnumerable<Claim> claims = identity.Claims;
            //    //foreach (var claim in claims)
            //    //{
            //    //    Console.WriteLine(claim);
            //    //}
            //    //Console.WriteLine($"Code: {identity.FindFirst("Code")?.Value}");
            //}
            //else
            //{
            //    Console.WriteLine("null identity");
            //}
            #endregion
            
            var userPage = await _dbContext.Users?.OrderBy(u => u.Id).GetPagedResult<Data.Model.User, List>(_mapper, pageIndex, pageSize);
            return userPage;
        }

        /// <summary>
        /// Auth: Hung
        /// Created: 25/04/2022
        /// Method xoa mot nguoi dung
        /// </summary>
        /// <param name="id"> id cua nguoi dung </param>
        /// <returns></returns>
        /// <exception cref="Exception"> Khi user co id da nhap khong ton tai, xu ly o controller </exception>
        public async Task DeleteAsync(long id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Throw exception cho controller xu ly
                throw new Exception($"There's no user with the id {id}");
            }
            
        }

        /// <summary>
        /// Auth: Hung
        /// Created: 25/02/2022
        /// Method cap nhat thong tin mot nguoi dung
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"> Khi user co id da nhap khong ton tai, xu ly o controller </exception>
        public async Task<View> UpdateAsync(Edit model)
        {
            var oldUser = await _dbContext.Users.FindAsync(model.Id);
            if(oldUser != null)
            {
                var updatedUser = _mapper.Map(model, oldUser);
                _dbContext.Users.Update(updatedUser);
                await _dbContext.SaveChangesAsync();
                var updatedUserView = _mapper.Map<View>(updatedUser);
                return updatedUserView;
            }
            else
            {
                throw new Exception($"There's no user with the id {model.Id}");
            }
        }
        
        /// <summary>
        /// Auth: Hung
        /// Created: 25/04/2022
        /// Method tim kiem nhung nguoi dung co username chua mot string nhat dinh
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns> Nhung nguoi dung co username thoa man dieu kien </returns>
        public async Task<Page<List>> SearchUsernameAsync(string username, long pageIndex, int pageSize)
        {
            var matchedUsers = await _dbContext.Users.Where(u => u.Username.Contains(username)).OrderBy(u => u.Id).GetPagedResult<Data.Model.User, List>(_mapper, pageIndex, pageSize);
            return matchedUsers;
        }

        /// <summary>
        /// Auth: Hung
        /// Created: 25/04/2022
        /// Method loc ra cac nguoi dung duoc tao truoc/sau/vao mot ngay nhat dinh
        /// </summary>
        /// <param name="dateStr"> ngay duoc chon lam moc, co format: yyyy-mm-dd </param>
        /// <param name="condition"> dieu kien loc after: sau, before: truoc, equal: bang voi ngay lam moc</param>
        /// <param name="pageIndex"> so thu tu trang </param>
        /// <param name="pageSize"> so ban ghi trong mot trang </param>
        /// <returns> Cac nguoi dung duoc loc ra </returns>
        /// <exception cref="Exception"> Dieu kien loc khong hop le hoac thoi gian nhap sai format </exception>
        public async Task<Page<List>> FilterCreatedDateAsync(string dateStr, string condition, long pageIndex, int pageSize)
        {
            // dateStr format: yyyy-mm-dd
            DateTime date;
            var isValidDate = DateTime.TryParse(dateStr, out date);
            if (isValidDate)
            {
                switch (condition.ToLower())
                {
                    
                    case Consts.TIME_BEFORE:
                        var matchedUsers = await _dbContext.Users.Where(u => DateTime.Compare(u.Created.Date, date.Date) < 0).OrderByDescending(u => u.Created).GetPagedResult<Data.Model.User, List>(_mapper, pageIndex, pageSize);
                        return matchedUsers;
                    case Consts.TIME_AFTER:
                        matchedUsers = await _dbContext.Users.Where(u => DateTime.Compare(u.Created.Date, date.Date) > 0).GetPagedResult<Data.Model.User, List>(_mapper, pageIndex, pageSize);
                        return matchedUsers;
                    case Consts.TIME_EQUAL:
                        matchedUsers = await _dbContext.Users.Where(u => DateTime.Compare(u.Created.Date, date.Date) == 0).GetPagedResult<Data.Model.User, List>(_mapper, pageIndex, pageSize);
                        return matchedUsers;
                    default:
                        throw new Exception($"Filtering condition not recognized!");
                }
            }
            else
            {
                throw new Exception($"Invalid DateTime data");
            }
            
        }

        /// <summary>
        /// Auth: Hung
        /// Created: 30/04/2022
        /// method update so du trong tai khoan nguoi dung
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<View> UpdateBalanceAsync(long id, long amount)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user != null)
            {
                if(user.Balance + amount > 0){
                    user.Balance += amount;
                    await _dbContext.SaveChangesAsync();
                    var userView = _mapper.Map<View>(user);
                    return userView;
                }
                else
                {
                    throw new Exception("Not enough money!");
                }
            }
            else
            {
                throw new Exception($"There is no user with the id {id}");
            }
        }

        public async Task<CurrentUser> GetLoginAsync (string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception($"There's no user with the email {email}");
            }
            var currentUser = _mapper.Map<CurrentUser>(user);
            return currentUser;
        }

        public async Task<Boolean> CheckCredentialsAsync(CurrentUser model, string password)
        {
            return String.Equals(model.Password, password);
        }
    }
}
