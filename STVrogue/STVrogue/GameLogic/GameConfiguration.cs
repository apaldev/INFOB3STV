using System;
using System.IO;
using System.Text;

namespace STVrogue.GameLogic
{
    /// <summary>
    /// Representing the configuration of a level. Methods to save an instance of this class
    /// to a file, and to load from a file are also provided.
    /// </summary>
    [Serializable()]
    public class GameConfiguration
    {
        public int RndSeed = 311223 ;
        public int NumberOfRooms;
        public int MaxRoomCapacity;
        public DungeonShapeType DungeonShape;
        public int InitialNumberOfMonsters;
        public int InitialNumberOfHealingPots;
        public int InitialNumberOfRagePots;
        public DifficultyMode DifficultyMode;

        /// <summary>
        /// Just a basic constructor.
        /// </summary>
        public GameConfiguration()
        {
        }

        /// <summary>
        /// A convenience method to get the full path leading to the root of this project's
        /// Solution directory. Getting this path is relevant in the following ways. The
        /// Console-application STVRogue will need to read a configuration file, and later
        /// perhaps also saved-gameplay that you want to replay. These files are to be placed
        /// in the "saved" sub-directory of this Solution-root directory, so that your
        /// STVRogue would be able to read those files when the Solution is placed in a
        /// different directory. You can find this "saved" directory at the same level as
        /// your root sln-file.
        ///
        /// Since the location of "saved" is relative to your Solution-root, you then need to
        /// somehow obtain the path to this root, which is what this method should do for you.
        /// Note that this method depends on the behavior of your build-tool, namely where it
        /// places your executables. But the implementation below should work for VS and Rider.
        /// I have not checked VScode.
        /// </summary>
        /// <returns></returns>
        public static string GetSolutionRootDir()
        {
            // Could not really come up with a generic way to do this; different build tools
            // might place the app in different locations :|
            // This assumes STVRogue will be run from ProjectName/bin/Debug/netcoreapp2.1/appname
            DirectoryInfo projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string solutionDir = projectDir.Parent.Parent.Parent.Parent.FullName;
            return solutionDir;
        }

        /// <summary>
        /// This will save the configuration to a file. The file will be place in the directory
        /// STVrogue/saved.
        /// </summary>
        public void SaveToFile(string filename)
        {
            string o = "" + RndSeed 
                          + ":" + NumberOfRooms
                          + ":" + MaxRoomCapacity
                          + ":" + DungeonShape
                          + ":" + InitialNumberOfMonsters
                          + ":" + InitialNumberOfHealingPots
                          + ":" + InitialNumberOfRagePots
                          + ":" + DifficultyMode;
            //Console.WriteLine(o);
            string fileFullPath = Path.Combine(GetSolutionRootDir(),"saved",filename) ;
            Console.WriteLine((">>> Saving conf. to: " + fileFullPath));
            using (StreamWriter outputFile = new StreamWriter(fileFullPath))
            {
                outputFile.WriteLine(o);
            }
        }

        /// <summary>
        /// Create an instance of GameConfiguration by reading it from a text-file. The file is
        /// assumed to be located in the STVRogue/saved directory. The format of the file is
        /// as follows:
        ///
        ///     number-of-room : maxRoomCapacity : dungeonShape : initialNumberOfMonsters : initialNumberOfHealingPots : initialNumberOfRagePots : difficultyMode
        ///
        /// In a single line; the components are separated by ":". 
        /// </summary>
        /// <param name="filename"></param>
        public GameConfiguration(string filename)
        {
            string fileFullPath = Path.Combine(GetSolutionRootDir(),"saved",filename) ;
            Console.WriteLine((">>> Reading a game-configuration from: " + fileFullPath));
            StreamReader sr = new StreamReader(fileFullPath);
            string input = sr.ReadLine();
            sr.Close();
            string[] components = input.Split(":");
            RndSeed = int.Parse(components[0]);
            NumberOfRooms = int.Parse(components[1]);
            MaxRoomCapacity = int.Parse(components[2]);
            switch(components[3])
            {
                case "LINEARshape": DungeonShape = DungeonShapeType.LINEAR; break;
                case "TREEshape": DungeonShape = DungeonShapeType.TREE; break;
                default: DungeonShape = DungeonShapeType.GRID; break;
            }
            InitialNumberOfMonsters = int.Parse(components[4]);
            InitialNumberOfHealingPots = int.Parse(components[5]);
            InitialNumberOfRagePots = int.Parse(components[6]);
            switch(components[7])
            {
                case "NEWBIEmode" : DifficultyMode = DifficultyMode.NEWBIEmode; break;
                case "ELITEmode"  : DifficultyMode = DifficultyMode.ELITEmode; break;
                default: DifficultyMode = DifficultyMode.NORMALmode; break;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Difficulty       : {DifficultyMode}");
            sb.AppendLine($"#monsters        : {InitialNumberOfMonsters}");
            sb.AppendLine($"#rooms           : {NumberOfRooms}");
            sb.AppendLine($"#dungeon-shape   : {DungeonShape}");
            sb.AppendLine($"max-room-capacity: {MaxRoomCapacity}");
            sb.AppendLine($"#heal-pots       : {InitialNumberOfHealingPots}");
            sb.AppendLine($"#rage-pots       : {InitialNumberOfRagePots}");
            sb.AppendLine($"Seed             : {RndSeed}");
            return sb.ToString();
        }
    }
}