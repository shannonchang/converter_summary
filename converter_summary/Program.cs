using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace converter_summary
{
    class Program
    {
        static void Main(string[] args)
        {
            string iniFile = "d:"+ Path.DirectorySeparatorChar + "CONVERTER_SUMMARY.INI";
            string seconds = "20";
            string counts = "60";
            if (args.Count() > 0)
            {
                test(args[0]);
            }
            
            Console.WriteLine("test");
            Console.ReadKey();
        }

        static void test(string path)
        {
            Console.WriteLine("trying path: " + path);
            if (File.Exists(path))
                Console.WriteLine("path  found");
            else
                Console.WriteLine("path not found");
        }
    }
}
