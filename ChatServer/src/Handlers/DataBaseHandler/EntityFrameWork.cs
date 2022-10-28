using System.Text.RegularExpressions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

//Todo: Hash the password
//Todo: possibly in the future transfer these to socketUser partials ex. SocketUser.Register() instead of Database.Register(..., user);
namespace ChatServer.Handlers
{
    public partial class TestOrm : DbContext
    {
        private DbSet<UserProperties> Users  { get; set; }
        private DbSet<ServerObject> Servers  { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChannelObject>()
                .HasMany(x => x.Messages)
                .WithOne(x => x.Channel);

            builder.Entity<ServerObject>()
                .HasMany(x => x.Users)
                .WithMany(x => x.Servers);
            
            //.WithMany(x=>x.)
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=root;Database=chat");
        }

        public async Task<UserProperties?> FindUserById(int id) =>
            await Users.Where(x => x.UserId == id).Include(x=>x.Servers).FirstOrDefaultAsync();

        public async Task<UserProperties?> FindUserBySession(string serverId) =>
            await Users.Where(x => x.Session == serverId).FirstOrDefaultAsync();
        
        public async Task<ServerObject?> FindServerById(int serverId) =>
            await Servers.Where(x => x.Id == serverId).Include(x=> x.Users).FirstOrDefaultAsync();
        
    }
}