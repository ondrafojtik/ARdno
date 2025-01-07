using itk.simple;
using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.UX;
using openDicom.DataStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityVolumeRendering;

public class TFPlaneControls : MonoBehaviour
{

    [SerializeField] GameObject _TFPlane;
    [SerializeField] GameObject _colorPointPREFAB;
    [SerializeField] GameObject _alphaPointPREFAB;

    [SerializeField] GameObject _colorPickerPREFAB;

    [SerializeField] MixedReality.Toolkit.UX.Slider _slider_min;
    [SerializeField] MixedReality.Toolkit.UX.Slider _slider_max;

    [SerializeField] GameObject _dataset_label_min;
    [SerializeField] GameObject _dataset_label_max;

    [SerializeField] GameObject _current_label_min;
    [SerializeField] GameObject _current_label_max;

    [SerializeField] PressableButton _zoom_button;

    public List<GameObject> _colorPoints = new List<GameObject>();
    public List<GameObject> _alphaPoints = new List<GameObject>();
    public GameObject _anchorTFPlane;

    public List<GameObject> _colorPickers = new List<GameObject>();

    public VolumeRenderedObject _volumerenderedobject = new VolumeRenderedObject();

    public Vector2 _zoom_scope = new Vector2(0.0f, 100.0f); // referring to "from - to" 
    public float _slider_min_zoom = 0.0f;
    public float _slider_max_zoom = 0.0f;

    public bool _zoomed = false;

    static float inverse_lerp(float value, float min, float max) // 0 - 1
    {
        float val = (value - min) / (max - min);
        return val;
    }

    public static int GetHUFromFloat(float value, float minHu, float maxHu)       //Getting HU value from float normalized value 
    {
        float huRange = maxHu - minHu;
        int huValue = (int)(minHu + (value * huRange));
        return huValue;
    }


    public static float fGetHUFromFloat(float value, float minHu, float maxHu)       //Getting HU value from float normalized value 
    {
        // zaokrouhlit na 3desetiny
        
        float huRange = maxHu - minHu;
        float huValue = (minHu + (value * huRange));
        return huValue;
    }

    public void UpdateHUValues()
    {
        if (_colorPoints.Count == 0 || _alphaPoints.Count == 0)
            return;

        for (int i = 0; i < _colorPoints.Count; i++)
        {
            Vector3 anchor_pos_ = _anchorTFPlane.transform.localPosition;

            float x_pos_of_point = _colorPoints[i].transform.localPosition.x;

            float value = anchor_pos_.x - x_pos_of_point;
            value = value / 10;


            float minHu = _volumerenderedobject.dataset.GetMinDataValue();
            float maxHu = _volumerenderedobject.dataset.GetMaxDataValue();

            //var text_ = _colorPoints[i].GetNamedChild("Cube");
            //text_.GetComponentInChildren<TMP_Text>().text = GetHUFromFloat(value, minHu, maxHu).ToString();
            _colorPoints[i].GetComponentInChildren<TMP_Text>().text = GetHUFromFloat(value, minHu, maxHu).ToString();

            if (_zoomed)
            {
                // adjust HU values
                _colorPoints[i].GetComponentInChildren<TMP_Text>().text = GetHUFromFloat(value, _zoom_scope.x, _zoom_scope.y).ToString();
            }

        }

        for (int i = 0; i <_alphaPoints.Count; i++)
        {
            Vector3 anchor_pos_ = _anchorTFPlane.transform.localPosition;

            float x_pos_of_point = _alphaPoints[i].transform.localPosition.x; // dataValue
            float z_pos_of_point = _alphaPoints[i].transform.localPosition.z; // alphaValue

            float dataValue__ = anchor_pos_.x - x_pos_of_point;
            dataValue__ = dataValue__ / 10;

            float alphaValue__ = anchor_pos_.z - z_pos_of_point;
            alphaValue__ = alphaValue__ / 10;

            float minHu = _volumerenderedobject.dataset.GetMinDataValue();
            float maxHu = _volumerenderedobject.dataset.GetMaxDataValue();

            Debug.Log(dataValue__);

            string tt = fGetHUFromFloat(alphaValue__, 0, 1).ToString("#.000") + Environment.NewLine + GetHUFromFloat(dataValue__, minHu, maxHu);

            //_alphaPoints[i].GetComponentInChildren<TMP_Text>().text = fGetHUFromFloat(alphaValue__, 0, 1).ToString("#.000"); // ToString("F3")
            _alphaPoints[i].GetComponentInChildren<TMP_Text>().text = tt;

            if (_zoomed)
            {
                // adjust HU values
                string t = fGetHUFromFloat(alphaValue__, 0, 1).ToString("#.000") + Environment.NewLine + GetHUFromFloat(dataValue__, _zoom_scope.x, _zoom_scope.y);

                //_alphaPoints[i].GetComponentInChildren<TMP_Text>().text = f+FromFloat(alphaValue__, 0, 1).ToString("#.000"); // ToString("F3")
                _alphaPoints[i].GetComponentInChildren<TMP_Text>().text = t;
                
            }
        }

    }

