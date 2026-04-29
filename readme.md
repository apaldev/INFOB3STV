# The STV Rogue Project

### Needed Software

You will need an IDE that supports C# development, and additionally unit testing, and coverage analysis. Here are the options:

* Jetbrains' [__Rider__](https://www.jetbrains.com/rider/) .NET IDE. You can get a [free education license](https://www.jetbrains.com/community/education/#students) for this. It is a great IDE (fast, powerful refactoring, works on windows/mac/linux). It has my preference.

   Once installed, from Rider install the plugin `dotCover` to track your tests' code-coverage later during the project.

   Also: install the CyclomaticComplexity plugin. You can also install Github Copilot plugin if you fancy it (not necessary for the project).

* __Visual Studio Enterprise__. If you really insist on using Visual Studio, you can. Keep in mind that you need the __Enterprise__ edition, as smaller version would not include their code coverage tool. UU used to have Enterprise free for students, but I don't know its status now. Last I checked, it was not in UU deal anymore.


### Which .NET and which C#?

The project is configured to use .NET 8.0 and C# 12. These are pretty recent.
To minimize potential issues during the review, please stick with this setup.

### What Is in The Project?

This provides an initial C# implementation of the game UI, the logical game entities of __STV Rogue__, as well as establishing the architecture of this game.
Various methods are left unimplemented for you. You can finish the project,
but please stick to the imposed architecture and keep the signatures of the current methods.

In the directory `STVRogue` you can find an `.sln` file that describes the configuration of the project.
Open this "solution"-file
in Rider or VS. It will contain several "projects". The ones relevant for you are these:

  * The main project is called `STVrogue`. The folder `GameLogic` contains the classes that comprise the logic of the STV Rogue game.
  The whole game-state is kept as an instance of the class called `Game`, which also provides the logic of a single turn update.
  The game main loop is implemented separately in a top level class called `GameRunner`.  
  The top level main class is the usual `Program` class.

     Dungeon generation (linear/tree/grid) is already implemented for you. But do test them; there are possibly still errors lurking in the dark.

     Some support classes you may find useful:

      * The class `STVRogue.Utils.HelperPredicates` contains some predicates you might want to borrow, e.g. the **forall** and **exists** quantifiers you can use to write in-code specifications.

      * The class `STVRogue.Utils.STVControlledRandom` offers a controlled random generator. For testing, it is recommended that you use this random generator to avoid making your tests flaky.

      * The classes `GameConsole` provides implementations for Console UI, allowing you to write and read from the Console. Don't directly use System Console.

      * The class `TestAgent` provides a template for an agent that can automatically play an instance of the STVRogue game. Using such an agent allows you to do automated system-level testing on the game. See the project `Coba_TestAgent` (in the Solution) for a simple example of how to use it.

  * The project `NUnitTests` contains example unit tests using the NUnit unit testing framework.

   [NUnit](https://nunit.org/) is a well known unit testing frameworks for C# and is a part of .Net Core.
   If you use Rider IDE there is no need to install it.
   Use NUnit for your unit testing :)

   NUnit offers a bunch of nice features such as Theory and combinatoric test. We will use NUnit. Finding tutorial for NUnit might be a bit challenging. I will list some below:

   1. From the lectures you should already know the key concepts of unit testing. The examples  provided in STV Rogue itself are the shortest route to learn NUnit.

   2. Reference documentations can be found in the [site of NUnit](https://nunit.org/).

   3. There is an __old__ [NUnit Quick Start tutorial](https://nunit.org/docs/2.5.9/quickStart.html) which is still useful to learn its main concepts.
