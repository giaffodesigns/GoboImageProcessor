using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;
using System.Text;
using static System.IO.Directory;
using static System.IO.Path;
using System.Text.RegularExpressions;

namespace GoboImageProcessor
{
    class GoboObj
    {
        // static values
        static private int _index = 1;
        public static int Count {
            get {
                return _index;
            }
        }
        static public Rgba32 onPixel = new Rgba32(0, 255, 0, 255);
        static public Rgba32 offPixel = new Rgba32(0, 0, 0, 255);
        static public int borderSize = 5;


        // object values/properties
        public string PathRelative { get; set; }
        private string pathFullOverride = null;     // backup value if a forced filepath is needed (default open-gobo)
        public string PathFull {
            get {
                if (pathFullOverride != null) {
                    return pathFullOverride;
                }

                return Paths.DirMA2 + @"gobos\" + PathRelative;
            }
            set {
                pathFullOverride = value;
                if (File.Exists(pathFullOverride)) {
                    _hasError = false;
                }
            }
        }
        public int Index { get; private set; }
        bool _hasError = false;
        

        // Names
        public string Name { get; set; }
        public string PublicName
        {
            get
            {
                return $"{Index:D2} {Name}";
            }
        }

        public string OnName
        {
            get
            {
                return $"On {Index:D2} {Name}";
            }
        }

        public string OffName
        {
            get
            {
                return $"Off {Index:D2} {Name}";
            }
        }

        private void setIndex()
        {
            Index = _index++;
        }


        // Constructors
        public GoboObj() { setIndex(); }

        public GoboObj(string name, string relativePath)
        {
            Name = name;
            PathRelative = relativePath;
            if (!File.Exists(PathFull)) {
                _hasError = true;
                throw new Exception($"Error creating gobo object.\nFile does not exist: {PathFull}");
            }
            setIndex();
        }

        public GoboObj(string name, string absolutePath, bool setAbsolute) {
            Name = name;
            pathFullOverride = absolutePath;
            if (!File.Exists(PathFull)) {
                _hasError = true;
                throw new Exception($"Error creating gobo object.\nFile does not exist: {PathFull}");
            }
            setIndex();
        }

        // Image Processing
        public int GenerateOutputImagePairs() {
            int createdTotal = 0;

            // error cases
            if (_hasError) {
                throw new Exception($"Gobo \"{PublicName}\" will not be processed. An error was detected during construction.");
            }

            //! COME BACK - update for other image types
            if (!Regex.IsMatch(PathFull, @"\.png$")) {
                throw new Exception($"Image not accepted. Only .PNG images are currently supported.\n{PathFull}");
            } 

            // gobo image processing
            var inStream = new FileStream(PathFull, FileMode.Open, FileAccess.Read);
            var imgOrig = new PngDecoder().Decode<Rgba32>(Configuration.Default, inStream);
            inStream.Close();

            // we are basing our border size on default image sizes of 128px x 128px
            int borderSizeActual = (int)(Math.Round((double)imgOrig.Width / 128) * borderSize);
            int heightNew = imgOrig.Height + (2 * borderSizeActual);
            int widthNew = imgOrig.Width + (2 * borderSizeActual);

            // generate blank canvases
            var imgOn = new Image<Rgba32>(widthNew, heightNew);
            var imgOff = new Image<Rgba32>(widthNew, heightNew);

            // fill each image with border color
            //// fill in top and bottom borders
            //for (int x = 0; x < widthNew; x++) {
            //    for (int i = 0; i < borderSize; i++) {
            //        imgOn[x, i] = imgOn[x, heightNew - i - 1] = onPixel;
            //        imgOff[x, i] = imgOff[x, heightNew - i - 1] = offPixel;
            //    }
            //}

            //// fill in left and right borders
            //for (int y = 0; y < heightNew; y++) {
            //    for (int i = 0; i < borderSize; i++) {
            //        imgOn[i, y] = imgOn[widthNew - i - 1, y] = onPixel;
            //        imgOff[i, y] = imgOff[widthNew - i - 1, y] = offPixel;
            //    }
            //}
            for (int x = 0; x < widthNew; x++) {
                for (int y = 0; y < heightNew; y++) {
                    imgOn[x, y] = onPixel;
                    imgOff[x, y] = offPixel;
                }
            }

            // fill in body of image
            for (int x = 0; x < imgOrig.Width; x++) {
                for (int y = 0; y < imgOrig.Height; y++) {
                    // handle offsets
                    int newX = x + borderSizeActual;
                    int newY = y + borderSizeActual;

                    // copy to new images
                    imgOn[newX, newY]  = imgOrig[x, y];
                    imgOff[newX, newY] = imgOrig[x, y];
                }
            }


            // write completed images to file
            Paths.EnsureExists(Paths.DirOutput);
            var encoder = new PngEncoder();

            Console.WriteLine($"Writing Image File: {Paths.DirOutput}{OnName}");
            var outStreamOn  = new FileStream($"{Paths.DirOutput}{OnName}.png", FileMode.OpenOrCreate, FileAccess.Write);
            encoder.Encode(imgOn,  outStreamOn);
            outStreamOn.Close();
            createdTotal++;

            Console.WriteLine($"Writing Image File: {Paths.DirOutput}{OffName}");
            var outStreamOff = new FileStream($"{Paths.DirOutput}{OffName}.png", FileMode.OpenOrCreate, FileAccess.Write);
            encoder.Encode(imgOff, outStreamOff);
            outStreamOff.Close();
            createdTotal++;

            return createdTotal;
        }
    }
}
