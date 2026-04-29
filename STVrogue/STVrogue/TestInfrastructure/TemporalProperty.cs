using System;
using System.Collections.Generic;
using STVrogue.GameLogic;
using STVrogue.Utils;
using static STVrogue.Utils.STVLogger ;

namespace STVrogue.TestInfrastructure
{

    /// <summary>
    /// Representing three types of judgement of a temporal property.
    /// </summary>
    public enum Judgement {
        Valid, Inconclusive, Invalid
    }

    /// <summary>
    /// A "temporal property" represents a correctness property that is judged over
    /// an entire gameplay. An example of such a property is if we want to assert
    /// that the player's hit point should never be negative. Another example is
    /// if we want to assert that the player kill point should eventually be above
    /// 0.
    /// 
    /// Note that "never be" and "eventually" are time-related modalities.
    ///
    /// <para></para> You can think a temporal property to be a predicate over
    /// a sequence of states, which the states sampled during a gameplay. For
    /// example a property "always p" (in other words, never (not p)) holds on
    /// the sequence of p holds on every state in the sequence.
    ///
    /// <para></para>
    /// This class TemporalProperty is an abstract class intended to be the parent
    /// of different types of temporal properties. It also defines a bunch of
    /// factory-methods that allow you to construct/formulate a temporal properties with a
    /// cleaner syntax.
    /// Use them for constructing your temporal properties, rather than calling
    /// their underlying constructors directly.
    ///
    /// Several examples are shown below.
    ///
    /// <list type="bullet">
    ///  <item> The player's HP is never negative:
    ///   <c>always(state => state.Player.Hp >= 0)</c>
    ///  </item>
    ///
    ///   <item> The player's kill point never decreases:
    ///   <c> always(state => state.Player.Kp, (before,now) => before &lt;= now) </c>
    ///   </item>
    ///
    ///   <item> Eventually the player's kill point will exceed 0:
    ///   <c>eventually(state => state.Player.Kp > 0)</c>
    ///   </item>
    ///
    /// </list>
    /// </summary>
    public abstract class TemporalProperty<GameState>
    {

        /// <summary>
        /// Reset the evaluation-state of this temporal property.
        /// </summary>
        public abstract void Reset();
        
        /// <summary>
        /// Imagine that a gameplay is abstractly a sequence of states. At the start of
        /// the play, this sequence is empty. When the play advances by one round/turn,
        /// a new state is added to this sequence. Recall that a temporal property is
        /// essentially a predicate over such a sequence of states. This method allows
        /// the validity of the predicate to be checked incrementally by passing it
        /// one state at a time. So, if the play advance by one turn, you would pass
        /// the resulting game state to this predicate by calling this method EvaluateNextState,
        /// and the method will return a judgement on whether the predicate, evaluated
        /// on the sequence of states so far (including the new state you just passed) is
        /// valid or invalid.
        ///
        ///  <list type="bullet">
        ///    <item>Invalid : the so-far gameplay violates this temporal property.
        ///    </item>
        ///    <item>Valid: the property is satisfied by the gameplay so far (it holds on the
        ///    gameplay.
        ///    </item>
        ///    <item>Inconclusive : we cannot confirm whether the property is valid or
        ///    invalid on the gameplay so far.
        ///    </item>
        /// </list>
        /// </summary>
        public abstract Judgement EvaluateNextState(GameState state);

        /// <summary>
        /// Create a copy of this temporal-property. The copy is a deep-clone, except for
        /// the underlying state predicates.
        /// </summary>
        public abstract TemporalProperty<GameState> Copy();
        
        /// <summary>
        /// <c>this.And(psi)</c> constructs a new temporal property representing a conjunction of
        /// this temporal property and the given psi.
        /// The conjunction xi of temporal properties phi and psi behaves as follows. Given
        /// a gameplay s (a sequence of states s), evaluating xi on s results in:
        ///   <list type="number">
        ///   <item>invalid, if either phi or psi evaluates to invalid on s </item>
        ///   <item> valid if both phi and psi evaluate to valid on s. </item>
        ///   <item> inconclusive neither (1) nor (2) above holds. </item>
        ///   </list>
        /// </summary>
        public TemporalProperty<GameState>  And(TemporalProperty<GameState>  psi)
        {
            return new TemporalAnd <GameState > (this, psi);
        }
        
