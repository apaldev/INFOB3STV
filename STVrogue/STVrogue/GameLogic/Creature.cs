using System;
using System.Collections.Generic;


namespace STVrogue.GameLogic
{
    public class Creature : GameEntity
    {
        #region Fields and properties
        
        public string Name { get ; private set ; }
        public int HpMax { get; private set;  }
        
        /// <summary>
        /// The current HP of the Creature, should never exceed HPmax.
        /// </summary>
        public int Hp { get; set; }    
        
        /// <summary>
        /// True if and only if the creature is alive.
        /// </summary>
        public bool Alive => Hp > 0;

        /// <summary>
        /// The current location of the creature (in which room).
        /// </summary>
        public Room Location { get; set; }

        public int AttackRating { get ; set; }
        
        #endregion

        public Creature(string id, string name, int hpmax, int ar) : base(id)
        {
            if(hpmax<=0 || ar<=0) 
                throw new ArgumentException();
            Name = name;
            HpMax = hpmax;
            Hp = hpmax;
            AttackRating = ar;
        }
        
        /// <summary>
        /// Move this creature to the given room. This is only allowed if r
        /// is a neighboring room of the creature's current location. Also
        /// keep in mind that rooms have capacity.
        ///
        /// <para></para>
        /// NOTE: override this in respective subclasses.
        /// </summary>
        public virtual void Move(Room r)
        {
            if (! Location.Neighbors.Exists(n => n.Item1 == r))
            {
                throw new ArgumentException();
            }
        }
        
        /// <summary>
        /// Attack the given foe. This is only possible if this creature is alive and
        /// if the foe is in the same room as this creature.
        /// The code below provides a base implementation of this method. You may have
        /// to override this for Player.
        /// </summary>
        public virtual void Attack(Creature foe)
        {
            if (!Alive 
                || Location != foe.Location 
                || !foe.Alive)
            {
                throw new ArgumentException();
            }
            foe.Hp = Math.Max(0,foe.Hp - AttackRating);
        }
       
    }

    /// <summary>
    /// Representing monsters.... you know, those scary things you don't want
    /// to mess with.
    /// </summary>
    public class Monster : Creature
    {
        public Monster(string id, string name) : base(id,name,3,1)
        {
            // change the behavior if you want monsters to have different HP and AR
        }
        
        public override void Move(Room r)
        {
            base.Move(r);
            if (r.NumberOfMonsters > r.Capacity) // kutu note
            {
                throw new ArgumentException();
            }
            r.Creatures.Add(this);
            Location.Creatures.Remove(this);
            Location = r;
        }
    }

    public class Player : Creature
    {
        public Player(string id, string name) : base(id,name,20,1)
        {
            // change the behavior if you want players to have different HP and AR,
            // or if you want to add more properties to the player.
        }

        #region getters setters
        
        /// <summary>
        /// The player's kill-point. Should never be negative.
        /// </summary>
        public int Kp { get; set; } = 0;

        /// <summary>
        /// The player's bag, containing all items the player has picked up
        /// (and has not used).
        /// </summary>
        public List<Item> Bag { get ; } =  new List<Item>();

        /// <summary>
        /// True if the player is enraged. The player enters this state whenever it uses a rage potion.
        /// The effect last for 5 turns including the turn when the potion is used.
        /// </summary>
        public bool Enraged { get; set; } = false;
        
        #endregion

        public override void Move(Room r)
        {
            base.Move(r);
            Location = r;
        }
        
        /// <summary>
        /// Use the given item. We also pass the current turn-number at which
        /// this action happens.
        /// </summary>
        public void Use(int turnNr, Item i)
        {
            throw new NotImplementedException();
        }
    }
    
}
