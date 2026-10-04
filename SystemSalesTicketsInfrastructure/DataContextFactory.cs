using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SystemSalesTickets.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var apiPath = @"C:\Users\user1\Desktop\פרויקט סופי\server\SystemSalesTickets\SystemSalesTickets";

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(
                    Path.Combine(apiPath, "appsettings.json"),
                    optional: false)
                .AddJsonFile(
                    Path.Combine(apiPath, "appsettings.Development.json"),
                    optional: false)
                .Build();

            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found.");

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new DataContext(optionsBuilder.Options);
        }
    }
}