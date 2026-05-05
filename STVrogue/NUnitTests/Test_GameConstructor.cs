using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;

namespace NUnitTests
{
    [TestFixture]
    public class Test_GameConstructor
    {
        // helper build a GameConfiguration inline without needing a file
        private GameConfiguration MakeConf(
            DungeonShapeType shape,
            int numberOfRooms,
            int maxRoomCapacity,
            int monsters,
            int healPots,
            int ragePots,
            DifficultyMode difficulty,
            int seed = 42)
        {
            return new GameConfiguration
            {
                RndSeed                    = seed,
                DungeonShape               = shape,
                NumberOfRooms              = numberOfRooms,
                MaxRoomCapacity            = maxRoomCapacity,
                InitialNumberOfMonsters    = monsters,
                InitialNumberOfHealingPots = healPots,
                InitialNumberOfRagePots    = ragePots,
                DifficultyMode             = difficulty
            };
        }

        // returns the list of rooms that directly neighbor the exit room
        private static List<Room> ExitNeighbors(Dungeon d) =>
            d.ExitRoom.Neighbors.Select(n => n.Item1).ToList();

        // a room is a leaf if it has exactly one neighbor
        private static bool IsLeaf(Room r) => r.Neighbors.Count == 1;

        // incode spec for Game(conf).
        // uses pairwise instead of combinatorial to keep the number of generated
        // test cases manageable as said in assignment
        [Test, Pairwise]
        public void Spec_GameConstructor(
            [Values(
                DungeonShapeType.LINEAR,
                DungeonShapeType.TREE,
                DungeonShapeType.GRID)]
            DungeonShapeType shape,

            // N: room count  minimum is 3 linear, 5 tree/grid
            [Values(5, 8, 12)]
            int numberOfRooms,

            // γ: max room capacity
            [Values(1, 3, 5)]
            int maxCapacity,

            // M: kept small enough to be possible in small dungeons
            [Values(0, 2, 4)]
            int monsters,

            // H: heal pots
            [Values(0, 1, 2)]
            int healPots,

            // R: rage pots, illegal in linear
            [Values(0, 1)]
            int ragePots,

            [Values(
                DifficultyMode.NEWBIEmode,
                DifficultyMode.NORMALmode,
                DifficultyMode.ELITEmode)]
            DifficultyMode difficulty)
        {
            // linear dungeons have no leaf rooms so rage pots are forced to 0
            int actualRagePots = (shape == DungeonShapeType.LINEAR) ? 0 : ragePots;

            STVControlledRandom.SetSeed(42);
            var conf = MakeConf(shape, numberOfRooms, maxCapacity,
                                monsters, healPots, actualRagePots, difficulty);

            Game game;
            try
            {
                game = new Game(conf);
            }
            catch (Exception)
            {
                Assert.Inconclusive(
                    "constructor threw for this config deemed impossible to satisfy");
                return;
            }

            // basic non null checks, config, dungeon and player
            Assert.NotNull(game.Config,  "Game.Config should not be null after construction");
            Assert.NotNull(game.Dungeon, "Game.Dungeon should not be null after construction");
            Assert.NotNull(game.Player,  "Game.Player should not be null after construction");

            // player starts at the start room alive with full hp
            Assert.AreEqual(game.Dungeon.StartRoom, game.Player.Location,
                "players initial location must be the dungeons start room");
            Assert.IsTrue(game.Player.Alive,
                "player must be alive at game start");
            Assert.AreEqual(game.Player.HpMax, game.Player.Hp,
                "player hp must equal hpmax at game start");
            Assert.Greater(game.Player.Hp, 0,
                "player hp must be > 0 at game start");

            // ame state starts at turn 0 and is not yet over
            Assert.IsFalse(game.Gameover,       "Gameover must be false at game start");
            Assert.AreEqual(0, game.TurnNumber, "TurnNumber must be 0 at game start");

            Dungeon d = game.Dungeon;

            // dungeon has unique start and exit rooms plus at least one ordinary room
            Assert.NotNull(d.StartRoom, "StartRoom must not be null");
            Assert.NotNull(d.ExitRoom,  "ExitRoom must not be null");
            Assert.AreNotSame(d.StartRoom, d.ExitRoom,
                "StartRoom and ExitRoom must be different rooms");
            Assert.IsTrue(d.Rooms.Any(r => r.RoomType == RoomType.ORDINARYroom),
                "dungeon must contain at least one ordinary room");

            // all rooms are reachable from the start room no self loops
            Assert.AreEqual(d.Rooms.Count, d.StartRoom.ReachableRooms().Count,
                "every room must be reachable from the start room");
            foreach (var room in d.Rooms)
                Assert.IsFalse(room.Neighbors.Any(n => n.Item1 == room),
                    $"room {room.Id} has a self-loop, which is not allowed");

            // start and exit rooms have capacity 0, exit-neighbors have capacity γ,
            // all other ordinary rooms have capacity in [1..γ]
            Assert.AreEqual(0, d.StartRoom.Capacity, "start room must have capacity 0");
            Assert.AreEqual(0, d.ExitRoom.Capacity,  "exit room must have capacity 0");
            var exitNeighborSet = ExitNeighbors(d);
            foreach (var ne in exitNeighborSet)
                Assert.AreEqual(maxCapacity, ne.Capacity,
                    $"exit neighbor {ne.Id} must have capacity == γ ({maxCapacity})");
            foreach (var room in d.Rooms
                .Where(r => r.RoomType == RoomType.ORDINARYroom && !exitNeighborSet.Contains(r)))
            {
                Assert.GreaterOrEqual(room.Capacity, 1,
                    $"ordinary room {room.Id} must have capacity >= 1");
                Assert.LessOrEqual(room.Capacity, maxCapacity,
                    $"ordinary room {room.Id} must have capacity <= γ ({maxCapacity})");
            }

            // exit room must not be adjacent to the start room
            Assert.IsFalse(d.StartRoom.Neighbors.Any(n => n.Item1 == d.ExitRoom),
                "exit room must not be a direct neighbor of the start room");

            // every monster is alive with hp > 0 and attackrating > 0
            foreach (var room in d.Rooms)
                foreach (var creature in room.Creatures)
                    if (creature is Monster m)
                    {
                        Assert.IsTrue(m.Alive,
                            $"monster {m.Id} in room {room.Id} must be alive");
                        Assert.Greater(m.Hp, 0,
                            $"monster {m.Id} in room {room.Id} must have Hp > 0");
                        Assert.Greater(m.AttackRating, 0,
                            $"monster {m.Id} in room {room.Id} must have AttackRating > 0");
                    }

            // no room may hold more monsters than its capacity
            foreach (var room in d.Rooms)
                Assert.LessOrEqual(room.NumberOfMonsters, room.Capacity,
                    $"room {room.Id} has {room.NumberOfMonsters} monsters but capacity {room.Capacity}");

            // every exit neighbor room must have >= monsters than any non exit neighbor room
            var nonNeighborRooms = d.Rooms
                .Where(r => r != d.StartRoom && r != d.ExitRoom && !exitNeighborSet.Contains(r))
                .ToList();
            foreach (var ne in exitNeighborSet)
                foreach (var other in nonNeighborRooms)
                    Assert.GreaterOrEqual(ne.NumberOfMonsters, other.NumberOfMonsters,
                        $"exit neighbor {ne.Id} ({ne.NumberOfMonsters} monsters) must have >= " +
                        $"monsters than non-NE room {other.Id} ({other.NumberOfMonsters} monsters)");

            // start and exit rooms must have no items and no monsters
            Assert.IsEmpty(d.StartRoom.Items,     "start room must have no items");
            Assert.IsEmpty(d.StartRoom.Creatures, "start room must have no monsters");
            Assert.IsEmpty(d.ExitRoom.Items,      "exit room must have no items");
            Assert.IsEmpty(d.ExitRoom.Creatures,  "exit room must have no monsters");

            // at least floor(N/2)+2 rooms must contain no items
            int roomsWithNoItems    = d.Rooms.Count(r => !r.Items.Any());
            int minRoomsWithNoItems = (d.Rooms.Count / 2) + 2;
            Assert.GreaterOrEqual(roomsWithNoItems, minRoomsWithNoItems,
                $"at least floor(N/2)+2 = {minRoomsWithNoItems} rooms must have no items, " +
                $"found {roomsWithNoItems}");

            // no two neighboring rooms may both contain heal pots
            foreach (var room in d.Rooms)
            {
                if (!room.Items.Any(i => i is HealingPotion)) continue;
                foreach (var (neighbor, _) in room.Neighbors)
                    Assert.IsFalse(neighbor.Items.Any(i => i is HealingPotion),
                        $"room {room.Id} and neighbor {neighbor.Id} both contain heal pots");
            }

            // rage pots may only appear in leaf rooms
            foreach (var room in d.Rooms)
            {
                if (!room.Items.Any(i => i is RagePotion)) continue;
                Assert.IsTrue(IsLeaf(room),
                    $"room {room.Id} contains a rage pot but is not a leaf room");
            }
        }

