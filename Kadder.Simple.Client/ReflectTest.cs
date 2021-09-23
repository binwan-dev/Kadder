using System;
using System.Reflection;

namespace Kadder.Simple.Server
{
    public class ReflectTest
    {
        public void Test()
        {
            var type = typeof(Person);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var property in properties)
                Console.WriteLine(property.Name);
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public string Age { get; set; }

        public int Sexx { get; set; }

        public string Sex { get; set; }
    }
}