using Stashify.Services;
using Stashify.Validation;
using System;

namespace Stashify
{
    class Program
    {
        static IFileValidator fileValidator;
        static IAssetDownloader assetDownloader;
        static IColorIndexBuilder colorIndexBuilder;
        static IImageGenerationService imageGenerationService;

        static void Main(string[] args)
        {
            fileValidator = new FileValidator();
            assetDownloader = new AssetDownloader();
            colorIndexBuilder = new ColorIndexBuilder();
            imageGenerationService = new ImageGenerationService();

            if (args.Length == 0)
            {
                PrintHelp();
            }
            else
            {
                var validationResult = fileValidator.Validate();

                if (!validationResult.IsValid)
                {
                    Console.WriteLine($"File Validation Failed {validationResult.Message()}");
                }

                if (args[0] == "/d")
                {
                    assetDownloader.Download();
                }
                else if (args[0] == "/i")
                {
                    colorIndexBuilder.Build();
                }
                else
                {
                    imageGenerationService.Generate(args[0]);
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("");
            Console.WriteLine("Stashify.exe /D");
            Console.WriteLine("Stashify.exe /I");
            Console.WriteLine("Stashify.exe inputFile");
            Console.WriteLine("");
            Console.WriteLine("  /D          Downloads assets specified in assets.txt");
            Console.WriteLine("  /I          Builds the color index (ColorIndex.tsv) from");
            Console.WriteLine("               the assets contained in the specified AssetFilePath");
            Console.WriteLine("               in config");
            Console.WriteLine("  inputFile   Stashifies the specified image file");
            Console.WriteLine("");
            Console.WriteLine("  Check the config file for further options.");
            Console.WriteLine("");
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
