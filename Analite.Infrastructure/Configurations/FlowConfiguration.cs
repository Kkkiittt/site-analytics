using Analite.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analite.Infrastructure.Configurations;

public class FlowConfiguration : IEntityTypeConfiguration<Flow>
{
	public void Configure(EntityTypeBuilder<Flow> builder)
	{
		builder.HasKey(f => f.Id);
		builder.HasIndex(f => new { f.CustomerId, f.StartAt, f.EndAt });
	}
}