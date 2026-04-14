using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

namespace Deveel.Data
{
	/// <summary>
	/// An implementation of <see cref="TenantInfo"/> that
	/// provides the connection string for a MongoDB tenant.
	/// </summary>
	public class MongoDbTenantInfo : ITenantInfo
	{
		/// <summary>
		/// Gets or sets the connection string for the MongoDB tenant.
		/// </summary>
		public string? ConnectionString { get; set;}

#if NET8_0 || NET9_0
        string? ITenantInfo.Id
        {
            get => Id ?? "";
            set => Id = value ?? "";
        }

        string? ITenantInfo.Identifier
        {
            get => Identifier ?? ""; 
            set => Identifier = value ?? "";
        }
#endif
        public string Id { get; set; }
        
        public string Identifier { get; set; }
        
        public string? Name { get; set; }
    }
}
