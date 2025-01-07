using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LoadTranferFunctionControls : MonoBehaviour
{
    [SerializeField] GameObject _solution;

    [SerializeField] GameObject _parentObject;
    [SerializeField] GameObject _baseObject;

    public List<string> _datasetDirectories = new List<string>(0);
    public string _baseDirectory = "";

    public string _currentDataset = "";

    public string _datasetPath = "";

    public List<GameObject> _buttons = new List<GameObject>();

    public void init()
    {

        for (int i = 0; i < _datasetDirectories.Count; i++)
        {
            List<string> words = _datasetDirectories[i].Split('/').ToList();

            GameObject test = Instantiate(_baseObject);
            string _name = words[words.Count - 1];

            // filter names that dont contain "meta" 
            List<string> _subNames = _name.Split('.').ToList();

            if (_subNames[_subNames.Count - 1] == "meta")
                continue;

            _name = _subNames[_subNames.Count - 2];


            test.GetComponentInChildren<TMP_Text>().text = _name;
            test.transform.SetParent(_parentObject.transform, false);
            test.SetActive(true);
            test.GetComponent<PressableButton>().OnClicked.AddListener(() =>
            {
                test.GetComponent<PressableButton>().enabled = false;

                Debug.Log(test.GetComponentInChildren<TMP_Text>().text);
                _currentDataset = test.GetComponentInChildren<TMP_Text>().text;
                _datasetPath = _baseDirectory + _currentDataset;

                for (int i = 0; i < _buttons.Count; i++)
                {
                    string t = _buttons[i].GetComponentInChildren<TMP_Text>().text;
                    if (t != _currentDataset)
                    {
                        _buttons[i].GetComponent<PressableButton>().ForceSetToggled(false);
                        _buttons[i].GetComponent<PressableButton>().enabled = true;
                    }
                }

                //loadDataset();
                loadTF();

            });
            _buttons.Add(test);

        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].GetComponent<PressableButton>().ForceSetToggled(false);
            _buttons[i].GetComponent<PressableButton>().enabled = true;
            //_buttons[i].GetComponent<PressableButton>().
        }

        // #TODO ugly setting default tf to appear as active in TF select window
        _buttons[1].GetComponent<PressableButton>().ForceSetToggled(true);

    }

    public void _datasetChosen()
    {
        //_datasetPath = _baseDirectory + _currentDataset;


        // disable all dataset
        //for (int i = 0; i < _buttons.Count; i++)
        //{
        //    string t = _buttons[i].GetComponentInChildren<TMP_Text>().text;
        //    if (t != _currentDataset) _buttons[i].GetComponent<PressableButton>().ForceSetToggled(false);
        //}

        // enable (and load) chosen one
        //for (int i = 0; i < _buttons.Count; i++)
        //{
        //    string t = _buttons[i].GetComponentInChildren<TMP_Text>().text;
        //    if (t == _currentDataset) _buttons[i].GetComponent<PressableButton>().ForceSetToggled(true);
        //}

        // load the dataset


    }


    void loadDataset()
    {
        _solution.GetComponent<VolumeSolution>().LoadDataset(_datasetPath);
    }

    void loadTF()
    {
        _solution.GetComponent<VolumeSolution>().LoadTF(_datasetPath);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
