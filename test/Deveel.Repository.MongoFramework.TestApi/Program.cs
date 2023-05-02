using Finbuckle.MultiTenant;

using MongoDB.Driver;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        var connectionString = builder.Configuration.GetConnectionString("Mongo");
        var urlBulder = new MongoUrlBuilder(connectionString);
        urlBulder.DatabaseName = "tenant1";

        builder.Services.AddMultiTenant<TenantInfo>()
            .WithInMemoryStore(options => {
                options.Tenants.Add(new TenantInfo {
                    Id = "1234567890",
                    Identifier = "tenant1",
                    Name = "Tenant 1",
                    ConnectionString = urlBulder.ToString()
                });
            })
            .WithRouteStrategy("tenantId")
            .WithClaimStrategy("tenant");

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMultiTenant();

        app.MapControllers();

        app.Run();
    }
}