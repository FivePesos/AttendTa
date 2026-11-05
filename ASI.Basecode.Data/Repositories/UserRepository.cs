using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Task.FromResult<User>(null);
            }

            return this.GetDbSet<User>()
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.UserId == email, cancellationToken);
        }

        public Task<User> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
            {
                return Task.FromResult<User>(null);
            }

            return this.GetDbSet<User>()
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        }

        public async Task<string> GetRoleCodeAsync(RoleCode roleId, CancellationToken cancellationToken = default)
        {
            var role = await this.GetDbSet<RoleLookup>()
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.RoleId == roleId, cancellationToken);

            return role?.Code;
        }
    }
}
