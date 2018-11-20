using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;

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
            if (iniPathCheck(iniHash))
            {
                sourceFiles = InputFileTest(Convert.ToString(iniHash["SOURCE_FILE"]));
                if (sourceFiles.Count() > 0)
                {
                    foreach (string sourceFileString in sourceFiles)
                    {
                        List<Hashtable> sourceFileHashList = sourceTest(sourceFileString);
                        exportFile(sourceFileHashList, Convert.ToString(iniHash["OUTPUT_PATH"]));
                    }
                }
            }
                
            Console.WriteLine("test");
            Console.ReadKey();
        }

        static Boolean iniPathCheck(Hashtable iniHash)
        {
            if (iniHash["SOURCE_FILE"] != null && iniHash["NO_ERROR_PATH"] != null && iniHash["ERROR_PATH"] != null
                && iniHash["OUTPUT_PATH"] != null && iniHash["LOG_FILE"] != null)
                return true;
            else return false;
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
            var bofRegex = new Regex(@"([A-Za-z/_]+\s?[A-Za-z/_]*)\s+:\s*([\S\s]+)");
            var softBinRegex = new Regex(@"(\S+)\s+\S+\s+(\S+)");
            var regex = bofRegex;
            Hashtable currentHT = bofHash;
            int siteCount = 0;
            Boolean finishRead = false;
            int testedDie = 0;
            int passDie = 0;
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
                    if (siteCount == 0)//BOF
                    {
                        char[] charsToTrim = { ' ' };
                        var matches = regex.Matches(readLine);
                        if (matches.Count > 0 && matches[0].Groups.Count > 1)
                        {
                            currentHT.Add(matches[0].Groups[1].Value.TrimEnd(charsToTrim), matches[0].Groups[2].Value);
                        }
                    }
                    else//soft bin
                    {
                        List<string> list = Program.matches(readLine, @"\S+", 0);
                        Hashtable softBinHash = new Hashtable();
                        if (list.Count > 0 && (list[0].StartsWith("FAIL")|| list[0].StartsWith("PASS")|| list[0].StartsWith("OS")))
                        {
                            softBinHash["SW_BIN"] = list[1];
                            softBinHash["HW_BIN"] = list[2];
                            softBinHash["NUMBER"] = list[3];
                            softBinHash["YIELD"] = list[4];
                            int location = 5;
                            
                            for(int i = 0; i < siteCount; i++)
                            {
                                if (list[location].Equals("."))
                                {
                                    softBinHash["site" + i] = 0;// list[location]沒有數值;
                                    location++;
                                }
                                    
                                else
                                {
                                    softBinHash["site" + i] = list[location + 1];

                                    testedDie += Convert.ToInt32(list[location + 1]);//有數值的都加起來
                                    if(list[0].StartsWith("PASS"))
                                        passDie += Convert.ToInt32(list[location + 1]);//PASS有數值的都加起來
                                    location = location + 2;
                                }
                                    
                                
                            }

                            softBinHash["DESCRIPTION"] = list[location];
                            hashList.Add(softBinHash);
                        }

                    }
                }
            }

            //hashList[0] is BofHash
            hashList[0]["TESTED DIE"] = testedDie;
            hashList[0]["PASS DIE"] = passDie;
            hashList[0]["YIELD"] = $"{(decimal)passDie / testedDie:P}";
            hashList[0]["SITE NUM"] = siteCount;
            //sourceHash[0] = bofHash;
            //sourceHash[1] = softBinHash;
            return hashList;
        }

        static void exportFile(List<Hashtable> sourceFileHashList, string outputPath)
        {
            //create file
            Hashtable bofHash = sourceFileHashList[0];
            string format = "ddd MMM dd HH:mm:ss yyyy";
            string format1 = "yyyy/MM/dd HH:mm:ss";
            string timeString = "";
            string temp1 = Convert.ToString(bofHash["Start Date/Time"]);
            DateTime temp = DateTime.Now;
            if(DateTime.TryParseExact(temp1, format, CultureInfo.CreateSpecificCulture("en-US"),
                System.Globalization.DateTimeStyles.None, out temp))
            {
                timeString = temp.ToString("yyyyMMddHHmmss");
            }
            else
            {
                temp = DateTime.ParseExact(temp1, format1, CultureInfo.CreateSpecificCulture("en-US"));
                timeString = temp.ToString("yyyyMMddHHmmss");
            }
            //DateTime temp = DateTime.Parse(temp1);
            
            outputPath = outputPath + bofHash["Spil_lot_no"] +"_"+timeString+"_"+ bofHash["Tester"] + "." + bofHash["Stage"];
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                // Add some text to the file.
                sw.Write("This is the ");
                sw.WriteLine("header for the file.");
                sw.WriteLine("-------------------");
                // Arbitrary objects can also be written to the file.
                sw.Write("The date is: ");
                sw.WriteLine(DateTime.Now);
            }
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
