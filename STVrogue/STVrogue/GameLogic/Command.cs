using System;

namespace STVrogue.GameLogic
{
    [Serializable()]
    public enum CommandType { DoNOTHING, MOVE, ATTACK, PICKUP, USE, FLEE }

    
    
    /// <summary>
    /// Representing a player command/action. 
    /// </summary>
    [Serializable()]
    public class Command
    {
        /// <summary>
        /// Construct a command with the given command-type and an array of arguments.
        /// </summary>
        public Command(CommandType name, params string[] args)
        {
            Name = name;
            Args = args;
        }

        public CommandType Name { get; private set; }
        
        /// <summary>
        /// Some commands have arguments. For example, "USE" should specify which item to use,
        /// and "ATTACK" should specify which monster to attack. The item/room/monster targeted by
        /// the command is specified through their unique ID. It is essential that you use the
        /// ID to allow series of commands to be saved.
        /// </summary>
        public string[] Args { get; private set; }
        
        public override string ToString()
        {
            string o = "" + Name;
            if (Args != null && Args.Length > 0)
            {
                o += "(";
                for (int i = 0; i < Args.Length; i++)
                {
                    if (i > 0) o += ",";
                    o += Args[i];
                }
                o += ")";
            }
            return o;
        }
    }
}
