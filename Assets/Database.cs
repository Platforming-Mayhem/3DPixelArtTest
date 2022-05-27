using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    public NewsDatabase newsDB;
    [SerializeField]
    List<int> bannedIDs = new List<int>();
    public DialogueManager diagM;
    public BarManager barM;
    private DialogueVariables dialogueVariables;

    private void Awake()
    {
        diagM = FindObjectOfType<DialogueManager>();
        barM = FindObjectOfType<BarManager>();
    }
    //gets news source based on the inputted source and current days alongside the selected categoryu
    public News GetSpecifiedNews(int days, string category)
    {
        List<News> potentialNews = new List<News>();
        foreach(News news in newsDB.allNews)
        {
            if(news.minDays == days && news.category == category)
            {
                if(!bannedIDs.Contains(news.ID))
                {
                    potentialNews.Add(news);
                } 
            }
        }
        if(potentialNews.Count <= 0 )
        {
            return(null);
        }
        int indexToSelect = Random.Range(0, potentialNews.Count);
        bannedIDs.Add(potentialNews[indexToSelect].ID);
        foreach( int x in bannedIDs){
            Debug.Log(x.ToString());
        }
        return(potentialNews[indexToSelect]);
    }

    public void CalculateBarChanges(Dropable[] dropList)
    {
        foreach(News newsP in newsDB.allNews)
        {
            foreach(Dropable drop in dropList)
            {
                if(drop.ID == newsP.ID)
                {
                    Debug.Log("Found a match.");
                    barM.AddAmountToBar(0, newsP.effectStr[0]);
                    barM.AddAmountToBar(1, newsP.effectStr[1]);
                    barM.AddAmountToBar(2, newsP.effectStr[2]);

                    int govD = PlayerPrefs.GetInt("Bar1");
                    int pubU = PlayerPrefs.GetInt("Bar2");
                    int pubO = PlayerPrefs.GetInt("Bar3");

                    Ink.Runtime.Object obj1 = new Ink.Runtime.IntValue(govD);
                    Ink.Runtime.Object obj2 = new Ink.Runtime.IntValue(pubU);
                    Ink.Runtime.Object obj3 = new Ink.Runtime.IntValue(pubO);

                    diagM.SetVariableState("gov_distrust", obj1);
                    diagM.SetVariableState("public_unrest", obj2);
                    diagM.SetVariableState("public_opinion", obj3);
                }
            }
        }
    }
}
