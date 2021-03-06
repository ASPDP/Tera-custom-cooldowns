﻿using System.IO;
using System.Net;

namespace TCC.Parsing
{
    public static class OpcodeDownloader
    {
        public static void DownloadIfNotExist(uint version, string directory)
        {
            DownloadOpcode(version, directory);
            DownloadSysmsg(version, directory);
        }

        private static void DownloadOpcode(uint version, string directory)
        {
            Directory.CreateDirectory(directory);

            var filename = directory + Path.DirectorySeparatorChar + version + ".txt";
            if (File.Exists(filename)) return;
            filename = directory + Path.DirectorySeparatorChar + "protocol." + version + ".map";
            if (File.Exists(filename)) return;
            try
            {
                Download("https://raw.githubusercontent.com/caali-hackerman/tera-data/master/map_base/protocol." + version + ".map", filename);
                return;
            }
            catch { /* ignored*/ }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/protocol." + version + ".map", filename);
            }
            catch { /* ignored*/ }
        }

        public static bool DownloadSysmsg(uint version, string directory, int revision = 0)
        {
            Directory.CreateDirectory(directory);

            var filename = directory + Path.DirectorySeparatorChar + "smt_" + version + ".txt";
            if (File.Exists(filename)) return false;
            filename = directory + Path.DirectorySeparatorChar + "sysmsg." + revision/100 + ".map";
            if (File.Exists(filename)) return false;
            try
            {
                Download("https://raw.githubusercontent.com/caali-hackerman/tera-data/master/map_base/sysmsg." + revision / 100 + ".map", filename);
                return true;
            }
            catch { /* ignored*/ }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/sysmsg." + revision / 100 + ".map", filename);
                return true;
            }
            catch { /* ignored*/ }
            return false;
        }

        private static void Download(string remote, string local)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
                client.DownloadFile(remote, local);
            }
        }
    }
}
