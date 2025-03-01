using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data
{
	public class DbBookRepository : EntityUserRepository<DbBook, Guid, string>
	{
		public DbBookRepository(DbContext context, IUserAccessor<string> userAccessor, ILogger<DbBookRepository>? logger = null)
			: base(context, userAccessor, logger)
		{
		}
	}
}
