using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
//using System.Data.Entity.Migrations;

// https://docs.microsoft.com/en-us/ef/core/modeling/relationships

public class Player
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    // old way
    //  public virtual ICollection<Game> Games { get; set; }
    public ICollection<PlayerGame>  PlayerGames { get; set; } = new List<PlayerGame>();
}

public class Game
{
    public int GameId { get; set; }
    public string Title { get; set; }
    public string Platform { get; set; }
    //public virtual ICollection<Player> Players { get; set; }
    public ICollection<PlayerGame>  PlayerGames { get; set; } = new List<PlayerGame>();
}

public class PlayerGame
{
    public int PlayerId { get; set; }
    public Player Player { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }
}

public class PlayerDbContext : DbContext
{
    public PlayerDbContext() 
    {
    }

    public DbSet<Player> Players { get; set;}
    public DbSet<Game> Games { get; set; }
    public DbSet<PlayerGame> PlayerGames { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=tcp:laaz200ersql.database.windows.net,1433;Initial Catalog=laaz200efsql;Persist Security Info=False;User ID=mike;Password=Rush!Anthem!2112;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<Player>().HasMany<Game>(p => p.Games).WithMany(g => g.Players);
        /* 
        modelBuilder.Entity<Player>().Property(p => p.PlayerId).ValueGeneratedOnAdd();
        modelBuilder.Entity<Game>().Property(g => g.GameId).ValueGeneratedOnAdd();
        modelBuilder.Entity<PlayerGame>().Property(pg => pg.PlayerId).ValueGeneratedNever();
        modelBuilder.Entity<PlayerGame>().Property(pg => pg.GameId).ValueGeneratedNever();
        */
        modelBuilder.Entity<PlayerGame>().HasKey(pg => new { pg.PlayerId, pg.GameId });
        
        modelBuilder.Entity<PlayerGame>()
            .HasOne(pg => pg.Player)
            .WithMany(p => p.PlayerGames)
            .HasForeignKey(pg => pg.PlayerId);
        modelBuilder.Entity<PlayerGame>()
            .HasOne(pg => pg.Game)
            .WithMany(g => g.PlayerGames)
            .HasForeignKey(pg => pg.GameId);
    }
}
/* 
internal sealed class dbConfiguration : DbMigrationsConfiguration<PlayerDbContext>
{
    public dbConfiguration() { AutomaticMigrationsEnabled=true; }
}
*/

public class app
{
    public static void CreatePlayerWithGame(string playerName, int gameId) 
        => AddPlayer(playerName, GetGame(gameId));

    public static Game GetGame(int gameId)
    {
        using (var db = new PlayerDbContext())
        {
            return db.Games.FirstOrDefaultAsync(g => g.GameId == gameId).Result;
        }
    }

    public static Player GetPlayer(int playerId)
    {
        using (var db = new PlayerDbContext())
        {
            return db.Players
            .Include(p => p.PlayerGames)
            .ThenInclude(pg => pg.Game)
            .FirstOrDefaultAsync(p => p.PlayerId == playerId).Result;
        }
    }

    public static Player AddPlayer(string playerName, Game game)
    {
        using (var db = new PlayerDbContext())
        {
            var playerGame = new PlayerGame
            {
                Player = new Player() 
                { 
                    PlayerName = playerName
                },
                GameId = game.GameId,
                Game = game
            };

            db.PlayerGames.Add(playerGame);
            db.SaveChanges();

            return playerGame.Player;
        }
    }
}