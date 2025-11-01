# Navigation Computer

BattleTech mod that adds several quality-of-life features to the navigation screen, including system search, map modes, and custom route planning.

## Features

1. **System Search:** Adds a powerful search function to find star systems by name, tags, or faction presence.
2. **Map Modes:** Adds several map modes to visualize information on the starmap, such as unvisited systems, system difficulty, and factory locations.
3. **Custom Routes:** Plan multi-stop routes by Shift-clicking systems, allowing you to build a path through several points of interest.

## Usage

### Hotkeys

While on the navigation screen:

* **F1**: Toggle **Unvisited Systems** map mode.
* **F2**: Toggle **System Difficulty** map mode.
* **F3**: Toggle **Factory Systems** map mode.
* **Ctrl+F**: Open system search.
* **Ctrl+C**: Search for systems with ComStar and/or former Star League presences.
* **Esc**: Exit current map mode or search.
* **Shift-Click** on a system: Add system to your current route.

### Custom Routes

Shift-clicking a system will extend your current travel route to that system. This is useful for planning routes through multiple systems you want to visit for contracts or shopping without having to plot each leg of the journey separately.

### Searching

The search function allows for complex queries to find exactly what you're looking for.

**Search Prefixes:**
<br>You can narrow your search by using prefixes. If no prefix is used, it will search system names, employers, and tags.
* `name:` - Search for a system by name (e.g., `name:detroit`).
* `tag:` - Search for a system by its tags (e.g., `tag:manufacturing`).
* `for:` or `employer:` - Search for factions offering contracts (e.g., `for:marik`).
* `against:` or `target:` - Search for factions targeted in contracts (e.g., `against:liao`).

**Query Logic:**
<br>Spaces are treated as part of the search term, allowing for multi-word searches like `free worlds league`.
* Use `&` to chain queries (AND logic). Both conditions must be true.
  *   Example: `for:marik & against:liao` finds systems with Marik contracts targeting Liao.
* Use `|` to create alternate conditions (OR logic). Either condition can be true.
  *   Example: `tag:rich | tag:manufacturing` finds systems that are rich OR have manufacturing.
* Use `-` to invert a query (NOT logic).
  *   Example: `for:marik & -tag:pirate` finds systems with Marik contracts that do NOT have a pirate presence.

## Screenshots

![Search Functionality](Screenshots/search.png?raw=true "Search Functionality")
![System Difficulty](Screenshots/systemDifficulty.png?raw=true "System Difficulty")
![Unvisited Systems](Screenshots/unvisitedSystems.png?raw=true "Unvisited Systems")
