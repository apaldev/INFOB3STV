using System;
using System.Collections.Generic;

namespace STVrogue
{
    /// <summary>
    /// This class provides methods to interact with the console. That is, to send
    /// strings to be printed on the console, and to read input key or string from
    /// the console. 
    /// <para></para>
    /// It has an extra functionality to store the strings sent to the console and
    /// the strings read from the console, until you decide to clear them.
    /// </summary>
    public class GameConsole
    {
        /// <summary>
        /// Memorized console outputs and inputs.
        /// </summary>
        public List<string> ConsoleEcho { get; private set; } = new List<string>();
        
        /// <summary>
        /// Write one or more strings to the Console, separated by new-lines.
        /// </summary>
        public void WriteLines(params string[] messages)
        {
            foreach (string msg in messages)
            {   ConsoleEcho.Add(msg);
                Console.WriteLine(msg);
            }
        }
        
        /// <summary>
        /// Read a key-board key pressed by the user. It returns a character representing
        /// the pressed key.
        /// </summary>
        public char ReadKey()
        {
            char c = Console.ReadKey().KeyChar;
            ConsoleEcho.Add(">>INPUT:" + c);
            return c;
        }
        
        /// <summary>
        /// Read a line of string that the user entered in the Console. The user should
        /// hit the ENTER-key to close the line.
        /// </summary>
        public  string ReadLine()
        {
            string line = Console.ReadLine();
            ConsoleEcho.Add(">>INPUT:" + line);
            return line;
        }
        
        /// <summary>
        /// Clear the memorized console outputs and inputs. 
        /// </summary>
        public void clearConsoleEcho()
        {
            ConsoleEcho.Clear();
        }
    }
    
}