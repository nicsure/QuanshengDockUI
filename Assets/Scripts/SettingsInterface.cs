using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsInterface : MonoBehaviour
{
    public static Canvas Canvas { get; private set; }

    public Canvas canvas;
    public Dropdown serialPortSelection;
    public Text SelectedPortName;
    public Dropdown monitorSelection;
    public Text monitorName;
    public Button OkayButton;
    public FlexibleColorPicker BackPicker, ForePicker;
    public AudioSource audio;

    void Start()
    {
        Canvas = canvas;
        canvas.enabled = false;
        serialPortSelection.options.Clear();
        monitorSelection.options.Clear();
        SelectedPortName.text = Serial.Port;
        monitorName.text = PlayerPrefs.GetString("monitor", "Disabled");
        int cnt = 0;
        foreach (string s in Serial.Names)
        {
            if (Serial.Port.Length == 0) Serial.Port = s;
            serialPortSelection.options.Add(new(s));
            if(s.Equals(Serial.Port))
                serialPortSelection.value = cnt;
            cnt++;
        }
        cnt = 1;
        monitorSelection.options.Add(new("Disabled"));
        foreach(string s in Microphone.devices)
        {
            monitorSelection.options.Add(new(s));
            if(s.Equals(monitorName.text))
                monitorSelection.value = cnt;
            cnt++;
        }
        BackPicker.color = Save.LcdBackground;
        LCD.LcdImage.color = Save.LcdBackground;
        LCD.LcdBorder.color = Save.LcdBackground;
        ForePicker.color = Save.LcdForeground;
        serialPortSelection.onValueChanged.AddListener(OnSerialPortSelectionChanged);
        monitorSelection.onValueChanged.AddListener(OnMonitorSelectionChanged);
        OkayButton.onClick.AddListener(OnOkayClick);
        BackPicker.onColorChange.AddListener(OnBackChanged);
        ForePicker.onColorChange.AddListener(OnForeChanged);
        PlayAudio();
    }

    private void PlayAudio()
    {
        audio.Stop();
        if(!monitorName.text.Equals("Disabled"))
        {
            audio.loop = true;
            audio.bypassEffects = true;
            audio.bypassListenerEffects = true;
            audio.bypassReverbZones = true;
            audio.priority = 128;
            audio.clip = Microphone.Start(monitorName.text, true, 10, 22050);
            audio.loop = true;
            audio.bypassEffects = true;
            audio.bypassListenerEffects = true;
            audio.bypassReverbZones = true;
            audio.priority = 128;
            while (!(Microphone.GetPosition(null) > 1000)) { }
            audio.Play();
        }
    }

    public void OnBackChanged(Color c)
    {
        LCD.LcdImage.color = LCD.LcdBorder.color = c;
        Save.LcdBackground = c;
    }

    public void OnForeChanged(Color c)
    {
        Save.LcdForeground = c;
    }

    public void OnSerialPortSelectionChanged(int opt)
    {
        SelectedPortName.text = serialPortSelection.options[opt].text;
        Serial.Port = SelectedPortName.text;
    }

    public void OnMonitorSelectionChanged(int opt)
    {
        Microphone.End(monitorName.text);
        monitorName.text = monitorSelection.options[opt].text;
        PlayerPrefs.SetString("monitor", monitorName.text);
        PlayAudio();
    }

    public void OnOkayClick()
    {
        canvas.enabled = false;
        MainInterface.Canvas.enabled = true;
    }

}
