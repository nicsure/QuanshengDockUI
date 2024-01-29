using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class LCD
{
    private static readonly Font font = Font.CreateDynamicFontFromOSFont("Monospace", 12);
    private static readonly List<GameObject>[] lines = { new(), new(), new(), new(), new(), new(), new(), new() };
    private static readonly Vector2 stretched = new(1.23f, 1f);
    private static readonly List<GameObject> deletion = new();
    public static RawImage LcdImage { get; set; }
    public static Image LcdBorder { get; set; }

    public static void ClearLines(int from, int to)
    {
        for (int i = from; i <= to; i++)
        {
            foreach(GameObject go in lines[i]) 
                deletion.Add(go);
            lines[i].Clear();
        }
    }

    public static void StatusDrawn() 
    {
        Delete();
    }

    private static void Delete()
    {
        foreach (GameObject go in deletion)
            Object.Destroy(go);
        deletion.Clear();
    }


    public static void DrawText(double x, double line, double height, string text, bool bold = false, bool stretch = false)
    {
        double em = 60 * height;
        int l = (int)line;
        if (l < 8)
        {
            GameObject go = new("TempLCD");
            Text test = go.AddComponent<Text>();
            go.layer = l;
            test.text = text;
            test.color = Save.LcdForeground;
            test.fontSize = (int)em;
            test.font = font;
            test.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            go.transform.SetParent(LcdImage.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            float left = (float)(x / 128), bott = 1f - (l * 0.125f);
            rect.anchorMin = new Vector2(left, bott-1f);  // Lower-left corner
            rect.anchorMax = new Vector2(1f+left, bott);  // Upper-right corner
            rect.pivot = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.localScale = stretch ? stretched : Vector2.one;
            lines[l].Add(go);
        }
        if (l > 0)
            Delete();
    }

    public static void DrawSignal(int slevel, int over)
    {
        string sig = new('|', (slevel + over)*3);
        DrawText(35, 4, 1, sig, false, false);
    }

}
