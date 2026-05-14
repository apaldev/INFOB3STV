using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using STVrogue.Utils;

namespace STVrogue.GameLogic
{
    
    /// <summary>
    /// Representing a dungeon. A dungeon consists of rooms, connected to from a graph.
    /// It has one unique starting room and one unique exit room. All rooms should be
    /// reachable from the starting room.
    /// </summary>
    public class Dungeon
    {

        #region Fields and Properties
        
        /// <summary>
        /// All rooms in the dungeon, including the start and exit rooms.
        /// </summary>
        public List<Room> Rooms { get; } = [];
        public Room StartRoom { get; protected set; }
        public Room ExitRoom { get; protected set; }
        
        IRandomGenerator randomGenerator;

        #endregion
        
        protected Dungeon() { }
        
        /// <summary>
        /// Create a dungeon with the indicated number of rooms and the indicated shape.
        /// A dungeon shape can be "linear" (list-shaped), "tree", or "random".
        /// <list type="bullet">
        /// <item>
        /// A dungeon should have a unique start-room and a unique exit-room. </item>
        /// <item>
        /// All rooms in the dungeon must be reachable from the start-room. </item>
        /// <item>
        /// Each room is set to have a random capacity between 1 and the given maximum-capacity.
        /// Start and exit-rooms should have capacity 0. </item>
        /// </list>
        ///
        /// The constructor also expects a random generator to be passed to it. For testing,
        /// use a deterministic random generator. 
        /// </summary>
        public Dungeon(IRandomGenerator rnd, DungeonShapeType shape, int numberOfRooms, int maximumRoomCapacity) : base()
        {   randomGenerator = rnd;
            switch (shape)
            {
                case DungeonShapeType.LINEAR:
                    MkLinearDungeon(numberOfRooms, maximumRoomCapacity);
                    break;
                case DungeonShapeType.TREE:
                    MkTreeDungeon(numberOfRooms, maximumRoomCapacity);
                    break;
                case DungeonShapeType.GRID:
                    MkGridDungeon(numberOfRooms, maximumRoomCapacity);
                    break;
            }
        }
        
        /// <summary>
        /// This creates a linear-shaped dungeon with the given number of rooms.
        /// </summary>
        private void MkLinearDungeon(int numberOfRooms, int maximumRoomCapacity)
        {
            if (numberOfRooms < 3)
                throw new ArgumentException("A linear dungeon should have at least three rooms.");
            
            Room prev = null;
            Room r = null;
            for (int k = 0 ; k<numberOfRooms; k++)
            {
                int capacity = randomGenerator.NextInt(maximumRoomCapacity) + 1;
                if (k == 0)
                {
                    r = new Room("R" + k, RoomType.STARTroom, 0);
                    StartRoom = r;
                    prev = r;
                }
                else if (k == numberOfRooms - 1)
                {
                    r = new Room("R" + k, RoomType.EXITroom, 0);
                    ExitRoom = r;
                    prev.Connect(r, Direction.EAST);
                    prev = r;
                }
                else
                {
                    r = new Room("R" + k, RoomType.ORDINARYroom, capacity);
                    prev.Connect(r, Direction.EAST);
                    prev = r;
                }
                Rooms.Add(r);
            }
        }
        
