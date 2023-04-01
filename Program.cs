Console.WriteLine("\n     ______                _        _                __    ____  _           _       _ \r\n    |  ____|              | |      (_)              / _|  / __ \\(_)         | |     | |\r\n    | |__ ___  _   _ _ __ | |_ __ _ _ _ __     ___ | |_  | |  | |_  ___  ___| |_ ___| |\r\n    |  __/ _ \\| | | | '_ \\| __/ _` | | '_ \\   / _ \\|  _| | |  | | |/ _ \\/ __| __/ __| |\r\n    | | | (_) | |_| | | | | || (_| | | | | | | (_) | |   | |__| | |  __| (__| |_\\__ |_|\r\n    |_|  \\___/ \\__,_|_| |_|\\__\\__,_|_|_| |_|  \\___/|_|    \\____/| |\\___|\\___|\\__|___(_)\r\n                                                               _/ |                    \r\n                                                              |___/                     \n");
Console.WriteLine("You enter the Cavern of Objects, a maze of rooms filled with dangerous pits in search of the Fountain of Objects. Light is visible only in the entrance, and no other light is seen anywhere in the caverns. You must navigate the Caverns with your other senses. Find the Fountain of Objects, activate it, and return to the entrance.\n");
Console.WriteLine("Dangers on your journey:");
Console.WriteLine("   - Look out for pits. You will feel a breeze if a pit is in an adjacent room. If you enter a room with a pit,\n     you will die");
Console.WriteLine("   - Maelstroms are violent forces of sentient wind. Entering a room with one could transport you to any other\n     location in the caverns. You will be able to hear their growling and groaning in nearby rooms");
Console.WriteLine("   - Amaroks roam the caverns. Encountering one is certain death, but you can smell their rotten stench in\n     nearby rooms.");
Console.WriteLine("   - You carry with you a bow and a quiver of arrows. You can use them to shoot monsters in the caverns but\n     be warned: you have a limited supply.");
Console.WriteLine();
Console.WriteLine("Enter the 'help' command if you need to be reminded of possible inputs.");
Console.WriteLine();
Console.WriteLine("What size of map would you like to play on? (1) small, 2) medium, 3) large)");

Console.ForegroundColor = ConsoleColor.Cyan;
int input = Convert.ToInt32(Console.ReadLine());
Game game = input switch
{
    1 => CreateSmallGame(),
    2 => CreateMediumGame(),
    3 => CreateLargeGame(),
    _ => throw new NotImplementedException()
};
Console.Clear();
game.Play();

static Game CreateSmallGame()
{
    Map map = new(4, 4);
    Position start = new(0, 0);

    map.SetRoomType(start, Room.Start);
    map.SetRoomType(new Position(0, 3), Room.Fountain);
    map.SetRoomType(new Position(2, 0), Room.Pit);

    Enemies[] enemies = new Enemies[]
    {
        new Maelstrom(new Position(2, 2)),
        new Amarok(new Position(3, 3))
    };
    return new Game(map, new Player(start), enemies);
}

static Game CreateMediumGame()
{
    Map map = new(6, 6);
    Position start = new(0, 0);

    map.SetRoomType(start, Room.Start);
    map.SetRoomType(new Position(2, 5), Room.Fountain);
    map.SetRoomType(new Position(3, 2), Room.Pit);
    map.SetRoomType(new Position(2, 4), Room.Pit);

    Enemies[] enemies = new Enemies[]
    {
        new Maelstrom(new Position(0, 3)),
        new Amarok(new Position(4, 2)),
        new Amarok(new Position(1, 5))
    };
    return new Game(map, new Player(start), enemies);
}

static Game CreateLargeGame()
{
    Map map = new(8, 8);
    Position start = new(0, 0);

    map.SetRoomType(start, Room.Start);
    map.SetRoomType(new Position(6, 6), Room.Fountain);
    map.SetRoomType(new Position(7, 7), Room.Pit);
    map.SetRoomType(new Position(2, 0), Room.Pit);
    map.SetRoomType(new Position(0, 3), Room.Pit);
    map.SetRoomType(new Position(2, 5), Room.Pit);

    Enemies[] enemies = new Enemies[]
    {
        new Maelstrom(new Position(5, 3)),
        new Maelstrom(new Position(6, 4)),
        new Amarok(new Position(2, 4)),
        new Amarok(new Position(1, 7)),
        new Amarok(new Position(6, 5))
    };
    return new Game(map, new Player(start), enemies);
}

