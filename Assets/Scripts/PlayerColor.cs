using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    public int colorNum;
    
    private Color[] colors = {
        GetColorFromHex("#B22222"),
        GetColorFromHex("#36C336"),
        GetColorFromHex("#2B2BCC"),
        GetColorFromHex("#E0C41C"),
        GetColorFromHex("#CF2090")
    };
    
    private static Color GetColorFromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.white; // fallback color
    }
    
    public Color GetColor() { return colors[colorNum]; }
    public Color GetColor(ulong id) { return colors[id]; }
    void Start() { }

}