        /// <summary>
        /// This creates a tree-shaped dungeon with the given number of rooms.
        /// </summary>
        private void MkTreeDungeon(int numberOfRooms, int maximumRoomCapacity)
        {   
            if (numberOfRooms < 5)
                // this implementation won't create a TREE of four rooms, you can allow four,
                // but the implementation will then place the exit next to the start room
                throw new ArgumentException("A tree-shaped dungeon should have at least five rooms.");

            // we will create the start-room, then we will breadth-firstly expand it to a tree.
            
            // list to be used as fifo-queue for the breadth-first expansion:
            List<Room> toBeExpanded = new List<Room>();
            StartRoom = new Room("R" + 0, RoomType.STARTroom, 0);
            Rooms.Add(StartRoom);
            toBeExpanded.Add(StartRoom);
            while (Rooms.Count < numberOfRooms)
            {
                Room R = toBeExpanded[0];
                toBeExpanded.RemoveAt(0);
                // make the children
                int branchingDegree = 3;
                int numOfChildrenToAdd = Math.Min(branchingDegree, numberOfRooms - Rooms.Count);
                for (int k = 0; k < numOfChildrenToAdd; k++)
                {
                    int nextIndex = Rooms.Count;
                    bool isExit = nextIndex == numberOfRooms - 1;
                    int capacity = isExit ? 0 : randomGenerator.NextInt(maximumRoomCapacity + 1);
                    RoomType roomType = isExit ? RoomType.EXITroom : RoomType.ORDINARYroom;
                    Room childRoom = new Room("R" + nextIndex, roomType, capacity);
                    Direction dir = Direction.NORTH;
                    switch (k % branchingDegree)
                    {
                        case 1: dir = Direction.EAST; break;  
                        case 2: dir = Direction.SOUTH; break;
                    }
                    
                    R.Connect(childRoom, dir);
                    Rooms.Add(childRoom);
                    toBeExpanded.Add(childRoom);
                    if (isExit)
                        ExitRoom = childRoom;
                }
            }
        }
        
        /// <summary>
        /// This creates a dungeon in the shape of a grid with rooms connected to
        /// the neighbors on left,right,up, and down. 
        /// </summary>
        void MkGridDungeon(int numberOfRooms, int maximumRoomCapacity)
        {
            if (numberOfRooms < 4)
                throw new ArgumentException("A grid-dungeon should have at least five rooms.");
            
            int numOfColumn = (int) Math.Sqrt((double)numberOfRooms) ;
            int numOfRow = numberOfRooms / numOfColumn ;
            if (numberOfRooms % numOfColumn != 0)
            {
                numOfRow++;
            }
            Room[ , ] created = new Room[numOfColumn, numOfRow] ;
            for (int k = 0; k < numOfColumn; k++)
            {
                for (int j = 0; j < numOfRow && Rooms.Count < numberOfRooms; j++)
                {
                    int nextIndex = Rooms.Count;
                    bool isStart = nextIndex == 0;
                    bool isExit = nextIndex == numberOfRooms - 1;
                    int capacity = (isStart || isExit) ? 0 : randomGenerator.NextInt(maximumRoomCapacity + 1);
                    RoomType roomType = isStart ? RoomType.STARTroom
                        : isExit ? RoomType.EXITroom
                        : RoomType.ORDINARYroom;
                    created[k,j] = new Room("R" + (k*numOfRow+j), roomType, capacity);
                    Rooms.Add(created[k,j]);
                    if (isExit)
                        ExitRoom = created[k,j];
                    if (isStart)
                        StartRoom = created[k,j];
                }
            }
            // connect the rooms to form a 2D grid:
            for (int k = 0; k < numOfColumn; k++)
            {
                for (int j = 0; j < numOfRow; j++)
                {
                    if (created[k,j] == null)
                        continue;
                    if (k > 0 && created[k-1,j] != null)
                        created[k,j].Connect(created[k-1,j], Direction.WEST);
                    if (j > 0 && created[k,j-1] != null)
                        created[k,j].Connect(created[k,j-1], Direction.NORTH);
                }
            }
            //done
        }

        #region additional getters

        /// <summary>
        /// Return all creatures in the Dungeon. The player is excluded.
        /// </summary>
        public List<Creature> Creatures
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Return all items in this Dungeon. The items in the player's bag
        /// are excluded.
        /// </summary>
        public List<Item> Items
        {
            get => throw new NotImplementedException();
        }
        #endregion
        
    }

    [Serializable()]
    public enum DungeonShapeType
    {
        LINEAR, 
        TREE,
        GRID
    }
    
