using NUnit.Framework;
using STVrogue.GameLogic;

using System;
namespace NUnitTests
{
    [TestFixture]
    public class Test_SavingLoadingGameConfig
    {
    
        [Test]
        public void Test_Saving_and_Loading_GameConfig()
        {
            GameConfiguration config = new GameConfiguration();
            config.NumberOfRooms = 10;
            config.DungeonShape = DungeonShapeType.GRID;
            config.DifficultyMode = DifficultyMode.ELITEmode;
            config.SaveToFile("test_rogueconfig.txt");
            GameConfiguration config2 = new GameConfiguration("test_rogueconfig.txt");
            Assert.AreEqual(config2.NumberOfRooms,config.NumberOfRooms); 
            Assert.AreEqual(config2.DungeonShape,config.DungeonShape); 
            Assert.AreEqual(config2.DifficultyMode,config.DifficultyMode);
        }
    }
}
