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
    private Dictionary<KeyCode, int> shortcuts = new () 
    {
        { KeyCode.Space, 16 },
        { KeyCode.Escape, 13 },
        { KeyCode.Backspace, 13 },
        { KeyCode.Delete, 13 },
        { KeyCode.UpArrow, 11 },
        { KeyCode.DownArrow, 12 },
        { KeyCode.Menu, 10 },
        { KeyCode.Return, 10 },
        { KeyCode.KeypadEnter, 10 },
        { KeyCode.Tab, 18 },
        { KeyCode.F, 15 },
        { KeyCode.Alpha0, 0 },
        { KeyCode.Alpha1, 1 },
        { KeyCode.Alpha2, 2 },
        { KeyCode.Alpha3, 3 },
        { KeyCode.Alpha4, 4 },
        { KeyCode.Alpha5, 5 },
        { KeyCode.Alpha6, 6 },
        { KeyCode.Alpha7, 7 },
        { KeyCode.Alpha8, 8 },
        { KeyCode.Alpha9, 9 },
    };
    private bool txLocked = true;

    public static Canvas Canvas { get; private set; }

    public Button settingsButton;
    public RawImage lcd;
    public Canvas canvas;
    public Image lcdBorder;
    public Text txLock;

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

    void Update()
    {
        if (canvas.enabled)
        {
            foreach(var key in shortcuts.Keys)
            {
                if (Input.GetKeyDown(key))
                    FunctionDown(shortcuts[key]);
                if (Input.GetKeyUp(key))
                    FunctionUp(shortcuts[key]);
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
        FunctionDown(key);
    }

    public void OnPointerUp(Button b)
    {
        int key = ButtonValue(b);
        FunctionUp(key);
    }

    public void FunctionDown(int key)
    {
        switch (key) 
        {
            case 20:
                txLocked = !txLocked;
                txLock.text = txLocked ? "TX\r\nLOCK" : "TX\r\nOK";
                break;
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
            case 17:
            case 18:
                Serial.SendCommand(Packet.KeyPress, (ushort)key);
                break;
            case 16:
                if(!txLocked)
                    Serial.SendCommand(Packet.KeyPress, (ushort)16);
                break;
            default:
                break;
        }
    }

    public void FunctionUp(int key)
    {
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

