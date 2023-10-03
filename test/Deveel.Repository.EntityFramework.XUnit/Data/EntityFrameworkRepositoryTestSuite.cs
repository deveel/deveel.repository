using Bogus;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class EntityFrameworkRepositoryTestSuite : EntityFrameworkRepositoryTestSuite<DbPerson> {
		public EntityFrameworkRepositoryTestSuite(SqlTestConnection sql, ITestOutputHelper? testOutput) 
			: base(sql, testOutput) {
		}

		protected override Faker<DbPerson> PersonFaker => new DbPersonFaker();
	}
}
