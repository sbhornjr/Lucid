using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public TextMeshProUGUI speechBox;
    public EntityManager entityManager;

    private const string LVL1dialogue1 = "\"Well, you’ve certainly met with an unfortunate fate, haven’t you?\"";
    private const string LVL1dialogue2 = "\"You don't remember how you got here do you? That's what I thought. You never do.\"";
    private const string LVL1dialogue3 = "\"For now, just know that you’re going to have to fight. And if you want to get out of here, you’re going to have to win.\"";
    private const string LVL1dialogue4 = "\"Don’t worry too much about dying -- Sure, you’ll be put through extreme amounts of pain, but if your heart stops I’ll just have you wake up right where you started. My treat!\"";
    private const string LVL1dialogue5 = "\"Try not to get lost. Au revoir!\"";

    private const string LVL2dialogue1 = "\"Now that wasn't terribly challenging, was it? You aren't nearly as broken as I would like...\"";
    private const string LVL2dialogue2 = "\"I suppose I shouldn't be too disappointed. The aesthetic of that playground doesn't nearly live up to the modern touch I've breathed into this place!\"";
    private const string LVL2dialogue3 = "\"I even added in some little agents of torment that I am sure you will love to navigate through...\"";
    private const string LVL2dialogue4 = "\"Let's try not to get through this one so quickly now, okay?\"";
    private const string LVL2dialogue5 = "\"xoxo\"";

    private const string LVL3dialogue1 = "\"Well, this isn't going terribly according to plan. You were supposed to die in there, over and over again, forever.\"";
    private const string LVL3dialogue2 = "\"Listen, I don't want to be rude. I like what we have! But if you keep going, then I'm going to have to stop relying on these puppets to stop you.\"";
    private const string LVL3dialogue3 = "\"I know what I said in the beginning came aross as a challenge, but you should know that I never intended you to get this close to me.\"";
    private const string LVL3dialogue4 = "\"I may not be so polite in the flesh.\"";

    private const string LVL4dialogue1 = "\"You really think you can fight your way back to your old life?\"";
    private const string LVL4dialogue2 = "\"You can't just decide that you deserve better. I make the rules. And I declare that you're going to stay here, forever.\"";
    private const string LVL4dialogue3 = "\"You've brought this on yourself.\"";

    public static string[] LVL1Dialogue = new string[] { LVL1dialogue1, LVL1dialogue2, LVL1dialogue3, LVL1dialogue4, LVL1dialogue5 };
    public static string[] LVL2Dialogue = new string[] { LVL2dialogue1, LVL2dialogue2, LVL2dialogue3, LVL2dialogue4, LVL2dialogue5 };
    public static string[] LVL3Dialogue = new string[] { LVL3dialogue1, LVL3dialogue2, LVL3dialogue3, LVL3dialogue4 };
    public static string[] LVL4Dialogue = new string[] { LVL4dialogue1, LVL4dialogue2, LVL4dialogue3 };

    public static uint currentDialogueIndex = 0;
    private uint currentSpeechIndex = 0;
    private string[] currentDialogue;

    private void Awake()
    {
        entityManager = FindObjectOfType<EntityManager>();
        //currentDialogue = LVL1Dialogue;
    }

    private void Update()
    {
        switch (currentDialogueIndex)
        {
            case 0:
                currentDialogue = LVL1Dialogue;
                break;
            case 1:
                currentDialogue = LVL2Dialogue;
                break;
            case 2:
                currentDialogue = LVL3Dialogue;
                break;
            default:
                currentDialogue = LVL4Dialogue;
                break;
        }
        speechBox.text = currentDialogue[currentSpeechIndex];

        if (Input.GetKeyDown("r") && gameObject.activeSelf)
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        
        if (currentSpeechIndex + 1 < currentDialogue.Length)
        {
            currentSpeechIndex++;
            speechBox.text = currentDialogue[currentSpeechIndex];
        }
    }

    public void LevelSetup(GameStateMachine.Level level)
    {
        switch (level)
        {
            case GameStateMachine.Level.Dungeon:
                currentDialogueIndex = 0;
                break;
            case GameStateMachine.Level.City:
                currentDialogueIndex = 1;
                break;
            case GameStateMachine.Level.Hell:
                currentDialogueIndex = 2;
                break;
            default:
                currentDialogueIndex = 3;
                break;
        }
        currentSpeechIndex = 0;
        entityManager.currentState = EntityManager.MovementStates.PlayerInMenu;
        gameObject.SetActive(true);
        entityManager.menuUI = gameObject;
    }
}
