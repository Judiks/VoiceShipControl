using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoiceShipControl.Shared
{
    public static class FileHelper
    {

        public static string GetFilePath(string fileName)
        {
            string[] files = Directory.GetFiles(PluginConstants.PathToFolder, fileName, SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                return files[0];
            }
            Console.WriteLine($"File '{fileName}' not found in '{PluginConstants.PathToFolder}' or its subdirectories.");
            return string.Empty;
        }
    }
}
