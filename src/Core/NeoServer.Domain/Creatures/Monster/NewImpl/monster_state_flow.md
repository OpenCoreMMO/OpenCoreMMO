# Monster State Flow (Tibia-inspired)

## 1. Idle State
* **Condition:** No targets around.
* **Action:** Stops all monster actions -> Go to sleep.
---

## 2. Target Detection
### 2.1 Visible & Reachable Targets
* **Condition:** Target is visible and reachable.
* **Action:** Attack nearest enemy.
### 2.2 Visible but Unreachable Targets
* **Case A:** Monster has distance attack
    * Always set the detected target as enemy
    * If target is **in range, unreachable, and no clear sight**: Attempt to move to a position with a clear sight to the target. If no such position is available, look for a new enemy but keep the target.
* **Case B:** Monster does not have distance attack
    * Always set the detected target as enemy
    * If target is **unreachable** or **no clear sight*s*: Set target as enemy, but look for a new reachable enemy.
---
## 3. Invisible Targets
* **If monster can see invisible targets:**
    * Apply same flow as in **Visible Targets**.
* **If monster cannot see invisible targets:**
    * Look for a visible enemy. DO NOT set the invisible target as a enemy
---
## 4. Combat Behavior
* **While monster has a reachable target:**
    * If melee: Follow target.
    * If ranged: Keep distance and attack.
* **Summoning:** Summons can be created as long as there is a detected target around.
* **When monster is low health:**
    * Escape.
---
## 5. Target Detection Logic

This section outlines the process by which a monster identifies and selects a target to engage in combat. The primary goal is to select the most strategically advantageous target based on a set of prioritized criteria.

### 5.1. Creature Detection

The first step is to identify all potential targets within the monster's field of view.

*   **Field of View:** The monster scans its surroundings for creatures.
*   **Valid Targets:** Only players and their summons are considered valid targets.
*   **Invisibility:** If a monster cannot see invisible creatures, such creatures are excluded from the list of potential targets.

### 5.2. Target Prioritization

Once a list of potential targets is compiled, each target is assigned a priority based on the following criteria, in descending order of importance:

1.  **Reachable:** A target that the monster can pathfind to.
2.  **Clear Sight:** A target for which there is an unobstructed line of sight for ranged attacks.
3.  **Proximity:** The distance between the monster and the target.

This prioritization ensures that the monster engages with targets it can actually fight, rather than being stuck on a target it cannot reach.

**Example:**

*   **Creature A:** Closer to the monster but unreachable.
*   **Creature B:** Farther from the monster but reachable.

In this scenario, **Creature B** will be prioritized over **Creature A**.

### Edge Case
* When the monster has "canpushcreatures" attribute it will be capable to reach the target when it's path is blocked by a monster with "canpushcreature = false". So the logic must consider the path as reachable but this scenario has lower priority.

### 5.3. Flow

The target selection process follows these steps:

1.  **Get Potential Targets:** Compile a list of all creatures in the monster's view, adhering to the Creature Detection Logic.
2.  **Prioritize Targets:** For each potential target, assign a priority based on the Target Prioritization rules.
3.  **Select Target:** The creature with the highest priority value is selected as the target.
4.  **Return Target:** The selected target is returned, or `null` if no valid targets are found.

### 5.4. Special Cases

*   **Multiple Valid Targets:** If multiple targets have the same priority, the closest one will be selected.

### 5.5. Optimization

The target selection algorithm can be computationally expensive, especially with a large number of creatures. To optimize this process:

*   **Sort by Distance:** Order the creatures by their distance from the monster.
*   **Iterative Checks:** Iterate through the sorted list, checking for clear sight and path reachability. This avoids unnecessary checks on distant or obstructed targets.

---

### 6. Glossary

*   **Clear Sight:** No obstacles (e.g., walls, trees) are present that would block a ranged attack.
*   **Reachable:** A valid path exists between the monster and the target.