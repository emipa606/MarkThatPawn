# GitHub Copilot Instructions for Mark That Pawn Mod

## Mod Overview and Purpose

**Mark That Pawn** is a RimWorld mod designed to enhance the player's ability to track specific pawns during chaotic events like large raids. By allowing players to mark pawns with icons, they can easily recognize and maintain watch over valuable or interesting individuals during battles. This prevents unfortunate scenarios, such as losing track of pawns and discovering them only after they have met an untimely end.

## Key Features and Systems

- **Icon Marking on Pawns**: Players can assign various icons to pawns for easy recognition.
- **Modifiable Icon Settings**: Users can change icon size, position, and the icon set used.
- **Responsive Icon Adaptation**: Icons can enlarge when zoomed out or change based on pawn conditions (e.g., status like drafted or downed).
- **Rule-Based Automatic Marking**: Define rules to automatically mark spawning pawns based on conditions such as weapon type, apparel, skill level, traits, etc.
- **Event Priority Rules**: Automatically alter markings based on events like a pawn being drafted, injured, or entering a mental state.
- **Integration with Other Mods**: Supports features like fog of war from CAI 5000 and badge imports from Pawn Badge.

## Coding Patterns and Conventions

- **Class Design**: Focus on static utility classes where functions involve operations without needing an instance, e.g., `Corpse_TickRare`, `GenSpawn_Spawn`.
- **MarkerRule Base Class**: Extensive use of an abstract class pattern (`MarkerRule`) for different marker rules which can be extended for specific attributes like `AgeMarkerRule`, `TraitMarkerRule`.
- **XML Loading**: `MarkerDef` class contains methods to handle XML-based resource loading, ensuring icon sets and rules can be easily modified and extended.

## XML Integration

- XML is used to configure icon sets and rules. Each icon set only requires a small XML definition to be selectable.
- Developers can extend XML configurations to include additional icon sets or marker rules by defining new XML elements as needed.

## Harmony Patching

- The mod makes extensive use of Harmony for altering game behavior without changing the original game files.
- Static classes like `PawnRenderer_RenderPawnAt` and `ThingWithComps_Tick` may employ Harmony patches to integrate mod functionalities smoothly with the game's rendering and update cycles.
  
## Suggestions for Copilot

1. **Class Initialization**: Ensure constructors for classes like `MarkThatPawnMod` and `MarkThatPawnSettings` are clear and set default configurations appropriately.

2. **Rule Management**: When extending `MarkerRule` classes (e.g., `GeneMarkerRule`, `FactionIconMarkerRule`), ensure to create intuitive private methods for selector menus, and handle rule serialization/deserialization accurately.

3. **User Interaction**: Leverage classes like `Dialog_AutoMarkingRules` to enhance user interactions with dialog windows for rule selections and preferences.

4. **XML Handling**: In `MarkerDef`, ensure the XML loading functions are robust, properly handling file paths and possible errors during file access.

5. **Compatibility Enhancements**: Consider cross-mod compatibility, especially with TD Find Lib, and ensure appropriate interfaces or class extensions exist (e.g., `TDFindLibRule`).

6. **Efficiency**: Optimize update methods like `Corpse_TickRare` and `ThingWithComps_Tick` to avoid unnecessary computations, especially in high tick-rate game environments.

By following these guidelines and patterns, new features can be integrated smoothly while maintaining the mod's performance and user experience.
