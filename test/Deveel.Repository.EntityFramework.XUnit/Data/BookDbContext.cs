using Microsoft.EntityFrameworkCore;

using System;

namespace Deveel.Data
{
	public class BookDbContext : DbContext
	{
		public BookDbContext(DbContextOptions<BookDbContext> options, IUserAccessor<string> userAccessor) : base(options)
		{
			this.userAccessor = userAccessor;
		}

		private readonly IUserAccessor<string> userAccessor;

		public DbSet<DbBook> Books { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<DbBook>()
				.HasKey(x => x.Id);

			modelBuilder.Entity<DbBook>()
				.HasOwnerFilter(nameof(DbBook.UserId), userAccessor);
		}
	}
}
