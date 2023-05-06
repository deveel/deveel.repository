using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
    /// <summary>
    /// Defines a class as an entity of a repository.
    /// </summary>
    /// <remarks>
    /// This marker attribute is used to fix issues in the discovery
    /// of entities managed by a repository, when the repository is
    /// generic and multiple types are used as entities (eg. facades,
    /// depdendencies, etc.).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityTypeAttribute : Attribute {
        /// <summary>
        /// Constructs the attribute for the given entity type.
        /// </summary>
        /// <param name="entityType">
        /// The type of the repository entity.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given <paramref name="entityType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the given <paramref name="entityType"/> is not a class or is abstract.
        /// </exception>
        public EntityTypeAttribute(Type entityType) {
            Guard.IsNotNull(entityType, nameof(entityType));
            Guard.IsTrue(entityType.IsClass && !entityType.IsAbstract, nameof(entityType), 
                $"The type '{entityType}' must be a non-abstract class");

            EntityType = entityType;
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        public Type EntityType { get; }

    }
}
