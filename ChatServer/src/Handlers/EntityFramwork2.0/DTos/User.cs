using ChatShared.DTos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

public class User : UserShared, IEntityTypeConfiguration<UserShared>
{
    public void Configure(EntityTypeBuilder<UserShared> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.Password).IsRequired();
        builder.Property(x => x.Email).IsRequired();
    }
}