class Game
{
    public Player Player { get; }
    public Map Map { get; }
    public Enemies[] Enemies { get; }
    public bool FountainEnabled { get; set; }
    public ISenses[] Senses;

    public Game(Map map, Player player, Enemies[] enemies)
    {
        Map = map;
        Player = player;
        Enemies = enemies;
        Senses = new ISenses[]
        {
            new LightSense(),
            new FountainSense(),
            new PitSense(),
            new MaelstromSense(),
            new AmarokSense()
        };
    }

    public void Play() 
    {
        while (!Player.IsDead && !Victory)
        {
            DisplayUpdate();
            ICommands command = Command();
            command.Run(this);
            
            if (Map.CurrentRoom(Player.Position) == Room.Pit) Player.Death("You fell into a pit and died.");
            else
            {
                foreach (Enemies enemy in Enemies)
                {
                    if (enemy.Position == Player.Position && !enemy.IsDead) enemy.Action(this);
                }
            }
        }

        if (Victory)
        {
            ConsoleColorUpdate.WriteLine("You enabled the fountain and successfully escaped! You win!", ConsoleColor.Green);
        }
        else ConsoleColorUpdate.WriteLine($"{Player.CauseOfDeath} You have lost.", ConsoleColor.Red);
    }

    private void DisplayUpdate()
    {
        ConsoleColorUpdate.WriteLine("----------------------------------------------------------------------------------", ConsoleColor.White);
        ConsoleColorUpdate.Write($"You are in the room at Row = {Player.Position.Row + 1}, Column = {Player.Position.Column + 1}. Remaining arrows: ", ConsoleColor.White);
        for (int i = Player.Arrows; i > 0; i--) { ConsoleColorUpdate.Write("↑", ConsoleColor.Yellow); }
        Console.WriteLine();        
        foreach (ISenses sense in Senses)
            if (sense.CanSense(this))
                sense.DisplaySense(this);
    }

    private static ICommands Command()
    {
        while (true)
        {
            ConsoleColorUpdate.WriteLine("What would you like to do?", ConsoleColor.White);
            Console.ForegroundColor = ConsoleColor.Cyan;
            string? input = Console.ReadLine();

            if (input == "move north") return new Movement(Direction.North);
            if (input == "move south") return new Movement(Direction.South);
            if (input == "move east") return new Movement(Direction.East);
            if (input == "move west") return new Movement(Direction.West);

            if (input == "shoot north" ) return new Arrow(Direction.North);
            if (input == "shoot south") return new Arrow(Direction.South);
            if (input == "shoot east") return new Arrow(Direction.East);
            if (input == "shoot west") return new Arrow(Direction.West);

            if (input == "enable fountain") return new EnableFountain();
            if (input == "help" || input == "?") return new HelpCommand();
            else ConsoleColorUpdate.WriteLine("I didn't understand your command, try again.", ConsoleColor.Red);            
        }
    }

    public Room CurrentRoom => Map.CurrentRoom(Player.Position);
    public bool Victory => CurrentRoom == Room.Start && FountainEnabled;
}

record Position(int Row, int Column);

class Player
{
    public Position Position { get; set; }
    public bool IsDead { get; private set; } = false;
    public string CauseOfDeath { get; private set; } = "";
    public int Arrows { get; set; } = 5;

    public Player(Position start) => Position = start;

    public void Death(string cause)
    {
        IsDead = true;
        CauseOfDeath = cause;
    }
}

abstract class Enemies
{
    public Position Position { get; set; }  
    public bool IsDead { get; set; }
    public Enemies(Position position) { Position = position; }
    public abstract void Action(Game game);
}

class Maelstrom : Enemies
{
    public Maelstrom(Position position) : base(position) { }
    public override void Action(Game game)  // Sends the player 2 spaces east and 1 north, while the maelstrom moves 2 spaces west and 1 south
    {
        ConsoleColorUpdate.WriteLine("You've run into a Maelstrom! It has sent you to another room!", ConsoleColor.Magenta);
        game.Player.Position = ClampToWall(new Position(game.Player.Position.Row + 1, game.Player.Position.Column + 2), game.Map.RowSize, game.Map.ColumnSize);
        Position = ClampToWall(new Position(Position.Row - 1, Position.Column - 2), game.Map.RowSize, game.Map.ColumnSize);
    }

