using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    private TextMeshProUGUI txt;

    private void Awake()
    {
        txt = GetComponent<TextMeshProUGUI>();
        txt.text = "";
    }

    private void Update()
    {
        if (Bartok.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }
        // в эту точку мы попадём, только когда игра завершилась
        if (Bartok.CURRENT_PLAYER == null)
        {
            return;
        }
        if (Bartok.CURRENT_PLAYER.type == PlayerType.human)
        {
            txt.text = "You won!";
        }
        else
        {
            txt.text = "Game Over";
        }
    }
}
