
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityVolumeRendering;
using itk.simple;
using System.Threading.Tasks;
//using Microsoft.MixedReality.Toolkit.UI;
using UnityEditor;
using TMPro;
using openDicom.DataStructure;
using UnityEngine.Diagnostics;
using MixedReality.Toolkit.UX;
using Unity.VisualScripting;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using MixedReality.Toolkit;

public class VolumeSolution : MonoBehaviour
{

    //[SerializeField] VolumeRenderedObject _volumeData;
    [SerializeField] GameObject _volumetricDataMainParentObject;
    [SerializeField] GameObject _crossSection;
    [SerializeField] VolumeRenderedObject _volumerenderedobject;
    [SerializeField] GameObject _meshContainer;
    
    [SerializeField] GameObject _volumetricDataMainParentObject_nrrd;
    [SerializeField] GameObject _crossSection_nrrd;
    [SerializeField] VolumeRenderedObject _volumerenderedobject_nrrd;
    [SerializeField] GameObject _meshContainerNRRD_nrrd;

    [SerializeField] TMP_Text text_load;
    
    [SerializeField] SlidersControl _slidersControl_DICOM;
    [SerializeField] SlidersControl _slidersControl_NRRD;

    [SerializeField] GameObject _slicingPlaneXZ;
    [SerializeField] GameObject _slicingPlaneXY;
    [SerializeField] GameObject _slicingPlaneZY;

    [SerializeField] SlicingPlaneConrols _slicingPlaneControls;

    [SerializeField] TFPlaneControls _tfplaneControls;


    [SerializeField] GameObject _TFPlane;
    [SerializeField] GameObject _slicingPlanes;
    [SerializeField] GameObject _dicomMenu;
    [SerializeField] GameObject _nrrdMenu;


    List<SlicingPlane> slicingPlanes = new List<SlicingPlane>();


    [SerializeField] GameObject _loadDatasetControls;
    [SerializeField] GameObject _loadTransferFunctionControls;
    
    public bool _showColorPicker = false;


    //UnityVolumeRendering.TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
    //public UnityVolumeRendering.TransferFunction tf_volume { get; set; }
    public UnityVolumeRendering.TransferFunction tf_segment { get; set; }
    public UnityVolumeRendering.VolumeDataset volume_dataset { get; set; }
    public UnityVolumeRendering.VolumeDataset segment_dataset { get; set; }

    float _minHu = 0;
    float _maxHu = 0;

    static float inverse_lerp(float value, float min, float max) // 0 - 1
    {
        float val = (value - min) / (max - min);
        return val;
    }

    static float transform_from_tfplane()
    {
        return 0;
    }

    static float transform_to_tfplane()
    {
        return 0;
    }


    //[field: SerializeField] public MeshRenderer VolumeMeshRenderer { get; set; }

    public void LoadDicomDataPath(string dicomFolderPath, out string dicomPath, out bool isImageSequence, out int errorFlag)
    {

        

        List<string> dicomFilesCandidates = Directory.GetFiles(dicomFolderPath).ToList();

        dicomFilesCandidates.RemoveAll(x => x.EndsWith(".meta"));

        
        if (dicomFilesCandidates.Count == 0)
        {
            errorFlag = 1;
            dicomPath = null;
            isImageSequence = false;
            return;
        }

        DatasetType datasetType = DatasetImporterUtility.GetDatasetType(dicomFilesCandidates.First());

        if (datasetType == DatasetType.ImageSequence || datasetType == DatasetType.DICOM)
        {
            isImageSequence = true;
            dicomPath = dicomFolderPath;
            errorFlag = 0;
        }
        else if (datasetType == DatasetType.Unknown)
        {
            isImageSequence = false;
            dicomPath = dicomFilesCandidates[0];
            errorFlag = 2;
        }
        else
        {
            isImageSequence = false;
            dicomPath = dicomFilesCandidates[0];
            errorFlag = 0;
        }
    }

