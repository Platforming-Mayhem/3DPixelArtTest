using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] expandedObjects;

    public GameObject[] slots;

    public NewsReportInfo info;

    public AudioClip submitWorked;

    public AudioClip noSubmit;

    public AudioClip expandSFX;

    public AudioClip pinSFX;

    [HideInInspector]
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        info.information = new List<string>();
        isSelected = false;
        foreach (GameObject a in expandedObjects)
        {
            a.SetActive(false);
        }
        audioSource = GetComponent<AudioSource>();
    }

    public GameObject previouslyDropped;

    public bool isSelected;

    public void Rearrange()
    {
        Dropable a = previouslyDropped.GetComponent<Dropable>();
        Dropable[] dropable = FindObjectsOfType<Dropable>();
        foreach (Dropable d in dropable)
        {
            if (d.gameObject != previouslyDropped && d.originalParent == a.originalParent)
            {
                d.index = -1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool CheckScript()
    {
        try
        {
            Dropable drop = FindObjectOfType<Dropable>();
            if (drop.choices.transform.childCount >= 3)
            {
                Debug.Log("Can submit!");
                return true;
            }
            else
            {
                Debug.Log("Can't submit");
                return false;
            }
        }
        catch
        {
            Debug.Log("Error: Can't submit");
            return false;
        }
    }

    IEnumerator LoadNextLevel(string sceneName)
    {
        yield return new WaitForSeconds(0.47f);
        SceneManager.LoadScene(sceneName);
    }

    public void SubmitButton(string sceneName)
    {
        bool canSubmit = CheckScript();
        if (canSubmit)
        {
            
            GameObject choices = FindObjectOfType<Dropable>().choices;
            foreach(Transform child in choices.transform)
            {
                TMP_Text text = child.GetComponentInChildren<TMP_Text>();
                info.information.Add(text.text);
            }
            PlayerPrefs.SetInt("index", 1);
            PlayerPrefs.SetInt("changePos", 1);
            audioSource.PlayOneShot(submitWorked);
            StartCoroutine(LoadNextLevel(sceneName));
        }
        else
        {
            audioSource.PlayOneShot(noSubmit);
        }
    }

    public void Expand(int index)
    {
        foreach (GameObject a in expandedObjects)
        {
            a.SetActive(false);
        }
        audioSource.PlayOneShot(expandSFX);
        expandedObjects[index].SetActive(true);
    }
}
