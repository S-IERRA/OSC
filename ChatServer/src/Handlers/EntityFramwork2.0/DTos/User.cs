using ChatShared.DTos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Rewrite;

public class User : UserShared2, IEntityTypeConfiguration<UserShared2>
{
    public void Configure(EntityTypeBuilder<UserShared2> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.Password).IsRequired();
        builder.Property(x => x.Email).IsRequired();
    }
}
