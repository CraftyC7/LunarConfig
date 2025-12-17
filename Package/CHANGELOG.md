## Version 0.2.1

Fixes
- Fixed almost anything breaking if you had invalid names in the scrap, enemy, or dungeon fields on moon settings.
- Fixed some issues around item configs not working if you didn't enable node text editing.

# Version 0.2.0 - DAWNLIB PORT

Features
- LunarConfig no longer requires any of it's former dependencies, only DawnLib (and BepinEx I guess).
- Cleaner code (keyword '-er').
- DawnLib content, and non-library affiliated content can now be configured with LunarConfig.
- Aliases used to refer to anything in LunarConfig can now be changed.
- Enemies can now have their bestiary text and keywords configured.
- Items can now be configured to be sold in the shop, and have the text of shop-related nodes configured.
- Some more map object settings.
- Map object curves are now in LunarConfigMoons.
- Map objects can now spawn where they shouldn't, however map objects that do not have a NetworkObject (usually the vanilla outside objects) are not able to spawn inside.
- Tags are now based on DawnLib, not LLL (might cause some things not to work while libraries port).
- As config files changed, a way was added to port old configurations, SEE README BEFORE ATTEMPTING (this also removed porting from CentralConfig).
- Added some notices to config fields that require other mods to work.

Fixes
- Mostly just issues caused by Lunar not being DawnLib-related.

## Version 0.1.13

Fixes
- Fixed an issue that breaks curve configurations on cultures that use commas as decimal points.

## Version 0.1.12

Fixes
- Fixed ANOTHER issue where dungeon configuration just wouldn't work.

## Version 0.1.11

Fixes
- Fixed an issue where dungeon configuration just wouldn't work.

## Version 0.1.10

Fixes
- Fixed an issue where disabling configuring moons would softlock the game.

## Version 0.1.9

Fixes
- Fixed an issue where changing the interior multiplier would cause desyncs in interiors. (Finally!)

## Version 0.1.8

Features
- Added various settings around scan nodes for items and enemies.

Fixes
- Fixed an issue where configuration would not generate if an enemy did not have an EnemyAI.

## Version 0.1.7

Features
- Added group spawn count value for enemies.

Fixes
- Fixed the changelog.
- Fixed an issue where trying to configure advanced dungeon properties would not work.

## Version 0.1.6

Fixes
- Moved the 'warning' to the correct setting after some further issue diagnosing.

## Version 0.1.5

Features
- Added more advanced dungeon configuration options.

Fixes
- Added a warning to configuring dungeon types due to reported desyncs (will fix whenever I find out the issue).

## Version 0.1.4

Fixes
- Fixed dungeon configuration not recognizing the internal name of a dungeon flow.

## Version 0.1.3

Features
- Added an option to initialize later than usual, allowing Lunar Config to port settings from CentralConfig
- Added an option to clear orphaned config entries

## Version 0.1.2

Features
- Added credits worth value for shop items
- Added normalized time to leave for enemies

Fixes
- Fixed an error that would occur if a moon had certain characters in it's name

## Version 0.1.1

- Removed accidental dependency on LobbyCompatability (oops)

## Version 0.1.0

- Initial Release