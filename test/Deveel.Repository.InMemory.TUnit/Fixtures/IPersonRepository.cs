namespace Deveel.Data {
	public interface IPersonRepository : IRepository<Person> {
		Task<Person?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
	}
}
