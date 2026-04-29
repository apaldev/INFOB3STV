using NUnit.Framework;
using STVrogue.Utils;
using STVrogue.GameLogic;

namespace NUnitTests
{
    class DummyDungeon : Dungeon
    {
        internal void SetStartRoom(Room room)
        {
            StartRoom = room;
        }
        internal void SetExitRoom(Room room)
        {
            ExitRoom = room;
        }
    }
    
    [TestFixture]
    public class Test_HelperPredicates
    {
        [Test]
        public void Test_IsLinear()
        {
            var start = new Room("start", RoomType.STARTroom, 0);
            var r1 = new Room("r1", RoomType.ORDINARYroom, 10);
            var r2 = new Room("r2", RoomType.ORDINARYroom, 10);
            var exit = new Room("exit", RoomType.EXITroom, 0);
            start.Connect(r1, Direction.EAST);
            r1.Connect(r2, Direction.EAST);
            r2.Connect(exit, Direction.EAST);
            var dungeon = new DummyDungeon();
            // start -- r1 -- r2 -- exit
            dungeon.Rooms.Add(start);
            dungeon.Rooms.Add(r1);
            dungeon.Rooms.Add(r2);
            dungeon.Rooms.Add(exit);
            Assert.IsFalse(HelperPredicates.IsLinear(dungeon));
            dungeon.SetStartRoom(start);
            dungeon.SetExitRoom(exit) ;
            Assert.IsTrue(HelperPredicates.IsLinear(dungeon));
            
            // start -- r1 -- r2     exit
            r2.Disconnect(exit);
            Assert.IsFalse(HelperPredicates.IsLinear(dungeon));


            // start -- r1 -- r2 -- exit
            //           |___________|
            r2.Connect(exit, Direction.EAST);
            r1.Connect(exit, Direction.SOUTH);
            Assert.IsFalse(HelperPredicates.IsLinear(dungeon));
            
            // start -- r1 ------ exit
            //           |___r2
            r2.Disconnect(exit);
            Assert.IsFalse(HelperPredicates.IsLinear(dungeon));
        }
    }
}