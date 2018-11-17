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
                IniTest(args[0]);
            }
            InputFileTest("D:" + Path.DirectorySeparatorChar + "TEST_FILE" + Path.DirectorySeparatorChar + "SUMMARY" + Path.DirectorySeparatorChar 
                + "SOURCE_FILE" + Path.DirectorySeparatorChar + "*.sum");
            Console.WriteLine("test");
            Console.ReadKey();
        }

        static void IniTest(string path)
        {
            Console.WriteLine("trying ini path: " + path);
            if (File.Exists(path))
                Console.WriteLine("ini path  found");
            else
                Console.WriteLine("ini path not found:"+ path);
        }

        static void InputFileTest(string path)
        {
            Console.WriteLine("trying input file path: " + path);
            string[] files = Directory.GetFiles(
               Path.GetDirectoryName(path), Path.GetFileName(path)
            );
            if (File.Exists(path))
                Console.WriteLine("input path  found");
            else
                Console.WriteLine("input path not found:" + path);
        }
    }
}
