using AGD.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Repositories.Repositories
{
    public interface IUnitOfWork : IDisposable
    {       
        JwtHelper JwtHelper { get; }
        Task<int> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        //private readonly DbContext _context;
        private readonly JwtHelper _jwtHelper;

        public UnitOfWork(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }

        public JwtHelper JwtHelper => _jwtHelper;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
