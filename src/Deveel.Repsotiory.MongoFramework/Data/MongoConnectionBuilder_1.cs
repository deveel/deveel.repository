// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// A typed builder for a <see cref="IMongoDbConnection"/> that
	/// is specific for a given <see cref="IMongoDbContext"/>.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the <see cref="IMongoDbContext"/> that is
	/// this builder is specific for.
	/// </typeparam>
	public sealed class MongoConnectionBuilder<TContext> : MongoConnectionBuilder where TContext : class, IMongoDbContext  {
		private readonly MongoConnectionBuilder builder;

		internal MongoConnectionBuilder(MongoConnectionBuilder builder) {
			this.builder = builder;
		}

		/// <inheritdoc/>
		public override IMongoDbConnection Connection => new MongoDbConnection<TContext>(builder.Connection);

		internal override bool IsUsingTenant => builder.IsUsingTenant;

		internal override Type? TenantType => builder.TenantType;
	}
}
