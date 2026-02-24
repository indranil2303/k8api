using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace infrastructure.persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LocaldbContext>
{
    public LocaldbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<LocaldbContext>();

        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(conn))
        {
            conn = "Server=(localdb)\\mssqllocaldb;Database=CatalogDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        builder.UseSqlServer(conn);
        return new LocaldbContext(builder.Options);
    }
}
