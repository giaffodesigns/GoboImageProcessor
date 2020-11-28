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
        static int borderSize = 5;
        static Rgba32 onPixel  = new Rgba32(  0, 255,   0, 255);
        static Rgba32 offPixel = new Rgba32(  0,   0,   0, 255);

        static int widthOrig;
        static int heightOrig;
        static int widthNew;
        static int heightNew;

        static int errorCt = 0;
        static int successCt = 0;

        static bool pauseAtEnd = false;

        static List<GoboObj> gobos = new List<GoboObj>();

        static bool ParseConfigFile()
        {
            string fn = Paths.LocalDir + @"\config.txt";
            if (!File.Exists(fn)) { fn = Paths.LocalDir + @"\backupConfig.txt"; }
            Console.WriteLine("Reading config from: " + fn);
            if (File.Exists(fn))
            {
                var reader = new StreamReader(fn);
                while (!reader.EndOfStream) { 
                    string data = reader.ReadLine();
                    var matches = Regex.Match(data, @"([^:]+):(.+)");
                    string val = matches.Groups[2].Value.Trim();
                    switch (matches.Groups[1].Value.ToLower()) {
                        case "bordersize":
                            Console.WriteLine($"Setting border size to {val}");
                            bool success = int.TryParse(val, out borderSize);
                            if (!success)
                            {
                                throw new Exception("invalid following argument to --bordersize: " + val);
                            }
                            break;

                        case "oncolor":
                            Console.WriteLine($"Setting on-color - {val}");
                            onPixel = Rgba32.ParseHex(val);
                            break;

                        case "offcolor":
                            Console.WriteLine($"Setting off-color - {val}");
                            offPixel = Rgba32.ParseHex(val);
                            break;

                        case "gma2path":
                            Console.WriteLine($"setting gma2 path - {val}");
                            Paths.ma2General = val;
                            break;

                        case "outputpath":
                            Console.WriteLine($"Setting image output path: {val}");
                            Paths.ma2ImageOutput = val;
                            break;

                        case "filelist":
                            Console.WriteLine($"setting filelist location - {val}");
                            Paths.GoboInfo = val;
                            break;

                        case "fixturetypename":
                            Console.WriteLine($"Setting output sub-folder for fixture type - {val}");
                            Paths.Output = val;
                            break;

                        case "pauseatend":
                            if (val == "true")
                            {
                                pauseAtEnd = true;
                            } else
                            {
                                pauseAtEnd = false;
                            }
                            Console.WriteLine($"Pause-At-End set to {pauseAtEnd}");
                            break;

                        default:
                            Console.WriteLine("unrecognized argument: " + matches.Groups[1].Value);
                            break;
                    }
                }
                Console.WriteLine("");
                reader.Close();

                return true;
            } else
            {
                return false;
            }
        }

        static void ParseArgs(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }
            // args:
            // GogoImageProcess --bordersize 5 --onColor ff000000 --offcolor 00000000 --gma2path
            for (int i = 0; i<args.Length; )
            {
                // if we're at the start of an argument definition
                if (args[i].StartsWith("--"))
                {
                    string argument = args[i].Substring(2).ToLower();
                    switch (argument)
                    {
                        case "bordersize":
                        case "b":
                            bool success = int.TryParse(args[i+1], out borderSize);
                            if (!success)
                            {
                                throw new Exception("invalid following argument to --bordersize: " + args[i + 1]);
                            } else
                            {
                                i += 2;
                            }
                            break;

                        case "oncolor":
                        case "on":
                            onPixel = Rgba32.ParseHex(args[i + 1]);
                            i += 2;
                            break;

                        case "offcolor":
                        case "off":
                            offPixel = Rgba32.ParseHex(args[i + 1]);
                            i += 2;
                            break;

                        case "gma2path":
                        case "v":
                            Paths.ma2General = args[i + 1];
                            i += 2;
                            break;

                        default:
                            Console.WriteLine("unrecognized argument: " + args[i]);
                            i++;
                            break;
                    }

                }
            }


        }

        static void DisplayArgs()
        {
            Console.WriteLine("Bordersize: " + borderSize.ToString());
            Console.WriteLine("On Color: " + onPixel.ToString());
            Console.WriteLine("Off Color: " + offPixel.ToString());
            Console.WriteLine($"Original Image Dimensions: {widthOrig} x {heightOrig}");
            Console.WriteLine($"New Image Dimensions: {widthNew} x {heightNew}");
            Console.WriteLine("");
            //Console.WriteLine(Paths.ma2General);
            //Console.WriteLine(Paths.GoboInfo);
            //Console.WriteLine(Paths.DebugFile);
        }

        static void DisplayCurrentDirectories()
        {
            Console.WriteLine("Current Directory: " + GetCurrentDirectory());
            Console.WriteLine("Executing Directory: " + GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Console.WriteLine("");
        }

        static int Main(string[] args)
        {
            //DisplayCurrentDirectories();
            try {
                #region Read and Apply Config
                /* ___Program Settings___ */
                // read arguments from config file (and read backupConfig.txt if config.txt is not available, for debugging purposes)
                ParseConfigFile();
                //ParseArgs(args);      // for command line args

                /* ___Gobo Image List___ */
                var testReader = new StreamReader(Paths.GoboInfo);
                if (testReader != null)
                {
                    Console.WriteLine("Gobo-info file length:" + testReader.ReadToEnd().Length.ToString());
                }
                else
                {
                    Console.WriteLine("File failed to open: " + Paths.GoboInfo);
                }
                Console.WriteLine("");
                testReader.Close();

                var reader = new StreamReader(Paths.GoboInfo);
                // populate list of gobo objecsts
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    var items = Regex.Match(line, @"([^;]+)\;([^;]+)");
                    gobos.Add(new GoboObj(items.Groups[1].Value, items.Groups[2].Value));
                }


                // handle image dimensions
                widthOrig = 128;
                heightOrig = 128;
                widthNew = widthOrig + (2 * borderSize);
                heightNew = heightOrig + (2 * borderSize);

                DisplayArgs();
                #endregion


                // find and confirm the existence of each image
                foreach (var gobo in gobos)
                {
                    // verify and set full filepaths to source images
                    gobo.SetFilePath(Paths.ma2General);

                    // create on and off copies
                    try
                    {
                        // read original image
                        var inStream = new FileStream(gobo.FilePath, FileMode.Open, FileAccess.Read);
                        var imgOrig = new PngDecoder().Decode<Rgba32>(Configuration.Default, inStream);
                        inStream.Close();

                        var imgOn = new Image<Rgba32>(widthNew, heightNew);
                        var imgOff = new Image<Rgba32>(widthNew, heightNew);

                        // fill in top and bottom borders
                        for (int x = 0; x < widthNew; x++)
                        {
                            for (int i = 0; i < borderSize; i++)
                            {
                                imgOn[x, i] = imgOn[x, heightNew - i - 1] = onPixel;
                                imgOff[x, i] = imgOff[x, heightNew - i - 1] = offPixel;
                            }
                        }

                        // fill in left and right borders
                        for (int y = 0; y < heightNew; y++)
                        {
                            for (int i = 0; i < borderSize; i++)
                            {
                                imgOn[i, y] = imgOn[widthNew - i - 1, y] = onPixel;
                                imgOff[i, y] = imgOff[widthNew - i - 1, y] = offPixel;
                            }
                        }

                        // fill in body of image
                        for (int x = 0; x < imgOrig.Width; x++)
                        {
                            for (int y = 0; y < imgOrig.Height; y++)
                            {
                                // handle offsets
                                int newX = x + borderSize;
                                int newY = y + borderSize;

                                // copy to new images
                                imgOn[newX, newY] = imgOff[newX, newY] = imgOrig[x, y];
                            }
                        }

                        // check that output directory exists; create if it doesn't
                        if (!Exists(Paths.Output))
                        {
                            Console.WriteLine("Creating output directory: " + Paths.Output);
                            CreateDirectory(Paths.Output);
                        }

                        // write images to files
                        var encoder = new PngEncoder();
                        var outStreamOn = new FileStream(Paths.Output + gobo.OnName + ".png", FileMode.OpenOrCreate, FileAccess.Write);
                        var outStreamOff = new FileStream(Paths.Output + gobo.OffName + ".png", FileMode.OpenOrCreate, FileAccess.Write);
                        encoder.Encode(imgOn, outStreamOn);
                        encoder.Encode(imgOff, outStreamOff);
                        outStreamOn.Close();
                        outStreamOff.Close();

                        successCt++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR: " + e.Message);
                        errorCt++;
                    }
                    Console.WriteLine("");
                }

            }
            catch (Exception e)
            {
                using (var writer = new StreamWriter(Paths.DebugFile))
                {
                    writer.WriteLine(e.Message);
                    foreach (var data in e.Data)
                    {
                        writer.WriteLine(data.ToString());
                    }
                }
            }

            // pausing for status read at end if opted-in
            Console.WriteLine("Successful writes: " + successCt.ToString());
            if (pauseAtEnd) { Console.Read(); }

            // return errors; if 0, all went as expected
            if (errorCt == 0) {
                return successCt;
            } else {
                return -errorCt;
            }
        }
    }

    static class Paths
    {
        static string _outputExt = "OutputImages";
        static public string ma2General = @"C:\ProgramData\MA Lighting Technologies\grandma\gma2_V_3.9";
        static public string ma2ImageOutput = @"C:\ProgramData\MA Lighting Technologies\grandma\gma2_V_3.9\images\";
        static public string Output
        {
            get
            {
                return ma2ImageOutput + $@"{_outputExt}\";
            }
            set
            {
                _outputExt = value;
            }
        }
        //static public string GoboInfo { get { return ma2General + @"\reports\GoboImageInfo.txt"; } }
        static public string GoboInfo { get { return LocalDir + @"\GoboImageFiles.txt"; } set { } }
        static public string DebugFile { get { return LocalDir + @"DebugLog.txt"; } }
        static public string LocalDir = GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
