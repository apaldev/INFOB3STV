using System;
using System.Collections.Generic;
using System.Text;

namespace STVrogue.Utils
{
    /// <summary>
    /// A utility class to help you track path-coverage. Use addTargetPath to specify
    /// the paths that we want to cover (target paths).
    ///
    /// <list type="bullet">
    ///   <item> Use <see cref="StartPath"/> to signal the start of a test execution. </item>
    ///   <item> Use <see cref="TickNode"/> to signal visit to a node/state. </item>
    ///   <item> Use <see cref="EndPath"/> to signal the end of the test execution. </item>
    ///   <item> Use <see cref="PrintSummary"/> etc to get information of coverage. </item>
    /// </list>
    /// </summary>
    public class PathCoverageTracker
    {
        List<String> testRerquirements = new List<String>();
        List<String> executed = new List<String>();
        String currentPath = null;

        public PathCoverageTracker(){ }

        /// <summary>
        /// Add a path to cover. Specify it as a string in the format of node-ids separated by ":".
        /// Example: "s1:s2:s3"
        /// </summary>
        public void AddTargetPath(String path) { testRerquirements.Add(path); }

        /// <summary>
        /// Start tracking a path.
        /// </summary>
        public void StartPath() { currentPath = ""; }

        /// <summary>
        /// Register that we pass the given node.
        /// </summary>
        public void TickNode(String node)
        {
            if (currentPath == "") currentPath += node;
            else currentPath += ":" + node;
        }

        /// <summary>
        /// Signal the end of the current path. It will then be added to the set of executed path. 
        /// </summary>
        public void EndPath()
        {
            foreach (String p in executed)
            {
                if (p.Equals(currentPath)) break;
            }
            executed.Add(currentPath);
        }

        /// <summary>
        /// Return the list of covered targets.
        /// </summary>
        public List<String> GetCoveredPaths()
        {
            List<String> covered = new List<String>();
            foreach (String target in testRerquirements)
            {
                foreach (String sigma in executed)
                {
                    if (Tour(sigma, target))
                    {
                        covered.Add(target); break;
                    }
                }
            }
            return covered;
        }

        /// <summary>
        /// Return the list of still uncovered targets.
        /// </summary>
        public List<String> GetUncoveredPaths()
        {
            List<String> covered = GetCoveredPaths();
            List<String> uncovered = new List<String>();
            foreach (String p in testRerquirements)
            {
                Boolean cov = false;
                foreach (String pcov in covered)
                {
                    if (p.Equals(pcov))
                    {
                        cov = true; break;
                    }
                }
                if (!cov) uncovered.Add(p);
            }
            return uncovered;
        }

        /// <summary>
        /// Return the list of paths to cover.
        /// </summary>
        public List<String> GetTestRerquirements()
        {
            return testRerquirements;
        }

        /// <summary>
        /// Return the current set of test paths. 
        /// </summary>
        public List<String> GetTestPaths()
        {
            return executed;
        }

        /// <summary>
        /// Check if path1 tours path2
        /// </summary>
        static public bool Tour(String path1, String path2)
        {
            return path1.Contains(path2);
        }

        public String PrintCovered()
        {
            StringBuilder sb = new StringBuilder();
            List<String> covered = GetCoveredPaths();
            for (int k = 0; k < covered.Count; k++)
            {
               sb.Append(covered[k]);
               sb.Append("\n");
            }
            sb.Append("Covered: " + covered.Count + " paths.");
            return sb.ToString();
        }

        public string PrintUncovered()
        {
            StringBuilder sb = new StringBuilder();
            List<String> uncovered = GetUncoveredPaths();
            for (int k = 0; k < uncovered.Count; k++)
            {
                sb.Append(uncovered[k]);
                sb.Append("\n");
            }
            sb.Append("Uncovered: " + uncovered.Count + " paths.");
            return sb.ToString();
        }

        /* Printing a coverage report. */
        public string PrintSummary()
        {
            int N = testRerquirements.Count ;
            int n = GetCoveredPaths().Count ;
            string z = "** The tests cover " + n + " targets out of " + N;
            if (n >= N) z += ". Well done!";
            else
            {
                z += "\n** Covered:";
                    foreach (String s in GetCoveredPaths())
                    {
                    z += "\n     " + s;
                    }
                z += "\n** Uncovered:";
                    foreach (String s in GetUncoveredPaths())
                    {
                    z += "\n     " + s;
                    }
            }
            return z ;
        }


    }
}
