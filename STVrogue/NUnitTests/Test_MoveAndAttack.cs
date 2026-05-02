using System;
using NUnit.Framework;
using STVrogue.GameLogic;

namespace NUnitTests
{
    [TestFixture]
    public class Test_MoveAndAttack
    {
        // two connected rooms (r0 -> r1)
        private (Room r0, Room r1) TwoConnectedRooms(int capacity = 5)
        {
            var r0 = new Room("R0", RoomType.STARTroom,    capacity);
            var r1 = new Room("R1", RoomType.ORDINARYroom, capacity);
            r0.Connect(r1, Direction.EAST);
            return (r0, r1);
        }

        // three rooms in a line: r0 – r1 – r2 (r0 and r2 are not connected)
        private (Room r0, Room r1, Room r2) ThreeLinearRooms(int capacity = 5)
        {
            var r0 = new Room("R0", RoomType.STARTroom,    capacity);
            var r1 = new Room("R1", RoomType.ORDINARYroom, capacity);
            var r2 = new Room("R2", RoomType.EXITroom,     capacity);
            r0.Connect(r1, Direction.EAST);
            r1.Connect(r2, Direction.EAST);
            return (r0, r1, r2);
        }

        //  Move(r), Monster 
        // monster moves to a neighboring room: location, creatures lists updated
        [Test, Description("monster moves to a neighboring room and its location is updated")]
        public void Test_Monster_Move_ToNeighbor()
        {
            var (r0, r1) = TwoConnectedRooms();
            var monster = new Monster("M0", "Goblin");
            r0.Creatures.Add(monster);
            monster.Location = r0;

            monster.Move(r1);

            Assert.AreEqual(r1, monster.Location,         "location should be r1");
            Assert.IsTrue(r1.Creatures.Contains(monster), "r1 should contain the monster");
            Assert.IsFalse(r0.Creatures.Contains(monster),"r0 should no longer contain the monster");
        }

        // moving to a non-neighboring room must throw
        [Test, Description("monster cannot move to a room that is not a neighbor")]
        public void Test_Monster_Move_ToNonNeighbor_Throws()
        {
            var (r0, _, r2) = ThreeLinearRooms();
            var monster = new Monster("M0", "Goblin");
            r0.Creatures.Add(monster);
            monster.Location = r0;

            Assert.Throws<ArgumentException>(() => monster.Move(r2));
        }

        // room at capacity (0) -> must throw
        [Test, Description("monster cannot enter a room that is already at capacity")]
        public void Test_Monster_Move_RoomAtCapacity_Throws()
        {
            var r0 = new Room("R0", RoomType.STARTroom,    5);
            var r1 = new Room("R1", RoomType.ORDINARYroom, 0); // no monsters allowed
            r0.Connect(r1, Direction.EAST);

            var monster = new Monster("M0", "Goblin");
            r0.Creatures.Add(monster);
            monster.Location = r0;

            Assert.Throws<ArgumentException>(() => monster.Move(r1));
        }

        // two monsters can share a room with enough capacity
        [Test, Description("two monsters can move into the same room if there is enough capacity")]
        public void Test_Monster_Move_TwoMonstersInRoom()
        {
            var r0 = new Room("R0", RoomType.STARTroom,    5);
            var r1 = new Room("R1", RoomType.ORDINARYroom, 5);
            r0.Connect(r1, Direction.EAST);

            var m1 = new Monster("M1", "Goblin");
            var m2 = new Monster("M2", "Troll");
            r0.Creatures.Add(m1); m1.Location = r0;
            r0.Creatures.Add(m2); m2.Location = r0;

            m1.Move(r1);
            m2.Move(r1);

            Assert.AreEqual(2, r1.Creatures.Count, "both monsters should be in r1");
        }

        //  Move(r), Player

        // player moves to a neighboring room: location is updated
        [Test, Description("player moves to a neighboring room and their location is updated")]
        public void Test_Player_Move_ToNeighbor()
        {
            var (r0, r1) = TwoConnectedRooms();
            var player = new Player("P0", "Frodo");
            player.Location = r0;

            player.Move(r1);

            Assert.AreEqual(r1, player.Location, "player location should be r1");
        }

        // moving to a non-neighboring room must throw
        [Test, Description("player cannot move to a room that is not a neighbor")]
        public void Test_Player_Move_ToNonNeighbor_Throws()
        {
            var (r0, _, r2) = ThreeLinearRooms();
            var player = new Player("P0", "Frodo");
            player.Location = r0;

            Assert.Throws<ArgumentException>(() => player.Move(r2));
        }

        //  Creature.Attack(foe)

