using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace Deveel.Data {
	public class PersonValidator : IEntityValidator<Person> {
		public async IAsyncEnumerable<ValidationResult> ValidateAsync(EntityManager<Person> manager, Person person, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
			if (person.Email != null && !MailAddress.TryCreate(person.Email, out var _))
				yield return new ValidationResult("The email address is not valid", new[] {nameof(Person.Email)});
		}
	}
}
