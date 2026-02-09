using System.Linq;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Custom WebApplicationFactory for Sales API tests using in-memory database.
/// </summary>
public class SalesWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<DefaultContext>) ||
                d.ServiceType == typeof(DbContextOptions)).ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            services.AddDbContext<DefaultContext>(options =>
            {
                options.UseInMemoryDatabase("SalesTests");
            });
        });
    }
}