    private static Position ClampToWall(Position position, int maxRows, int maxColumns)
    {
        int row = position.Row;
        if (row < 0) row = 0;
        if (row >= maxRows) row = maxRows - 1;

        int column = position.Column;
        if (column < 0) column = 0;
        if (column >= maxColumns) column = maxColumns - 1;

        return new Position(row, column);
    }
}

class Amarok : Enemies
{
    public Amarok(Position position) : base(position) { }
    public override void Action(Game game)
    {
        game.Player.Death("You have been killed after encountering an Amarok.");
    }
}

class Map
{
    public int RowSize { get; }
    public int ColumnSize { get; }
    private readonly Room[,] Rooms;

    public Map(int rows, int columns)
    {
        RowSize = rows;
        ColumnSize = columns;
        Rooms = new Room[rows, columns];
    }
    public Room CurrentRoom(Position position) => OnTheMap(position) ? Rooms[position.Row, position.Column] : Room.OutOfBounds;
    public bool NeighborCheck(Position position, Room type)
    {
        if (CurrentRoom(new Position(position.Row + 1, position.Column + 1)) == type) return true;
        if (CurrentRoom(new Position(position.Row + 1, position.Column)) == type) return true;
        if (CurrentRoom(new Position(position.Row + 1, position.Column - 1)) == type) return true;
        if (CurrentRoom(new Position(position.Row, position.Column + 1)) == type) return true;
        if (CurrentRoom(new Position(position.Row, position.Column)) == type) return true;
        if (CurrentRoom(new Position(position.Row, position.Column - 1)) == type) return true;
        if (CurrentRoom(new Position(position.Row - 1, position.Column + 1)) == type) return true;
        if (CurrentRoom(new Position(position.Row - 1, position.Column)) == type) return true;
        if (CurrentRoom(new Position(position.Row - 1, position.Column - 1)) == type) return true;
        return false;
    }
    public bool OnTheMap(Position position)
    {
        if (position.Row >= 0 && position.Row < Rooms.GetLength(0) && position.Column >= 0 && position.Column < Rooms.GetLength(1)) return true;
        else return false;
    }
    public void SetRoomType(Position position, Room type) => Rooms[position.Row, position.Column] = type;
}

interface ICommands
{
    void Run(Game game);
}

class Movement : ICommands
{
    public Direction Direction { get; }

    public Movement(Direction direction) { Direction = direction; }

    public void Run(Game game) 
    {
        Position currentPosition = game.Player.Position;
        Position NewPosition = Direction switch
        {
            Direction.North => new Position(currentPosition.Row + 1, currentPosition.Column),
            Direction.South => new Position(currentPosition.Row - 1, currentPosition.Column),
            Direction.East => new Position(currentPosition.Row, currentPosition.Column + 1),
            Direction.West => new Position(currentPosition.Row, currentPosition.Column - 1),
            _ => throw new NotImplementedException(),
        };

        if (game.Map.OnTheMap(NewPosition))
            game.Player.Position = NewPosition;
        else
        {
            ConsoleColorUpdate.WriteLine("There is a wall there.", ConsoleColor.Red);
        }
    }
}

class Arrow : ICommands
{
    public Direction Direction { get; }
    public Arrow(Direction direction) { Direction = direction; }

    public void Run(Game game)
    {
        bool enemyKilled = false;
        string enemyName = "";
        if (game.Player.Arrows > 0)
        {
            game.Player.Arrows--;
            ConsoleColorUpdate.Write($"You fire your bow to the {Direction}! ", ConsoleColor.Green);
            foreach (Enemies enemy in game.Enemies)
            {
                if (Direction == Direction.North && (enemy.Position.Row - game.Player.Position.Row) == 1 && enemy.Position.Column == game.Player.Position.Column && !enemy.IsDead)
                    { enemy.IsDead = true; enemyKilled = true; enemyName = enemy.ToString(); }
                else if (Direction == Direction.South && (enemy.Position.Row - game.Player.Position.Row) == -1 && enemy.Position.Column == game.Player.Position.Column && !enemy.IsDead)
                    { enemy.IsDead = true; enemyKilled = true; enemyName = enemy.ToString(); }
                else if (Direction == Direction.East && enemy.Position.Row == game.Player.Position.Row && (enemy.Position.Column - game.Player.Position.Column) == 1 && !enemy.IsDead)
                    { enemy.IsDead = true; enemyKilled = true; enemyName = enemy.ToString(); }
                else if (Direction == Direction.East && enemy.Position.Row == game.Player.Position.Row && (enemy.Position.Column - game.Player.Position.Column) == -1 && !enemy.IsDead)
                    { enemy.IsDead = true; enemyKilled = true; enemyName = enemy.ToString(); }
            }
            if (enemyKilled == true) ConsoleColorUpdate.Write($"You hear the cry of a dying {enemyName}! Well done!", ConsoleColor.Green);
            else ConsoleColorUpdate.Write("You hear your arrow plink off of the cave wall in the distance...\n", ConsoleColor.Red);
        }
        else ConsoleColorUpdate.WriteLine("You are out of arrows, you can't fire anymore!", ConsoleColor.Red);
    }
}

