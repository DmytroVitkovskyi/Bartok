using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerType
{
    human,
    ai
}
[System.Serializable]
public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardBartok> hand; // ????? ? ????? ??????

    // ????????? ????? ? ????
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();
        hand.Add(eCB);

        // ???? ??? ???????, ????????????? ????? ?? ??????????? ? ??????? LINQ
        if (type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();

            // ??? ????? LINQ
            cards = cards.OrderBy(cd => cd.rank).ToArray();

            hand = new List<CardBartok>(cards);
        }

        eCB.SetSortingLayerName("10"); // ????????? ???????????? ????? ? ???????
        // ????
        eCB.eventualSortLayer = handSlotDef.layerName;

        FanHand();
        return eCB;
    }

    // ??????? ????? ?? ???
    public CardBartok RemoveCard(CardBartok cb)
    {
        if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return cb;
    }
    public void FanHand()
    {
        // startRot - ???? ???????? ?????? ????? ???????????? ??? Z
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }

        // ??????????? ??? ????? ? ????? ???????
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);
            
            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

            pos = rotQ * pos;

            // ????????? ?????????? ??????? ???? ??????
            //  (????? ? ?????? ????? ????)
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            // ???? ??? ?? ????????? ???????, ?????? ??????????? ????? ??????????
            if (Bartok.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }

            // ?????????? ????????? ??????? ? ??????? i-? ????? ? ?????
            hand[i].MoveTo(pos, rotQ); // ???????? ?????, ??? ??? ??????
                                       //   ?????? ????????????
            hand[i].state = CBState.toHand;
            // ???????? ???????????, ????? ??????? ? ???? state ????????
            // CBState.hand

            /* // ?????????? ????????? ??????? ? ??????? i-? ????? ? ?????
            hand[i].transform.localPosition = pos;
            hand[i].transform.rotation = rotQ;
            hand[i].state = CBState.hand;
             */

            hand[i].faceUp = (type == PlayerType.human);

            // ?????????? SortOrder ????, ????? ?????????? ?????????? ??????????
            hand[i].eventualSortOrder = i * 4;
            //hand[i].SetSortOrder(i * 4);
        }
    }

    // ????????? ?? ??? ???????, ??????????? ???????????
    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn");

        // ?????? ?? ?????? ??? ?????? ????????
        if (type == PlayerType.human) return;

        CardBartok cb;

        List<CardBartok> validCards = new List<CardBartok>();
        foreach (var item in hand)
        {
            if (Bartok.S.ValidPlay(item))
            {
                validCards.Add(item);
            }
        }
        if (validCards.Count == 0)
        {
            // ... ????? ?????
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }
        //  ? ??? ???? ?????, ???????? ????? ???????, ???????? ?????
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallBack(CardBartok tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        // ????? ????????? ???????????, ???????? ????? ????
        Bartok.S.PassTurn();
    }
}
