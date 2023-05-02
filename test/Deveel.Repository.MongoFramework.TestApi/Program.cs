using Finbuckle.MultiTenant;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Deveel.Data;

using MongoDB.Driver;
using Deveel.Data.Entities;
using System.Text;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

		var jwtSecretKey = builder.Configuration["JWT:SecretKey"];

        builder.Services.AddControllers();

		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options => {
				options.Audience = "api.deveel.com";
				options.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuer = true,
					ValidateAudience = false,
					ValidateLifetime = false,
					ValidIssuer = "deveel",
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
				};
			});

        var connectionString = builder.Configuration.GetConnectionString("Mongo");
        var tenant1UrlBulder = new MongoUrlBuilder(connectionString);
        tenant1UrlBulder.DatabaseName = "tenant1";

		var tenant2UrlBulder = new MongoUrlBuilder(connectionString);
		tenant2UrlBulder.DatabaseName = "tenant2";

        builder.Services.AddMultiTenant<TenantInfo>()
            .WithInMemoryStore(options => {
                options.Tenants.Add(new TenantInfo {
                    Id = builder.Configuration["Tenant1:TenantId"],
                    Identifier = "tenant1",
                    Name = "Tenant 1",
                    ConnectionString = tenant1UrlBulder.ToString()
                });

				options.Tenants.Add(new TenantInfo {
					Id = builder.Configuration["Tenant2:TenantId"],
					Identifier = "tenant2",
					Name = "Tenant 2",
					ConnectionString = tenant2UrlBulder.ToString()

				});
            })
			.WithRouteStrategy("tenantId")
			.WithClaimStrategy("tenant");

		builder.Services.AddMongoTenantContext<TenantPersonsDbContext>()
			.UseTenantConnection<TenantInfo>();

		var urlBuilder = new MongoUrlBuilder(connectionString);
		urlBuilder.DatabaseName = "test_db";

		builder.Services.AddMongoContext<PersonsDbContext>()
			.UseConnection(urlBuilder.ToString());

		builder.Services.AddMongoContext<UsersDbContext>()
			.UseConnection(urlBuilder.ToString())
			.AddRepository<UserEntity>(ServiceLifetime.Scoped);

		builder.Services.AddRepository<PersonRepository>(ServiceLifetime.Scoped);
		builder.Services.AddRepository<TenantPersonRepository>(ServiceLifetime.Scoped);

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