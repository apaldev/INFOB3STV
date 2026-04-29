using System;
using System.Linq;
using STVrogue.GameLogic;
using STVrogue.TestInfrastructure;
using STVrogue.Utils;

namespace STVrogue
{
    /// <summary>
    /// This class implements the STVRogue's game main loop. It maintains a game-state,
    /// represented by an instance of the class <see cref="STVrogue.GameLogic.Game"/>.
    /// The latter also provides a method <see cref="STVrogue.GameLogic.Game.Update"/>
    /// that contains the logic of a single turn update.
    ///
    /// <para> </para>
    /// Most of the I/O (printing messages, getting commands) with the user is
    /// programmed in this GameRunner class.
    /// 
    /// <para> </para>
    /// The class also allows you to attach a "Test-agent" that can
    /// automatically play the game for testing purposes. No real implementation
    /// of TestAgent is provided; this is left for your.
    ///
    /// <para> </para>
    /// This class is not finished. You have to finish it.
    /// </summary>
    public class GameRunner 
    {
        /// <summary>
        /// The entire game-state is kept here.
        /// </summary>
        Game _game;
        
        /// <summary>
        /// A test-agent that can be used to auto-play and test the game, if one is provided.
        /// </summary>
        TestAgent _agent;
        
        GameConsole _console;

        public GameRunner(Game game, GameConsole console)
        {
            this._game = game;
            this._console = console;
            this._game.GameConsole = console;
        }
        
        public GameRunner(Game game, GameConsole console, TestAgent agent) : this(game, console)
        {
            this._agent = agent;
        }
        
        /// <summary>
        /// To run the game. 
        /// </summary>
        public void Run()
        {
            Run(null);
        }
        
