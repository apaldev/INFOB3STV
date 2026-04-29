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

        public bool Gameover { get; private set; } = false;

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
        IRandomGenerator rnd = new RandomGenerator();
        //IRandomGenerator rnd = new STVControlledRandom();
        
        #endregion
        
        /// <summary>
        /// Try to create an instance of Game satisfying the specified configuration.
        /// It should throw an exception if it does not manage to generate a dungeon
        /// satisfying the configuration.
        /// </summary>
        public Game(GameConfiguration conf) 
        {
            // A dummy implementation that ignores the configuration. You should fix this
            // by implementing this constructor according to its description in the Project
            // Document.
            Player = new Player("P0", "Bagginssess");
            Config = conf;
            STVControlledRandom.SetSeed(conf.RndSeed);
            STVLogger.Log(">>> Creating an instance of Game, but ignoring the passed configuration. Fix this.");
            // Dummy implementation that always creates a linear dungeon of 4 rooms with
            // some monsters and item
            Dungeon = new Dungeon(this.rnd, DungeonShapeType.LINEAR, 4, 2);
            Player.Location = Dungeon.StartRoom;
            SeedMonstersAndItems(2, 2, 2);
        }
        
        /// <summary>
        /// THIS IS A DUMMY IMPLEMENTAION. you are expected to implement
        /// this method according to the description in the Project Document.
        /// 
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
            // dummy implemetation that just put some monsters and pots in the room after-after
            // the start room
            Room r = Dungeon.Rooms[2];
            r.Creatures.Add(new Monster("M0", "Goblin"));
            r.Creatures.Add(new Monster("M1", "Orc"));
            r.Items.Add(new HealingPotion("H0",2));
            r.Items.Add(new HealingPotion("H1",2));
            return true;
        }
        

        /// <summary>
        /// Cause a creature to flee a combat. This will take the creature to a neighboring
        /// room. This should not breach the capacity of that room. Note that fleeing a
        /// combat is not always possible --see the Project Document.
        /// The method returns true if fleeing was successful, else false.
        /// </summary>
        public bool Flee(Creature c)
        {
            throw new NotImplementedException();
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
            switch (playerAction.Name)
            {
                case CommandType.MOVE:
                    string roomId = playerAction.Args[0];
                    // dummy logic for move-to:
                    Room roomToMoveTo = (from r in Dungeon.Rooms where r.Id == roomId select r).First();
                    Player.Move(roomToMoveTo);
                    break;
                case CommandType.ATTACK :
                    break;
                case CommandType.FLEE:
                    break;
                case CommandType.DoNOTHING:
                    break;
            }
            TurnNumber++;
        }
    }
}