    public void InitiateColorPoints()
    {
        // clear from prev object
        //_colorPoints.Clear();
        bool all_done = false;
        do
        {
            all_done = false;
            if (_colorPoints.Count > 0)
            {
                for (int i = 0; i < _colorPoints.Count; i++)
                {
                    Destroy(_colorPoints[i]);
                    _colorPoints.Remove(_colorPoints[i]);
                    all_done = true;
                    break;
                }
            }

        }
        while (all_done);

        
        
        UnityVolumeRendering.TransferFunction tf = _volumerenderedobject.transferFunction;
        for (int i = 0; i < tf.colourControlPoints.Count; i++)
        {
            TFColourControlPoint tfpoint = _volumerenderedobject.transferFunction.colourControlPoints[i];

            Vector3 localPosReference = _TFPlane.transform.localPosition;
            //Instantiate(_colorPointPREFAB, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, _TFPlane.transform);
            GameObject colorPoint = Instantiate(_colorPointPREFAB, _TFPlane.transform);

            // converting to bottom left space
            colorPoint.transform.localPosition = new Vector3(5, 0, 6); // 6 -> below histogram
            Color c = tfpoint.colourValue;
            
            colorPoint.GetComponent<MeshRenderer>().material.color = c;

            //var cube = colorPoint.transform.GetChild(0);
            //cube.GetComponent<MeshRenderer>().material.color = c;

            Vector3 pos_ = colorPoint.transform.localPosition;
            pos_.x -= (tfpoint.dataValue * 10); // 10 == width of plane
            colorPoint.transform.localPosition = pos_;

            colorPoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //Debug.Log("value_origin: " + (tfpoint.dataValue * 10));

            colorPoint.transform.hasChanged = false;
            _colorPoints.Add(colorPoint);

        }
    }

    public void InitiateAlphaPoints()
    {
        bool all_done = false;
        do
        {
            all_done = false;
            if (_alphaPoints.Count > 0)
            {
                for (int i = 0; i < _alphaPoints.Count; i++)
                {
                    Destroy(_alphaPoints[i]);
                    _alphaPoints.Remove(_alphaPoints[i]);
                    all_done = true;
                    break;
                }
            }

        }
        while (all_done);


        // Set alphaPoints
        for (int i = 0; i < _volumerenderedobject.transferFunction.alphaControlPoints.Count; i++)
        {
            TFAlphaControlPoint tfpoint = _volumerenderedobject.transferFunction.alphaControlPoints[i];

            Vector3 localPosReference = _TFPlane.transform.localPosition;
            GameObject alphaPoint = Instantiate(_alphaPointPREFAB, _TFPlane.transform);

            // converting to bottom left space
            alphaPoint.transform.localPosition = new Vector3(5, 0, 5); // literally BottomLeft

            Vector3 pos_ = alphaPoint.transform.localPosition;
            pos_.x -= (tfpoint.dataValue * 10); // 10 == width of plane
            pos_.z -= (tfpoint.alphaValue * 10);
            alphaPoint.transform.localPosition = pos_;

            alphaPoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            alphaPoint.transform.hasChanged = false;
            _alphaPoints.Add(alphaPoint);
        }
    }