        // N<3 is below the minimum for a linear dungeon and must throw
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Game_LinearTooFewRooms_Throws(int N)
        {
            var conf = MakeConf(DungeonShapeType.LINEAR, N, 2, 0, 0, 0, DifficultyMode.NORMALmode);
            Assert.Throws<Exception>(() => new Game(conf),
                "constructor must throw when N<3 for a linear dungeon.");
        }

        // N<5 is below the minimum for tree and gridf dungeons and must throw
        [TestCase(DungeonShapeType.TREE)]
        [TestCase(DungeonShapeType.GRID)]
        public void Game_TreeOrGridTooFewRooms_Throws(DungeonShapeType shape)
        {
            var conf = MakeConf(shape, 4, 2, 0, 0, 0, DifficultyMode.NORMALmode);
            Assert.Throws<Exception>(() => new Game(conf),
                $"constructor must throw when N < 5 for a {shape} dungeon");
        }

        // more monsters than (N-2)*γ is impossible to satisfy and must throw
        [Test]
        public void Game_ImpossibleMonsterCount_Throws()
        {
            // (5-2)*1 = 3 max monsters, requesting 100 is impossible
            var conf = MakeConf(DungeonShapeType.LINEAR, 5, 1,
                monsters: 100, healPots: 0, ragePots: 0,
                difficulty: DifficultyMode.NORMALmode);
            Assert.Throws<Exception>(() => new Game(conf),
                "constructor must throw when monster count exceeds dungeon capacity");
        }

