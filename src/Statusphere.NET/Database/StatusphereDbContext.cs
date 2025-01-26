using Microsoft.EntityFrameworkCore;

namespace Statusphere.NET.Database;

public class StatusphereDbContext(DbContextOptions<StatusphereDbContext> options) : DbContext(options)
{
    public DbSet<Status> Statuses => Set<Status>();
}
