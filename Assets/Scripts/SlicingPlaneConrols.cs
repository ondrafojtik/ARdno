using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityVolumeRendering;

public class SlicingPlaneConrols : MonoBehaviour
{

    [SerializeField] List<PressableButton> _buttons;
    [SerializeField] List<MixedReality.Toolkit.UX.Slider> _sliders;

    public List<GameObject> _slicingPlanes = new List<GameObject>();

    public void ToggleAll()
    {
        Debug.Log("slicingPlanesCount: " + _slicingPlanes.Count);
        PressableButton b = _buttons[0];
        if(b.IsToggled)
        {
            Debug.Log("on");
            for (int i = 0; i < _slicingPlanes.Count; i++)
                _slicingPlanes[i].SetActive(true);
            _buttons[1].ForceSetToggled(true);
            _buttons[2].ForceSetToggled(true);
            _buttons[3].ForceSetToggled(true);
        }
        else
        {
            Debug.Log("of");
            for (int i = 0; i < _slicingPlanes.Count; i++)
                _slicingPlanes[i].SetActive(false);
            _buttons[1].ForceSetToggled(false);
            _buttons[2].ForceSetToggled(false);
            _buttons[3].ForceSetToggled(false);
        }
    }

    public void ToggleXY()
    {
        //Debug.Log("toggleXY");
        PressableButton b = _buttons[1];
        if (b.IsToggled)
        {
            //Debug.Log("on");
            _slicingPlanes[0].SetActive(true);
        }
        else
        {
            //Debug.Log("of");
            _slicingPlanes[0].SetActive(false);
            _buttons[0].ForceSetToggled(false);
        }
    }

    public void ToggleXZ()
    {
        PressableButton b = _buttons[2];
        if (b.IsToggled)
        { 
            _slicingPlanes[1].SetActive(true);
        }
        else
        { 
            _slicingPlanes[1].SetActive(false);
            _buttons[0].ForceSetToggled(false);
        }
    }

    public void ToggleZY()
    {
        Debug.Log("toggleXY");
        PressableButton b = _buttons[3];
        if (b.IsToggled)
        {
            Debug.Log("on");
            _slicingPlanes[2].SetActive(true);
            
            //_slicingPlanes[2].SetActive(true);
        }
        else
        {
            Debug.Log("of");
            _slicingPlanes[2].SetActive(false);
            _buttons[0].ForceSetToggled(false);
            //_slicingPlanes[2].SetActive(false);
        }
    }


    public void XYSliderUpdated(SliderEventData data)
    {
        if (_slicingPlanes.Count != 0) 
            _slicingPlanes[0].transform.localPosition = new Vector3(0, 0, _sliders[0].Value); ;
    }

    public void XZSliderUpdated(SliderEventData data)
    {
        if (_slicingPlanes.Count != 0)
            _slicingPlanes[1].transform.localPosition = new Vector3(0, _sliders[1].Value, 0);
    }

    public void ZYSliderUpdated(SliderEventData data)
    {
        if (_slicingPlanes.Count != 0)
            _slicingPlanes[2].transform.localPosition = new Vector3(_sliders[2].Value, 0, 0);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _sliders[0].Value = _slicingPlanes[0].transform.localPosition.z;
        _sliders[1].Value = _slicingPlanes[1].transform.localPosition.y;
        _sliders[2].Value = _slicingPlanes[2].transform.localPosition.x;

    }
}
