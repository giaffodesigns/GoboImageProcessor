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

        static List<GoboObj> gobos = new List<GoboObj>();



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

            // handle image dimensions
            widthOrig  = 128;
            heightOrig = 128;
            widthNew  = widthOrig  + (2 * borderSize);
            heightNew = heightOrig + (2 * borderSize);
        }

        static int Main(string[] args)
        {
            
            ParseArgs(args);


            #region Open and parse image list
            // tested. OK.
            var reader = new StreamReader(Paths.GoboInfo);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                var items = Regex.Match(line, @"([^;]+)\;([^;]+)");
                gobos.Add(new GoboObj(items.Groups[1].Value, items.Groups[2].Value));
            }
            #endregion

            // find and confirm the existence of each image
            foreach(var gobo in gobos)
            {
                // verify and set full filepaths to source images
                gobo.SetFilePath(Paths.ma2General);

                // create on and off copies
                try
                {
                    // read original imge
                    var inStream     = new FileStream(gobo.FilePath, FileMode.Open, FileAccess.Read);
                    var imgOrig = new PngDecoder().Decode<Rgba32>(Configuration.Default, inStream);
                    inStream.Close();

                    // come back to handle resizing later
                    //if (imgOrig.Width != widthOrig || imgOrig.Height != heightOrig)
                    //{
                    //    ResizeExtensions.Resize(imgOrig.)
                    //}

                    var imgOn  = new Image<Rgba32>(widthNew, heightNew);
                    var imgOff = new Image<Rgba32>(widthNew, heightNew);

                    // fill in top and bottom borders
                    for (int x = 0; x < widthNew; x++)
                    {
                        for (int i = 0; i < borderSize; i++)
                        {
                            imgOn[x, i]  = imgOn[x, heightNew - i - 1]  = onPixel;
                            imgOff[x, i] = imgOff[x, heightNew - i - 1] = offPixel;
                        }
                    }

                    // fill in left and right borders
                    for (int y = 0; y < heightNew; y++)
                    {
                        for (int i = 0; i < borderSize; i++)
                        {
                            imgOn[i, y]  = imgOn[widthNew-i-1, y] = onPixel;
                            imgOff[i, y] = imgOff[widthNew-i-1, y] = offPixel;
                        }
                    }

                    // fill in body of image
                    for (int x=0; x<imgOrig.Width; x++)
                    {
                        for (int y=0; y<imgOrig.Height; y++)
                        {
                            // handle offsets
                            int newX = x + borderSize;
                            int newY = y + borderSize;

                            // copy to new images
                            imgOn[newX, newY] = imgOff[newX, newY] = imgOrig[x, y];
                        }
                    }

                    // check that output directory exists; create if it doesn't
                    if (!Directory.Exists(Paths.Output))
                    {
                        CreateDirectory(Paths.Output);
                    }

                    // write images to files
                    var encoder = new PngEncoder();
                    var outStreamOn  = new FileStream(Paths.Output+gobo.OnName+".png",  FileMode.OpenOrCreate, FileAccess.Write);
                    var outStreamOff = new FileStream(Paths.Output+gobo.OffName+".png", FileMode.OpenOrCreate, FileAccess.Write);
                    encoder.Encode(imgOn, outStreamOn);
                    encoder.Encode(imgOff, outStreamOff);
                    outStreamOn.Close();
                    outStreamOff.Close();

                    successCt++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    errorCt++;
                }
            }

            // return errors; if 0, all went as expected
            return -errorCt;
        }
    }

    static class Paths
    {
        static public string ma2General = @"C:\ProgramData\MA Lighting Technologies\grandma\gma2_V_3.9";
        static public string Output
        {
            get
            {
                return ma2General + @"\images\OutputImages\";
            }
        }
        static public string GoboInfo { get { return ma2General + @"\reports\GoboImageInfo.txt"; } }
        
    }
}
