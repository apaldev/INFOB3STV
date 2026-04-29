using System;
using System.Linq;
using NUnit.Framework;
using static STVrogue.Utils.HelperPredicates;

namespace NUnitTests
{

    class SomeClassToTest
    {
        public static void Cond(int x)
        {
            int y= 0;
            if (x == 0)
            {
                y=1;
            }
            else 
                y = 2;

            if (x == 3) 
                y = 3;
        }

        public static void Loop(int x)
        {
            int y = 0;
            for (int i = 0; i < x; i++)
            {
                if (i==3) 
                    continue;
                if (i == 5)  break;
                y++;
            }
        }

        public static void Expr(int x, int y)
        {
            int z = 0;
            if (x == 0 && y == 0)
                z = 1;
            else
                z = 2;
            if (x == 3 || y == 3) 
                z = 3;
            else 
                z = 4;
        }
    }
    [TestFixture]
    public class Test_StatementCoverage
    {
        [Test]
        public void Test_cond0()
        {
            SomeClassToTest.Cond(0);
        }
        
        [Test]
        public void Test_cond2()
        {
            SomeClassToTest.Cond(2);
        }
        
        [Test]
        public void Test_cond3()
        {
            SomeClassToTest.Cond(3);
        }

        [Test]
        public void Test_loop0()
        {
            SomeClassToTest.Loop(0);
        }
        
        [Test]
        public void Test_loop2()
        {
            SomeClassToTest.Loop(2);
        }
        
        [Test]
        public void Test_loop4()
        {
            SomeClassToTest.Loop(4);
        }
        
        [Test]
        public void Test_loop6()
        {
            SomeClassToTest.Loop(6);
        }

        [Test]
        public void Test_Expr00()
        {
            SomeClassToTest.Expr(0,1);
        }
        
        [Test]
        public void Test_Expr30()
        {
            SomeClassToTest.Expr(3,0);
            int[] a = {1, 2};
        }

        [Test]
        public void Test_ForallArray()
        {
            int[] a = {3,3};
            Assert.False(Forall(a, x => x == 3));
            Assert.True(Forall(a, i => a[i]==3));
        }

    }
}