using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundResultUI : MonoBehaviour
{
    private TextMeshProUGUI txt;

    private void Awake()
    {
        txt = GetComponent<TextMeshProUGUI>();
        txt.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Bartok.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }
        // в эту точку мы попадём, только когда игра завершилась
        Player cP = Bartok.CURRENT_PLAYER;
        if (cP == null || cP.type == PlayerType.human)
        {
            txt.text = "";
        }
        else
        {
            txt.text = "Player " + cP.playerNum + " won!";
        }
    }
}
