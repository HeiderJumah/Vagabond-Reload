using System.Collections.Generic;
using UnityEngine;

public static class LevelState
{
    // Track if the boss has been defeated per level
    private static Dictionary<string, bool> bossDefeated = new Dictionary<string, bool>();

    private static Dictionary<string, bool> bossActive = new Dictionary<string, bool>();

    public static bool IsBossDefeated(string levelName)
    {
        return bossDefeated.ContainsKey(levelName) && bossDefeated[levelName];
    }

    public static void SetBossDefeated(string levelName)
    {
        bossDefeated[levelName] = true;
    }

    public static bool IsBossActive(string levelName)
    {
        return bossActive.ContainsKey(levelName) && bossActive[levelName];
    }

    public static void SetBossActive(string levelName, bool active)
    {
        bossActive[levelName] = active;
        Debug.Log("LevelState SetBossActive: " + levelName + " = " + active);
    }
}