        /// <summary>
        /// <c>this.Or(psi)</c> constructs a new temporal property representing a disjunction of
        /// this temporal property and the given psi. It is defined as not(And(not this, not psi)).
        /// </summary>
        public TemporalProperty<GameState>  Or(TemporalProperty<GameState>  psi)
        {
            return new TemporalNot<GameState>(
                new TemporalNot<GameState>(this)
                    .And(new TemporalNot<GameState>(psi)));
        }
    }
    
    /// <summary>
    /// Provide some static methods to construct temporal properties.
    /// </summary>
    public class TemporalPropertyDSL
    {
        /// <summary>
        /// A generalization of <see cref="And(STVrogue.TestInfrastructure.TemporalProperty{GameState})"/>
        /// that can take multiple conjuncts.
        /// </summary>
        public static TemporalProperty<GameState> And<GameState> (params TemporalProperty<GameState>[] phis)
        {
            var all = phis[0].Copy();
            for (int k = 1; k < phis.Length; k++)
            {
                all = all.And(phis[k]);
            }
            return all;
        }
        
          /// <summary>
        /// A generalization of <see cref="Or(STVrogue.TestInfrastructure.TemporalProperty{GameState})"/>
        /// that can take multiple disjuncts.
        /// </summary>
        public static TemporalProperty<GameState> Or<GameState>(params TemporalProperty<GameState>[] phis)
        {
            for (int k = 0; k < phis.Length; k++)
            {
                phis[k] = Not(phis[k]);
            }
            return Not(And(phis));
        }
        
        /// <summary>
        /// Construct a new temporal property representing the negation of
        /// the given temporal property phi.
        /// Given a gameplay s (sequence of states s), evaluating negation on s results in:
        ///
        /// <list type="number">
        ///   <item> invalid, if  phi evaluates to valid on s. </item>
        ///   <item> valid, if phi evaluates to invalid on s. </item>
        ///   <item> inconclusive, if phi evaluates to inconclusive on s. </item>
        /// </list>
        /// </summary>
        public static TemporalProperty<GameState> Not<GameState>(TemporalProperty<GameState> phi)
        {
            return new TemporalNot<GameState>(phi);
        }

        /// <summary>
        /// Construct a temporal property of the form always p. Such a property is valid on a gameplay
        /// (sequence of states) s if p holds on every state in s. Else it is invalid.
        /// </summary>
        public static TemporalProperty<GameState> Always<GameState>(Predicate<GameState> p)
        {
            return new TemporalAlways<GameState>(p);
        }

        /// <summary>
        /// Construct an "always" type of property. Always(f,p) is valid on a gameplay (sequence of
        /// states) s if for every state w in s, and its previous state v, p(f(v),f(w)) holds.
        /// If the sequence contains less that two states, the property is inconclusive.
        /// </summary>
        public static TemporalProperty<GameState> Always<GameState>(Func<GameState, int> getValue, Func<int, int, bool> p)
        {
            return new TemporalAlways<GameState>(getValue, p);
        }
        
        /// <summary>
        /// Construct a temporal property of the form eventually p. Such a property is valid on a gameplay
        /// (sequence of states) s if p holds on at least one state w in s.
        /// </summary>
        public static TemporalProperty<GameState> Eventually<GameState>(Predicate<GameState> p)
        {
            return Not(new TemporalAlways<GameState>(st=> !p(st)));
        }
        
        /// <summary>
        /// Construct an "eventually" type of property. Eventually(f,p) is valid on a gameplay (sequence of
        /// states) s if there is a state w in s, and its previous state v, such that p(f(v),f(w)) holds.
        /// If the sequence contains less that two states, the property is inconclusive.
        /// </summary>
        public static TemporalProperty<GameState> Eventually<GameState>(Func<GameState, int> getValue, Func<int, int, bool> p)
        {
            return Not(Always(getValue, (prev,now)=> !p(prev,now)));
        }
        
    }

    /// <summary>
    /// This is the underlying class representing a temporal property of the form "Always p".
    /// A gameplay satisfies this property if the predicate p holds on every game state
    /// through out the play.
    /// </summary>
    public class TemporalAlways<GameState> : TemporalProperty<GameState>
    {
        readonly Predicate<GameState> p;
        Func<GameState, int> getValue;
        Func<int,int, bool>  pWithPrev;
        int? previousValue = null ;
        public string desc = "";

        Judgement judgementSoFar = Judgement.Inconclusive;
        
        /// <summary>
        /// Construct a temporal property of the form Always(p) where p is a state predicate.
        /// </summary>
        public TemporalAlways(Predicate<GameState> p) { this.p = p; }
        
