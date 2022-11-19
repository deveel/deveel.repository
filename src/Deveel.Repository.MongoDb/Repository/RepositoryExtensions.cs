// Copyright 2022 Deveel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MongoDB.Driver;

namespace Deveel.Data
{
    public static class RepositoryExtensions
    {
        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.FindAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.FindAllAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.CountAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.ExistsAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);
    }
}