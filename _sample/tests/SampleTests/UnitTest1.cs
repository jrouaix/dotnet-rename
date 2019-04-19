using SampleLib;
using SampleLib.Sub;
using System;
using Xunit;

namespace SampleTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var class1 = new Class1();
            var class2 = new FunckyNamespace.Class2();
            var class3 = new SampleLib.Sub.Class3();
            var class4 = new Class3();
            Console.WriteLine($"{class1} {class2} {class3} {class4}");
        }
    }
}