    public void InitiateTFPlanePoints()
    {
        UnityVolumeRendering.TransferFunction tf = _volumerenderedobject.transferFunction;

        InitiateColorPoints();
        InitiateAlphaPoints();
        
        _anchorTFPlane = Instantiate(_colorPointPREFAB, _TFPlane.transform);
        _anchorTFPlane.transform.localPosition = new Vector3(5, 0, 5); // literally BottomLeft
        _anchorTFPlane.SetActive(false);

        //Texture2D tex = HistogramTextureGenerator.GenerateHistogramTextureFromTo(_volumerenderedobject.dataset, -1024, 10);
        Texture2D tex = HistogramTextureGenerator.GenerateHistogramTexture(_volumerenderedobject.dataset);
        _TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_HistTex", tex);

        //Texture2D tfTex = _volumerenderedobject.transferFunction.GetTexture();

        // Assets/UnityVolumeRendering/Assets/Textures/CrossSectionPlaneTexture.png
        //float from = inverse_lerp(0, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
        //float to = inverse_lerp(6, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
        //from = inverse_lerp(from, 0, tfTex.width);
        //to = inverse_lerp(to, 0, tfTex.width);
        //var rect = new Rect(from, 0, to, tfTex.height);
        //var s = Sprite.Create(tfTex, rect, Vector2.one * 0.5f);
        //tfTex = s.texture;

        UpdateTFPlaneTex();
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", tfTex);
        //_TFPlane.SetActive(true);

        UpdateHUValues();

        _zoom_scope = new Vector2(0.0f, 100.0f);

        // set dataset labels
        //_dataset_label_min.GetComponent<TMP_Text>().text = _volumerenderedobject.dataset.GetMinDataValue().ToString();
        //_dataset_label_max.GetComponent<TMP_Text>().text = _volumerenderedobject.dataset.GetMaxDataValue().ToString();

        _dataset_label_min.GetComponent<TMP_Text>().text = $"{_volumerenderedobject.dataset.GetMinDataValue().ToString()} HU";
        _dataset_label_max.GetComponent<TMP_Text>().text = $"{_volumerenderedobject.dataset.GetMaxDataValue().ToString()} HU";

        // set current labels
        // 
        _current_label_min.GetComponent<TMP_Text>().text = $"{_zoom_scope.x.ToString()} HU";
        _current_label_max.GetComponent<TMP_Text>().text = $"{_zoom_scope.y.ToString()} HU"; 
        

        // set current labels
        //UpdateZoomScope();
    }


