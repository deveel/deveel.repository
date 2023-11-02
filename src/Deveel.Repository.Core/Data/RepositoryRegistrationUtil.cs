using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	static class RepositoryRegistrationUtil {
		public static bool IsValidRepositoryType(Type repositoryType)
			=> Implements(typeof(IRepository<>), repositoryType) ||
				Implements(typeof(IRepository<,>), repositoryType);

		public static bool IsValidProviderType(Type providerType)
			=> Implements(typeof(IRepositoryProvider<>), providerType) ||
				Implements(typeof(IRepositoryProvider<,>), providerType);

		public static bool Implements(Type genericType, Type type) {
			if (type.IsGenericType) {
				var genericTypeDefinition = type.GetGenericTypeDefinition();
				if (genericTypeDefinition == genericType)
					return true;
			}

			foreach (var iface in type.GetInterfaces()) {
				if (Implements(genericType, iface))
					return true;
			}

			var baseType = type.BaseType;
			while (baseType != null) {
				if (Implements(genericType, baseType))
					return true;

				baseType = baseType.BaseType;
			}

			return false;
		}

		private static Type? GetEntityType(Type serviceType) {
			if (serviceType.IsGenericType) {
				var genericTypeDefinition = serviceType.GetGenericTypeDefinition();
				var genericTypes = serviceType.GenericTypeArguments;

				if (genericTypes.Length == 1 && genericTypes[0].IsClass &&
					typeof(IRepository<>).IsAssignableFrom(genericTypeDefinition) ||
					typeof(IRepositoryProvider<>).IsAssignableFrom(genericTypeDefinition)) {
					return genericTypes[0];
				} else if (genericTypes.Length == 2 && genericTypes[0].IsClass &&
					typeof(IRepository<,>).IsAssignableFrom(genericTypeDefinition) || 
					typeof(IRepositoryProvider<,>).IsAssignableFrom(genericTypeDefinition)) {
					return genericTypes[0];
				}
			}

			foreach (var iface in serviceType.GetInterfaces()) {
				var entityType = GetEntityType(iface);
				if (entityType != null)
					return entityType;
			}

			return null;
		}

		private static Type? GetKeyType(Type serviceType) {
			if (serviceType.IsGenericType) {
				var genericTypeDefinition = serviceType.GetGenericTypeDefinition();
				var genericTypes = serviceType.GenericTypeArguments;

				if (genericTypes.Length == 2 && genericTypes[0].IsClass) {
					if (typeof(IRepository<,>).IsAssignableFrom(genericTypeDefinition) ||
						typeof(IRepositoryProvider<,>).IsAssignableFrom(genericTypeDefinition))
					return genericTypes[1];
				}
			}

			foreach (var iface in serviceType.GetInterfaces()) {
				var keyType = GetKeyType(iface);
				if (keyType != null)
					return keyType;
			}

			return null;
		}

		private static bool RegisterIfAssignable(IList<Type> types, Type genericType, Type entityType, Type repositoryType) {
			var serviceType = genericType.MakeGenericType(entityType);
			if (serviceType.IsAssignableFrom(repositoryType)) {
				if (!types.Contains(serviceType))
					types.Add(serviceType);

				return true;
			}

			return false;
		}

		private static bool RegisterIfAssignable(IList<Type> types, Type genericType, Type entityType, Type keyType, Type repositoryType) {
			var serviceType = genericType.MakeGenericType(entityType, keyType);
			if (serviceType.IsAssignableFrom(repositoryType)) {
				if (!types.Contains(serviceType))
					types.Add(serviceType);

				return true;
			}

			return false;
		}

		public static IReadOnlyList<Type> GetRepositoryServiceTypes(Type repositoryType) {
			if (!Implements(typeof(IRepository<>), repositoryType) &&
				!Implements(typeof(IRepository<,>), repositoryType))
				return Array.Empty<Type>();

			var types = new List<Type>();

			foreach (var iface in repositoryType.GetInterfaces()) {
				var entityType = GetEntityType(iface);

				if (entityType == null)
					// skip the type if we cannot determine the entity
					continue;

				if (RegisterIfAssignable(types, typeof(IRepository<>), entityType, repositoryType)) {
					RegisterIfAssignable(types, typeof(IQueryableRepository<>), entityType, repositoryType);
					RegisterIfAssignable(types, typeof(IFilterableRepository<>), entityType, repositoryType);
					RegisterIfAssignable(types, typeof(IPageableRepository<>), entityType, repositoryType);

					if (!types.Contains(iface))
						types.Add(iface);
				}

				var keyType = GetKeyType(iface);
				if (keyType != null) {
					if (RegisterIfAssignable(types, typeof(IRepository<,>), entityType, keyType, repositoryType)) {
						RegisterIfAssignable(types, typeof(IQueryableRepository<,>), entityType, keyType, repositoryType);
						RegisterIfAssignable(types, typeof(IFilterableRepository<,>), entityType, keyType, repositoryType);
						RegisterIfAssignable(types, typeof(IPageableRepository<,>), entityType, keyType, repositoryType);

						if (!types.Contains(iface))
							types.Add(iface);
					}
				}
			}

			var baseType = repositoryType.BaseType;
			while (baseType != null) {
				if (Implements(typeof(IRepository<>), baseType) && 
					!types.Contains(baseType))
					types.Add(baseType);
				if (Implements(typeof(IRepository<,>), baseType) &&
					!types.Contains(baseType))
					types.Add(baseType);

				baseType = baseType.BaseType;
			}

			return types.AsReadOnly();
		}

		public static IReadOnlyList<Type> GetRepositoryProviderServiceTypes(Type providerType) {
			var types = new List<Type>();

			foreach (var iface in providerType.GetInterfaces()) {
				var entityType = GetEntityType(iface);

				if (entityType == null)
					continue;

				if (RegisterIfAssignable(types, typeof(IRepositoryProvider<>), entityType, providerType)) {
					if (!types.Contains(iface))
						types.Add(iface);
				}

				var keyType = GetKeyType(iface);
				if (keyType != null) {
					if (RegisterIfAssignable(types, typeof(IRepositoryProvider<,>), entityType, keyType, providerType)) {
						if (!types.Contains(iface))
							types.Add(iface);
					}
				}
			}

			var baseType = providerType.BaseType;
			while (baseType != null) {
				if (Implements(typeof(IRepositoryProvider<>), baseType)) {
					if (!types.Contains(baseType))
						types.Add(baseType);
				}
				if (Implements(typeof(IRepositoryProvider<,>), baseType)) {
					if (!types.Contains(baseType))
						types.Add(baseType);
				}

				baseType = baseType.BaseType;
			}

			return types.AsReadOnly();
		}
	}
}