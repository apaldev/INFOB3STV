using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using STVrogue.GameLogic;

namespace STVrogue;

public class TestAgent
{
    
    public TestAgent() {
        // Constructor for the TestAgent class.
        // You can initialize any necessary variables or data structures here.
    }
    
    /// <summary>
    /// This method is called to determine the next action for the agent. It takes the console output
    /// from the game and the current game state as input.
    /// </summary>
    public virtual char NextAction(List<string> consoleOutput, Game gameState) {
            // This is a test agent that always returns ' ' as the next action.
            // You can replace this with your own logic to determine the next action.
            throw new NotImplementedException("TestAgent.NextAction is not implemented.");
    }
}