using System.Text.RegularExpressions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

//Todo: Hash the password
//Todo: possibly in the future transfer these to socketUser partials ex. SocketUser.Register() instead of Database.Register(..., user); (lots of fuck-ery)
//FOR FUTURE: check message owner ship by USER ID not by session, checking by session will create FUCKING HUGE problems
//Anything that may require changes in the future and not immediate action (editing messages) needs to be checked by id not by session due to changing sessions

//Vulnerability on signed ids, passing a invalid negative id may crash?

namespace ChatServer.Handlers
{
    public partial class EntityFrameworkOrm : DbContext
    {
        private DbSet<UserProperties> Users  { get; set; }
        private DbSet<ServerObject> Servers  { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //.WithMany(x=>x.)
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=root;Database=chat");
        }

        public async Task<UserProperties?> FindUserById(int id) =>
            await Users.Where(x => x.UserId == id).Include(x=>x.Servers).FirstOrDefaultAsync();

        public async Task<UserProperties?> FindUserBySession(string session) =>
            await Users.Where(x => x.Session == session).FirstOrDefaultAsync();
        
        public async Task<ServerObject?> FindServerById(int serverId) =>
            await Servers.Where(x => x.Id == serverId).Include(x=> x.Users).FirstOrDefaultAsync();
    }
}