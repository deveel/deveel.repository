﻿using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public class RepositoryPageQueryResultModel<TItem> : RepositoryPageQueryResultBaseModel<TItem>
		where TItem : class, IEntity {
		public RepositoryPageQueryResultModel(RepositoryPageQueryModel<TItem> pageRequest, int totalItems, IEnumerable<TItem>? items = null) 
			: base(pageRequest, totalItems, items) {
		}

		public RepositoryPageQueryResultModel() {
		}

        [Required]
		public virtual RepositoryPageQueryModel<TItem> Query {
			get => PageQuery ?? throw new InvalidOperationException("The query was not set");
			set => PageQuery = value;
		}
	}
}
