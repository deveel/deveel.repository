namespace Deveel.Data {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityTypeAttribute : Attribute {
        public Type EntityType { get; }

        public EntityTypeAttribute(Type entityType) {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

            if (!EntityType.IsClass || EntityType.IsAbstract)
                throw new ArgumentException($"The type '{EntityType}' must be a non-abstract class");
        }
    }
}
