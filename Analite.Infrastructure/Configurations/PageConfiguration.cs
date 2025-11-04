using Analite.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analite.Infrastructure.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
	public void Configure(EntityTypeBuilder<Page> builder)
	{
		builder.HasKey(p => p.Id);
	}
}