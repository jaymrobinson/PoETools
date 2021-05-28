# PoETools
 A tool to create stash art using images from the game Path of Exile. This application was built in Visual Studio 2015 using C# and targeting .NET Framework 4.6.2.
 
 Note: I am not affiliated in any way with Grinding Gear Games, the developers of Path of Exile.
 
 # How to use
Stashify.exe /D
Stashify.exe /I
Stashify.exe inputFile

  /D          Downloads assets specified in assets.txt
  /I          Builds the color index (ColorIndex.tsv) from
               the assets contained in the specified AssetFilePath
               in config
  inputFile   Stashifies the specified image file

  Check the config file for further options.

# Downloading image assets
When starting off with a newly compiled binary, you must first download the image assets. The asset files are downloaded from the URLs indicated in "assets.txt" from the Path of Exile CDN server with a 500ms delay between each download to avoid straining GGG's servers. Use the following command to download assets.

  Stashify.exe /D
  
# Building the color index
After downloading the asset files, you must build the color index. Use the following command to build the color index.

  Stashify.exe /I

# Converting a source image to stash art
Once you have downloaded the image assets and built the color index, you can then begin to stashify source images into stash art. Use the following command to stashify source images.

  Stashify.exe inputFile