    public async Task<(VolumeDataset, bool)> load_(ImageSequenceImportSettings settings, bool isNRRD, string _dicomFilderPath, string _segmentFolderPath)
    {
        Debug.Log("loading dataset: " + _dicomFilderPath);

        //ProgressHandler progressHandler = new ProgressHandler();

        string dicomFolderPath = _dicomFilderPath;
        string segmentFolderPath = _segmentFolderPath;


        //text_load.text = dicomFolderPath;
        //string datasetName = streaming_assets.Split('/').Last();

        //text_load.text = "10";

        LoadDicomDataPath(dicomFolderPath, out string filePath, out bool isDicomImageSequence, out int errorFlag);


        //text_load.text = "11";

        var importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
        SimpleITKImageSequenceImporter sequenceImporter = new SimpleITKImageSequenceImporter();
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();
        VolumeDataset dataset = null;
        bool isDatasetReversed = true;


        //text_load.text = "12";
        if (!isNRRD)
        {
            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates, settings);

            Debug.Log("candidate count: " + fileCandidates.Count());

            //text_load.text = fileCandidates.Count().ToString();

            try
            {
                var result = await sequenceImporter.ImportSeriesAsync(sequence.First(), settings);
                dataset = result;
                //isDatasetReversed = result.Item2;
            }
            catch (Exception e) { Debug.Log(e); }
            
        }
        else
        {
            try
            {
                //progressHandler.ReportProgress(0.2f, "Loading main file...");
                Debug.Log("deje seto!");
                filePath = segmentFolderPath;
                var result = await fileImporter.ImportAsync(filePath);
                dataset = result;
                //isDatasetReversed = result.Item2;
            }
            catch (Exception e) { Debug.Log(e); }
        }


        //text_load.text = "13";

