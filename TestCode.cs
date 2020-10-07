using System;
using System.Collections.Generic;
using System.Text;

namespace GoboImageProcessor
{
    class TestCode
    {
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
    }
}
