using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace LPToolKit.Util
{
    /// <summary>
    /// Prefered object to use when referring to a file to be loaded
    /// that will be relative to some base directory.  This object
    /// leaves the base directory and virtual path (with the ~ symbol
    /// indicating the base directory) separate, along with an optional
    /// field of what is requesting the file for better logging and 
    /// error handling.
    /// </summary>
    /// <remarks>
    /// Also contains utilities for mapping paths in a way appropriate 
    /// for the current platform, with support for virtual paths where 
    /// "~" refers to a file being relative to some base path.
    /// </remarks>
    public class FilePath
    {
        #region Constructors

        public FilePath()
        {

        }

        public FilePath(string filename)
        {
            Filename = filename;
        }

        public FilePath(string baseFolder, string filename)
        {
            BaseFolder = baseFolder;
            Filename = filename;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The base directory for this path.
        /// </summary>
        public string BaseFolder = null;

        /// <summary>
        /// The filename relative to the base folder when "~" is included.
        /// </summary>
        public string Filename;

        /// <summary>
        /// A string identifying who is requesting this file. 
        /// </summary>
        public string Source = null;

        /// <summary>
        /// The fully qualified path created by combining the base folder
        /// and filename, and adjusting slashes appropriately for the 
        /// current operating system.
        /// </summary>
        public string FullPath
        {
            get { return _fullPath ?? (_fullPath = (BaseFolder == null ? Map(Filename) : Map(BaseFolder, Filename))); }
        }
        private string _fullPath = null;

        /// <summary>
        /// Returns true iff the file exists.
        /// </summary>
        public bool Exists
        {
            get { return File.Exists(FullPath); }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format("[Path: {0}, Source: {1}]", FullPath, Source ?? "unknown");
        }
        #endregion

        #region Static Utility Methods

        /// <summary>
        /// Corrects the slash direction for the current operating system.
        /// </summary>
        public static string Map(string filename)
        {
            if (filename == null)
            {
                return filename;
            }
            switch (Platform.OS.Platform)
            {
                case Platform.Platforms.MacOSX:
                    filename = filename.Replace("\\", "/");
                    break;
                case Platform.Platforms.Windows:
                    filename = filename.Replace("/", "\\");
                    break;
            }
            return filename;
        }

        /// <summary>
        /// Maps a path relative to a base folder, with support for "~".
        /// </summary>
        public static string Map(string baseFolder, string filename)
        {
            Assert.NotNull("Map.baseFolder", baseFolder);
            Assert.NotNull("Map.filename", filename);
            return Map(filename.Replace("~", baseFolder));
        }

        #endregion
    }

    /// <summary>
    /// Utility methods that all code in the library should use for reading
    /// and writing files.  By doing so, we can add support for logging and
    /// caching later if needed.
    /// </summary>
    public class FileIO
    {
        public static string LoadTextFile(FilePath path)
        {
            Session.UserSession.Current.Console.Add("FileIO.LoadTextFile: " + path.ToString(), "FileIO");
            if (!path.Exists)
            {
                throw new FileNotFoundException("Cannot find file " + path.ToString());
            }
            return File.ReadAllText(path.FullPath);
        }

        public static T LoadJsonFile<T>(FilePath path)
        {
            Session.UserSession.Current.Console.Add("FileIO.LoadJsonFile: " + path.ToString(), "FileIO");
            if (!path.Exists)
            {
                throw new FileNotFoundException("Cannot find file " + path.ToString());
            }
            string json = File.ReadAllText(path.FullPath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveTextFile(FilePath path, string contents)
        {
            Session.UserSession.Current.Console.Add("FileIO.SaveTextFile: " + path.ToString(), "FileIO");
            File.WriteAllText(path.FullPath, contents);
        }

        public static byte[] LoadBinaryFile(FilePath path)
        {
            Session.UserSession.Current.Console.Add("FileIO.LoadBinaryFile: " + path.ToString(), "FileIO");
            if (!path.Exists)
            {
                throw new FileNotFoundException("Cannot find file " + path.ToString());
            }
            return File.ReadAllBytes(path.FullPath);
        }

        public static void SaveJsonFile(FilePath path, object data)
        {
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path.FullPath, json);            
        }
    }
}
