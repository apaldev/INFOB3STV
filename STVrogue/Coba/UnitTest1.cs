using System;
using System.Diagnostics;
using NUnit.Framework;

//
// Just a space for trying out random things.
// 
namespace Coba
{
    class Coba1
    {
        public string Name { get; }
        
        public Coba1(string name)
        {
            Name = name;
        }
    }
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            int x = 10;
            Console.WriteLine(("Coba console write..."));
            var tr = new TextWriterTraceListener(System.Console.Out);
            Trace.Listeners.Add(tr);
            Debug.AutoFlush = true;
            Debug.WriteLine("Coba debug write...");
            x = x + 1;
            Assert.Pass();
        }
        
        [Test]
        public void Test2()
        {
            Coba1 coba1 = new Coba1("Haha");
            Assert.That(coba1.Name == "Haha"); 
            Console.WriteLine(">>> " + coba1.Name);
        }

        [Test]
        public void TestDivision()
        {
            int k = 0;
            Console.WriteLine("*** Div:" + k/3);
        }
        
        [Test]
        public void TestIntFromFloatRounding()
        {
            double x = 2.999;
            Console.WriteLine("*** Rounding:" + (int) x);
        }

        [Test]
        public void TestConsoleErrorWrite()
        {
            Console.WriteLine(">> using Console.WriteLine");
            Console.Error.WriteLine("*** using Console.Error.Writeln");
        }


        int foo(int x)
        {
            if (x == 0 || x == 1)
            {
                return 0;
            }
            else
            {
                x = x % 2 == 
                    0 ? 
                    2 
                    : x;
                return x;
            }
        }

        [Test]
        public void test_foo1()
        {
            int y = foo(4);
        }
    }
}