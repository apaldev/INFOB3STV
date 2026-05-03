using System;
using System.Linq;
using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;
using static STVrogue.Utils.HelperPredicates;

namespace NUnitTests
{
    [TestFixture]
    public class Test_Dungeon
    {
        // deterministic rng for reproducible tests
        private static IRandomGenerator Rnd() => new RandomGenerator(42);

        // linear: bad inputs 

        // N < 3 should throw
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Linear_TooFewRooms_ThrowsArgumentException(int N)
        {
            Assert.Throws<ArgumentException>(() =>
                new Dungeon(Rnd(), DungeonShapeType.LINEAR, N, 3));
        }

        // linear: valid inputs 

        // checks post1 through post7
        [TestCase(3, 0)]
        [TestCase(3, 1)]
        [TestCase(3, 3)]
        [TestCase(4, 2)]
        [TestCase(5, 5)]
        [TestCase(10, 0)]
        [TestCase(10, 4)]
        public void Linear_ValidArgs_SatisfiesAllPostconditions(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.LINEAR, N, gamma);

            // post1 - room count
            Assert.That(dungeon.Rooms.Count, Is.EqualTo(N));

            // post2 - unique ids
            Assert.IsTrue(AllIdsAreUnique(dungeon));

            // post3 - unique start and exit
            Assert.IsTrue(HasUniqueStartAndExit(dungeon));

            // post4 - all rooms reachable
            Assert.IsTrue(AllReachableFromStart(dungeon));

            // post5 - start/exit capacity = 0
            Assert.That(dungeon.StartRoom.Capacity, Is.EqualTo(0));
            Assert.That(dungeon.ExitRoom.Capacity, Is.EqualTo(0));

            // post6 - ordinary room capacity in [0..γ]
            var ordinary = dungeon.Rooms.Where(r => r.RoomType == RoomType.ORDINARYroom).ToList();
            Assert.IsTrue(Forall(ordinary, r => r.Capacity >= 0 && r.Capacity <= gamma));

            // post7 - shape is truly linear
            Assert.IsTrue(IsLinear(dungeon));
        }

        // γ=0 means every ordinary room must also have capacity 0 (boundary of post6)
        [TestCase(3, 0)]
        [TestCase(5, 0)]
        public void Linear_GammaZero_AllOrdinaryRoomsHaveCapacityZero(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.LINEAR, N, gamma);

            Assert.IsTrue(Forall(dungeon.Rooms,
                r => r.RoomType != RoomType.ORDINARYroom || r.Capacity == 0));
        }

        // exit room must not be accidentally set to an ordinary room (post3 detail)
        [TestCase(3, 2)]
        [TestCase(5, 2)]
        [TestCase(10, 2)]
        public void Linear_ExitRoom_MustBeEXITroomType(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.LINEAR, N, gamma);

            Assert.That(dungeon.ExitRoom.RoomType, Is.EqualTo(RoomType.EXITroom));
        }

        // tree: bad inputs 

        // N < 5 should throw
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        public void Tree_TooFewRooms_ThrowsArgumentException(int N)
        {
            Assert.Throws<ArgumentException>(() =>
                new Dungeon(Rnd(), DungeonShapeType.TREE, N, 3));
        }

        //  tree: valid inputs 

        // checks post1 through post6
        [TestCase(5, 0)]
        [TestCase(5, 3)]
        [TestCase(7, 2)]
        [TestCase(10, 4)]
        [TestCase(13, 5)]
        public void Tree_ValidArgs_SatisfiesPostconditions(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.TREE, N, gamma);

            // post1
            Assert.That(dungeon.Rooms.Count, Is.EqualTo(N));
            // post2
            Assert.IsTrue(AllIdsAreUnique(dungeon));
            // post3
            Assert.IsTrue(HasUniqueStartAndExit(dungeon));
            // post4
            Assert.IsTrue(AllReachableFromStart(dungeon));
            // post5
            Assert.That(dungeon.StartRoom.Capacity, Is.EqualTo(0));
            Assert.That(dungeon.ExitRoom.Capacity, Is.EqualTo(0));
            // post6
            var ordinary = dungeon.Rooms.Where(r => r.RoomType == RoomType.ORDINARYroom).ToList();
            Assert.IsTrue(Forall(ordinary, r => r.Capacity >= 0 && r.Capacity <= gamma));
        }

        // the last room is assigned as ExitRoom but its RoomType stays ORDINARYroom (bug)
        [TestCase(5, 2)]
        [TestCase(10, 2)]
        public void Tree_ExitRoom_MustBeEXITroomType(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.TREE, N, gamma);

            Assert.That(dungeon.ExitRoom.RoomType, Is.EqualTo(RoomType.EXITroom));
        }

        // grid: bad inputs 

        // N < 4 should throw
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void Grid_TooFewRooms_ThrowsArgumentException(int N)
        {
            Assert.Throws<ArgumentException>(() =>
                new Dungeon(Rnd(), DungeonShapeType.GRID, N, 3));
        }

        //  grid: valid inputs

        // checks post1 through post6
        [TestCase(4, 0)]
        [TestCase(4, 3)]
        [TestCase(9, 2)]
        [TestCase(16, 4)]
        [TestCase(6,  3)]
        public void Grid_ValidArgs_SatisfiesPostconditions(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.GRID, N, gamma);

            // post1
            Assert.That(dungeon.Rooms.Count, Is.EqualTo(N));
            // post2
            Assert.IsTrue(AllIdsAreUnique(dungeon));
            // post3
            Assert.IsTrue(HasUniqueStartAndExit(dungeon));
            // post4
            Assert.IsTrue(AllReachableFromStart(dungeon));
            // post5
            Assert.That(dungeon.StartRoom.Capacity, Is.EqualTo(0));
            Assert.That(dungeon.ExitRoom.Capacity, Is.EqualTo(0));
            // post6
            var ordinary = dungeon.Rooms.Where(r => r.RoomType == RoomType.ORDINARYroom).ToList();
            Assert.IsTrue(Forall(ordinary, r => r.Capacity >= 0 && r.Capacity <= gamma));
        }

        // all rooms are created as ORDINARYroom — start/exit get the wrong type (bug)
        [TestCase(4, 2)]
        [TestCase(9, 2)]
        public void Grid_StartAndExitRoom_MustHaveCorrectRoomType(int N, int gamma)
        {
            var dungeon = new Dungeon(Rnd(), DungeonShapeType.GRID, N, gamma);

            Assert.That(dungeon.StartRoom.RoomType, Is.EqualTo(RoomType.STARTroom));
            Assert.That(dungeon.ExitRoom.RoomType, Is.EqualTo(RoomType.EXITroom));
        }
    }
}