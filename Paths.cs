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

namespace GoboImageProcessor {
    static class Paths {
        // Directories
        static public string DirLocal = GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
        static public string DirConfig = DirLocal + @"pipe\";
        static public string DirLogs = DirLocal + @"logs\";

        private static string _dirName_Output = "OutputImages";
        static public string DirMA2 = @"C:\ProgramData\MA Lighting Technologies\grandma\gma2_V_3.9\";
        static public string DirOutput {
            get {
                return DirMA2 + @$"images\{_dirName_Output}\";
            }
            set {
                _dirName_Output = value;
            }
        }

        // Filepaths
        static public DateTime now = DateTime.Now;
        static private string nowDetails = $"{now.Year}.{now.Month:00}.{now.Day:00}_{now.Hour:00}-{now.Minute:00}-{now.Second:00}";

        static public string FileGoboInfo = DirConfig + @"GoboImageFiles.tsv";
        static public string FileDebug = DirLogs + $"log_{nowDetails}.txt";
        static public string FileOpenGobo = DirLocal + @"assets\Open Gobo.png";
        static public string FileResponse = DirConfig + @"response.tsv";

        // read location for config file
        static public string FileConfig {
            get {
                string filePath = DirConfig + @"config.tsv";
                if (!File.Exists(filePath)) { filePath = DirConfig + @"backupConfig.tsv"; }                                    // CHECK THAT THIS EXISTS
                Console.WriteLine("Reading config from: " + filePath);
                return filePath;
            }
        }

        static public void EnsureExists(string filePath) {
            if (!Exists(filePath)) {
                CreateDirectory(filePath);
            }
            return;
        }
    }
}