        /// <summary>
        /// Construct a temporal property of the form Always(f,p) where f is a function
        /// that maps a state to an int and p is a predicate over integers. Given a sequence
        /// of state s, the property is valid on s (satisfied by s) if for every state w
        /// in s, and its previous state v, p(f(v),f(w)) holds.
        ///
        /// <para></para> Such a predicate p is called a "change predicate" as it expresses
        /// how the current state is expected to change from the previous state.
        ///
        /// <example>
        /// <code>
        ///    Always(state => state.Player.Kp,
        ///           (previousValue,killPoint) => killPoint >= previousValue )
        /// </code>
        /// <para></para>
        /// which states that it should always be the case that player's kill point
        /// is greater or equal than its previous value. In other words, this property
        /// says that the kill point of the player should never decrease.
        /// </example>
        /// </summary>
        public TemporalAlways(Func<GameState,int> getValue,Func<int,int,bool> p)
        {
            this.getValue = getValue;
            pWithPrev = p;
        }

        public override  void Reset()
        {
            judgementSoFar = Judgement.Inconclusive;
            previousValue = null;
        }
        
        public override Judgement EvaluateNextState(GameState state)
        {
            if (judgementSoFar == Judgement.Invalid) 
                return Judgement.Invalid;
            if (p != null)
            {
                judgementSoFar = p(state) ? Judgement.Valid : Judgement.Invalid;
            }
            else
            {
                int newValue = getValue(state);
                if (previousValue != null)
                {
                    //Console.WriteLine($">>> prev:{previousValue.Value}, now:{newValue}");
                    judgementSoFar = pWithPrev(previousValue.Value, newValue) ? Judgement.Valid : Judgement.Invalid;
                }
                // else inconclusive, but this is already the val of judgementSoFar,
                // at the beginning.,So we don't need to do anything.
                
                previousValue = newValue;
            }

            if (judgementSoFar == Judgement.Invalid)
            {
                STVLogger.Log($"## An always property {desc} is violated. State = {state.ToString()}");
            }
            return judgementSoFar;
        }

        public override TemporalProperty<GameState> Copy()
        {
            TemporalAlways<GameState> phi = new TemporalAlways<GameState>(this.p);
            phi.getValue = this.getValue;
            phi.pWithPrev = this.pWithPrev;
            return phi;
        }
    }
    
    /// <summary>
    /// This is the underlying class representing a temporal property of the form not phi,
    /// where phi is another temporal property.
    /// </summary>
    public class TemporalNot<GameState> : TemporalProperty<GameState>
    {
        private TemporalProperty<GameState> phi;
        public override void Reset()
        {
            phi.Reset();
        }
        
        TemporalNot() { }

        public TemporalNot(TemporalProperty<GameState> phi)
        {
            this.phi = phi.Copy();
        }

        public override Judgement EvaluateNextState(GameState state)
        {
            var j = phi.EvaluateNextState(state);
            switch (j)
            {
                case Judgement.Valid : return Judgement.Invalid;
                case Judgement.Invalid : return Judgement.Valid;
                default: return Judgement.Inconclusive;
            }
        }

        public override TemporalProperty<GameState> Copy()
        {
            var psi = new TemporalNot<GameState>();
            psi.phi = this.phi.Copy();
            return psi;
        }
    }

    /// <summary>
    /// This is the underlying class representing a temporal property of the form
    /// phi and psi, where phi and psi are temporal properties.
    /// </summary>
    public class TemporalAnd<GameState> : TemporalProperty<GameState>
    {
        TemporalProperty<GameState> phi1;
        TemporalProperty<GameState> phi2;
        
        TemporalAnd() {}

        public TemporalAnd(TemporalProperty<GameState> phi, TemporalProperty<GameState> psi)
        {
            phi1 = phi.Copy();
            phi2 = psi.Copy();
        }
        public override void Reset()
        {
            phi1.Reset();
            phi2.Reset();
        }

        public override Judgement EvaluateNextState(GameState state)
        {
            var j1 = phi1.EvaluateNextState(state);
            var j2 = phi2.EvaluateNextState(state);
            if (j1 == Judgement.Invalid || j2 == Judgement.Invalid)
                return Judgement.Invalid;
            if (j1 == Judgement.Valid && j2 == Judgement.Valid)
                return Judgement.Valid;
            return Judgement.Inconclusive;
        }

        public override TemporalProperty<GameState> Copy()
        {
            var psi = new TemporalAnd<GameState>();
            psi.phi1 = this.phi1.Copy();
            psi.phi2 = this.phi2.Copy();
            return psi;
        }
    }
    
}
