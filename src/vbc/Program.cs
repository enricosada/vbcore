using System;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("VBC... compiling...!");
            Console.WriteLine("Arguments:");
            foreach (var a in args)
            {
                Console.WriteLine(a);
            }
        }
    }
}
