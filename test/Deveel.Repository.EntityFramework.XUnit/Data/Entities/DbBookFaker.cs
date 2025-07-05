using Bogus;

namespace Deveel.Data.Entities
{
	public class DbBookFaker : Faker<DbBook>
	{
		public DbBookFaker(string userId)
		{
			RuleFor(x => x.Title, f => f.Lorem.Sentence());
			RuleFor(x => x.Author, f => f.Name.FullName());
			RuleFor(x => x.Synopsis, f => f.Lorem.Paragraph());
			RuleFor(x => x.UserId, f => f.Random.Bool() ? userId : f.Random.Guid().ToString());
		}
	}
}
