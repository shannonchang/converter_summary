using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace converter_summary
{
    class Program
    {
        static void Main(string[] args)
        {
            string iniFile = "d:"+ Path.DirectorySeparatorChar + "CONVERTER_SUMMARY.INI";
            string[] sourceFiles = { };
            string seconds = "20";
            string counts = "60";
            Hashtable iniHash = new Hashtable();
            //if (args.Count() > 0)
            {
                //iniHash = IniTest(args[0]);
                iniHash = IniTest(iniFile);
            }
            if(iniHash["SOURCE_FILE"]!=null)
                sourceFiles = InputFileTest(Convert.ToString(iniHash["SOURCE_FILE"]));
            if (sourceFiles.Count() > 0)
            {
                foreach(string sourceFileString in sourceFiles)
                {
                    sourceTest(sourceFileString);
                }
            }
            Console.WriteLine("test");
            Console.ReadKey();
        }

        static Hashtable IniTest(string path)
        {
            Hashtable iniHash = new Hashtable();
            Console.WriteLine("trying ini path: " + path);
            if (File.Exists(path))
            {
                Console.WriteLine("ini path  found");
                string[] iniString = File.ReadAllLines(path);
                var regex = new Regex(@"(\S+)\s+=\s+(\S+)");
                foreach (string line in iniString)
                {
                    var matches = regex.Matches(line);
                    if (matches.Count > 0 && matches[0].Groups.Count > 1)
                    {
                        iniHash.Add(matches[0].Groups[1].Value, matches[0].Groups[2].Value);
                    }
                        
                }
            }
                
            else
                Console.WriteLine("ini path not found:"+ path);

            return iniHash;
        }

        static string[] InputFileTest(string path)
        {
            Console.WriteLine("trying input file path: " + path);
            string[] files = Directory.GetFiles(
               Path.GetDirectoryName(path), Path.GetFileName(path)
            );
            
            return files;
        }

        static Hashtable sourceTest(string path)
        {
            string[] sourceStrings = File.ReadAllLines(path);
            Hashtable sourceHash = new Hashtable();
            return sourceHash;
        }
    }
}
