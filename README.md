# Astrea Archipelago

This is a mod plugin for the game [Astrea](https://store.steampowered.com/app/1755830/Astrea_SixSided_Oracles) to work with the multiworld randomizer [Archipelago](https://github.com/archipelagoMW/Archipelago/).

The archipelago world is currently [in development](https://github.com/MazelessMinotaur/AstreaGalaxy)

# Be Aware

This is very much still in proof of concept. It is likely breaking changes will occur as this is worked on.

This plugin has not been fully tested. I have run into issues where the game softlocks. Genreally, this is resolved by restart the game without the mod, but this has not always fixed it, leaving a run unable to be finished.

If you are giving this mod a go and run into issues (or have comments in general) please post them in the [Archipelago Astrea thread](https://discord.com/channels/731205301247803413/1287170160540778588)

There are many missing features, such as:

- Removing items from the game. Right now, this mod only grants bonus items to the player. At each node/battle, you still receive the normal items (but may receive additional depending on the archipelago)
- Locations past Chapter 1 (Tainted Reef). While not difficult to add items/locations past this, I'm working just on chapter one fot the basic implementation
- Updating rewards while in the map view
- Testing with newest patch. The new daily run/run modifiers generally work okay, but some seem to cause issue (attempting to do a run where you draft dice just locked up the game)
- Making this an offical mod. At the moment you need to build and move files manually, goal is to get this into a mod on steam
- Storing world state. Right now, all testing has been done in a single run. Loading a run in progess may cause issues, and rejoining a archipelago alreayd in progess may not trigger items correctly
- Entering server address/port/password. This is all hard coded.
- A bunch of other fixes/ideas I want to implement

# Setup

All setup has only been tested by 1 person (me) so far. This guide is likely missing steps.

## Plugin

First, get this repo to build the archipelago plugin. I am looking to improve this process as it is quite a pain

- Add [BepinEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) to your local Astrea installation. This can be found via steam. In your steam library, right click Astrea, follow Properties...->Installed Files->Browse... This will open your Astrea install folder
- Build this repo. You'll need to set "DicePath" in the .csproj file to match the Astrea path you had found previously. This worked best for me in VSCode. 
- You may need to install depenancies. [Nuget](https://www.nuget.org/) should generally handle this.
- Copy the output file from /bin/debug/netstandar2.0/AstreaArchipelago.dll into AstreaPath/BepInEx/plugins. Ideally, this should already be there from the build process.
- You may need to copy the Archipelago plugin into the AstreaPath/BepInEx/plugins. This should be at Users\USER\\.nuget\packages\archipelago.multiclient.net\6.3.1\lib\netstandard2.0

While the main purpose of this mod is to work with Archipelago worlds, you can test to make sure it is running right. Logs should be sent to Astrea/BepInEx/LogOutput.txt

Additioanlly, as of writing this there are various random [experiements](https://github.com/MazelessMinotaur/AstreaArchipelago/blob/main/src/Experiments.cs) I am messing with. 
The most obious that should show if this mod is working are you should receive double star shards, and whenever you deal purification you gain that much star shards (which is then doubled possibly)

## Archipelago World

The archipelago world for this mod can be found at [AstreaGalaxy](https://github.com/MazelessMinotaur/AstreaGalaxy). This doesn't have a setup guide yet, but one will be added shortly.
For now it is possible to build a world/archipelago with a very basic yaml but the default setup is not created yet.
