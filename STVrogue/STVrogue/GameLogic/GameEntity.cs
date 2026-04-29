using System;

namespace STVrogue.GameLogic
{
    /// <summary>
    /// A parent class representing all game entities in STV Rogue.
    /// </summary>
    public class GameEntity
    {
        /// <summary>
        /// Every entity is identified by a unique ID.
        /// </summary>
        public string Id { get; private set; }

        public GameEntity(string uniqueId)
        {
            // exception if the id is null
            Id = uniqueId ?? throw new ArgumentException();
        }
        
    }
}
