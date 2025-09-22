using AGD.DAL.Basic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Repositories.Repositories
{
    //public class UserRepository : GenericRepository<User>
    //{
    //    private new readonly Dbcontext _context;
    //    public UserRepository() => _context ??= new Dbcontext();
    //    public UserRepository(Dbcontext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<User?> GetByUsernameAsync(string username)
    //    {
    //        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
    //    }

    //    public async Task<User?> GetByEmailAsync(string email)
    //    {
    //        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    //    }
    //}
}
