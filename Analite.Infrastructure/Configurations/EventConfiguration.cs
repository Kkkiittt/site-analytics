using Analite.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analite.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
	public void Configure(EntityTypeBuilder<Event> builder)
	{
		builder.HasKey(e => e.SessionId);
		builder.HasIndex(e => new { e.CustomerId, e.OccuredAt }).IncludeProperties(e => e.PageId);
	}
}