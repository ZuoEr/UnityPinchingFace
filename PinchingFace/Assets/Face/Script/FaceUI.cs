using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FaceUI : MonoBehaviour
{
    public Transform content;
    public Transform itemPrefab;
    public PinchingFace pinchingFace;

    Dictionary<string, string> _controlNames = new Dictionary<string, string>();


    void Start()
    {
        JObject faceFormula = JObject.Parse(File.ReadAllText("Assets/Face/Res/faceFormula.json", Encoding.UTF8));
        JObject controlNames = JObject.Parse(File.ReadAllText("Assets/Face/Res/controlNames.json", Encoding.UTF8));
        InitControlNames(controlNames);

        pinchingFace.SetFormula(faceFormula);

        var i = 0;
        foreach (KeyValuePair<string, JToken> pair in faceFormula)
        {
            var child = GameObject.Instantiate<Transform>(itemPrefab);
            child.SetParent(content);

            InitItem(child, pair.Key);

            i++;
        }
    }

    void InitControlNames(JObject controlNames)
    {
        foreach (var pair in controlNames)
        {
            _controlNames.Add(pair.Value["controller"].ToObject<string>(), pair.Value["name"].ToObject<string>());
        }
    }

    void InitItem(Transform item, string controlName)
    {
        Text text = item.GetChild(0).GetComponent<Text>();
        Slider slider = item.GetChild(1).GetComponent<Slider>();
        string name = _controlNames[controlName];

        text.text = name;
        slider.onValueChanged.AddListener(delegate
        {
            pinchingFace.setControlLevel2(controlName, slider.value);
        });
    }
}