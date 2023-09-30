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