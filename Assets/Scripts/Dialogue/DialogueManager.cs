using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    // private Animator layoutAnimator;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Globals Ink File")]
    [SerializeField] private TextAsset globalsvar;
    [SerializeField] private Story currentStory;
    public bool dialogueIsPlaying ;

    protected static DialogueManager instance;
    [SerializeField] CanvasManagement canvas;

    private const string LAYOUT_TAG = "layout";

    [SerializeField] public DialogueVariables dialogueVariables;

    private void Awake() 
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;


        dialoguePanel = GameObject.Find("DialogueBox");
        dialogueText = GameObject.Find("diagText").GetComponent<TextMeshProUGUI>();
        canvas = FindObjectOfType<CanvasManagement>();
        choices[0] = GameObject.Find("Choice0");
        choices[1] = GameObject.Find("Choice1");
        choices[2] = GameObject.Find("Choice2");


        dialogueVariables = new DialogueVariables(globalsvar);
        Debug.Log(dialogueVariables);
    }

    public static DialogueManager GetInstance() 
    {
        return instance;
    }

    private void Start() 
    {
        dialogueIsPlaying = false;

        // get all of the choices text 
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices) 
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update() 
    {
        if (Input.GetButtonDown("Jump"))
            Debug.Log("HI");



        // return right away if dialogue isn't playing
        if (!dialogueIsPlaying) 
        {
            return;
        }

        // handle continuing to the next line in the dialogue when submit is pressed
        // NOTE: The 'currentStory.currentChoiecs.Count == 0' part was to fix a bug after the Youtube video was made
        if (currentStory.currentChoices.Count == 0)
        {
            if(Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Jump"))
            {
                Debug.Log("Continuing");
                ContinueStory();
                if (SceneManager.GetActiveScene().name == "Train")
                {
                    Debug.Log(dialogueText);
                }
            }
        }    
    }

    public void EnterDialogueMode(TextAsset inkJSON) 
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        canvas.DialogueAppear(1.0f);
        Debug.Log("Appearing");

        Debug.Log(inkJSON.text);

        dialogueVariables.StartListening(currentStory);
        // reset portrait, layout, and speaker
        
        ContinueStory();
    }

    private IEnumerator ExitDialogueMode() 
    {
        canvas.DialogueDisappear(1.0f);
        yield return new WaitForSeconds(0.2f);
        dialogueVariables.StopListening(currentStory);
        dialogueIsPlaying = false;
        dialogueText.text = "";
        if(SceneManager.GetActiveScene().name == "Tutorial")
        {
            SceneManager.LoadScene("Day");
        }


        Debug.Log("Finished");
    }

    private void ContinueStory() 
    {
        if (currentStory.canContinue) 
        {
            // set text for the current dialogue line
            dialogueText.text = currentStory.Continue();
            Debug.Log(dialogueText);
            // display choices, if any, for this dialogue line
            DisplayChoices();
            // handle tags
            HandleTags(currentStory.currentTags);
        }
        else 
        {
            StartCoroutine(ExitDialogueMode());
        }
    }


    public void SetVariableState(string variableName, Ink.Runtime.Object variableValue)
    {
        Debug.Log(variableName);
        Debug.Log(variableValue);
        Debug.Log(dialogueVariables);
        if (dialogueVariables.variables.ContainsKey(variableName))
        {
            dialogueVariables.variables.Remove(variableName);
            dialogueVariables.variables.Add(variableName, variableValue);
        }
        else
        {
            Debug.LogWarning("Tried to update variable that wasn't initialized by globals.ink: " + variableName);
        }
    }
    private void HandleTags(List<string> currentTags)
    {
        // loop through each tag and handle it accordingly
        foreach (string tag in currentTags) 
        {
            // parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2) 
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();
            
            // handle the tag
            switch (tagKey) 
            {
                case LAYOUT_TAG:
                    if(tagValue == "left")
                    {
                        SpriteRenderer spriteRenderer = dialoguePanel.GetComponent<SpriteRenderer>();
                        spriteRenderer.flipX = true;
                    } else if(tagValue == "right")
                    {
                        SpriteRenderer spriteRenderer = dialoguePanel.GetComponent<SpriteRenderer>();
                        spriteRenderer.flipX = false;
                    }
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }

    private void DisplayChoices() 
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // defensive check to make sure our UI can support the number of choices coming in
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " 
                + currentChoices.Count);
        }

        int index = 0;
        // enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach(Choice choice in currentChoices) 
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }
        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < choices.Length; i++) 
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice() 
    {
        // Event System requires we clear it first, then wait
        // for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        // NOTE: The below two lines were added to fix a bug after the Youtube video was made
        ContinueStory();
    }

}