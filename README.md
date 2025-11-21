# Navigation Computer

A bespoke version of [Navigation Computer](https://github.com/BattletechModders/NavigationComputer) for BattleTech Extended Tactics.

## What's Changed

### ✨ New Map Modes

* **Factory Systems (F3):** Highlights all factory systems on the map.
  * Hovering over a factory shows a comprehensive list of all 'Mechs sold there.
* **Black Market Zones (F4):** Highlights all pirate and criminal zones of influence.
* **Active Flashpoints (F5):** Highlights systems with active flashpoints.

### 🎨 Improved Map Visuals

* New visual style for better clarity and aesthetics:
  * Abandoned systems are now clearly and visually distinct from inhabited ones.
  * Inhabited systems now shine with a brightness relative to their population size.
  * Star clusters are made larger and dimmer to highlight the stars they contain.

> [!NOTE]  
> By default, the game differentiates **visited and unvisited** systems. This version changes that to differentiate **inhabited and abandoned** systems instead, while keeping a visual cue (outer ring) for visited systems.

* New store icons for Wolf's Dragoons and the Outworld Alliance.

### ⬆️ Improved System Search

* The system search bar now supports multi-word and partial matches for more flexible searching.
* A new hotkey (CTRL+C) helps you quickly locate systems where ComStar forces may appear as OPFOR.

### 🛠️ Indicator Filters

* A new dropdown menu on the navigation screen allows you to toggle various system indicators.
* All filter menus are hidden by default to reduce clutter, but can be expanded by clicking the info button in the top-right corner of the screen.

## Usage

### Installation

Download the [latest release](https://github.com/AkiraBrahe/NavigationComputer/releases/latest) of the mod and unpack it into your `Battletech\Mods` folder after installing BattleTech Extended Tactics.

### Hotkeys

While on the navigation screen:

* **F1**: Toggle **Unvisited Systems** map mode.
* **F2**: Toggle **System Difficulty** map mode.
* **F3**: Toggle **Factory Systems** map mode.
* **F4**: Toggle **Black Market Zones** map mode.
* **F5**: Toggle **Active Flashpoints** map mode.
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

![Factory Systems](Screenshots/factorySystems.png?raw=true "Factory Systems")
![Black Market Zones](Screenshots/blackMarketZones.png?raw=true "Black Market Zones")
