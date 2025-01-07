using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityVolumeRendering;
using static VolumeSolution;

public class SlidersControl : MonoBehaviour
{

    public float _minHu { get; set; }
    public float _maxHu { get; set; }

    //public VolumeRenderedObject _volumerenderedobject { get; set; }
    [SerializeField] VolumeRenderedObject _volumerenderedobject;

    // sliders
    [SerializeField] List<SliderData> _sliders;
    [SerializeField] List<DensitySlider> _densitySlider;

    List<float> _initialSliderValues = new List<float>();

    [Serializable]
    public struct SliderData
    {
        public Slider _slider;
        public TMP_Text _huValueText;
    }

    [Serializable]
    public struct DensitySlider
    {
        public Slider _slider;
        public TMP_Text _huValueText;
    }

    public void SetVolumeRenderedObject(VolumeRenderedObject vro)
    {
        _volumerenderedobject = vro;
    }

    public static int GetHUFromFloat(float value, float minHu, float maxHu)       //Getting HU value from float normalized value 
    {
        float huRange = maxHu - minHu;
        int huValue = (int)(minHu + (value * huRange));
        return huValue;
    }

    static float inverse_lerp(float value, float min, float max) // 0 - 1
    {
        float val = (value - min) / (max - min);
        return val;
    }

    public void saveTF()
    {
        string filepath = "Q:/dev/hololens/ARdno_FNO/Assets/Resources/defaultSeg.txt";
        TransferFunctionDatabase.SaveTransferFunction(_volumerenderedobject.transferFunction, filepath);
    }

    public void ResetTF()
    {
        for (int i = 0; i < _sliders.Count; i++)
        {
            TFColourControlPoint point = _volumerenderedobject.transferFunction.colourControlPoints[i];
            point.dataValue = _initialSliderValues[i];
            _volumerenderedobject.transferFunction.colourControlPoints[i] = point;
        }
        _volumerenderedobject.transferFunction.GenerateTexture();
        for (int i = 0; i < _sliders.Count; i++)
        {
            _sliders[i]._slider.Value = _initialSliderValues[i];
        }

        UpdateHuLabels();
    }

    public void UpdateHuLabels()
    {
        for (int i = 0; i < _sliders.Count; i++)
        {
            _sliders[i]._huValueText.text = $"{GetHUFromFloat(_sliders[i]._slider.Value, _minHu, _maxHu)}<br>HU";
        }


        _densitySlider[0]._huValueText.text = $"{GetHUFromFloat(_densitySlider[0]._slider.Value, _minHu, _maxHu)}<br>HU";
        _densitySlider[1]._huValueText.text = $"{GetHUFromFloat(_densitySlider[1]._slider.Value, _minHu, _maxHu)}<br>HU";

    }

    public void initSpecialSlider()
    {
        Debug.Log("COUNT: " + _volumerenderedobject.transferFunction.colourControlPoints.Count);

        for (int i = 0; i < _sliders.Count; i++)
        {
            _initialSliderValues.Add(_volumerenderedobject.transferFunction.colourControlPoints[i].dataValue);
            Debug.Log("slider: " + i + " value: " + _volumerenderedobject.transferFunction.colourControlPoints[i].dataValue);
        }
        ResetTF();

    }

    public void ResetDensitySlider()
    {
        float min = inverse_lerp(-400, _minHu, _maxHu);
        float max = inverse_lerp(1500, _minHu, _maxHu);

        _densitySlider[0]._slider.Value = min;
        _densitySlider[1]._slider.Value = max;
    }

    public void SliderUpdated(SliderEventData data)
    {
        if (_initialSliderValues.Count == 0) return;

        for (int i = 0; i < _sliders.Count; i++)
        {
            TFColourControlPoint point = _volumerenderedobject.transferFunction.colourControlPoints[i];
            point.dataValue = _sliders[i]._slider.Value;
            _volumerenderedobject.transferFunction.colourControlPoints[i] = point;
        }
        _volumerenderedobject.transferFunction.GenerateTexture();
        UpdateHuLabels();
    }

    public void initDensitySlider(float minHu, float maxHu)
    {
        Slider left = _densitySlider[0]._slider;
        Slider right = _densitySlider[1]._slider;

        float min = inverse_lerp(minHu, _minHu, _maxHu);
        float max = inverse_lerp(maxHu, _minHu, _maxHu);

        _volumerenderedobject.SetVisibilityWindow(min, max);

        _densitySlider[0]._slider.Value = min;
        _densitySlider[1]._slider.Value = max;


        //_volumerenderedobject.SetVisibilityWindow(leftHu, rightHu);
        _volumerenderedobject.SetVisibilityWindow(min, max);

    }

    public void DensitySliderUpdated(SliderEventData data)
    {
        Slider left = _densitySlider[0]._slider;
        Slider right = _densitySlider[1]._slider;

        if (left.Value >= right.Value)
        {
            float v = left.Value;
            _densitySlider[0]._slider.Value = right.Value;
            _densitySlider[1]._slider.Value = v;
        }


        _volumerenderedobject.SetVisibilityWindow(left.Value, right.Value);

        UpdateHuLabels();
    }

    public Action IntervalSliderValueChanged { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _minHu = 0;
        _maxHu = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
