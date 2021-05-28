using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Stashify.Services
{
    public class AssetDownloader : IAssetDownloader
    {
        public void Download()
        {
            File.Copy("black.png", Path.Combine(Configuration.ASSET_DIR, "black.png"));

            using (var client = new WebClient())
            {
                foreach (var line in File.ReadAllLines(Configuration.ASSET_LIST_FILE))
                {
                    Console.WriteLine("Downloading: " + line);
                    string filename = line.Split(new char[] { '/' }).LastOrDefault();
                    client.DownloadFile(line, Path.Combine("assets", filename));
                    Thread.Sleep(500);
                }
            }
        }
    }
}
