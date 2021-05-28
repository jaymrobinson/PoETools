using System;
using System.Configuration;

namespace Stashify
{
    public static class Configuration
    {
        public static string ASSET_DIR = ConfigurationManager.AppSettings["AssetFilePath"] ?? "assets";
        public static string ASSET_LIST_FILE = "assets.txt";
        public static string COLOR_INDEX_FILE = "ColorIndex.tsv";
        public static bool DRAW_GRID_LINES = Convert.ToBoolean(ConfigurationManager.AppSettings["DrawGridLines"] ?? "false");
        public static int IMAGE_WIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["HorizontalResolution"] ?? "74");
        public static int IMAGE_HEIGHT = Convert.ToInt32(ConfigurationManager.AppSettings["VerticalResolution"] ?? "74");
        public static bool PRIORITIZE_HUE = Convert.ToBoolean(ConfigurationManager.AppSettings["PrioritizeHue"] ?? "true");
        public static bool PRESERVE_ASPECT_RATIO = Convert.ToBoolean(ConfigurationManager.AppSettings["PreserveAspectRatio"] ?? "true");

    }
}
