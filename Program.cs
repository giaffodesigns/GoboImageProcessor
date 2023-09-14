using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static System.IO.Directory;
using static System.IO.Path;
using static System.Environment;
using SixLabors.ImageSharp.Processing;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace GoboImageProcessor
{
    class Program
    {
        // ERROR/SUCCESS LOGGING
        static int errorCt = 0;
        static int successCt = 0;

        static bool pauseAtEnd = false;

        static List<GoboObj> gobos = new List<GoboObj>();

        // parse config file and populate values declared at top of Program class
        static bool ParseConfigFile()
        {
            if (File.Exists(Paths.FileConfig))
            {
                var reader = new StreamReader(Paths.FileConfig);
                while (!reader.EndOfStream) { 
                    string data = reader.ReadLine();
                    var matches = Regex.Match(data, @"([^:]+)\t(.+)");
                    string val = matches.Groups[2].Value.Trim();
                    switch (matches.Groups[1].Value.ToLower()) {                                                            // LOG ALL OF THIS
                        case "bordersize":
                            Console.WriteLine("Setting border size");
                            bool success = int.TryParse(val, out GoboObj.borderSize);
                            if (!success)
                            {
                                throw new Exception("invalid following argument to --bordersize: " + val);
                            }
                            break;

                        case "oncolor":
                            Console.WriteLine("Setting oncolor");
                            GoboObj.onPixel = Rgba32.ParseHex(val);
                            break;

                        case "offcolor":
                            Console.WriteLine("Setting off color");
                            GoboObj.offPixel = Rgba32.ParseHex(val);
                            break;

                        case "gma2path":
                            Console.WriteLine("setting gma2 path");
                            val = val.Replace(@"/", @"\");
                            val += @"\";
                            Paths.DirMA2 = val;
                            break;

                        case "filelist":
                            Console.WriteLine("setting filelist location");
                            Paths.FileGoboInfo = val.Replace('/', '\\');
                            break;

                        case "fixturetypename":
                            Console.WriteLine("setting output location for fixture type");
                            Paths.DirOutput = val;
                            break;

                        case "pauseatend":
                            Console.WriteLine("setting pause-at-end status");
                            pauseAtEnd = bool.Parse(val);
                            break;


                        default:
                            Console.WriteLine("unrecognized argument: " + matches.Groups[1].Value);
                            break;
                    }
                }
                reader.Close();

                return true;
            } else
            {
                return false;
            }
        }

        static void DisplayArgs()
        {
            Console.WriteLine("Bordersize: " + GoboObj.borderSize.ToString());
            Console.WriteLine("On Color: " + GoboObj.onPixel.ToString());
            Console.WriteLine("Off Color: " + GoboObj.offPixel.ToString());
            Console.WriteLine(Paths.DirMA2);
            Console.WriteLine(Paths.FileGoboInfo);
            Console.WriteLine(Paths.FileDebug);
        }

        static void ParseImageList() {
            // WHERE DOES THE OPEN IMAGE COME INTO PLAY?
            var testReader = new StreamReader(Paths.FileGoboInfo);
            if (testReader != null) {
                Console.WriteLine("File Length:" + testReader.ReadToEnd().Length.ToString());
            } else {
                throw new Exception($"Gogo Image List file failed to open.\nLocation used: {Paths.FileGoboInfo}");
            }
            testReader.Close();

            // add open gobo
            gobos.Add(new GoboObj("Open", Paths.FileOpenGobo, true));

            // add fixture gobos
            var reader = new StreamReader(Paths.FileGoboInfo);
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                var items = Regex.Match(line, @"([^;]+)\t([^;]+)");
                string name = items.Groups[1].Value;
                string relativePath = items.Groups[2].Value.Replace('/', '\\');
                gobos.Add(new GoboObj(name, relativePath));
            }
        }

        static int Main(string[] args)
        {
            using (var outputStream = new StreamWriter(Paths.FileDebug))
            {
                // set streams to files to be read
                Console.SetOut(outputStream);
                Console.SetError(outputStream);


                try
                {
                    // read arguments from config file (and read backupConfig.txt if config.txt is not available, for debugging purposes)
                    ParseConfigFile();

                    // DEBUG PRINTING: Display working args
                    DisplayArgs();

                    // Parse gobo image list and popoulate gobos (List<GoboObj>)
                    ParseImageList();


                    // create processed image pairs for each gobo
                    foreach (var gobo in gobos)
                    {
                        // create on and off copies
                        try
                        {
                            Console.WriteLine($"Processing gobo \"{gobo.PublicName}\"...");
                            successCt += gobo.GenerateOutputImagePairs();
                            Console.WriteLine("...OK");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            errorCt++;
                        }
                    }

                }
                catch (Exception e)
                {
                    using (var writer = new StreamWriter(Paths.FileDebug))
                    {
                        writer.WriteLine(e.Message);
                        foreach (var data in e.Data)
                        {
                            writer.WriteLine(data.ToString());
                        }
                    }
                }


                // create ouptut file for communication back to Lua application
                using (var writer = new StreamWriter(Paths.FileResponse)) {
                    writer.WriteLine($"created\t{successCt}");
                    writer.WriteLine($"errors\t{errorCt}");
                    writer.WriteLine($"logfile\t{Paths.FileDebug}");
                    writer.WriteLine($"outputdir\t{Paths.DirOutput}");
                }

                // pause if requested in application
                //if (pauseAtEnd) {
                //    Console.ReadKey();
                //}

                // return errors; if 0, all went as expected
                return -errorCt;
            }
        }
    }


}
