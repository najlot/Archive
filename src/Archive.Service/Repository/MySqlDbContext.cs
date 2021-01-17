using Microsoft.EntityFrameworkCore;
using Archive.Service.Configuration;
using Archive.Service.Model;

namespace Archive.Service.Repository
{
	public class MySqlDbContext : DbContext
	{
		private readonly MySqlConfiguration _configuration;

		public MySqlDbContext(MySqlConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var connectionString = $"server={_configuration.Host};" +
				$"port={_configuration.Port};" +
				$"database={_configuration.Database};" +
				$"uid={_configuration.User};" +
				$"password={_configuration.Password}";

			optionsBuilder.UseMySql(connectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ArchiveEntryModel>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.OwnsMany(e => e.Groups, e => { e.HasKey(e => e.Id); e.ToTable("ArchiveEntry_Groups"); });
			});
			modelBuilder.Entity<UserModel>(entity =>
			{
				entity.HasKey(e => e.Id);
			});
		}

		public DbSet<ArchiveEntryModel> ArchiveEntries { get; set; }
		public DbSet<UserModel> Users { get; set; }
	}
}