    public void AddColorPoint()
    {
        System.Random rand = new System.Random();

        float dataValue = (float)rand.NextDouble();
        float r = (float)rand.NextDouble();
        float g = (float)rand.NextDouble();
        float b = (float)rand.NextDouble();
        float a = 0.3f;
        Color c = new Color(r, g, b, a);

        GameObject colorPoint = Instantiate(_colorPointPREFAB, _TFPlane.transform);
        colorPoint.transform.localPosition = new Vector3(5, 0, 6); // 6 -> below histogram
        colorPoint.GetComponent<MeshRenderer>().material.color = c;

        Vector3 pos_ = colorPoint.transform.localPosition;
        pos_.x -= (dataValue * 10); // 10 == width of plane
        colorPoint.transform.localPosition = pos_;

        colorPoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        //Debug.Log("value_origin: " + (tfpoint.dataValue * 10));

        colorPoint.transform.hasChanged = true;
        _colorPoints.Add(colorPoint);

        // also add it to the TF
        _volumerenderedobject.transferFunction.AddControlPoint(new TFColourControlPoint(dataValue, c));
        // and re-do
        _volumerenderedobject.transferFunction.GenerateTexture();
        UpdateTFPlaneTex();
        Update_();
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());
    }

    public void RemoveColorPoint()
    {
        int index = _colorPoints.Count - 1;


        Destroy(_colorPoints[index]);
        _colorPoints.Remove(_colorPoints[index]);

        //_colorPoints.Remove(_colorPoints[_colorPoints.Count - 1]);
        _volumerenderedobject.transferFunction.colourControlPoints.RemoveAt(_volumerenderedobject.transferFunction.colourControlPoints.Count - 1);
        _volumerenderedobject.transferFunction.GenerateTexture();
        UpdateTFPlaneTex();
        Update_();
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());
    }

    public void AddAlphaPoint()
    {
        
        System.Random rand = new System.Random();

        float dataValue = 0.5f;//float)rand.NextDouble();
        float alphaValue = 0.5f;//(float)rand.NextDouble();

        if (_zoomed)
        {
            GameObject alphaPoint = Instantiate(_alphaPointPREFAB, _TFPlane.transform);
            alphaPoint.transform.localPosition = new Vector3(5, 0, 5); // literally BottomLeft

            Vector3 pos_ = alphaPoint.transform.localPosition;
            pos_.x -= (dataValue * 10); // 10 == width of plane
            pos_.z -= (alphaValue * 10);
            alphaPoint.transform.localPosition = pos_;

            alphaPoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            alphaPoint.transform.hasChanged = false;
            _alphaPoints.Add(alphaPoint);

            Vector2 real_zoom = GetFinalZoomScope();
            float v = inverse_lerp(dataValue, real_zoom.x, real_zoom.y);


            _volumerenderedobject.transferFunction.AddControlPoint(new TFAlphaControlPoint(v, alphaValue));
            
        }
        else
        {
            GameObject alphaPoint = Instantiate(_alphaPointPREFAB, _TFPlane.transform);
            alphaPoint.transform.localPosition = new Vector3(5, 0, 5); // literally BottomLeft

            Vector3 pos_ = alphaPoint.transform.localPosition;
            pos_.x -= (dataValue * 10); // 10 == width of plane
            pos_.z -= (alphaValue * 10);
            alphaPoint.transform.localPosition = pos_;

            alphaPoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            alphaPoint.transform.hasChanged = false;
            _alphaPoints.Add(alphaPoint);

            _volumerenderedobject.transferFunction.AddControlPoint(new TFAlphaControlPoint(dataValue, alphaValue));
        }

        UpdateHUValues();


        // and re-do

        if (_zoomed)
        {
            Vector2 real_zoom = GetFinalZoomScope();
            AdjustTFPlane(real_zoom.x, real_zoom.y);

        }


        _volumerenderedobject.transferFunction.GenerateTexture();

        Update_();
        UpdateTFPlaneTex();
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());

    }

    public void RemoveAlphaPoint()
    {
        int index = _alphaPoints.Count - 1;


        Destroy(_alphaPoints[index]);
        _alphaPoints.Remove(_alphaPoints[index]);

        //_colorPoints.Remove(_colorPoints[_colorPoints.Count - 1]);
        _volumerenderedobject.transferFunction.alphaControlPoints.RemoveAt(_volumerenderedobject.transferFunction.alphaControlPoints.Count - 1);
        _volumerenderedobject.transferFunction.GenerateTexture();

        Update_();
        UpdateTFPlaneTex(); 
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());
    }

    public void InstantiateColorPickers()
    {
        Debug.Log("color picker!");

        for (int i = 0; i < _colorPoints.Count; i++)
        {
            GameObject colorPicker = Instantiate(_colorPickerPREFAB, _colorPoints[i].transform);
            colorPicker.transform.localScale = new Vector3(50, 50, 50);

            colorPicker.transform.localPosition = new Vector3(0, -4.56f, -0.92f);

            //colorPicker.GetComponent<ColorPickerControls>()._colorPoint = _colorPoints[i];
            //var tt = colorPicker.GetComponentInChildren<ColorPickerControls>()._r;
            //var tt = colorPicker.GetComponent<ColorPickerControls>()._r;
            colorPicker.GetComponentInChildren<ColorPickerControls>()._colorPoint = _colorPoints[i];
            colorPicker.GetComponentInChildren<ColorPickerControls>().InitSliders();
            //Debug.Log(tt);

            _colorPickers.Add(colorPicker);
        }
    }

    public void DeleteColorPickers()
    {
        if (_colorPickers.Count == 0)
            return;

        for (int i = 0; i < _colorPickers.Count; i++)
        {
            Destroy(_colorPickers[i]);
        }

        _colorPickers.Clear();
    }

    private Texture2D clip_image(Texture2D tex, float from, float to)
    {
        float from_datasetspace = inverse_lerp(from, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
        float to_datasetspace = inverse_lerp(to, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

        float from_imagespace = from_datasetspace * tex.width;
        float to_imagespace = to_datasetspace * tex.width;

        int blockWidth = (int)(to_imagespace - from_imagespace);
        int blockHeight = tex.height;

        var data = tex.GetPixels((int)from_imagespace, 0, blockWidth, blockHeight);
        Texture2D newTex = new Texture2D(blockWidth, blockHeight);
        newTex.SetPixels(data);
        newTex.Apply();

        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_HistTex", newTex);
        return newTex;
    }

    // based of from-to range - update textures
    public void AdjustTFPlane(float from, float to)
    {
        // clip _HistTex
        Texture2D tex = HistogramTextureGenerator.GenerateHistogramTexture(_volumerenderedobject.dataset);
        _TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_HistTex", clip_image(tex, from, to));

        // clip _TFTex
        Texture2D tex_tf = _volumerenderedobject.transferFunction.GetTexture();
        _TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", clip_image(tex_tf, from, to));

    }

    public void ConvertPointToZoomSpace(GameObject alphapoint)
    {
        float from = _zoom_scope.x;
        float to = _zoom_scope.y;

        

        float value = alphapoint.transform.localPosition.x;
        value = _anchorTFPlane.transform.localPosition.x - value;
        value = value / 10;

        // value
        float from_ = inverse_lerp(from, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
        float to_ = inverse_lerp(to, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

        float v = inverse_lerp(value, from_, to_);

        alphapoint.transform.localPosition = new Vector3(5, 0, 6);
        Vector3 pos_ = alphapoint.transform.localPosition;
        pos_.x -= (v * 10); // 10 == width of plane
        alphapoint.transform.localPosition = pos_;

        
    }
    
    public void ConvertTFPointsToZoomSpace()
    {
        float from = _zoom_scope.x;
        float to = _zoom_scope.y;

        // convert alphapoints and colorpoints to correct places based of zoom 
        for (int i = 0; i < _colorPoints.Count; i++)
        {

            float value = _colorPoints[i].transform.localPosition.x;
            value = _anchorTFPlane.transform.localPosition.x - value;
            value = value / 10;

            // value
            float from_ = inverse_lerp(from, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
            float to_ = inverse_lerp(to, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

            //if (value <= from_ || value >= to_)
            //{
            //    continue;
            //}

            float v = inverse_lerp(value, from_, to_);

            _colorPoints[i].transform.localPosition = new Vector3(5, 0, 6);
            Vector3 pos_ = _colorPoints[i].transform.localPosition;
            pos_.x -= (v * 10); // 10 == width of plane
            _colorPoints[i].transform.localPosition = pos_;


        }

        for (int i = 0; i < _alphaPoints.Count; i++)
        {

            float value = _alphaPoints[i].transform.localPosition.x;
            value = _anchorTFPlane.transform.localPosition.x - value;
            value = value / 10;

            // value
            float from_ = inverse_lerp(from, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
            float to_ = inverse_lerp(to, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

            //if (value <= from_ || value >= to_)
            //{
            //    continue;
            //}

            float v = inverse_lerp(value, from_, to_);

            Vector3 oldPos = _alphaPoints[i].transform.localPosition;

            _alphaPoints[i].transform.localPosition = _anchorTFPlane.transform.localPosition;
            Vector3 pos_ = _alphaPoints[i].transform.localPosition;
            pos_.x -= (v * 10); // 10 == width of plane
            pos_.z = oldPos.z;
            _alphaPoints[i].transform.localPosition = pos_;


        }
    }

    public void ConvertTFPointsToNormalSpace()
    {
        float from = _zoom_scope.x;
        float to = _zoom_scope.y;

        // colorPoints
        for (int i = 0; i < _colorPoints.Count; i++)
        {
            //string t = _colorPoints[i].GetComponentInChildren<TMP_Text>().text;
            //float value = float.Parse(t, CultureInfo.InvariantCulture.NumberFormat);
            //float Z_val_x = _anchorTFPlane.transform.localPosition.x - _alphaPoints[i].transform.localPosition.x;
            //Z_val_x = Z_val_x / 10; 
            //Vector2 zoomSpace = GetFinalZoomScope();
            //float value = fGetHUFromFloat(Z_val_x, zoomSpace.x, zoomSpace.y);
            
            //float v = inverse_lerp(value, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

            // test
            // test
            float Z_val_x = _anchorTFPlane.transform.localPosition.x - _colorPoints[i].transform.localPosition.x;
            Z_val_x = Z_val_x / 10;
            Vector2 zoomScope = GetFinalZoomScope();
            float l_Z_val = inverse_lerp(Z_val_x, zoomScope.x, zoomScope.y);

            float l_N_val = inverse_lerp(l_Z_val, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

            float n_V_ = fGetHUFromFloat(Z_val_x, zoomScope.x, zoomScope.y);
            float n_v = inverse_lerp(n_V_, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());




            _colorPoints[i].transform.localPosition = new Vector3(5, 0, 6);
            Vector3 pos_ = _colorPoints[i].transform.localPosition;
            pos_.x -= (n_v * 10); // 10 == width of plane
            _colorPoints[i].transform.localPosition = pos_;

        }

        for (int i = 0; i < _alphaPoints.Count; i++)
        {
            //string t = _alphaPoints[i].GetComponentInChildren<TMP_Text>().text;

            //Debug.Log("test: " + t);
            //var lines = t.Split(Environment.NewLine);
            //var lines = t.Split('\n');

            //float value = float.Parse(lines[1], CultureInfo.InvariantCulture.NumberFormat);

            //float v = inverse_lerp(value, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

            float Z_val_x = _anchorTFPlane.transform.localPosition.x - _alphaPoints[i].transform.localPosition.x;
            Z_val_x = Z_val_x / 10;
            Vector2 zoomScope = GetFinalZoomScope();
            float l_Z_val = inverse_lerp(Z_val_x, zoomScope.x, zoomScope.y);

            float l_N_val = inverse_lerp(l_Z_val, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

            float n_V_ = fGetHUFromFloat(Z_val_x, zoomScope.x, zoomScope.y);
            float n_v = inverse_lerp(n_V_, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());





            Vector3 oldPos = _alphaPoints[i].transform.localPosition;

            _alphaPoints[i].transform.localPosition = _anchorTFPlane.transform.localPosition;
            Vector3 pos_ = _alphaPoints[i].transform.localPosition;
            pos_.x -= (n_v * 10); // 10 == width of plane
            pos_.z = oldPos.z;
            _alphaPoints[i].transform.localPosition = pos_;


        }
    }

    public void ZoomIn()
    {
        //if (_zoomed == true)
        //    return;

        //_zoomed = true;

        // set correct TFPlane (zoomed in)
        //float from = 0.0f;
        //float to = 1000.0f;
        //to = 6.0f;

        //_zoom_scope = new Vector2(from, to);
        //_zoom_scope = GetFinalZoomScope();

        float from = _zoom_scope.x;
        float to = _zoom_scope.y;

        AdjustTFPlane(from, to);

        ConvertTFPointsToZoomSpace();

    }

    public void ZoomOut()
    {
        //if (_zoomed == false)
        //    return;
        //
        //_zoomed = false;

        float from = _volumerenderedobject.dataset.GetMinDataValue();
        float to = _volumerenderedobject.dataset.GetMaxDataValue();

        //_zoom_scope = new Vector2(from, to);

        AdjustTFPlane(from, to);

        ConvertTFPointsToNormalSpace();
        
    }

    public void UpdateTFPlaneTex()
    {
        if (_zoomed)
        {
            Texture2D tex = _volumerenderedobject.transferFunction.GetTexture();
            Vector2 real_zoom = GetFinalZoomScope();
            _TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", clip_image(tex, real_zoom.x, real_zoom.y));
        }
        else
        {
            _TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());
        }
    }

    public void Update_()
    {
        //Debug.Log("CHANGED!");

        // reset it back
        for (int i = 0; i < _colorPoints.Count; i++)
            _colorPoints[i].transform.hasChanged = false;
        for (int i = 0; i < _alphaPoints.Count; i++)
            _alphaPoints[i].transform.hasChanged = false;
        if (_colorPickers.Count >= 1)
            for (int i = 0; i < _colorPickers.Count; i++)
                _colorPickers[i].GetComponentInChildren<ColorPickerControls>().changed = false;

        // get data back
        // Get Color Points
        UnityVolumeRendering.TransferFunction tf = ScriptableObject.CreateInstance<UnityVolumeRendering.TransferFunction>();
        for (int i = 0; i < _colorPoints.Count; i++)
        {
            if (_zoomed)
            {
                //string t = _colorPoints[i].GetComponentInChildren<TMP_Text>().text;
                //float value = float.Parse(t, CultureInfo.InvariantCulture.NumberFormat);
                
                //float v = inverse_lerp(value, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

                // test
                float Z_val_x = _anchorTFPlane.transform.localPosition.x - _colorPoints[i].transform.localPosition.x;
                Z_val_x = Z_val_x / 10;
                Vector2 zoomScope = GetFinalZoomScope();
                float l_Z_val = inverse_lerp(Z_val_x, zoomScope.x, zoomScope.y);

                float l_N_val = inverse_lerp(l_Z_val, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

                float n_V_ = fGetHUFromFloat(Z_val_x, zoomScope.x, zoomScope.y);
                float n_v = inverse_lerp(n_V_, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());
                

                tf.AddControlPoint(new TFColourControlPoint(n_v, _colorPoints[i].GetComponent<MeshRenderer>().material.color));
            }
            else
            {
                Vector3 anchor_pos_ = _anchorTFPlane.transform.localPosition;

                float x_pos_of_point = _colorPoints[i].transform.localPosition.x;

                float value = anchor_pos_.x - x_pos_of_point;
                value = value / 10;

                tf.AddControlPoint(new TFColourControlPoint(value, _colorPoints[i].GetComponent<MeshRenderer>().material.color));
            }


        }

        for (int i = 0; i < _alphaPoints.Count; i++)
        {
            if (_zoomed)
            {
                //string t = _alphaPoints[i].GetComponentInChildren<TMP_Text>().text;

                //var lines = t.Split(Environment.NewLine);
                //var lines = t.Split('\n');
            
                //float value = float.Parse(lines[1], CultureInfo.InvariantCulture.NumberFormat);
                //float alpha = float.Parse(lines[0], CultureInfo.InvariantCulture.NumberFormat);
                //
                //// czech windows thing
                //
                Vector3 anchor_pos_ = _anchorTFPlane.transform.localPosition;
                float z_pos_of_point = _alphaPoints[i].transform.localPosition.z; // alphaValue
                float alphaValue__ = anchor_pos_.z - z_pos_of_point;
                alphaValue__ = alphaValue__ / 10;
                
                //float v = inverse_lerp(value, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

                // test
                float Z_val_x = _anchorTFPlane.transform.localPosition.x - _alphaPoints[i].transform.localPosition.x;
                Z_val_x = Z_val_x / 10;
                Vector2 zoomScope = GetFinalZoomScope();
                float l_Z_val = inverse_lerp(Z_val_x, zoomScope.x, zoomScope.y);

                float l_N_val = inverse_lerp(l_Z_val, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

                float n_V_ = fGetHUFromFloat(Z_val_x, zoomScope.x, zoomScope.y);
                float n_v = inverse_lerp(n_V_, _volumerenderedobject.dataset.GetMinDataValue(), _volumerenderedobject.dataset.GetMaxDataValue());

                tf.AddControlPoint(new TFAlphaControlPoint(n_v, alphaValue__));
            }
            else
            {
                Vector3 anchor_pos_ = _anchorTFPlane.transform.localPosition;

                float x_pos_of_point = _alphaPoints[i].transform.localPosition.x; // dataValue
                float z_pos_of_point = _alphaPoints[i].transform.localPosition.z; // alphaValue

                float dataValue__ = anchor_pos_.x - x_pos_of_point;
                dataValue__ = dataValue__ / 10;

                float alphaValue__ = anchor_pos_.z - z_pos_of_point;
                alphaValue__ = alphaValue__ / 10;

                tf.AddControlPoint(new TFAlphaControlPoint(dataValue__, alphaValue__));
            }

        }

        tf.GenerateTexture();
        _volumerenderedobject.SetTransferFunction(tf);

        // update tfplane texture
        //_TFPlane.GetComponent<MeshRenderer>().material.SetTexture("_TFTex", _volumerenderedobject.transferFunction.GetTexture());
        UpdateTFPlaneTex();

        // update HU
        UpdateHUValues();
    }

    public Vector2 GetFinalZoomScope()
    {
        Vector2 res = new Vector2(_zoom_scope.x, _zoom_scope.y);
        res.x += _slider_min_zoom;
        res.y += _slider_max_zoom;
        return res;
    }

    public void UpdateZoomScope()
    {
        Vector2 real_values = GetFinalZoomScope();

        //if (real_values.x > real_values.y)
        //{
        //    float tmp = real_values.x;
        //    real_values.x = real_values.y;
        //    real_values.y = tmp;
        //}

        // update labels
       // _current_label_min.GetComponent<TMP_Text>().text = real_values.x.ToString();
       // _current_label_max.GetComponent<TMP_Text>().text = real_values.y.ToString();

        _current_label_min.GetComponent<TMP_Text>().text = $"{real_values.x.ToString()} HU";
        _current_label_max.GetComponent<TMP_Text>().text = $"{real_values.y.ToString()} HU";

        //_current_label_min.GetComponent<TMP_Text>().text = $"{_current_label_min.ToString()} HU";
        //_current_label_max.GetComponent<TMP_Text>().text = $"{_current_label_max.ToString()} HU";


    }

    public float ZoomScopeButtonScale = 50.0f;
    public void ButtonMinZoomPlus()
    {
        _zoom_scope.x += ZoomScopeButtonScale;
        UpdateZoomScope();
    }

    public void ButtonMinZoomMinus()
    {
        _zoom_scope.x -= ZoomScopeButtonScale;
        UpdateZoomScope();
    }

    public void ButtonMaxZoomPlus()
    {
        _zoom_scope.y += ZoomScopeButtonScale;
        UpdateZoomScope();
    }

    public void ButtonMaxZoomMinus()
    {
        _zoom_scope.y -= ZoomScopeButtonScale;
        UpdateZoomScope();
    }

    public void MinSliderUpdated(SliderEventData data)
    {
        _slider_min_zoom = _slider_min.Value;
        UpdateZoomScope();
    }

    public void MaxSliderUpdated(SliderEventData data)
    {
        _slider_max_zoom = _slider_max.Value;
        UpdateZoomScope();
    }

    public void ToggleZoom()
    {
        _zoomed = _zoom_button.IsToggled;
        Debug.Log("zoomed:" + _zoomed);

        //_zoomed = !_zoomed;

        if (_zoomed)
            ZoomIn();
        if (!_zoomed)
            ZoomOut();

    }

    public void saveTF()
    {
        string filepath = "Q:/dev/hololens/ARdno_FNO/Assets/Resources/nowCreated.txt";
        TransferFunctionDatabase.SaveTransferFunction(_volumerenderedobject.transferFunction, filepath);
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // (ifvaluechanged) eventually

        int hasChanged = 0;
        for (int i = 0; i < _colorPoints.Count; i++)
            hasChanged += Convert.ToInt32(_colorPoints[i].transform.hasChanged);
        for (int i = 0; i < _alphaPoints.Count; i++)
            hasChanged += Convert.ToInt32(_alphaPoints[i].transform.hasChanged);
        if (_colorPickers.Count >= 1)
        { 
            for (int i = 0; i < _colorPickers.Count; i++)
                hasChanged += Convert.ToInt32(_colorPickers[i].GetComponentInChildren<ColorPickerControls>().changed);
        }


        if (hasChanged != 0)
        {
            Update_();
        }
    }
}
