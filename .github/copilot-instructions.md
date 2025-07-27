# GitHub Copilot Instructions for RimWorld Mod: MarkThatPawn

## Mod Overview and Purpose

**MarkThatPawn** is a RimWorld mod developed to enhance gameplay by allowing players to apply visual markers and icons to pawns. These markers help in managing pawns by categorizing them based on various parameters such as health, skills, equipment, and roles within the community. The mod aims to provide a streamlined way for players to quickly assess and organize their pawns during gameplay.

## Key Features and Systems

- **Marker Rules:** The core of the mod is the extensive `MarkerRule` system, which allows for dynamic and static rules to define how pawns receive markers. Different rule classes such as `AgeMarkerRule`, `TraitMarkerRule`, and `WeaponMarkerRule` extend the functionalities to various aspects of pawn properties.
- **Gizmo Integration:** Additional gizmos are added to pawns, enabling quick access to marking functionalities directly within the game interface.
- **Global Marking Tracker:** A game component that manages markers globally, ensuring persistence and consistency across game sessions.
- **Dialog Interfaces:** Custom dialog windows for rule management, ensuring easy setup and configuration of auto-marking rules.

## Coding Patterns and Conventions

- **Static Classes:** Utilizes static classes like `Corpse_TickRare` and `GenSpawn_Spawn` to encapsulate functionality that doesn't require object instantiation.
- **Inheritance and Polymorphism:** The `MarkerRule` class hierarchy demonstrates the use of inheritance to create flexible and extendable marker rules. New marker types can be added by extending from `MarkerRule`.
- **Method Naming:** Follows C# conventions for method naming, using PascalCase for public methods and camelCase for private methods (e.g., `showAnimalSelectorMenu()`).

## XML Integration

- **XML Definitions:** The mod likely integrates XML files for defining markers and textures, though specific XML files are not provided here. These XML files define properties and paths for assets loaded at runtime.
- **Data-driven Development:** XML data allows modders to configure and extend marker settings without recompiling the code, providing flexibility for user customization.

## Harmony Patching

- **Seamless Integration:** The mod integrates with RimWorld’s existing codebase using Harmony patches. This technique allows the modification of game behaviors without altering the original game files, facilitating compatibility with other mods.
- **Patching Strategy:** Static classes like `HediffSet_DirtyCache` and `Pawn_Kill` may use Harmony to modify specific methods in the game's logic, enabling enhanced marker behavior in diverse gameplay situations.

## Suggestions for Copilot

- **Marker Rule Extension:** Suggest code patterns for adding new `MarkerRule` derivatives, integrating additional conditions or parameters as needed.
- **Gizmo Enhancements:** Provide templates for creating new gizmo actions that interact with the marker system, offering more in-game control to players.
- **Harmony Patch Examples:** Offer examples for patching methods that are common points of integration in RimWorld, like pawn interaction methods (`Pawn_GetGizmos`, `Pawn_Kill`).
- **Optimization Tips:** Recommend best practices for optimizing performance when using global components like `GlobalMarkingTracker` to minimize impact on game performance.

By following these instructions, developers and contributors can efficiently collaborate on the MarkThatPawn mod, ensuring consistent quality and functionality across updates.
