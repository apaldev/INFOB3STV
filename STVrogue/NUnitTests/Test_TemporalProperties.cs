using System;
using System.Linq;
using NUnit.Framework;
using STVrogue.TestInfrastructure;
using static STVrogue.TestInfrastructure.TemporalPropertyDSL;
using static STVrogue.Utils.HelperPredicates;

namespace NUnitTests
{
    class IntSequence 
    {
        
        int[] sequence;
            
        public IntSequence(params int[] data) {  sequence = data; }
        
        public Judgement Satisfies(TemporalProperty<int> phi)
        {
            phi.Reset();
            Judgement j = Judgement.Inconclusive;
            foreach (var x in sequence)
            {
                j = phi.EvaluateNextState(x);
            }

            return j;
        }

        static Judgement SatisfiedByAll(TemporalProperty<int> phi, IntSequence[] seqs)
        {
            int numOfValids = 0;
            for (var i = 0; i < seqs.Length; ++i)
            {
                var j = seqs[i].Satisfies(phi);
                if (j == Judgement.Invalid) return j;
                if (j == Judgement.Valid) numOfValids++;
            }
            if (numOfValids >= seqs.Length) return Judgement.Valid;
            return Judgement.Inconclusive;
        }
    }
    
    [TestFixture]
    public class Test_TemporalProperties
    {
        [Test]
        public void Test1()
        {
            var seq1 = new IntSequence(0, 1, 2, 3, 4);
            Assert.AreEqual(Judgement.Valid, seq1.Satisfies(Always<int>(x => x >= 0))) ;
            Assert.AreEqual(Judgement.Invalid, seq1.Satisfies(Always<int>(x => x != 2))) ;
            Assert.AreEqual(Judgement.Valid, seq1.Satisfies(Eventually<int>(x => x == 3))) ;
            Assert.AreEqual(Judgement.Invalid, seq1.Satisfies(Eventually<int>(x => x < 0))) ;
            Assert.AreEqual(Judgement.Valid, seq1.Satisfies(
                    And(Always<int>(x => x >= 0),
                           Eventually<int>(x => x==4)))
                    );
            Assert.AreEqual(Judgement.Valid, seq1.Satisfies(
                Or(Always<int>(x => x < 0),
                      Eventually<int>(x => x==4)))
            );
        }

        [Test]
        public void TestChangePreds()
        {
            var seq1 = new IntSequence(0, 1, 2, 3, 4);
            Assert.AreEqual(Judgement.Valid, 
                seq1.Satisfies(Always<int>(x => 2*x,  (x0,x) =>
                {    Console.WriteLine($">> x0={x0}, x={x}");
                     return x == 2+x0;
                }))) ;
            Assert.AreEqual(Judgement.Invalid, 
                seq1.Satisfies(Eventually<int>(x => 2*x,  (x0,x) => x>0 && x == x0))) ;
            
            var seq2 = new IntSequence(0, 1);
            Assert.AreEqual(Judgement.Valid, 
                seq2.Satisfies(Always<int>(x => 2*x,  (x0,x) => x == x0+2))) ;
            
            var seq3 = new IntSequence(0);
            Assert.AreEqual(Judgement.Inconclusive, 
                seq3.Satisfies(Always<int>(x => 2*x,  (x0,x) => x == x0+2))) ;
        }

        [Test]
        public void Test3()
        {
            IntSequence[] seqs =
            {
                new IntSequence(0, 1, 2, 3, 4),
                new IntSequence(1, 1, 2, 3, 3),
                new IntSequence(4, 3, 2, 1, 0)
            };
            Assert.IsTrue(Forall(seqs, seq => 
                seq.Satisfies(Always<int>(x => 0 <= x && x <= 4)) == Judgement.Valid ));
            Assert.IsFalse(Forall(seqs, seq => 
                seq.Satisfies(Eventually<int>(x => x == 4)) == Judgement.Valid ));
            Assert.IsTrue(Exists(seqs, seq => 
                seq.Satisfies(Eventually<int>(x => x == 4)) == Judgement.Valid ));
        }
    }
}