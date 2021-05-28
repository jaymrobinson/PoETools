# PoETools/Stashify
 A tool to create stash art using images from the game Path of Exile. This application was built in Visual Studio 2015 using C# and targeting .NET Framework 4.5.2.
 
 Note: I am not affiliated in any way with Grinding Gear Games, the developers of Path of Exile.
 
# Downloading image assets
When starting off with a newly compiled binary, you must first download the image assets. The asset files are downloaded from the URLs indicated in "assets.txt" from the Path of Exile CDN server with a 500ms delay between each download to avoid straining GGG's servers. This file contains a list of one stash unit items that I could easily find on the web. It appears to be a bit old, and does not contain many of the newer game assets. You can add URLs to this file before downloading, or download additional asset files manually into the asset directory. Use the following command to download assets.

  Stashify.exe /D
  
# Building the color index
After downloading the asset files, you must build the color index. It uses all of the files in the asset directory to build the index. Use the following command to build the color index.

  Stashify.exe /I

# Converting a source image to stash art
Once you have downloaded the image assets and built the color index, you can then begin to stashify source images into stash art. Use the following command to stashify source images.

  Stashify.exe inputFile

# Additional options
Additional options can be found in the config file.

