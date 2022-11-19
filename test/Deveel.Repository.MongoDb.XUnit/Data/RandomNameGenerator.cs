using System;

namespace Deveel.Data {
	public sealed class RandomNameGenerator {
		private readonly Random random;

		public RandomNameGenerator() {
			random= new Random();
		}

		private static readonly string[] FirstNames = {
			"Anthony", "Annabelle", "Francis", "Mark", "John", "Frank", "Cody",
			"Damon", "Albert", "Sophia", "Jeanne", "Jill", "Anna", "Bashir"
		};

		private static readonly string[] LastNames = {
			"Brown", "White", "Green", "Doe", "Fawkes", "Hill", "Gray", "Black",
			"Dowd", "Lee", "Young", "Brin"
		};

		public (string, string) NewName() {
			var firstNameIndex = random.Next(0, FirstNames.Length - 1);
			var lastNameIndex = random.Next(0, LastNames.Length - 1);

			return (FirstNames[firstNameIndex], LastNames[lastNameIndex]);
		}
	}
}
