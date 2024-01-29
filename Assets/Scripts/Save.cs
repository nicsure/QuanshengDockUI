using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Save
{

    public static Color LcdBackground
    {
        get => lcdBackground;
        set
        {
            lcdBackground = value;
            PlayerPrefs.SetFloat("lcdbackr", value.r);
            PlayerPrefs.SetFloat("lcdbackg", value.g);
            PlayerPrefs.SetFloat("lcdbackb", value.b);
            PlayerPrefs.SetFloat("lcdbacka", value.a);
        }
    }
    private static Color lcdBackground = new(
        PlayerPrefs.GetFloat("lcdbackr", 0f),
        PlayerPrefs.GetFloat("lcdbackg", 0f),
        PlayerPrefs.GetFloat("lcdbackb", 0f),
        PlayerPrefs.GetFloat("lcdbacka", 1f));

    public static Color LcdForeground
    {
        get => lcdForeground;
        set
        {
            lcdForeground = value;
            PlayerPrefs.SetFloat("lcdforer", value.r);
            PlayerPrefs.SetFloat("lcdforeg", value.g);
            PlayerPrefs.SetFloat("lcdforeb", value.b);
            PlayerPrefs.SetFloat("lcdforea", value.a);
        }
    }
    private static Color lcdForeground = new(
        PlayerPrefs.GetFloat("lcdforer", 0f),
        PlayerPrefs.GetFloat("lcdforeg", 1f),
        PlayerPrefs.GetFloat("lcdforeb", 1f),
        PlayerPrefs.GetFloat("lcdforea", 1f));

}
