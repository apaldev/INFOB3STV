using System;
using System.Diagnostics;

namespace STVrogue.Utils
{
    public class STVLogger
    {
        /// <summary>
        /// Logging level 0 or less means that logged texts will be ignored. Logging lever
        /// higher than 1 means the logged messages will be logged :) The default is 1.
        /// </summary>
        public static int loggingLevel = 1 ;
        
        /// <summary>
        /// Log the string. The current behavior is to simply print the string to
        /// the console. Change the method if you want a different behavior.
        /// </summary>
        public static void Log(string s)
        {
            if (loggingLevel <= 0) return;
            Console.Out.WriteLine("** LOG: " + s);
            Debug.WriteLine("** LOG: " + s);
        }
    }
}
