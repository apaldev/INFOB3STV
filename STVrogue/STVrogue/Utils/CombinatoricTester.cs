using System;
using System.Collections.Generic;
using System.Linq;

namespace STVrogue.Utils
{
    /// <summary>
    /// Since NUnit can also do combinatoric testing, we don't need this class anymore.
    /// </summary>
    public class CombinatoricTester
    {
        Dictionary<string, Dictionary<string,Object>> classes = new Dictionary<string, Dictionary<string,Object>>();
        string[] classNames;
        
        List<string[]> executedTests = new List<string[]>();

        public CombinatoricTester(params string[] classes)
        {
            classNames = classes;
        }
        
        public void AddBlock(string theClass, string thePartition, Object testValue)
        {
            if (classes.ContainsKey(theClass))
            {
                var blocks = classes[theClass];
                blocks[thePartition] = testValue;
            }
            else
            {
                var blocks = new Dictionary<string, Object>();
                blocks[thePartition] = testValue;
                classes[theClass] = blocks;
            }
        }

        /// <summary>
        /// Override this.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Object RawInvokeMethodUnderTest(params Object[] args)
        {
            throw new NotImplementedException();
        }

        public Object InvokeMethodUnderTest(params string[] blocks)
        {
            Object[] args = new Object[blocks.Length];
            for (int k=0; k<blocks.Length; k++)
            {
                var blockName = blocks[k];
                var theClass = classes[classNames[k]];
                var testValue = theClass[blockName];
                args[k] = testValue;
            }
            // now invoke method under test:
            executedTests.Add(blocks);
            return RawInvokeMethodUnderTest(args);
        }

        public void ClearTrackedTests()
        {
            executedTests.Clear();
        }

        public string PrintExecutedTests()
        {
            string z = "";
            int k = 0;
            foreach(var test in executedTests)
            {
                if (k>0) z += "\n";
                z += "   " + (k+1) + ": (";
                for (int i = 0; i < test.Length; i++)
                {
                    if (i > 0) z += ",";
                    z += test[i];
                }
                z += ")";
                k++;
            }
            return z;
        }

        List<string[]> GetBlocks()
        {
            List<string[]> blocks = new List<string[]>() ;
            for (int k = 0; k < classNames.Length; k++)
            {
                var blocks_k = classes[classNames[k]].Keys.ToArray();
                blocks.Add(blocks_k);
            }

            return blocks;
        }

        List<string> UncoveredBlocks()
        {
            var allBlocks = GetBlocks();
            List<string> uncovered = new List<string>();
            // for each class-k
            for (int k = 0; k < classNames.Length; k++)
            {
                // for each block b in class-k, check if b is covered:
                foreach (var b in allBlocks[k])
                {
                    bool covered = false;
                    foreach (var test in executedTests)
                    {
                        if (b == test[k])
                        {
                            covered = true;
                            break;
                        }
                    }
                    if(! covered) uncovered.Add(b);
                }
            }
            return uncovered;
        }

        List<KeyValuePair<string, string>> UncoveredPairs()
        {
            var allBlocks = GetBlocks();
            List<KeyValuePair<string, string>> uncovered = new List<KeyValuePair<string, string>>();
            // for each pair of class-k1 x class-k2:
            for (int k1 = 0; k1 < classNames.Length; k1++)
            {
                for (int k2 = k1 + 1; k2 < classNames.Length; k2++)
                {
                    // for each pair b1xb2 in class-k1 x class-k2, check if the pair is covered:
                    foreach (var b1 in allBlocks[k1])
                    {
                        foreach (var b2 in allBlocks[k2])
                        {
                            bool covered = false;
                            foreach (var test in executedTests)
                            {
                                if (b1 == test[k1] && b2 == test[k2])
                                {
                                    covered = true;
                                    break;
                                }
                            }

                            var pair = new KeyValuePair<string, string>(b1, b2);
                            if (!covered) uncovered.Add(pair);
                        }
                    }
                }
            }
            return uncovered;
        }

        public string PrintUncovered()
        {
            var allBlocks = GetBlocks();
            int numOfBlocks = 0;
            foreach (var blocks in allBlocks)
            {
                numOfBlocks += blocks.Length;
            }

            int numOfPairs = 0;
            for (int k1 = 0; k1 < allBlocks.Count; k1++)
            {
                for (int k2 = k1+1 ; k2 < allBlocks.Count; k2++)
                {
                    numOfPairs += allBlocks[k1].Length * allBlocks[k2].Length;
                }
            }
                
            string z = "";
            var uncoveredBlocks = UncoveredBlocks();
            z += "** uncoverd blocks = " + uncoveredBlocks.Count + "/" + numOfBlocks ;
            if (uncoveredBlocks.Count > 0)
            {
                z += ":\n    ";
                int i = 0;
                foreach (var b in uncoveredBlocks)
                {
                    if (i > 0) z += ", ";
                    z += b; 
                    i++;
                }
            }

            var uncoveredPairs = UncoveredPairs();
            z += "\n** uncoverd pairs = " + uncoveredPairs.Count + "/" + numOfPairs ;
            if (uncoveredPairs.Count > 0)
            {
                z += ":\n    ";
                int i = 0;
                foreach (var pair in uncoveredPairs)
                {
                    if (i > 0) z += ", (";
                    z += pair.Key + "," + pair.Value + ")"; 
                    i++;
                }
            }

            return z;
        }
        
    }
}