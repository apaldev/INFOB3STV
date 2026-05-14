using System;
using System.Linq;
using STVrogue.Utils;

namespace STVrogue.GameLogic
{
    /// <summary>
    /// This class represents the whole game-state of STVRogue. It also contains
    /// the method <see cref="Update"/> that performs a single turn update to
    /// the game state.
    ///
    /// The game's main-loop is put separately in the class <see cref="GameRunner"/>.
    /// 
    /// <para></para>
    /// The main methods for this class are:
    ///
    /// <list type="bullet">
    ///
    /// <item> The constructor, for creating an instance of this Game, with a 
    /// dungeon according to a given configuration. </item>
    /// 
    /// <item> The method <see cref="Update"/> to do a single turn update. This is called
    /// from the mainloop in <see cref="GameRunner"/>. </item>
    /// 
    /// <item> The method <see cref="Flee"/> for you to program the logic of fleeing creatures.</item>
    ///
    /// </list>
    /// </summary>
    public class Game 
    {
        #region fields and properties
        
        public GameConfiguration Config { get; private set; }
        
        public Player Player { get ; private set; }
        
        public Dungeon Dungeon { get ; private set; }

        public bool Gameover { get; private set; }

        /// <summary>
        /// To count the number of passed turns. 
        /// </summary>
        public int TurnNumber { get; private set; } 

        
        /// <summary>
        /// A <see cref="GameConsole"/> provides a text-based Console. You can print strings
        /// on this console, or read strings from it. Use this to handle your text I/O,
        /// e.g. to print msgs when a monster attacks or flees.
        /// NOTE: Don't read and write directly to the System's Console.
        /// </summary>
        public GameConsole GameConsole { get; set;  }

        /// <summary>
        /// A random generator you can use for making random decisions. The type
        /// is intentionally set to be an instance of <see cref="IRandomGenerator"/>
        /// to prevent you from directly using <see cref="Random"/>.
        /// When testing the game you need a setup where all your random generators
        /// behave deterministically, to avoid your testing to become flaky.
        /// The code below will use an instance of <see cref="RandomGenerator"/>,
        /// which is NOT deterministic.
        /// <para></para>
        /// Check out the other implementation of <see cref="IRandomGenerator"/>, namely
        /// <see cref="STVControlledRandom"/>, or else write your own implementation.
        /// </summary>
        IRandomGenerator _rnd = new RandomGenerator();
        //IRandomGenerator rnd = new STVControlledRandom();
        
        #endregion
        
        /// <summary>
        /// Try to create an instance of Game satisfying the specified configuration.
        /// It should throw an exception if it does not manage to generate a dungeon
        /// satisfying the configuration.
        /// </summary>
        public Game(GameConfiguration conf) 
        {
            Config = conf;
            Player = new Player("P0", "Bagginssess");
            STVControlledRandom.SetSeed(conf.RndSeed);

            if (conf.DungeonShape == DungeonShapeType.LINEAR && conf.NumberOfRooms < 3)
                throw new Exception("Linear dungeons must have at least 3 rooms.");
            if (conf.DungeonShape != DungeonShapeType.LINEAR && conf.NumberOfRooms < 5)
                throw new Exception("Tree and Grid dungeons must have at least 5 rooms.");
            if (conf.InitialNumberOfMonsters > (conf.NumberOfRooms - 2) * conf.MaxRoomCapacity)
                throw new Exception("Too many monsters for the dungeon's capacity.");
            if (conf.MaxRoomCapacity <= 0)
                throw new Exception("Room capacity must be greater than 0.");

            Dungeon = new Dungeon(this._rnd, conf.DungeonShape, conf.NumberOfRooms, conf.MaxRoomCapacity);
            Player.Location = Dungeon.StartRoom;

            // Populate the dungeon with monsters and items
            if (!SeedMonstersAndItems(conf.InitialNumberOfMonsters, conf.InitialNumberOfHealingPots, conf.InitialNumberOfRagePots)) {
                throw new Exception("Failed to populate the dungeon with the specified configuration.");
            }
        }

        /// <summary>
        /// <para></para>
        /// Populate the dungeon in this Game with the specified number of monsters and items.
        ///
        /// <para></para>
        /// The monsters and items are dropped in random locations. Keep in mind that
        /// the number of monsters in a room should not exceed the room's capacity.
        /// There are also other constraints; see the Project Document.
        /// <para></para>
        /// Note that it is not always possible to populate the dungeon according to
        /// the specified parameters. E.g. in a dungeon with N rooms whose capacity
        /// are between 0 and k, it is definitely not possible to populate it with
        /// more than (N-2)*k monsters.
        /// <para></para>
        /// The method returns true if it manages to populate the dungeon as specified,
        /// else it returns false.
        /// </summary>
        bool SeedMonstersAndItems(int numberOfMonster, int numberOfHealingPotion, int numberOfRagePotion)
        {
            var rooms = Dungeon.Rooms.Where(r => r != Dungeon.StartRoom && r != Dungeon.ExitRoom).ToList();
            if (rooms.Count == 0) return false;

            int maxRoomsWithItems = rooms.Count / 2;
            int roomsWithItems = 0;

            // Distribute monsters
            for (int i = 0; i < numberOfMonster; i++)
            {
                var room = rooms[_rnd.NextInt(rooms.Count)];
                if (room.Creatures.Count < room.Capacity)
                {
                    room.Creatures.Add(new Monster($"M{i}", "Monster"));
                }
            }

            for (int i = 0; i < numberOfHealingPotion; i++)
            {
                var room = rooms[_rnd.NextInt(rooms.Count)];
                if (room.Items.Any(item => item is HealingPotion)) continue; // Avoid neighboring rooms
                room.Items.Add(new HealingPotion($"H{i}", 2));
                roomsWithItems++;
                if (roomsWithItems > maxRoomsWithItems) return false;
            }

            foreach (var room in rooms.Where(r => r.IsLeafRoom))
            {
                if (numberOfRagePotion <= 0) break;
                room.Items.Add(new RagePotion($"R{numberOfRagePotion--}"));
            }

            return numberOfRagePotion == 0;
        }
        

