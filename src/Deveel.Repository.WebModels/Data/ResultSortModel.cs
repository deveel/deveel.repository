using System;
using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public sealed class ResultSortModel : IResultSort {
		[Required]
		public string Field { get; set; }

		public bool? Ascending { get; set; }

		IFieldRef IResultSort.Field => new StringFieldRef(Field);

		bool IResultSort.Ascending => Ascending ?? false;
	}
}
