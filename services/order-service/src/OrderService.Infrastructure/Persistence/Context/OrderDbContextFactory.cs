using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Context
{
    public class OrderDbContextFactory
     : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5433;Database=orderdb;Username=orderuser;Password=orderpass");

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
