using System;
using System.Diagnostics;
using NUnit.Framework;
using STVrogue.GameLogic;

namespace NUnitTests
{
    /*
     * This class shows an example of testing using NUnit "Theory". A theory is
     * basically a specification of a program under-test. It is similar to
     * the concept of "property" in property-based testing a la QuickCheck.
     *
     * A theory can fully specify the program under test; or else you can split
     * it in multiple theories to cover various aspects of the program under
     * test.
     *
     * Tests will be generated from your specified datapoint-source. Do note
     * that this data-source is type-bound. It means that if your theory takes
     * two parameters of the same type e.g. int, NUnit will draw data from the
     * same datapoint-source for both parameters (it cannot distinguish which
     * int should be used for the first parameter, and which one for the second).
     *
     * NUnit will generate all possible value-combinations over the values you
     * supply in the datapoint-sources. So be careful that this may blow up.
     */
    [TestFixture]
    public class NUnitTest_withTheory
    {
        // specify here the data to be used for generating the tests.
        // Below we specify strings to use and int to use.
        [DatapointSource] 
        public string[] Ids = {null, "", "C0", "C1"};
        [DatapointSource]
        public int[] IntValues = { -1,0,1,99,int.MaxValue };

        
        // Theory-1 expresses which exception should be thrown when the inputs are invalid:
        [Theory]
        public void creatureConstructor1_Theory1(string id, int hp, int ar)
        {
            Assume.That(id==null || hp <= 0 || ar <= 0);
            Debug.WriteLine("** negative test with (" + id + "," + hp + "," + ar + ")");
            Assert.Throws<ArgumentException>(() => new Creature(id, "Orc", hp, ar));
        }
        
        // Theory-2 expresses how a constructor of Creature should behave when given
        // valid inputs
        [Theory]
        public void creatureConstructor1_Theory2(string id, int hp, int ar)
        {
            Assume.That(id!=null && hp > 0 && ar > 0); 
            Creature C = new Creature(id,"Orc",hp,ar);
            Assert.That(C.Hp==hp && C.HpMax==hp);
            Assert.That(C.AttackRating > 0);
        }
    }
}