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

using System;

namespace Deveel.Data {
    /// <summary>
    /// A factory that provides instances of transactions
    /// used to isolate the access to data layers of an underlying storage
    /// </summary>
	/// <seealso cref="ITransactionalRepository{TEntity}"/>
    public interface IDataTransactionFactory {
        /// <summary>
        /// Creates a new stransaction and starts it
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns a new instance of <see cref="IDataTransaction"/> that
        /// can be used in a repository to isolate
        /// the access to the data of an underlying storage
        /// </returns>
        Task<IDataTransaction> CreateTransactionAsync(CancellationToken cancellationToken = default);
    }
}