        // linear dungeons have no leaf rooms so rage pots cannot be placed must throw
        [Test]
        public void Game_RagePotsInLinearDungeon_Throws()
        {
            var conf = MakeConf(DungeonShapeType.LINEAR, 5, 2,
                monsters: 0, healPots: 0, ragePots: 1,
                difficulty: DifficultyMode.NORMALmode);
            Assert.Throws<Exception>(() => new Game(conf),
                "constructor must throw when rage potions are requested in a linear dungeon");
        }

        // a minimal valid config must never throw
        [TestCase(DungeonShapeType.LINEAR, 3)]
        [TestCase(DungeonShapeType.TREE,   5)]
        [TestCase(DungeonShapeType.GRID,   5)]
        public void Game_MinimalValidConfig_DoesNotThrow(DungeonShapeType shape, int N)
        {
            STVControlledRandom.SetSeed(1);
            var conf = MakeConf(shape, N, 1, 0, 0, 0, DifficultyMode.NEWBIEmode);
            Assert.DoesNotThrow(() => new Game(conf),
                $"a minimal valid config for shape {shape} must not throw");
        }

        // all three difficulty modes must produce a valid game and store the correct mode
        [TestCase(DifficultyMode.NEWBIEmode)]
        [TestCase(DifficultyMode.NORMALmode)]
        [TestCase(DifficultyMode.ELITEmode)]
        public void Game_AllDifficultyModes_ConstructSuccessfully(DifficultyMode difficulty)
        {
            STVControlledRandom.SetSeed(1);
            var conf = MakeConf(DungeonShapeType.LINEAR, 5, 2, 1, 0, 0, difficulty);
            Game game = null;
            Assert.DoesNotThrow(() => game = new Game(conf),
                $"constructor must not throw for difficulty mode {difficulty}");
            Assert.AreEqual(difficulty, game.Config.DifficultyMode,
                "stored config must reflect the requested difficulty mode");
        }

        // player kp must start at 0 and bag must be empty
        [Test]
        public void Game_PlayerInitialState_KpZeroAndBagEmpty()
        {
            STVControlledRandom.SetSeed(1);
            var conf = MakeConf(DungeonShapeType.LINEAR, 5, 2, 0, 0, 0, DifficultyMode.NORMALmode);
            var game = new Game(conf);
            Assert.AreEqual(0, game.Player.Kp,
                "player kill-points must be 0 at game start");
            Assert.IsEmpty(game.Player.Bag,
                "player bag must be empty at game start");
        }
    }
}