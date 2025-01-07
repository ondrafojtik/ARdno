using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerControls : MonoBehaviour
{

    [SerializeField] MixedReality.Toolkit.UX.Slider _slider_r;
    [SerializeField] MixedReality.Toolkit.UX.Slider _slider_g;
    [SerializeField] MixedReality.Toolkit.UX.Slider _slider_b;

    [SerializeField] GameObject _buttonColorPlate;
    //[SerializeField] UnityVolumeRendering.TransferFunction _tf;

    public GameObject _colorPoint;

    public float _r = 0.0f;
    public float _g = 0.0f;
    public float _b = 0.0f;

    public bool changed = false;

    public void InitSliders()
    {
        if (_colorPoint == null)
            return;

        Color c = _colorPoint.GetComponent<MeshRenderer>().material.color;

        _slider_r.Value = c.r;
        _r = c.r;

        _slider_g.Value = c.g;
        _g = c.g;

        _slider_b.Value = c.b;
        _b = c.b;
    }

    private void UpdateButtonColorPlate()
    {
        if (_colorPoint == null)
            return;

        Color c = new Color(_r, _g, _b);
        //_buttonColorPlate.GetComponent<MeshRenderer>().material.color = c;
        _buttonColorPlate.GetComponent<Image>().color = c;

        _colorPoint.GetComponent<MeshRenderer>().material.color = c;

        changed = true;
        //_tf.GenerateTexture();
    }

    public void RSliderUpdated(SliderEventData data)
    {
        _r = _slider_r.Value;
        UpdateButtonColorPlate();
    }
    public void GSliderUpdated(SliderEventData data)
    {
        _g = _slider_g.Value;
        UpdateButtonColorPlate();
    }

    public void BSliderUpdated(SliderEventData data)
    {
        _b = _slider_b.Value;
        UpdateButtonColorPlate();
    }

    public Color GetColor()
    {
        //float r = _slider_r.Value;
        //float g = _slider_g.Value;
        //float b = _slider_b.Value;

        //Color c = new Color(r,g,b);

        Color c = new Color(_r, _g, _b);
        
        return c;
    }

    // Start is called before the first frame update
    void Start()
    {
        //_colorPoint = new GameObject();



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
