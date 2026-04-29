using System;
using STVrogue.GameLogic;

namespace STVrogue.TestInfrastructure
{
    public class ExampleTemporalSpecification
    {
        /// <summary>
        /// The player's hit point is never negative.
        /// </summary>
        static public TemporalProperty<Game> example1 = new TemporalAlways<Game>(G=>G.Player.Hp >= 0);
        
        /// <summary>
        /// The player's kill point is never negative.
        /// </summary>
        static public TemporalProperty<Game> example2 = new TemporalAlways<Game>(G=>G.Player.Kp >= 0);
        
        /// <summary>
        /// The player's kill point never decreases.
        /// </summary>
        static public TemporalProperty<Game> example3 
            = new TemporalAlways<Game>(G =>G.Player.Kp, (before,now) => before >= now) ;
    }
}