class EnableFountain : ICommands
{
    public void Run(Game game)
    {
        if (game.Map.CurrentRoom(game.Player.Position) == Room.Fountain) game.FountainEnabled = true;
        else
        {
            ConsoleColorUpdate.WriteLine("The fountain isn't in this room, nothing has happened.", ConsoleColor.Red);
        }
    }
}

class HelpCommand : ICommands
{
    public void Run(Game game)
    {
        ConsoleColorUpdate.WriteLine("Available commands:\nmove {north/south/east/west}: Moves you in the requested direction one space, you cannot move diagonally.", ConsoleColor.White);
        ConsoleColorUpdate.WriteLine("shoot {north/south/east/west}: Fires an arrow in the requested direction, attacking one tile ahead of you in that direction.", ConsoleColor.White);
        ConsoleColorUpdate.WriteLine("enable fountain: If you are in the same room as the fountain, it will enable the fountain of objects.", ConsoleColor.White);
        ConsoleColorUpdate.WriteLine("help: Displays this menu. Good luck! :)", ConsoleColor.White);
    }
}

interface ISenses 
{
    bool CanSense(Game game);
    void DisplaySense(Game game);
}

class LightSense : ISenses
{
    public bool CanSense(Game game) => game.Map.CurrentRoom(game.Player.Position) == Room.Start;
    public void DisplaySense(Game game)
    {
        ConsoleColorUpdate.WriteLine("You can see light in this room! You must be at the entrance.", ConsoleColor.Yellow);
    }
}

class FountainSense : ISenses
{
    public bool CanSense(Game game) => game.Map.CurrentRoom(game.Player.Position) == Room.Fountain;
    public void DisplaySense(Game game)
    {
        if (game.FountainEnabled) ConsoleColorUpdate.WriteLine("You hear rushing waters from the fountain! It has been reactivated!", ConsoleColor.Blue);
        else ConsoleColorUpdate.WriteLine("You can hear the sound of dripping! You must be at the fountain.", ConsoleColor.Blue);
    }
}

class PitSense : ISenses
{
    public bool CanSense(Game game) => game.Map.NeighborCheck(game.Player.Position, Room.Pit);
    public void DisplaySense(Game game) => ConsoleColorUpdate.WriteLine("You feel a draft. There is a pit in a nearby room.", ConsoleColor.DarkYellow); 
}

class MaelstromSense : ISenses
{
    public bool CanSense(Game game)
    {
        foreach (Enemies enemy in game.Enemies)
        {
            if (enemy is Maelstrom && !enemy.IsDead)
            {
                int rowDiff = Math.Abs(enemy.Position.Row - game.Player.Position.Row);
                int colDiff = Math.Abs(enemy.Position.Column - game.Player.Position.Column);

                if (rowDiff <= 1 && colDiff <= 1) return true;
            }
        }
        return false;
    }
    public void DisplaySense(Game game) => ConsoleColorUpdate.WriteLine("You hear the growling and groaning of a maelstrom nearby.", ConsoleColor.DarkYellow);
}

class AmarokSense : ISenses
{
    public bool CanSense(Game game)
    {
        foreach (Enemies enemy in game.Enemies)
        {
            if (enemy is Amarok && !enemy.IsDead)
            {
                int rowDiff = Math.Abs(enemy.Position.Row - game.Player.Position.Row);
                int colDiff = Math.Abs(enemy.Position.Column - game.Player.Position.Column);

                if (rowDiff <= 1 && colDiff <= 1) return true;
            }
        }
        return false;
    }

    public void DisplaySense(Game game) => ConsoleColorUpdate.WriteLine("You can smell the rotten stench of an amarok in a nearby room.", ConsoleColor.DarkYellow);
}

static class ConsoleColorUpdate
{
    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
    }
    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
    }
}

enum Room { Empty, Start, Fountain, Pit, OutOfBounds }
enum Direction { North, South, East, West }