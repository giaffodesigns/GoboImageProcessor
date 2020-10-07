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

namespace GoboImageProcessor
{
    class Program
    {
        static string maVersionFolder = "gma2_V_3.9";
        static int _borderSize = 3;
        static Rgba32 onPixel  = new Rgba32(255, 255, 255, 255);
        static Rgba32 offPixel = new Rgba32(  0,   0,   0, 255);

        // come back to handle variables for border and colors
        //static 

        //static void parseArgs(string[] args)
        //{

        //}

        static int Main(string[] args)
        {
            // args:
            // GogoImageProcess --bordersize 5 --onColor ff000000 --offcolor 00000000
            int borderSize;

            var gobos = new List<GoboObj>();

            #region Open and parse image list
            // tested. OK.
            var reader = new StreamReader(Paths.goboInfo);
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
                gobo.SetFilePath(maVersionFolder);

                // copy to outputimages folder
                try
                {
                    var instream = new FileStream(Paths.input, FileMode.Open, FileAccess.Read);
                    var outstream = new FileStream(Paths.output, FileMode.OpenOrCreate, FileAccess.Write);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                //Console.WriteLine(gobo.PublicName);
            }




            //// decode existing image
            //var decoder = new PngDecoder();
            ////var conf = new Configuration();
            ////var conf = Configuration.Default;
            //var instream = new FileStream(Paths.input, FileMode.Open, FileAccess.Read);
            //var img = decoder.Decode<Rgba32>(Configuration.Default, instream);
            //instream.Close();


            //// edit outer pixel along entire frame
            //// top and bottom rows
            //int borderWidth = 10;
            //var wPixel = new Rgba32(255, 255, 255, 255);
            //for (int x = 0; x < img.Width; x++)
            //{
            //    // top and bottom border
            //    for (int i = 0; i < borderWidth; i++)
            //    {
            //        //img[x, i] = wPixel;
            //        //img[x, img.Height - 1 - i] = wPixel;
            //        img[x, i] = img[x, img.Height - 1 - i] = wPixel;
            //    }
            //}

            //// left and right columns
            //for (int y = 0; y < img.Height; y++)
            //{
            //    for (int i = 0; i < borderWidth; i++)
            //    {
            //        img[i, y] = img[img.Width - 1 - i, y] = wPixel;
            //    }
            //}


            //// encode as new image
            //var encoder = new PngEncoder();
            //var outstream = new FileStream(Paths.output, FileMode.OpenOrCreate, FileAccess.Write);
            //encoder.Encode<Rgba32>(img, outstream);
            //outstream.Close();
            Console.Read();
            return 0;
        }
    }

    struct Paths
    {
        static public string maGobos = @"C:\ProgramData\MA Lighting Technologies\grandma\";
        static public string goboInfo = @"..\..\..\SourceImages\GoboImageInfo.txt";
        static public string output = @"..\..\..\OutputImages\Output1.png";
        static public string input = @"..\..\..\SourceImages\Input1.png";
    }
}
