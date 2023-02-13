using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc;

[assembly: ExcludeFromCodeCoverage]


namespace Deveel.Data {
	public class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddAuthorization();
			builder.Services.AddMvc();
			builder.Services.AddControllers();


			var app = builder.Build();

			// Configure the HTTP request pipeline.

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}