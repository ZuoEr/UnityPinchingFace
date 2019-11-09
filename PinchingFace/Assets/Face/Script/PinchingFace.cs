using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PinchingFace : MonoBehaviour
{
    public SkinnedMeshRenderer mesh;


    JObject _formula;

    Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();

    Dictionary<string, Vector3> _boneBasePosition = new Dictionary<string, Vector3>();
    Dictionary<string, Vector3> _boneBaseRotation = new Dictionary<string, Vector3>();
    Dictionary<string, Vector3> _boneBaseScale = new Dictionary<string, Vector3>();

    Dictionary<string, Vector3> _boneUpdatePosition = new Dictionary<string, Vector3>();
    Dictionary<string, Vector3> _boneUpdateRotation = new Dictionary<string, Vector3>();
    Dictionary<string, Vector3> _boneUpdateScale = new Dictionary<string, Vector3>();

    Dictionary<string, KeyValuePair<JObject, float>> _controlLevel2 = new Dictionary<string, KeyValuePair<JObject, float>>();



    public void SetFormula(JObject formula)
    {
        _formula = formula;
    }

    public void setControlLevel2(string name, float value)
    {
        var data = _formula[name];
        var min = data.Value<float>("min");
        var max = data.Value<float>("max");
        var controlData = data.Value<JObject>("control");
        if (value < min)
        {
            value = min;
        }
        else if (value > max)
        {
            value = max;
        }
        if (_controlLevel2.ContainsKey(name))
        {
            _controlLevel2[name] = new KeyValuePair<JObject, float>(controlData, value);
        }
        else
        {
            _controlLevel2.Add(name, new KeyValuePair<JObject, float>(controlData, value));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBoneValue();
    }

    void InitBoneValue()
    {
        foreach (var boneNode in mesh.bones)
        {
            _bones.Add(boneNode.name, boneNode);
            _boneBasePosition.Add(boneNode.name, boneNode.transform.localPosition);
            _boneBaseRotation.Add(boneNode.name, boneNode.transform.localEulerAngles);
            _boneBaseScale.Add(boneNode.name, boneNode.transform.localScale);
        }
    }

    void Update()
    {
        InitUpdateValue();
        UpdateControlLevel2();
    }

    void InitUpdateValue()
    {
        _boneUpdatePosition.Clear();
        _boneUpdateRotation.Clear();
        _boneUpdateScale.Clear();
        foreach (var pair in _boneBasePosition)
        {
            _boneUpdatePosition.Add(pair.Key, new Vector3());
            _boneUpdateRotation.Add(pair.Key, new Vector3());
            _boneUpdateScale.Add(pair.Key, new Vector3());
        }
    }

    void UpdateControlLevel2()
    {
        foreach (var controlLevel1 in _controlLevel2)
        {
            var controlLevel1Data = controlLevel1.Value.Key;
            float controlLevel1Value = controlLevel1.Value.Value;
            UpdateControlLevel1(controlLevel1Data, controlLevel1Value);
        }
    }

    void UpdateControlLevel1(JObject controlLevel1s, float v)
    {
        foreach (var pair in controlLevel1s)
        {
            var controlLevelValue = v;
            var controlLevel = pair.Value;
            var min = controlLevel.Value<float>("min");
            var max = controlLevel.Value<float>("max");
            if (controlLevelValue < min)
            {
                controlLevelValue = min;
            }
            else if (controlLevelValue > max)
            {
                controlLevelValue = max;
            }
            controlLevelValue = Math.Abs(controlLevelValue);

            if (controlLevelValue != 0)
            {
                var bones = controlLevel.Value<JObject>("bone");
                if (bones != null)
                {
                    UpdateBones(bones, controlLevelValue);
                }
                var morphs = controlLevel.Value<JObject>("morph");
                if (morphs != null)
                {
                    UpdateMorphs(morphs, controlLevelValue);
                }
            }
        }
    }

    void UpdateBones(JObject bones, float value)
    {
        foreach (var bone in bones)
        {
            var trans = bone.Value["trans"];
            var rotate = bone.Value["rotate"];
            var scale = bone.Value["scale"];
            if (trans != null)
            {
                var x = trans.Value<float>("x") / 100 * value;
                var y = trans.Value<float>("y") / 100 * value;
                var z = trans.Value<float>("z") / 100 * value;
                if (_boneUpdatePosition.ContainsKey(bone.Key))
                {
                    _boneUpdatePosition[bone.Key] += new Vector3(x, y, -z);
                }
            }
            if (rotate != null)
            {
                var x = rotate.Value<float>("x") * value;
                var y = rotate.Value<float>("y") * value;
                var z = rotate.Value<float>("z") * value;
                if (_boneUpdateRotation.ContainsKey(bone.Key))
                {
                    _boneUpdateRotation[bone.Key] += new Vector3(-z, -y, x);
                }
            }
            if (scale != null)
            {
                var x = scale.Value<float>("x") * value;
                var y = scale.Value<float>("y") * value;
                var z = scale.Value<float>("z") * value;
                if (_boneUpdateScale.ContainsKey(bone.Key))
                {
                    _boneUpdateScale[bone.Key] += new Vector3(x, y, z);
                }
            }
        }
    }

    void UpdateMorphs(JObject morphs, float value)
    {
        foreach (var pair in morphs)
        {
            string morphName = pair.Key;
            float morphCoefficient = pair.Value.ToObject<float>();
            var index = mesh.sharedMesh.GetBlendShapeIndex(morphName);
            mesh.SetBlendShapeWeight(index, morphCoefficient * value * 100);
        }
    }

    void LateUpdate()
    {
        // bone
        LateUpdateBone();
    }

    void LateUpdateBone()
    {
        foreach (var pair in _boneUpdatePosition)
        {
            _bones[pair.Key].localPosition = _boneBasePosition[pair.Key] + pair.Value;
        }
        foreach (var pair in _boneUpdateRotation)
        {
            _bones[pair.Key].localRotation = Quaternion.Euler(_boneBaseRotation[pair.Key] + pair.Value);
        }
        foreach (var pair in _boneUpdateScale)
        {
            _bones[pair.Key].localScale = _boneBaseScale[pair.Key] + pair.Value;
        }
    }

}