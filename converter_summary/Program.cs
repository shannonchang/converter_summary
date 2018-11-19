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
                    List<Hashtable> sourceFileHashList = sourceTest(sourceFileString);
                    Console.WriteLine("test");
                    Console.ReadKey();
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

        static List<Hashtable> sourceTest(string path)
        {
            string[] sourceStrings = File.ReadAllLines(path);
            Hashtable[] sourceHash = { new Hashtable(), new Hashtable() };
            Hashtable bofHash = new Hashtable();
            List<Hashtable> hashList = new List<Hashtable>();
            var bofRegex = new Regex(@"([A-Za-z/_]+\s?[A-Za-z/_]*)\s+:([\S\s]+)");
            var softBinRegex = new Regex(@"(\S+)\s+\S+\s+(\S+)");
            var regex = bofRegex;
            Hashtable currentHT = bofHash;
            int siteCount = 0;
            Boolean finishRead = false;
            foreach (string readLine in sourceStrings)
            {
                if(readLine.StartsWith("Software Bin Summary"))
                {
                    List<string> list = Program.matches(readLine, @"\d+", 0);
                    if (list.Count > 0)
                    {
                        siteCount = Convert.ToInt32(list[0]);
                        //regex = softBinRegex;
                        //currentHT = softBinHash;
                    }
                    else
                    {
                        //error handle
                    }
                    hashList.Add(currentHT);
                }

                if(readLine.StartsWith("Hardware Bin Summary"))
                {
                    finishRead = true;
                }

                if (!finishRead)
                {
                    if (siteCount == 0)
                    {
                        var matches = regex.Matches(readLine);
                        if (matches.Count > 0 && matches[0].Groups.Count > 1)
                        {
                            currentHT.Add(matches[0].Groups[1].Value, matches[0].Groups[2].Value);
                        }
                    }
                    else
                    {
                        List<string> list = Program.matches(readLine, @"\S+", 0);
                        Hashtable softBinHash = new Hashtable();
                        if (list.Count > 0 && (list[0].StartsWith("FAIL")|| list[0].StartsWith("PASS")|| list[0].StartsWith("OS")))
                        {
                            softBinHash["SW_BIN"] = list[1];
                            softBinHash["HW_BIN"] = list[2];
                            softBinHash["NUMBER"] = list[3];
                            softBinHash["YIELD"] = list[4];
                            softBinHash["DESCRIPTION"] = list[4+siteCount+1];
                            hashList.Add(softBinHash);
                        }

                    }
                }
                
                
            }
            //sourceHash[0] = bofHash;
            //sourceHash[1] = softBinHash;
            return hashList;
        }

        // 傳回text 中符合正規表示式pattern 的所有段落。
        static List<String> matches(String text, String pattern, int groupId)
        {
            List<String> rzList = new List<String>();
            Match match = Regex.Match(text, pattern);
            for (int i = 0; match.Success; i++)
            {
                rzList.Add(match.Groups[groupId].Value);
                match = match.NextMatch();
            }
            return rzList;
        }
    }
}