    /// <summary>
    /// Representing different types of rooms.
    /// </summary>
    public enum RoomType
    {
        STARTroom,  // the starting room of the player. 
        EXITroom,   // representing the player's final destination.
        ORDINARYroom  // the type of the rest of the rooms. 
    }

    public enum Direction
    {
        NORTH, EAST, SOUTH, WEST
    }

    /// <summary>
    /// Representing a room in a dungeon.
    /// </summary>
    public class Room : GameEntity
    {

        #region fields and properties

        /// <summary>
        /// The type of this node: either start-node, exit-node, or common-node.
        /// </summary>
        public RoomType RoomType { get; }

        /// <summary>
        /// The number of monsters in this room cannot exceed this capacity.
        /// </summary>
        public int Capacity { get;  }
        
        /// <summary>
        /// Neighbors are nodes that are considered connected to this node.
        /// The connection is bidirectional in the sense that in can be traverse
        /// in both directions (from this room to a neighbor, and the otherway around).
        /// So, if u is in this.neighbors of this room, you have to make sure that
        /// this room is also in u.neighbors.
        /// <para></para>
        /// A connection also has a "direction", e.g. to indicate that a neighbor
        /// room u is to the north or east of this room. There are four directions
        /// possible: north, east, south, and west.
        /// </summary>
        public List<(Room,Direction)> Neighbors { get; } = new List<(Room,Direction)>();

        /// <summary>
        /// All creatures, excluding the players, which are currently in this room.
        /// </summary>
        public List<Creature> Creatures { get;  } = new List<Creature>();

        /// <summary>
        /// All items, excluding those in the player's bag, which are currently in this room.
        /// </summary>
        public List<Item> Items { get; } = new List<Item>();
        
        #endregion
        
        
        public Room(string uniqueId, RoomType roomTy, int capacity) : base(uniqueId)
        {
            RoomType = roomTy;
            Capacity = capacity;
        }

        #region additional getters
       
        /// <summary>
        /// The number of monsters in this room. The player does not count as a monster.
        /// </summary>
        public int NumberOfMonsters => Creatures.Count(c => c is Monster);

        /// <summary>
        /// Check if this is a leaf room (a room with no children).
        /// </summary>
        public bool IsLeafRoom => Neighbors.Count == 1;

        #endregion

        /// <summary>
        /// To add the given room as a neighbor of this room.
        /// </summary>
        public void Connect(Room r, Direction direction)
        {
            Direction opposite = Direction.NORTH;
            switch (direction)
            {
                case Direction.NORTH:
                    opposite = Direction.SOUTH;
                    break;
                case Direction.EAST:
                    opposite = Direction.WEST;
                    break;
                case Direction.SOUTH:
                    opposite = Direction.NORTH;
                    break;
                case Direction.WEST:
                    opposite = Direction.EAST;
                    break;
            }
            Neighbors.Add((r,direction)); 
            r.Neighbors.Add((this,opposite));
        }

        /// <summary>
        /// To disconnect the given room. That is, the room r will no longer be a
        /// neighbor of this room.
        /// </summary>
        public void Disconnect(Room r)
        {
            Neighbors.RemoveAll(n => n.Item1 == r);
            r.Neighbors.RemoveAll(n => n.Item1 == this);
        }

        /// <summary>
        /// return the set of all rooms which are reachable from this room.
        /// </summary>
        public List<Room> ReachableRooms()
        {
            Room x = this;
            var seen = new List<Room>();
            var todo = new List<Room>();
            todo.Add(x);
            while (todo.Count > 0)
            {
                x = todo[0] ; todo.RemoveAt(0) ;
                seen.Add(x);
                foreach ((Room,Direction) y in x.Neighbors)
                {   Room r = y.Item1;
                    if (seen.Contains(r) 
                        || todo.Contains(r))
                        continue;
                    todo.Add(r);
                }
            }
            return seen;
        }

        /// <summary>
        /// Check if the given room is reachable from this room.
        /// </summary>
        public bool CanReach(Room r)
        {
            return ReachableRooms().Contains(r); // not the most efficient way of checking it btw
        }
    }



}
