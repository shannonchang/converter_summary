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
            List<string> passFiles = new List<string>();
            List<string> errorFiles = new List<string>();
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
                        try
                        {
                            List<Hashtable> sourceFileHashList = sourceTest(sourceFileString);
                            if(exportFile(sourceFileHashList, Convert.ToString(iniHash["OUTPUT_PATH"])))
                            {
                                passFiles.Add(sourceFileString);
                            }

                        }catch(Exception e)
                        {
                            if (e.Message.Equals("SumFileError"))
                            {
                                errorFiles.Add(sourceFileString);
                            }
                        }
                    }
                }
                sourceFileMovement(passFiles, errorFiles, iniHash);//搬移pass/error source files
            }
            else
            {
                Console.WriteLine("INI File error");
            }
                
            Console.WriteLine("Program finish");
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
            int siteCount = 0; //initialize site count
            Boolean finishRead = false;//flag to check if reading continue
            int testedDie = 0;//initialize test die
            int passDie = 0;//initialize pass die
            try
            {
                foreach (string readLine in sourceStrings)
                {
                    if (readLine.StartsWith("Software Bin Summary"))
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

                    if (readLine.StartsWith("Hardware Bin Summary"))
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
                            if (list.Count > 0 && (list[0].StartsWith("FAIL") || list[0].StartsWith("PASS") || list[0].StartsWith("OS")))
                            {
                                softBinHash["TYPE"] = list[0];
                                softBinHash["SW_BIN"] = list[1];
                                softBinHash["HW_BIN"] = list[2];
                                softBinHash["NUMBER"] = list[3];
                                softBinHash["YIELD"] = list[4];
                                int location = 5;

                                for (int i = 0; i < siteCount; i++)
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
                                        if (list[0].StartsWith("PASS"))
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
                
            }
            catch (Exception e)
            {
                throw e;
            }
            
            //verify error 
            if (!verifySource(hashList[0]))
            {
                throw new Exception("SumFileError");
            }

            return hashList;
        }

        //check sum file format, move to error while format fail
        static Boolean verifySource(Hashtable currentHT)
        {
            char[] format = { ' ' };
            //TEST SITE,TEST BIN,TEST PROGRAM,STAGE,STEP,START TIME,LOT ID can not be null
            if (currentHT["Spil_lot_no"] != null && currentHT["Stage"] != null && currentHT["Process"] != null &&
                currentHT["Start Date/Time"] != null && currentHT["Testbin/all"] != null && currentHT["TestProgram_Name"] != null)
            {
                if (Convert.ToString(currentHT["Spil_lot_no"]).TrimEnd(format) != "" && Convert.ToString(currentHT["Stage"]).TrimEnd(format) != ""
                    && Convert.ToString(currentHT["Process"]).TrimEnd(format) != "" && Convert.ToString(currentHT["Start Date/Time"]).TrimEnd(format) != ""
                    && Convert.ToString(currentHT["Testbin/all"]).TrimEnd(format) != "" && Convert.ToString(currentHT["TestProgram_Name"]).TrimEnd(format) != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
                    
            }
                
            else
                return false;
        }

        static Boolean exportFile(List<Hashtable> sourceFileHashList, string outputPath)
        {
            try
            {
                //create file
                Hashtable bofHash = sourceFileHashList[0];
                string format = "ddd MMM dd HH:mm:ss yyyy";
                string format1 = "yyyy/MM/dd HH:mm:ss";
                string titleTimeString = "";
                string startTimeString = "";
                string stopTimeString = "";
                DateTime outputTime = DateTime.Now;
                int siteCount = 0;
                //time
                if (bofHash["Start Date/Time"] != null)
                {
                    titleTimeString = Convert.ToString(bofHash["Start Date/Time"]);
                    startTimeString = Convert.ToString(bofHash["Start Date/Time"]);
                    stopTimeString = Convert.ToString(bofHash["Stop Date/Time"]);
                }
                else
                {
                    titleTimeString = Convert.ToString(bofHash["Stop Date/Time"]);
                    stopTimeString = Convert.ToString(bofHash["Stop Date/Time"]);
                }

                //siteCount
                if (bofHash["SITE NUM"] != null)
                {
                    siteCount = Convert.ToInt32(bofHash["SITE NUM"]);
                }

                if (DateTime.TryParseExact(titleTimeString, format, CultureInfo.CreateSpecificCulture("en-US"),
                    System.Globalization.DateTimeStyles.None, out outputTime))
                {
                    titleTimeString = outputTime.ToString("yyyyMMddHHmmss");
                    if (startTimeString != "")
                    {
                        startTimeString = DateTime.ParseExact(startTimeString, format, CultureInfo.CreateSpecificCulture("en-US")).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    if (stopTimeString != "")
                    {
                        stopTimeString = DateTime.ParseExact(stopTimeString, format, CultureInfo.CreateSpecificCulture("en-US")).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
                else
                {
                    outputTime = DateTime.ParseExact(titleTimeString, format1, CultureInfo.CreateSpecificCulture("en-US"));
                    titleTimeString = outputTime.ToString("yyyyMMddHHmmss");
                    if (startTimeString != "")
                    {
                        startTimeString = DateTime.ParseExact(startTimeString, format1, CultureInfo.CreateSpecificCulture("en-US")).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    if (stopTimeString != "")
                    {
                        stopTimeString = DateTime.ParseExact(stopTimeString, format1, CultureInfo.CreateSpecificCulture("en-US")).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
                //DateTime temp = DateTime.Parse(temp1);

                outputPath = outputPath + bofHash["Spil_lot_no"] + "_" + titleTimeString + "_" + bofHash["Tester"] + "." + bofHash["Stage"] + ".FT";
                using (StreamWriter sw = new StreamWriter(outputPath))
                {
                    //BOF
                    sw.WriteLine("[BOF]");
                    sw.WriteLine("DEVICE ID           :  " + bofHash["Device_Name"]);
                    sw.WriteLine("LOT ID              :  " + bofHash["Spil_lot_no"]);
                    sw.WriteLine("CUSTOMER DEVICE ID  :  " + bofHash["Device_Name"]);
                    sw.WriteLine("CUSTOMER LOT ID     :  " + bofHash["Customer_No"]);
                    sw.WriteLine("STAGE               :  " + bofHash["Stage"]);
                    sw.WriteLine("STEP                :  " + bofHash["Process"]);
                    sw.WriteLine("START TIME          :  " + startTimeString);
                    sw.WriteLine("STOP TIME           :  " + stopTimeString);
                    sw.WriteLine("TEST SITE           :  HS03");
                    sw.WriteLine("TEST BIN            :  " + bofHash["Testbin/all"]);
                    sw.WriteLine("TEST PROGRAM        :  " + bofHash["TestProgram_Name"]);
                    sw.WriteLine("TESTER ID           :  " + bofHash["Tester_ID"]);
                    sw.WriteLine("HANDLER ID          :  " + bofHash["Prober/handler"]);
                    sw.WriteLine("LOAD BOARD ID       :  " + bofHash["ProbeCard/FixBoard"]);
                    sw.WriteLine("SITE NUM            :  " + bofHash["SITE NUM"]);
                    sw.WriteLine("ADAPTOR ID          :  ");
                    sw.WriteLine("TOP SOCKET ID       :  " + bofHash["Socket_No"]);
                    sw.WriteLine("SOCKET ID           :  " + bofHash["Socket_No"]);
                    sw.WriteLine("CHANGE KIT ID       :  ");
                    sw.WriteLine("TEST VERSION        :  ");
                    sw.WriteLine("TEMPERATURE         :  " + bofHash["Temperature"]);
                    sw.WriteLine("SOFTWARE VERSION    :  ");
                    sw.WriteLine("SOAK TIME           :  ");
                    sw.WriteLine("TECH ID             :  " + bofHash["OP_id"]);
                    sw.WriteLine("TESTED DIE          :  " + bofHash["TESTED DIE"]);
                    sw.WriteLine("PASS DIE            :  " + bofHash["PASS DIE"]);
                    sw.WriteLine("YIELD               :  " + bofHash["YIELD"]);
                    sw.WriteLine("");
                    //BOF end
                    //Soft bin 
                    sw.WriteLine("[SOFT BIN]");
                    string siteString = "";
                    for (int i = 0; i < siteCount; i++)
                    {
                        siteString += String.Format("{0,8},", "SITE" + i + "");
                    }
                    sw.WriteLine(String.Format("{0,12},{1,8},{2,8},{3,8},{4,8},{5}{6},{7}",
                        "SW_BIN", "HW_BIN", "NUMBER", "YIELD", "TYPE", siteString, "SW_DESCRIPTION", "HW_DESCRIPTION"));
                    for (int i = 1; i < sourceFileHashList.Count; i++)
                    {
                        Hashtable currentHT = sourceFileHashList[i];
                        siteString = "";
                        string s = Convert.ToString(currentHT["DESCRIPTION"]);
                        for (int j = 0; j < siteCount; j++)
                        {
                            siteString += String.Format("{0,8},", Convert.ToString(currentHT["site" + j]));
                        }
                        sw.WriteLine(String.Format("{0,12},{1,8},{2,8},{3,8},{4,8},{5}{6}{7}{8},{9}{10}{11}"
                            , currentHT["SW_BIN"], currentHT["HW_BIN"], currentHT["NUMBER"], currentHT["YIELD"], currentHT["TYPE"]
                            , siteString, "{[", s.Substring(0, Math.Min(s.Length, 100)), "]}", "{[", s.Substring(0, Math.Min(s.Length, 100)), "]}"));
                    }
                    sw.WriteLine("");
                    //Soft bin end

                    sw.WriteLine("[EXTENSION]");
                    sw.WriteLine("");
                    sw.WriteLine("[EOF]");
                    sw.WriteLine("");
                }
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        //Handle pass/error file movement and log file 
        static void sourceFileMovement(List<string> passFiles, List<string> errorFiles, Hashtable iniHash)
        {
            string NO_ERROR_PATH = Convert.ToString(iniHash["NO_ERROR_PATH"]);
            string ERROR_PATH = Convert.ToString(iniHash["ERROR_PATH"]);
            string LOG_FILE = Convert.ToString(iniHash["LOG_FILE"]);
            LOG_FILE = LOG_FILE.Replace("yyyyMM", DateTime.Now.ToString("yyyyMM"));
            foreach (string passFile in passFiles)
            {
                System.IO.File.Move(passFile, NO_ERROR_PATH + Path.GetFileName(passFile));
            }
            foreach (string errorFile in errorFiles)
            {
                System.IO.File.Move(errorFile, ERROR_PATH + Path.GetFileName(errorFile));
            }
            //write log file
            using (StreamWriter sw = new StreamWriter(LOG_FILE))
            {
                foreach (string passFile in passFiles)
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")+", " + passFile + ", " + NO_ERROR_PATH + Path.GetFileName(passFile));
                }
                foreach (string errorFile in errorFiles)
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")+", " + errorFile + ", Error");
                }
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
