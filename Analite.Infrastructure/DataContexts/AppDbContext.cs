using Analite.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Analite.Infrastructure.EFCore;

public class AppDbContext : DbContext
{
	public DbSet<Customer> Customers { get; protected set; } = null!;
	public DbSet<Page> Pages { get; protected set; } = null!;
	public DbSet<Block> Blocks { get; protected set; } = null!;
	public DbSet<Event> Events { get; protected set; } = null!;

	public DbSet<Flow> Flows { get; protected set; } = null!;

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}