        /// <summary>
        /// To run the game. The method implements a game loop where at every iteration
        /// it prints the game status to the Console, and waits for the user's input.
        /// The input is the translated to some action, and the method goes to the next
        /// iteration in the game loop.
        ///
        /// <para></para>
        /// NOTE: phi is only relevant if you do the "stronger system-testing" Optional
        /// part of the project. Else you can ignore it.
        /// </summary>
        /// 
        /// <param name="phi">A temporal property to check as the game runs. This is
        /// for the "stronger system-testing" Optional task of the project.
        /// If you don't do that, you can just pass null as phi.
        /// </param>
        /// <returns> A judgement. Only relevant for the "stronger system-testing" Optional
        /// task of the project.
        /// If temporal property phi is not null, and the run satisfies
        /// phi, this method returns a Valid judgement. If the run violates phi, Invalid is
        /// returned. If Valid or Invalid cannot be decided, the method returns Inconclusive. 
        /// </returns>
        public Judgement Run(TemporalProperty<Game> phi)
        {
            // Don't write directly to system-console. Use methods from game.GameConsole:
            _game.GameConsole.WriteLines(" _______ _________            _______  _______  _______           _______ ",
               "(  ____ \\\\__   __/|\\     /|  (  ____ )(  ___  )(  ____ \\|\\     /|(  ____ \\",
               "| (    \\/   ) (   | )   ( |  | (    )|| (   ) || (    \\/| )   ( || (    \\/",
               "| (_____    | |   | |   | |  | (____)|| |   | || |      | |   | || (__    ",
               "(_____  )   | |   ( (   ) )  |     __)| |   | || | ____ | |   | ||  __)   ",
               "      ) |   | |    \\ \\_/ /   | (\\ (   | |   | || | \\_  )| |   | || (      ",
               "/\\____) |   | |     \\   /    | ) \\ \\__| (___) || (___) || (___) || (____/\\",
               "\\_______)   )_(      \\_/     |/   \\__/(_______)(_______)(_______)(_______/",
               "Welcome stranger...");
            
            // ignore this judgement unless you do the "stronger system-testing" Optional.
            if (phi != null) phi.Reset();
            Judgement phiJudgement = Check(phi);
            
            // The game loop. Repeat until gameover or user quit:
            bool quit = false;
            while (!_game.Gameover && !quit)
            {
                _console.clearConsoleEcho();
                
                var monsters = (from crit in _game.Player.Location.Creatures 
                    select $"{crit.Name} ({crit.Id })").ToList() ;
                var items = (from item in _game.Player.Location.Items
                    select $"{item.GetType().Name} ({item.Id})").ToList() ;
                
                var neighbors = (from neighbor in _game.Player.Location.Neighbors 
                    select $"{neighbor.Item1.Id}({neighbor.Item2})").ToList();
                
                _game.GameConsole.WriteLines("",
                    $"TURN {_game.TurnNumber}",
                    $"You are in a room ({_game.Player.Location.Id}). It is dark, and it feels dangerous...",
                    "Monsters in the room: " + (monsters.Count == 0 ? "don't see one." : string.Join(", ", monsters)),
                    "Items in the room: " + (items.Count == 0 ? "none. Sorry." : string.Join(", ", items)),
                    "Rooms to go: " + (neighbors.Count == 0 ? "Hmm.. looks like you are trapped." : string.Join(",", neighbors)),
                    "You are " + (_game.Player.Alive ? "alive" : "DEAD"), 
                    $"Your health: {_game.Player.Hp}, kill-counts: {_game.Player.Kp}",
                    "In your bag: nothing/nada/zero :(",
                    "Your action: Move(m)   | Pick-items(p) | Do-nothing(SPACE) | Quit(q)",
                    "             Attack(a) |    Flee(f)    | Use-item(u) ");
                // determine what the user action is, and covert it to an instance of Command:
                char c = GetUserOrTestAgentInput();
                Command command = null;
                switch (c)
                {
                    case 'm':
                        var directions = (from neighbor in _game.Player.Location.Neighbors 
                            select DirectionToLetter(neighbor.Item2)).ToList();
                        _game.GameConsole.WriteLines(" Move to which room? " + string.Join("|", directions));
                        char d = GetUserOrTestAgentInput();
                        try
                        {
                            Direction chosenDirection = LetterToDirection(d);
                            string roomId = _game.Player.Location.Neighbors.First(n => n.Item2 == chosenDirection).Item1.Id;
                            command = new Command(CommandType.MOVE, roomId) ;
                        }
                        catch(Exception e) { }
                        break;
                    case 'a':
                        command = new Command(CommandType.ATTACK, "");
                        break;
                    case 'u':
                        command = new Command(CommandType.USE, "") ;
                        break;
                    case 'f':
                        command = new Command(CommandType.FLEE, "");
                        break;
                    case 'p':
                        command = new Command(CommandType.PICKUP, "");
                        break;
                    case ' ':
                        command = new Command(CommandType.DoNOTHING, "");
                        break;
                    case 'q':
                        _game.GameConsole.WriteLines("** Aaaw you QUIT!");
                        quit = true;
                        continue;
                }

                if (command == null)
                {
                    _game.GameConsole.WriteLines("** Invalid command. Try again.");
                    continue;
                }
                // now the command is known, invoke a single-turn update, and deal with
                // printing proper msgs to the console:
                _game.GameConsole.WriteLines("", "** "  + _game.Player.Name + " " + command);
                switch (command.Name)
                {
                    case CommandType.MOVE:
                        _game.Update(command);
                        break;
                    case CommandType.ATTACK :
                        _game.GameConsole.WriteLines("      Clang! Wooosh. WHACK!");
                        _game.Update(command);
                        break;
                    case CommandType.FLEE:
                        _game.GameConsole.WriteLines("      We knew you are a coward.");
                        _game.Update(command);
                        break;
                    case CommandType.PICKUP:
                        _game.Update(command);
                        break;
                    case CommandType.USE:
                        _game.Update(command);
                        break;
                    case CommandType.DoNOTHING:
                        _game.GameConsole.WriteLines("      Lazy. Start working!");
                        _game.Update(command);
                        break;
                }
                
                phiJudgement = Check(phi);
            }
            _game.GameConsole.WriteLines("** Game is over. Score:" + _game.Player.Kp + ". Go ahead and brag it out.");
            return phiJudgement;
        }
        
        Judgement Check(TemporalProperty<Game> phi)
        {
            if (phi == null) return Judgement.Inconclusive;
            Judgement j = phi.EvaluateNextState(_game);
            if (j == Judgement.Invalid)
            {
                STVLogger.Log($"##### VIOLATES the given temporal property!");
            }
            return j;
        }
        
        private Direction LetterToDirection(char c)
        {
            switch (c)
            {
                case 'n': return Direction.NORTH;
                case 's': return Direction.SOUTH;
                case 'e': return Direction.EAST;
                case 'w': return Direction.WEST;
                default: throw new ArgumentException($"Invalid direction-letter: {c}");
            }
        }
        
        private char DirectionToLetter(Direction d)
        {
            switch (d)
            {
                case Direction.NORTH: return 'n';
                case Direction.SOUTH: return 's';
                case Direction.EAST: return 'e';
                default: return 'w';
            }
        }
        
        /// <summary>
        /// Read a single command-character from the user. If a test-agent is provided,
        /// it will instead ask the agent to provide the command-character.
        /// </summary>
        /// <returns></returns>
        char GetUserOrTestAgentInput()
        {
            if (_agent != null)
            {
                return _agent.NextAction(_console.ConsoleEcho, _game);
            }
            else
            {
                return _game.GameConsole.ReadKey();
            }
        }
        
    }
}