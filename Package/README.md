Lunar Config is an all-in-one configuration mod for Lethal Company. Lunar Config currently allows you to change various properties around items, enemies, moons, and map objects (traps) with more planned in the future. This mod supports content in the vanilla game and added via LethalLevelLoader, LethalLib, and CRLib (it also requires all of these, along with WeatherRegistry, in order to function properly!).

# Notice!

The configuration files used by Lunar Config **NEED** to be shared between all clients playing in a lobby, differences are very prone to causing desyncs!

Also know that Lunar Config can be overwritten by other mods configuration, so if you are experiencing issues with your configurations not working correctly, please check any mods that may be potentially overwriting Lunar Config.

Changing some settings may also cause issues by nature, like the stunability of enemies, so keep this in mind when meddling with certain settings.

# Usage

After installing the mod, if you launch the game and load into a lobby, several configuration files should be generated. If you change anything in the 'LunarConfigCentral' file you will need to repeat this process for the configuration to refresh.

Any setting you want to change requires you to enable the "Configure Content" value in the given entry, otherwise the changes will not be acknowledged. I'm not going to go through each setting here as they are for the most part self-explanatory, but you can always ask questions in [the mod's discord thread](https://discord.com/channels/1168655651455639582/1390479837025538048/1390479837025538048).

## Porting

Lunar Config includes an option to port most configuration settings from CentralConfig. To do so, enable the "Run Late" option in 'LunarConfigCentral', you may also want to delete all Lunar Config files in the LunarConfig folder before doing this for the best result. Then run the game and load into a lobby to refresh your configuration files and most settings should port from CentralConfig (this isn't perfect, but it can get many settings).

**Make sure you disable the 'Run Late' setting after you run the game once, as leaving it on will disable some of the mod's functionality!**

## Disabling Settings

Lunar Config includes **several** configuration settings, and it's unlikely you'll use all of them; in order to disable changing a setting to save on performance or prevent changing something you don't want to touch, you have three options:

**Entire File**\
You can either disable an entire configuration file if you don't want to change anything about any of a certain type of object, for instance you can disable modifying all enemies. With this option, the configuration file may still appear but it will not be refreshed and none of it's settings will be applied.

**Specific Object**\
If you don't want to change anything about a specific object, you can leave the "Configure Content" value of the entry disabled. This setting comes automatically disabled on all entries, requiring you to enable it if you want to change something.

**Setting Type**\
In the 'LunarConfigCentral' file, you should find entries relating to all the configuration files you have enabled, there you can disable a certain type of setting and it will not be shown in any of that file's configuration entries or touched by the mod.

## Help and Issues

If you need any help you are free to ask in this mod's thread in the [Lethal Company Modding Discord server](https://discord.com/channels/1168655651455639582/1390479837025538048/1390479837025538048). You can also report issues there, or in the mod's [GitHub Page](https://github.com/CraftyC7/LunarConfig). If you have any questions, need any help, encounter any issues, or even have any suggestions, don't hesitate to reach out using one of those!

## Upcoming?

Hopefully as I have time I intend to add some of the following:

* Relying on primarily DawnLib as opposed to 4 different libraries.
* CSync integration, so host configuration is used by all clients automatically.
* Cleaner and more optimized code (please don't look at my mess now).
* Injection and modification of any setting based on the current moon, LLL tags, current interior, or current weather.

## Credits

* The Lethal Company Modding Discord, essentially enabling me to make this mod, as I likely couldn't have figured it out without some of the people there.
* LLL, LL, and CRLib, for the most part having easy to access systems regarding objects (and of course making this mod possible in the first place by having custom content to configure).
