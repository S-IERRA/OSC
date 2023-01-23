using ChatShared.Types;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Handlers;

public class Channel : ChannelShared, IEntityTypeConfiguration<ChannelShared>
{
	public void Configure(EntityTypeBuilder<ChannelShared> builder)
	{
		builder.ToTable("Channels");

		builder.HasKey(x => new { x.Id, x.ServerId });

		builder.HasMany(x => x.Messages)
			.WithOne(x => x.Channel)
			.HasForeignKey(x => new { x.ChannelId, x.ServerId });
	}
}

public class Message : MessageShared, IEntityTypeConfiguration<MessageShared>
{
	public void Configure(EntityTypeBuilder<MessageShared> builder)
	{
		builder.ToTable("Messages");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.AuthorId).IsRequired();
		builder.HasOne(e => e.Author)
			.WithMany()
			.HasForeignKey(e => new { e.AuthorId, e.ServerId });

		//Todo: Critical, doesnt allow dms
		builder.Property(e => e.ServerId).IsRequired();
		builder.HasOne(e => e.Server)
			.WithMany()
			.HasForeignKey(e => e.ServerId);

		builder.Property(e => e.ChannelId).IsRequired();
		builder.HasOne(e => e.Channel)
			.WithMany(e => e.Messages)
			.HasForeignKey(e => new { e.ChannelId, e.ServerId });

		builder.Property(e => e.Content)
			.IsRequired();
	}
}