        return (dataset, isDatasetReversed);
        

    }

   
    public async Task<VolumeDataset> load_segmentation(ImageSequenceImportSettings settings, VolumeDataset volumeDataset)
    {
        //ProgressHandler progressHandler = new ProgressHandler();

        text_load.text = "1";
        string segmentFolderPath = Application.dataPath + "/StreamingAssets/jatra_l/segmentation/Segmentation.nrrd";

        
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();
        text_load.text = "2";
        //await fileImporter.ImportSegmentationAsync(segmentFolderPath, volumeDataset);
        volumeDataset = await fileImporter.ImportAsync(segmentFolderPath);

        text_load.text = "3";
        return volumeDataset;


    }

    public UnityVolumeRendering.TransferFunction SetupTF(string _tf_filepath)
    {
        UnityVolumeRendering.TransferFunction tf = TransferFunctionDatabase.LoadTransferFunction(_tf_filepath);
        tf.GenerateTexture();
        return tf;
    }

    public async void SetupVRO(VolumeRenderedObject vro, GameObject meshContainer, VolumeDataset _dataset, GameObject VolumeMainObject, GameObject crossSection)
    {
        vro = VolumeMainObject.GetComponent<VolumeRenderedObject>();
        vro = await VolumeObjectFactory.FillObjectAsync(_dataset, VolumeMainObject, meshContainer);
        vro.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        vro.transform.position = VolumeMainObject.transform.position;
        vro.transform.Rotate(180.0f, 0.0f, 0.0f);
        crossSection.GetComponent<CutoutSphere>().SetTargetObject(vro);
    }

    public async Task LoadSegmentedModel(string _dicomFolder, string _nrrdFilepath, VolumeDataset dataset, GameObject VolumeMainObject, VolumeRenderedObject vro, GameObject crossSection, GameObject __meshContainer, bool isNRRD, SlidersControl slidersControl, List<SlicingPlane> slicingPlanes)
    {
        string dicomFolderPath = _dicomFolder;
        string segmentFolderPath = _nrrdFilepath;

        //_initialSliderValues = new List<float>();
        ImageSequenceImportSettings settings = new ImageSequenceImportSettings();
        (VolumeDataset result, bool isDatasetReversed) = await load_(settings, isNRRD, dicomFolderPath, segmentFolderPath);
        dataset = result;

        //Texture3D tex = await dataset.GetDataTextureAsync(settings.progressHandler);
        

        // setup vro
        SetupVRO(vro, __meshContainer, dataset, VolumeMainObject, crossSection);
        // setup tf
        string resources_folder = Application.dataPath + "/Resources/";
        string tf_filepath = "";
        if (isNRRD) tf_filepath = resources_folder + "defaultSeg.txt";
        if (!isNRRD) tf_filepath = resources_folder + "defaultTF.txt";


        //UnityVolumeRendering.TransferFunction tf__ = ;
        
        //vro.SetTransferFunction(SetupTF(tf_filepath));

        vro.SetTransferFunction(TransferFunctionDatabase.LoadTransferFunction(tf_filepath));
        vro.transferFunction.GenerateTexture();

        //UnityVolumeRendering.TransferFunction tf = TransferFunctionDatabase.LoadTransferFunction(tf_filepath);



        // setup sliders
        _minHu = dataset.GetMinDataValue();
        _maxHu = dataset.GetMaxDataValue();

        // #test
        //_slidersControl._volumerenderedobject = vro;
        slidersControl._minHu = _minHu;
        slidersControl._maxHu = _maxHu;
        




        if (isNRRD)
        {
            float min = inverse_lerp(0, _minHu, _maxHu);
            float max = inverse_lerp(0, _minHu, _maxHu);

            vro.SetVisibilityWindow(min, max);
            //__meshContainer.SetActive(true);

            slidersControl.initSpecialSlider();
            slidersControl.initDensitySlider(-0, 0);

            //float min = inverse_lerp(0, _minHu, _maxHu);
            //float max = inverse_lerp(3, _minHu, _maxHu);
            //
            //vro.SetVisibilityWindow(min, max);
            //__meshContainer.SetActive(true);
            //
            //slidersControl.initSpecialSlider();
            //slidersControl.initDensitySlider(-0, 3);

        }
        else 
        {
            float min = inverse_lerp(400, _minHu, _maxHu);
            float max = inverse_lerp(800, _minHu, _maxHu);

            //vro.SetVisibilityWindow(min, max);
            vro.SetVisibilityWindow(_minHu, _maxHu);
            //__meshContainer.SetActive(true);

            slidersControl.initSpecialSlider();
            //slidersControl.initDensitySlider(-400, 1500);
            slidersControl.initDensitySlider(_minHu, _maxHu);

        }

        //_slidersControl.UpdateHuLabels();
        slidersControl.UpdateHuLabels();

        if (slicingPlanes.Count != 0)
        {
            for (int i = 0; i < slicingPlanes.Count; i++)
            {
                MeshRenderer sliceMeshRend = slicingPlanes[i].GetComponent<MeshRenderer>();
                //Resources.UnloadUnusedAssets();
                //Resources.UnloadAsset(sliceMeshRend.material);
                //sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
                Material sliceMat = slicingPlanes[i].GetComponent<MeshRenderer>().sharedMaterials[0];
                sliceMat.SetTexture("_DataTex", dataset.GetDataTexture());
                sliceMat.SetTexture("_TFTex", vro.transferFunction.GetTexture());

            }
        }

        //vro.GetComponent<Material>().SetTexture(isNRRD.ToString(), tex);
        //__meshContainer.GetComponent<Material>().SetTexture(isNRRD.ToString(), tex);

    }


    public void ToggleTF()
    {
        _TFPlane.SetActive(!_TFPlane.active);
    }

    public void ToggleSlicing()
    {
        _slicingPlanes.SetActive(!_slicingPlanes.active);
    }

    public void ToggleDICOMC()
    {
        _dicomMenu.SetActive(!_dicomMenu.active);
    }

    public void ToggleNRRDC()
    {
        _nrrdMenu.SetActive(!_nrrdMenu.active);
    }

    public void ToggleColorPicker()
    {

        //Debug.Log("COLOR PICKER: " + _showColorPicker);
        _showColorPicker = !_showColorPicker;

        if (_showColorPicker)
        {
            // instantiate ColorPicker on correct positions 
            _tfplaneControls.InstantiateColorPickers();
        }
        else
        {
            _tfplaneControls.DeleteColorPickers();
        }
    }

    public async void LoadDataset(string path)
    {
        // deprecated
        string segmentFolderPath = Application.dataPath + "/StreamingAssets/jatra_l/segmentation/Segmentation.nrrd";

        _meshContainer.SetActive(false);


        Task t;

        string dicomFolderPath = path;
        t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, volume_dataset, _volumetricDataMainParentObject, _volumerenderedobject, _crossSection, _meshContainer, false, _slidersControl_DICOM, slicingPlanes);

        await t;

        _tfplaneControls._volumerenderedobject = _volumerenderedobject;


        // TADY
        _tfplaneControls.InitiateTFPlanePoints();


        Resources.UnloadUnusedAssets();


        //_meshContainer.SetActive(true);
    }

    public async void LoadTF(string path)
    {
        path = path + ".txt";

        _volumerenderedobject.transferFunction = TransferFunctionDatabase.LoadTransferFunction(path);
        //_volumerenderedobject.transferFunction.GenerateTexture();
        //_tfplaneControls._volumerenderedobject.transferFunction = TransferFunctionDatabase.LoadTransferFunction(path);
        _tfplaneControls.InitiateTFPlanePoints();
        _tfplaneControls.Update_();
    }


    // Start is called before the first frame update
    public async void Start()
    {
        //_volumetricDataMainParentObject = GameObject.Find("NRRD");
        
        string baseDatasetFolderPath = Application.dataPath + "/StreamingAssets/Datasets/";
        List<string> directories = Directory.GetDirectories(baseDatasetFolderPath).ToList();

        for (int i = 0; i < directories.Count; i++)
        {
            Debug.Log("DIRECTORY: " + i.ToString() + " - " + directories[i]);
        }

        _loadDatasetControls.GetComponentInChildren<LoadDatasetControls>()._datasetDirectories = directories;
        _loadDatasetControls.GetComponentInChildren<LoadDatasetControls>()._baseDirectory = baseDatasetFolderPath;
        _loadDatasetControls.GetComponentInChildren<LoadDatasetControls>().init();

        string baseTransferFunctionFolderPath = Application.dataPath + "/Resources/";
        List<string> _tfFiles = Directory.GetFiles(baseTransferFunctionFolderPath).ToList();

        _loadTransferFunctionControls.GetComponentInChildren<LoadTranferFunctionControls>()._datasetDirectories = _tfFiles;
        _loadTransferFunctionControls.GetComponentInChildren<LoadTranferFunctionControls>()._baseDirectory = baseTransferFunctionFolderPath;
        _loadTransferFunctionControls.GetComponentInChildren<LoadTranferFunctionControls>().init();


        //string dicomFolderPath = Application.dataPath + "/StreamingAssets/arm/DICOM";
        //string dicomFolderPath = Application.dataPath + "/StreamingAssets/jatra_l/DICOM";
        string segmentFolderPath = Application.dataPath + "/StreamingAssets/jatra_l/segmentation/Segmentation.nrrd";

        


        
        slicingPlanes.Add(_slicingPlaneXZ.GetComponent<SlicingPlane>());
        slicingPlanes.Add(_slicingPlaneXY.GetComponent<SlicingPlane>());
        slicingPlanes.Add(_slicingPlaneZY.GetComponent<SlicingPlane>());

        _slicingPlaneControls._slicingPlanes.Add(_slicingPlaneXY);
        _slicingPlaneControls._slicingPlanes.Add(_slicingPlaneXZ);
        _slicingPlaneControls._slicingPlanes.Add(_slicingPlaneZY);

        //_slicingPlaneControls._slicingPlanes.Add(_slicingPlaneXY.GetComponentInParent<GameObject>());
        //_slicingPlaneControls._slicingPlanes.Add(_slicingPlaneXZ.GetComponentInParent<GameObject>());
        //_slicingPlaneControls._slicingPlanes.Add(_slicingPlaneZY.GetComponentInParent<GameObject>());

        Task t;


        //t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, segment_dataset, _volumetricDataMainParentObject_nrrd, _volumerenderedobject_nrrd, _crossSection_nrrd, _meshContainerNRRD_nrrd, true, _slidersControl_NRRD, slicingPlanes);


        /*
        t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, volume_dataset, _volumetricDataMainParentObject, _volumerenderedobject, _crossSection, _meshContainer, false, _slidersControl_DICOM, slicingPlanes);

        await t;

        dicomFolderPath = directories[1];

        t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, volume_dataset, _volumetricDataMainParentObject, _volumerenderedobject, _crossSection, _meshContainer, false, _slidersControl_DICOM, slicingPlanes);

        await t;
        //_meshContainer.GetComponent<MeshRenderer>().sharedMaterial = new Material()

        */


        //dicomFolderPath = directories[1];
        //t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, volume_dataset, _volumetricDataMainParentObject, _volumerenderedobject, _crossSection, _meshContainer, false, _slidersControl_DICOM, slicingPlanes);

        //await t;
        /*
        dicomFolderPath = directories[1];

        t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, volume_dataset, _volumetricDataMainParentObject, _volumerenderedobject, _crossSection, _meshContainer, false, _slidersControl_DICOM, slicingPlanes);

        await t;
        */

        /*
        string dicomFolderPath = directories[0];
        t = LoadSegmentedModel(dicomFolderPath, segmentFolderPath, volume_dataset, _volumetricDataMainParentObject, _volumerenderedobject, _crossSection, _meshContainer, false, _slidersControl_DICOM, slicingPlanes);

        await t;

        _tfplaneControls._volumerenderedobject = _volumerenderedobject;
        // TADY
        _tfplaneControls.InitiateTFPlanePoints();

        Resources.UnloadUnusedAssets();
        */
    }



    // Update is called once per frame
    void Update()
    {
        
    }


}
