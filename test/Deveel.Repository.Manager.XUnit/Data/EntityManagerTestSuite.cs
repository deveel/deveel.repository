using Xunit.Abstractions;

namespace Deveel.Data {
	public class EntityManagerTestSuite : EntityManagerTestSuite<EntityManager<Person>> {
		public EntityManagerTestSuite(ITestOutputHelper testOutput) : base(testOutput) {
		}
	}
}