        /// <summary>
        /// Cause a creature to flee a combat. This will take the creature to a neighboring
        /// room. This should not breach the capacity of that room. Note that fleeing a
        /// combat is not always possible --see the Project Document.
        /// The method returns true if fleeing was successful, else false.
        /// </summary>
        public bool Flee(Creature c)
        {
            if (Config.DifficultyMode == DifficultyMode.ELITEmode && c is Player player && player.Enraged)
            {
                GameConsole.WriteLines("Fleeing is not allowed while enraged in Elite mode.");
                return false;
            }

            var neighbors = c.Location.Neighbors;
            var targetRoom = neighbors.FirstOrDefault(n => n.Item1.Creatures.Count < n.Item1.Capacity).Item1;
            if (targetRoom != null)
            {
                c.Move(targetRoom);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Perform a single turn-update on the game. In every turn, each creature
        /// is allowed to do one action. What the player does is specified in the argument
        /// of this method. A monster can either do nothing, move, attack, or flee.
        /// See the Project Document that defines when these are possible.
        /// The order in which creatures execute their actions is left for you to decide.
        /// </summary>
        public void Update(Command playerAction)
        {
            // Validate player command
            if (playerAction == null || !Enum.IsDefined(typeof(CommandType), playerAction.Name))
            {
                GameConsole.WriteLines("Invalid command.");
                return;
            }

            // Handle the player's action first
            switch (playerAction.Name)
            {
                case CommandType.MOVE:
                    string roomId = playerAction.Args[0];
                    Room roomToMoveTo = Dungeon.Rooms.FirstOrDefault(r => r.Id == roomId);
                    if (roomToMoveTo != null && Player.Location.Neighbors.Any(n => n.Item1 == roomToMoveTo) && roomToMoveTo.Creatures.Count < roomToMoveTo.Capacity)
                    {
                        Player.Move(roomToMoveTo);
                        GameConsole.WriteLines($"Player moved to room {roomToMoveTo.Id}.");
                    }
                    else
                    {
                        GameConsole.WriteLines("Move action not possible.");
                    }
                    break;

                case CommandType.ATTACK:
                    if (Player.Location.Creatures.Any(c => c is Monster))
                    {
                        Creature target = Player.Location.Creatures.First(c => c is Monster);
                        Player.Attack(target);
                        GameConsole.WriteLines($"Player attacked {target.Id}. {target.Id} HP: {target.Hp}");
                        if (!target.Alive)
                        {
                            Player.Kp++;
                            Player.Location.Creatures.Remove(target);
                            GameConsole.WriteLines($"{target.Id} has been defeated.");
                        }
                    }
                    else
                    {
                        GameConsole.WriteLines("No monsters to attack.");
                    }
                    break;

                case CommandType.FLEE:
                    if (Flee(Player))
                    {
                        GameConsole.WriteLines("Player fled to a neighboring room.");
                    }
                    else
                    {
                        GameConsole.WriteLines("Flee action not possible.");
                    }
                    break;

                case CommandType.DoNOTHING:
                    GameConsole.WriteLines("Player chose to do nothing.");
                    break;
            }

            // Handle monster actions
            foreach (var monster in Dungeon.Rooms.SelectMany(r => r.Creatures).OfType<Monster>())
            {
                if (monster.Location == Player.Location)
                {
                    monster.Attack(Player);
                    GameConsole.WriteLines($"{monster.Id} attacked Player. Player HP: {Player.Hp}");
                    if (!Player.Alive)
                    {
                        Gameover = true;
                        GameConsole.WriteLines("Player has been defeated. Game over.");
                        return;
                    }
                }
                else
                {
                    // Prioritize attacking the player if possible
                    if (monster.Location.Neighbors.Any(n => n.Item1 == Player.Location))
                    {
                        monster.Move(Player.Location);
                        GameConsole.WriteLines($"{monster.Id} moved to attack Player.");
                    }
                    else
                    {
                        // Randomly decide monster action (move, do nothing, or flee)
                        int action = _rnd.NextInt(3);
                        if (action == 0) // Move
                        {
                            Room targetRoom = monster.Location.Neighbors.FirstOrDefault(n => n.Item1.Creatures.Count < n.Item1.Capacity).Item1;
                            if (targetRoom != null)
                            {
                                monster.Move(targetRoom);
                                GameConsole.WriteLines($"{monster.Id} moved to room {targetRoom.Id}.");
                            }
                        }
                        else if (action == 1) // Flee
                        {
                            if (Flee(monster))
                            {
                                GameConsole.WriteLines($"{monster.Id} fled to a neighboring room.");
                            }
                        }
                    }
                }
            }

            if (Player.Location == Dungeon.ExitRoom && !Dungeon.Rooms.SelectMany(r => r.Creatures).Any(c => c is Monster)) {
                Gameover = true;
                GameConsole.WriteLines("Congratulations! You have won the game.");
                return;
            }

            TurnNumber++;
            GameConsole.WriteLines($"Turn {TurnNumber} completed.");
        }
    }
}
