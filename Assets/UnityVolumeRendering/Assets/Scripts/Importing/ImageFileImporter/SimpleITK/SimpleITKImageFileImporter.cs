//#if UVR_USE_SIMPLEITK
using UnityEngine;
using System;
using itk.simple;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using openDicom.Image;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

// #TODO

/*
namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// </summary>
    public class SimpleITKImageFileImporter : IImageFileImporter
    {

        // nothing (just for IImageFileImporter)
        public async Task<VolumeDataset> ImportAsync(string filePath)
        {
            // Create dataset
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();
            return volumeDataset;
        }


        public VolumeDataset Import(string filePath)
        {
            ImageFileReader reader = new ImageFileReader();

            reader.SetFileName(filePath);

            Image image = reader.Execute();

            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            VectorUInt32 size = image.GetSize();

            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)size[dim];

            // Read pixel data
            float[] pixelData = new float[numPixels];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

            VectorDouble spacing = image.GetSpacing();

            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();
            volumeDataset.data = pixelData;
            volumeDataset.dimX = (int)size[0];
            volumeDataset.dimY = (int)size[1];
            volumeDataset.dimZ = (int)size[2];
            volumeDataset.datasetName = "test";
            volumeDataset.filePath = filePath;
            volumeDataset.scaleX = (float)(spacing[0] * size[0]);
            volumeDataset.scaleY = (float)(spacing[1] * size[1]);
            volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

            volumeDataset.FixDimensions();

            return volumeDataset;
        }
        public async Task<(VolumeDataset, bool)> ImportAsync(string filePath, string datasetName)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorDouble spacing = null;
            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();
            bool isDatasetReversed = true;


            await Task.Run(() =>
            {
                ImageFileReader reader = new ImageFileReader();

                reader.SetFileName(filePath);

                Image image = reader.Execute();

                // Cast to 32-bit float
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

                size = image.GetSize();

                int numPixels = 1;
                for (int dim = 0; dim < image.GetDimension(); dim++)
                    numPixels *= (int)size[dim];

                // Read pixel data
                pixelData = new float[numPixels];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

                spacing = image.GetSpacing();

                volumeDataset.data = pixelData.Reverse().ToArray();
                volumeDataset.dimX = (int)size[0];
                volumeDataset.dimY = (int)size[1];
                volumeDataset.dimZ = (int)size[2];

                volumeDataset.datasetName = datasetName;
                volumeDataset.filePath = filePath;
                volumeDataset.scaleX = (float)(spacing[0] * size[0]);
                volumeDataset.scaleY = (float)(spacing[1] * size[1]);
                volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

                volumeDataset.FixDimensions();
            });

            return (volumeDataset, isDatasetReversed);
        }
        public async Task ImportSegmentationAsync(string filePath, VolumeDataset volumeDataset, bool isDatasetReversed)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            Image image = null;
            ImageFileReader reader = null;

            await Task.Run(() =>
            {
                reader = new ImageFileReader();
                reader.SetFileName(filePath);
                image = reader.Execute();
            });

            uint numChannels = image.GetNumberOfComponentsPerPixel();

            if (numChannels > 1)
            {
                //ErrorNotifier.Instance.AddErrorMessageToUser($"Segmentation file in dataset named: {volumeDataset.datasetName} contains multiple layers. All segments must be in the same layer!!!");
                return;
            }

            await Task.Run(() =>
            {
                int segmentNumber = 0;
                List<string> metaDataKeys = reader.GetMetaDataKeys().ToList();

                while (true)
                {
                    string key = $"Segment{segmentNumber}_Name";
                    string keyValue = $"Segment{segmentNumber}_LabelValue";
                    if (metaDataKeys.Contains(key))
                    {
                        float segmentValue = float.Parse(reader.GetMetaData(keyValue), CultureInfo.InvariantCulture);
                        string segmentName = reader.GetMetaData(key);

                        volumeDataset.LabelNames.Add(segmentValue, segmentName);
                        segmentNumber++;
                    }
                    else
                    {
                        break;
                    }

                }


                // Cast to 32-bit float
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);


                size = image.GetSize();


                int numPixels = 1;
                for (int dim = 0; dim < image.GetDimension(); dim++)
                    numPixels *= (int)size[dim];

                // Read pixel data
                pixelData = new float[numPixels];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

                volumeDataset.labelData = isDatasetReversed ? pixelData.Reverse().ToArray() : pixelData;
                volumeDataset.labelDimX = (int)size[0];
                volumeDataset.labelDimY = (int)size[1];
                volumeDataset.labelDimZ = (int)size[2];
            });

        }
    }
}
*/



namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// </summary>
    public class SimpleITKImageFileImporter : IImageFileImporter
    {
        public VolumeDataset Import(string filePath)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorDouble spacing = null;

            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();

            ImportInternal(volumeDataset, pixelData, size, spacing, filePath);

            return volumeDataset;
        }
        public async Task<VolumeDataset> ImportAsync(string filePath)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorDouble spacing = null;

            // Create dataset
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();

            await Task.Run(() => ImportInternal(volumeDataset,pixelData,size,spacing,filePath));

            return volumeDataset;
        }

        private void ImportInternal(VolumeDataset volumeDataset, float[] pixelData, VectorUInt32 size, VectorDouble spacing,string filePath)
        {
            ImageFileReader reader = new ImageFileReader();

            reader.SetFileName(filePath);
            reader.SetImageIO("NrrdImageIO");
            Image image = reader.Execute();

            // Convert to LPS coordinate system (may be needed for NRRD and other datasets)
            SimpleITK.DICOMOrient(image, "LPS");
            
            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            size = image.GetSize();

            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)size[dim];

            // Read pixel data
            pixelData = new float[numPixels];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

            spacing = image.GetSpacing();

            volumeDataset.data = pixelData;
            volumeDataset.dimX = (int)size[0];
            volumeDataset.dimY = (int)size[1];
            volumeDataset.dimZ = (int)size[2];
            volumeDataset.datasetName = Path.GetFileName(filePath);
            volumeDataset.filePath = filePath;
            volumeDataset.scale = new Vector3(
                (float)(spacing[0] * size[0]) / 1000.0f, // mm to m
                (float)(spacing[1] * size[1]) / 1000.0f, // mm to m
                (float)(spacing[2] * size[2]) / 1000.0f // mm to m
            );

            // Convert from LPS to Unity's coordinate system
            ImporterUtilsInternal.ConvertLPSToUnityCoordinateSpace(volumeDataset);

            volumeDataset.FixDimensions();
        }
        
        //[BurstCompile]
        public struct DivideLabelMapLayers : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> allLayersData;
            [ReadOnly] public int currentLayer;
            [ReadOnly] public int numberOfLayers;
            [WriteOnly] public NativeArray<float> layerData;


            public void Execute(int index)
            {
                int reversedIndex = allLayersData.Length - (index * numberOfLayers) - (numberOfLayers - currentLayer);
                layerData[index] = allLayersData[reversedIndex];   //Also reversing the data array
            }
        }

        public async Task ImportSegmentationAsync(string filePath, VolumeDataset volumeDataset)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            Image image = null;
            ImageFileReader reader = null;
            int numOfChannels = 1;

            await Task.Run(() =>
            {
                reader = new ImageFileReader();
                reader.SetImageIO("NrrdImageIO");
                reader.SetFileName(filePath);
                image = reader.Execute();
            });

            await Task.Run(() =>
            {
                int segmentNumber = 0;
                List<string> metaDataKeys = reader.GetMetaDataKeys().ToList();
                numOfChannels = (int)image.GetNumberOfComponentsPerPixel();

                for (int i = 0; i < numOfChannels; i++)
                {
                    volumeDataset.LabelNames.Add(new Dictionary<float, string>());
                    volumeDataset.LabelValues.Add(new Dictionary<float, float>());
                }

                while (true)
                {
                    string key = $"Segment{segmentNumber}_Name";
                    string keyValue = $"Segment{segmentNumber}_LabelValue";
                    string layerValue = $"Segment{segmentNumber}_Layer";

                    if (metaDataKeys.Contains(key))
                    {
                        float segmentValue = float.Parse(reader.GetMetaData(keyValue), CultureInfo.InvariantCulture);
                        string segmentName = reader.GetMetaData(key);
                        int layer = int.Parse(reader.GetMetaData(layerValue));

                        volumeDataset.LabelNames[layer].Add(segmentValue, segmentName);
                        Debug.Log("LAYER: " + layer + " SEGMENTVALUE: " + segmentValue);
                        Debug.Log("LAYER: " + layer + " SEGMENTNAME: " + segmentName);
                        segmentNumber++;
                    }
                    else
                    {
                        break;
                    }
                }

                Debug.Log("number of channgels: " + numOfChannels);
                for (int i = 0; i < volumeDataset.LabelNames.Count(); i++)
                {
                    Debug.Log("volumeDataset.LabelNames <keys>: " + i + " " + volumeDataset.LabelNames[i].Keys);
                    Debug.Log("volumeDataset.LabelNames: <values> " + i + " " + volumeDataset.LabelNames[i].Values);
                    Debug.Log("volumeDataset.LabelValues: " + i + " " + volumeDataset.LabelValues[i]);

                }
                // Cast to 32-bit float
                /*
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkVectorFloat32);
                size = image.GetSize();

                int numPixels = numOfChannels;

                for (int dim = 0; dim < image.GetDimension(); dim++)
                    numPixels *= (int)size[dim];

                // Read pixel data
                pixelData = new float[numPixels];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);
                */
            });

            // #TODO ERROR
            //if (numOfChannels > 8)
            //    ErrorNotifier.Instance.AddErrorMessageToUser("Label map contains more than 8 layers, which is not supported.");

            /*
            NativeArray<float> pixelDataNative = new NativeArray<float>(pixelData, Allocator.TempJob);
            NativeArray<float>[] labelData = new NativeArray<float>[numOfChannels];
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(numOfChannels, Allocator.TempJob);
            
            int layerDataSize = pixelData.Length / numOfChannels;
            
            
            for (int i = 0; i < numOfChannels; i++)
            {
                labelData[i] = new NativeArray<float>(layerDataSize, Allocator.Persistent);
            
                DivideLabelMapLayers divideJob = new DivideLabelMapLayers()
                {
                    allLayersData = pixelDataNative,
                    currentLayer = i,
                    numberOfLayers = numOfChannels,
                    layerData = labelData[i]
                };
            
                handles[i] = divideJob.Schedule(layerDataSize, 64);
            
            }
            
            JobHandle combinedHandles = JobHandle.CombineDependencies(handles);
            
            while (!combinedHandles.IsCompleted)
                await Task.Delay(1000);
            
            combinedHandles.Complete();
            
            volumeDataset.nativeLabelData = labelData;
            volumeDataset.labelDimX = (int)size[0];
            volumeDataset.labelDimY = (int)size[1];
            volumeDataset.labelDimZ = (int)size[2];
            volumeDataset.HowManyLabelMapLayers = numOfChannels;
            
            pixelDataNative.Dispose();
            handles.Dispose();
            */
        }

    }
}


//#endif
