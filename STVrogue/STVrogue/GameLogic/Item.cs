using System;
namespace STVrogue.GameLogic
{
    public class Item : GameEntity
    {
        public Item(String id) : base(id){ }
        
    }

    public class HealingPotion : Item
    {
        /// <summary>
        /// It can heal this amount of HP.
        /// </summary>
        public int HealValue { get ; private set ; }

        public HealingPotion(String id, int heal) : base(id)
        {
            HealValue = heal;
        }
        
    }

    public class RagePotion : Item
    {
        public RagePotion(String id) : base(id){ }
        
    }

}
