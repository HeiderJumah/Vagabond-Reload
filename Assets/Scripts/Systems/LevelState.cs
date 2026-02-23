using System.Collections.Generic;
using UnityEngine;

public static class LevelState
{
    // Track if the boss has been defeated per level
    private static Dictionary<string, bool> bossDefeated = new Dictionary<string, bool>();

    public static bool IsBossDefeated(string levelName)
    {
        return bossDefeated.ContainsKey(levelName) && bossDefeated[levelName];
    }

    public static void SetBossDefeated(string levelName)
    {
        bossDefeated[levelName] = true;
    }
}