        // foe hp drops by attacker's attack rating
        [Test, Description("attacking a foe reduces their hp by the attacker's attack rating")]
        public void Test_Attack_ReducesFoeHp()
        {
            var (r0, _) = TwoConnectedRooms();
            var attacker = new Monster("M0", "Goblin"); // AR = 1
            var foe      = new Monster("M1", "Troll");  // HP = 3
            r0.Creatures.Add(attacker); attacker.Location = r0;
            r0.Creatures.Add(foe);      foe.Location      = r0;

            int hpBefore = foe.Hp;
            attacker.Attack(foe);

            Assert.AreEqual(hpBefore - attacker.AttackRating, foe.Hp, "hp should drop by AR");
        }

        // foe hp is floored at 0 (no negative hp)
        [Test, Description("a lethal attack brings hp to 0 and the foe is considered dead")]
        public void Test_Attack_HpFlooredAtZero()
        {
            var (r0, _) = TwoConnectedRooms();
            var attacker = new Creature("M0", "BigBoss", 10, 999); // one-shot kill
            var foe      = new Monster("M1", "Goblin");
            r0.Creatures.Add(attacker); attacker.Location = r0;
            r0.Creatures.Add(foe);      foe.Location      = r0;

            attacker.Attack(foe);

            Assert.AreEqual(0, foe.Hp,  "hp should not go below 0");
            Assert.IsFalse(foe.Alive,   "foe should be dead");
        }

        // foe in a different room -> must throw
        [Test, Description("a creature cannot attack a foe that is in a different room")]
        public void Test_Attack_FoeInDifferentRoom_Throws()
        {
            var (r0, r1) = TwoConnectedRooms();
            var attacker = new Monster("M0", "Goblin");
            var foe      = new Monster("M1", "Troll");
            r0.Creatures.Add(attacker); attacker.Location = r0;
            r1.Creatures.Add(foe);      foe.Location      = r1;

            Assert.Throws<ArgumentException>(() => attacker.Attack(foe));
        }

        // dead attacker -> must throw
        [Test, Description("a dead creature cannot attack")]
        public void Test_Attack_DeadAttacker_Throws()
        {
            var (r0, _) = TwoConnectedRooms();
            var attacker = new Monster("M0", "Goblin");
            var foe      = new Monster("M1", "Troll");
            r0.Creatures.Add(attacker); attacker.Location = r0;
            r0.Creatures.Add(foe);      foe.Location      = r0;

            attacker.Hp = 0; // kill attacker

            Assert.Throws<ArgumentException>(() => attacker.Attack(foe));
        }

        // already dead foe -> must throw
        [Test, Description("a creature cannot attack a foe that is already dead")]
        public void Test_Attack_DeadFoe_Throws()
        {
            var (r0, _) = TwoConnectedRooms();
            var attacker = new Monster("M0", "Goblin");
            var foe      = new Monster("M1", "Troll");
            r0.Creatures.Add(attacker); attacker.Location = r0;
            r0.Creatures.Add(foe);      foe.Location      = r0;

            foe.Hp = 0; // kill foe first

            Assert.Throws<ArgumentException>(() => attacker.Attack(foe));
        }

        // player attacks monster in same room: monster hp drops, player survives
        [Test, Description("player attacks a monster in the same room and survives")]
        public void Test_Player_Attack_Monster()
        {
            var (r0, _) = TwoConnectedRooms();
            var player  = new Player("P0", "Frodo");
            var monster = new Monster("M0", "Goblin");
            player.Location  = r0;
            r0.Creatures.Add(monster);
            monster.Location = r0;

            int hpBefore = monster.Hp;
            player.Attack(monster);

            Assert.IsTrue(player.Alive, "player should remain alive");
            Assert.AreEqual(hpBefore - player.AttackRating, monster.Hp, "monster hp should drop by AR");
        }

        // various attack ratings: foe hp clamped to max(0, hp - ar)
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(100)]
        [Description("foe hp is always clamped to 0 regardless of how high the attack rating is")]
        public void Test_Attack_ParameterizedAR(int ar)
        {
            var (r0, _) = TwoConnectedRooms();
            var attacker = new Creature("M0", "Boss",   10, ar);
            var foe      = new Creature("M1", "Minion", 3,  1);
            r0.Creatures.Add(attacker); attacker.Location = r0;
            r0.Creatures.Add(foe);      foe.Location      = r0;

            attacker.Attack(foe);

            int expectedHp = Math.Max(0, 3 - ar);
            Assert.AreEqual(expectedHp, foe.Hp, $"with AR={ar}, foe hp should be {expectedHp}");
        }
    }
}
