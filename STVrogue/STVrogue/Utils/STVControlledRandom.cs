using System;


namespace STVrogue.Utils
{
    
    /// <summary>
    /// A generic interface for a random generator.
    /// </summary>
    public interface IRandomGenerator
    {
        /// <summary>
        /// Return the value of the seed used by this generator.
        /// </summary>
        /// <returns></returns>
        public int Seed();
        
        /// <summary>
        /// Generate a random integer in the range of [0..maxvakye)
        /// </summary>
        public int NextInt(int maxvalue);

        /// <summary>
        /// Generate a random flo.ating number in the range of [0..1]
        /// </summary>
        public double NextDouble();
    }

    /// <summary>
    /// An implementation of <see cref="IRandomGenerator"/>. This implementation
    /// just wraps around <see cref="Random"/>. You can set its seed, but it does
    /// not implement the Singleton pattern.
    /// </summary>
    public class RandomGenerator : IRandomGenerator
    {

        int? seed = null;
        Random rnd;
        
        public RandomGenerator()
        {
            rnd = new Random();
        }

        public RandomGenerator(int seed)
        {
            rnd = new Random(seed);
            this.seed = seed;
        }
        public int Seed()
        {
            if (seed == null)
                throw new Exception("The seed is unknown");
            return (int) seed;
        }

        public int NextInt(int maxvalue)
        {
            return rnd.Next(maxvalue);
        }

        public double NextDouble()
        {
            return rnd.NextDouble();
        }
    }
    
    /// <summary>
    /// Provide an implementation of a controlled random generator. It uses a Singleton
    /// design pattern so that internally there is only one random generator that is
    /// shared by all instances of this class STVControlledRandom.
    /// <para></para>
    /// Internally a random generator keeps a state to create the next random value.
    /// This next value is actually deterministically depend on the current state
    /// of the random generator. So, ultimately the whole series of random values
    /// the generator produces only depends on its initial state, which is the generator
    /// seed.
    /// <para></para>
    /// Use the static method SetSeed(value) to set the value of this seed. This seed
    /// value is global: it is shared by all instances of STVControlledRandom.
    /// <para></para>
    /// The method Reset() will reset the state of all instances of STVControlledRandom to
    /// the their initial state, which is the value of the above meant common seed.
    /// <para></para>
    /// IMPORTANT NOTE:
    /// When testing STV method classes that use STVControlledRandom, make sure that you
    /// call Reset() before the run of every test-method. This makes sure that the
    /// tests' results do not depend on the order with which the test methods are called.
    /// </summary>
    public class STVControlledRandom : IRandomGenerator
    {
        /// <summary>
        /// The the seed value of all instances of STVControlledRandom. They all share
        /// this seed value.
        /// </summary>
        static int seed = 4731 ;
        
        /// <summary>
        /// This is the single, actual random generator that will be shared by all
        /// instances of STVControlledRandom.
        /// </summary>
        static Random rnd;

        /// <summary>
        /// For setting the seed value of all instances of STVControlledRandom.
        /// They all share this seed value. Setting the seed will cause the
        /// actual random generator <see cref="seed"/> of all instances of STVControlledRandom.
        /// </summary>
        public static void SetSeed(int seedvalue)
        {
            Reset();
            seed = seedvalue; 
        }
        
        /// <summary>
        /// Reset the state of all instances of STVControlledRandom to the value of
        /// their common seed.
        /// </summary>
        public static void Reset()
        {
            rnd = null;
        }
        
        /// <summary>
        /// Return the seed value of this STVControlledRandom.
        /// </summary>
        public int Seed()
        {
            return seed;
        }

        public int NextInt(int maxvalue)
        {
            if (rnd == null) rnd = new Random(seed);
            return rnd.Next(maxvalue);
        }

        public double NextDouble()
        {
            if (rnd == null) rnd = new Random(seed);
            return rnd.NextDouble();
        }
    }
}