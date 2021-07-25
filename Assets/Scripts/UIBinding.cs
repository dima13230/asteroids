using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIBinding : MonoBehaviour
{

    public Text ScoresLabel;
    public Text LifesLabel;
    public GameObject MenuObject;
    public Button ResumeButton;
    public Text ControlTypeButtonText;

    // Пусть будет так, чтобы не было нужды каждый раз обращаться к менеджеру через GameManager.Instance
    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        ScoresLabel.text = "Scores: " + gameManager.Scores.ToString();
        LifesLabel.text = "Lifes: " + gameManager.Lifes.ToString();

        if (gameManager.CurrentGameState == GameManager.GameState.Paused || gameManager.CurrentGameState == GameManager.GameState.MainMenu)
        {
            if(!MenuObject.activeInHierarchy)
                MenuObject.SetActive(true);
        }
        else
        {
            if (MenuObject.activeInHierarchy)
                MenuObject.SetActive(false);
        }

        if (gameManager.CurrentGameState == GameManager.GameState.MainMenu)
            ResumeButton.interactable = false;
        else
            ResumeButton.interactable = true;
    }

    public void ControlTypeButtonPress()
    {
        gameManager.SwitchControlType();

        string controlScheme = gameManager.Spaceship.SpaceshipControlType == SpaceshipController.ControlType.Keyboard ? "Keyboard" : "Keyboard + Mouse";

        ControlTypeButtonText.text = "Control: " + controlScheme;
    }
}
