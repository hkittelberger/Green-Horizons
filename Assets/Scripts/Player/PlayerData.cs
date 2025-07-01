using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public Color playerColor;

    // Resource counts
    public int water;
    public int alloy;
    public int oil;
    public int food;
    public int brick;

    public PlayerData(Color color)
    {
        playerColor = color;

        // Initialize default resources (customize as needed)
        water = 0;
        alloy = 0;
        oil = 0;
        food = 0;
        brick = 0;
    }
}
