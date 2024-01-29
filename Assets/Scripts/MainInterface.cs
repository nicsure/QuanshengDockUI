using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainInterface : MonoBehaviour
{
    public static Canvas Canvas { get; private set; }

    public Button settingsButton;
    public RawImage lcd;
    public Canvas canvas;
    public Image lcdBorder;

    void Start()
    {
        Canvas = canvas;
        settingsButton.onClick.AddListener(new(SettingsClick));
        LCD.LcdImage = lcd;
        LCD.LcdBorder = lcdBorder;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child.TryGetComponent<Button>(out var button))
            {
                button.AddComponent<ButtonHandler>().Init(button, OnPointerDown, OnPointerUp);
            }
        }
    }

    private static int ButtonValue(Button b)
    {
        if (!b.name.StartsWith("But")) return -1;
        string s = b.name[3..];
        return int.TryParse(s, out int i) ? i : -1;
    }

    public void OnPointerDown(Button b)
    {
        int key = ButtonValue(b);
        switch (key) 
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
                Serial.SendCommand(Packet.KeyPress, (ushort)key);
                break;
            default:
                break;
        }
    }

    public void OnPointerUp(Button b)
    {
        int key = ButtonValue(b);
        switch (key)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
                Serial.SendCommand(Packet.KeyPress, (ushort)19);
                break;
            default:
                break;
        }
    }

    private void SettingsClick()
    {
        canvas.enabled = false;
        SettingsInterface.Canvas.enabled = true;
    }

}

public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private Action<Button> Down, Up;

    public void Init(Button button, Action<Button> down, Action<Button> up)
    {
        this.button = button;
        Up = up;
        Down = down;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Down(button);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Up(button);
    }
}

