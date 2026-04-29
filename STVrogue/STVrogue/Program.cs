// The pre-processing directive below should be the first non-comment line of your file.
// Keep it during development. When you release the software, remove it.

#define TEST_SETUP
using System;
using STVrogue.GameLogic;
using STVrogue.Utils;

namespace STVrogue
{
    /// <summary>
    /// This contains the top-level main of STV Rogue, which in turn will call
    /// <see cref="GameRunner"/>, where the game main-loop is implemented.
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            // (1) reading the configuration of the game-level to generate
            GameConfiguration conf = new GameConfiguration("rogueconfig.txt");
            // (2) create an instance of a Game:
            Game game = new Game(conf);
            // (3) attach the Game to a runner. The runner contains the logic of the game's
            // main-loop.
            GameRunner runner = new GameRunner(game, new GameConsole());
            // (4) Run the main-loop:
            runner.Run();
        }
    }
}