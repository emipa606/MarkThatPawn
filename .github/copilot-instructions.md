# GitHub Copilot Instructions for RimWorld Mod: "Mark That Pawn"

---

### Mod Overview and Purpose

**Mark That Pawn** is a RimWorld mod that empowers players with the ability to mark pawns using a variety of icons. This enhances your control over pawns in chaotic situations, such as large raids, by allowing you to identify, track, and manage specific pawns of interest. Say goodbye to losing track of that valuable pawn amidst the battlefield frenzy!

### Key Features and Systems

- **Icon Customization**: Modify the size and position of icons for optimal on-screen visibility.
- **Dynamic Icon Sets**: Choose from different icon sets based on pawn type, and further customize icon sets per pawn.
- **Priority and Event-Based Rules**: Automatically mark pawns based on a comprehensive list of criteria (weapons, apparel, skills, traits, etc.), and adjust markings based on events like drafting or downed status.
- **Advanced Visualization**: Enable icon pulsing, icon rotation among applicable choices, and scalable icon sizes depending on zoom level.
- **Integration Enhancements**:
  - **XML Definitions**: Add more icon sets easily with minimal XML definitions.
  - **Harmony**: Utilize Harmony for seamless patching and mod integration.
  - **External Mod Compatibility**: Support for external mods like `TD Find Lib` for enhanced rule-defining capabilities and `CAI 5000` for fog of war.

### Coding Patterns and Conventions

- **Static Classes for Utility Methods**: Most utilities and helpers are encapsulated in static classes to ensure that no instances are accidentally created.
- **MarkerRule-based System**: Use a marker rule-based system (`MarkerRule` class as an abstract base) to define criteria for marking pawns, promoting extensibility and uniform behavior.
- **Naming Conventions**: Follow C# naming conventions:
  - **PascalCase** for class and method names
  - **camelCase** for private variables and method parameters.

### XML Integration

- XML is used primarily for defining icon sets and their attributes. Adding new icons is straightforward and involves editing or adding to XML definition files.
- Ensure all XML definitions are validated to prevent runtime errors.

### Harmony Patching

- Harmony is leveraged to patch methods where native game behavior needs to be modified or extended.
- Each patched method should be well-documented, including the reason for patching and expected changes.
- Preserve original game mechanics unless a conscious decision is made to alter them for improved mod functionality.

### Suggestions for Copilot

- **Automated Method Stubs**: Use Copilot to generate stubs for new methods, especially when dealing with repetitive rule-based logic within the `MarkerRule` subclasses.
- **XML Template Assistance**: Copilot can assist in auto-completing XML templates and configurations when defining new icon sets, reducing manual input errors.
- **Harmony Patch Automation**: Let Copilot generate framework boilerplate code needed for creating new Harmony patches.
- **UI Logic Prototypes**: Aid in drafting UI elements and associated logic in `Pawn_GetGizmos` and other related interfacing methods.
  
For further enhancements or if you have suggestions for new features or additional rules, please reach out via our dedicated Discord channel or as a comment within the project repository.

---------

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
