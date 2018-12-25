using System;

namespace sqlef
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
/* 
            using (var context = new PlayerDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            var game = new Game() { Title = "Halo", Platform = "XBox"};
            app.AddPlayer("Mike", game);
*/
            using (var context = new PlayerDbContext())
            {
                var player = app.GetPlayer(1);
                Console.WriteLine(player.PlayerName);
            }c

        }
    }
}
