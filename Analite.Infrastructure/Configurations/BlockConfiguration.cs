using Analite.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analite.Infrastructure.Configurations;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
	public void Configure(EntityTypeBuilder<Block> builder)
	{
		builder.HasKey(b => b.Id);
		builder.HasIndex(b => new { b.CustomerId, b.Name }).IsUnique();
	}
}