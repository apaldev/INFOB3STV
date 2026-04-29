using System;
using System.Diagnostics;
using NUnit.Framework;
using STVrogue.GameLogic;

namespace NUnitTests
{
    /* Just an example of an NUnit test class to show how to write one. */
    [TestFixture]
    public class NUnitTestClass1
    {

        /// <summary>
        /// An example of a simple test. We will test the constructor of Creature. 
        /// </summary>
        [Test, Description("Test the constructor of Creature")]
        public void test1_Creature_contr()
        {
            var c = new Creature("M0","smaegol",10, 1);
            Assert.IsTrue(c.Hp == 10 && c.HpMax == 10 );
            Assert.IsTrue(c.AttackRating > 0);
        }
        
        /// <summary>
        /// Below is an example of a parameterized test in NUnit, that allows you to
        /// reuse the same testcode over multiple testdata. Essentially, the idea is
        /// similar to property-based testing.
        ///
        /// <para></para>
        /// Parameters of a TestCase-attribute are limited to primitive types.
        /// Else, use TestCaseSource. See the next example.
        /// </summary>
        [TestCase(1,100)]
        [TestCase(100,100)]
        [TestCase(99,1)]
        [Description("Test the constructor of Creature")]
        public void parameterizedTest_Creature_contr(int hp, int ar)
        {
            // on other arguments, we want to verify these correctness properties:
            var c = new Creature("M0","smaegol",hp,ar);
            Assert.IsTrue(c.Hp==hp && c.HpMax==hp);
            Assert.IsTrue(c.AttackRating > 0);
        }
        
        /// <summary>
        /// Another example of a parameterized test. The test inputs are taken
        /// from the specified TestCaseSource. Note that the source must be
        /// a static member.
        /// </summary>
        [TestCaseSource(nameof(SomeMonsters))]
        [Description("Test player attacks monster.")]
        public void parameterizedTest_PlayerAttack(Monster m)
        {
            var player = new Player("P0","Frodo");
            player.Attack(m);
            Assert.IsTrue(player.Alive);
            Assert.IsTrue(m.Hp>0 == m.Alive);
        }

        static Monster[] SomeMonsters() 
        {
            var m1 = new Monster("M1", "Goblin");
            var m2 = new Monster("M2", "Troll");
            m2.Hp = 10;
            Monster[] monsters = { m1, m2 };
            return monsters;
        }
        
        /*
         
         
         
         */
        /// <summary>
        /// An example of generating a combinatoric test with Nunit. The test below will generate
        /// the full combinations of the values you give.
        /// Be careful with this, as this can easily explode!
        /// <para></para>
        /// Note that the Values-attribute can only take primitive values. Else use 
        /// ValueSource (see NUnit documentation).
        /// </summary>
        [Test, Combinatorial]
        [Description("Test that on bad inputs the constructor does throw an exception")]
        public void fullCombinatoricTest_execeptionCase_Creature_contr([Values(-1, 0, 1)] int hp, [Values(-1, 0, 1)] int ar)
        {
            TestContext.Out.WriteLine("** (" + hp + "," + ar + ")");
            Debug.WriteLine("** (" + hp + "," + ar + ")");
            // suppose we only want to test the cases when either hp or ar is 
            // 0 or less. Other cases are 'not in the scope'. Use Assume filter
            // your test cases:
            Assume.That(hp <= 0 || ar <= 0);
            if (hp <= 0 || ar <= 0)
            // on the remaining cases, the constructor should throws an
            // illegal-argument-exception:
            Assert.Throws<ArgumentException>(() => new Creature("M0","smaegol", hp, ar));
        }

        /*
           
         */
        /// <summary>
        /// An example of generating pair-wise test using Nunit. It will generate a bunch of
        /// tests that will give full pair coverage over the set of values you specify.
        /// Be mindful that Nunit use a heuristic that not necessarily produce a minimal
        /// test-set.
        /// </summary>
        [Test, Pairwise]
        public void pairwiseTest_nonExceptionCase_Creature_contr(
            [Values("smaegol","nazgul")] string name,
            [Values(1,99,int.MaxValue)] int hp, 
            [Values(1, 99, int.MaxValue)] int ar)
        {
            Debug.WriteLine("** (" + name + "," + hp + "," + ar + ")");

            Creature C = new Creature("M0",name,hp,ar);
            Assert.IsTrue(C.Hp==hp && C.HpMax==hp);
            Assert.IsTrue(C.AttackRating > 0);
        }
    }
}