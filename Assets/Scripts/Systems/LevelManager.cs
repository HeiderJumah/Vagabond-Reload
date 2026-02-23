using UnityEngine;

[CreateAssetMenu(menuName = "Vagabond/LevelManager")]
public class LevelManager : ScriptableObject
{
    // static to hold current level connetion
    public static LevelManager LevelConnection { get; set; }    
}