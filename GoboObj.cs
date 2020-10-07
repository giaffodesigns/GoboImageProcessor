using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GoboImageProcessor
{
    class GoboObj
    {
        static private int _series = 1;


        public string Name { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int Series { get; private set; }

        #region Names
        public string PublicName
        {
            get
            {
                return $"{Series:D2} {Name}";
            }
        }

        public string OnName
        {
            get
            {
                return $"ON{Series:D2} {Name}";
            }
        }

        public string OffName
        {
            get
            {
                return $"OFF{Series:D2} {Name}";
            }
        }

        private void setSeries()
        {
            Series = _series++;
        }
        #endregion

        public GoboObj() { setSeries(); }

        public GoboObj(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
            setSeries();
        }

        public void SetFilePath(string maVersionFolder)
        {
            string testPath = @"C:\ProgramData\MA Lighting Technologies\grandma\" + maVersionFolder + @"\gobos\" + FileName;
            if (File.Exists(testPath))
            {
                FilePath = testPath;
                //Console.WriteLine($"SUCCESS: {testPath}");
            }
            else
            {
                Console.WriteLine($"Gobo image could not be found for \"{Name}\"");
            }
        }
    }
}
