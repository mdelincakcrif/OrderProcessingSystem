using Microsoft.EntityFrameworkCore;
using WebApi.Dal;

namespace WebApi.Tests.TestHelpers;

public static class InMemoryDbHelper
{
    public static OrderProcessingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderProcessingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new OrderProcessingDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
