using System;

namespace Deveel.Data {
    /// <summary>
    /// A marker contract that declares explicitly an entity
    /// that can be managed by a repository
    /// </summary>
    /// <remarks>
    /// The meaning of forcing classes to implement this interface
    /// in order to be managed by <see cref="IRepository"/> instances
    /// is to avoid non-intentional usage, that might lead to several
    /// issues at runtime (such as serialization, behaviors, etc.)
    /// </remarks>
    public interface IDataEntity {
        /// <summary>
        /// Gets a unique identifier of the entity
        /// </summary>
        /// <remarks>
        /// The returned value of the property might be
        /// <c>null</c> or an empty string, when the entity
        /// is not established.
        /// </remarks>
        string? Id { get; }
    }
}
