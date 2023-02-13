using Deveel.Data;
using Deveel.Data.Models;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Repository.TestApi.Models {
	public class PersonPageQueryModel : RepositoryPageQueryModel<TestPersonModel> {
		[FromQuery]
		public string? LastName { get; set; }

		[FromQuery]
		public int? MaxAge { get; set; }

		protected override void GetFilters(IList<IQueryFilter> filter) {
			if (!String.IsNullOrWhiteSpace(LastName))
				filter.Add(QueryFilter.Where<TestPersonModel>(x => x.LastName == LastName));

			if (MaxAge != null) {
				filter.Add(QueryFilter.Where<TestPersonModel>(x => x.BirthDate != null && ((DateTimeOffset.Now.Subtract(x.BirthDate.Value).Days / 365.25) <= MaxAge)));
			}
		}

		protected override void GetRouteValues(IDictionary<string, object> routeValues) {
			if (!String.IsNullOrWhiteSpace(LastName))
				routeValues["lastName"] = LastName;
			if (MaxAge != null)
				routeValues["maxAge"] = MaxAge;

			base.GetRouteValues(routeValues);
		}
	}
}
