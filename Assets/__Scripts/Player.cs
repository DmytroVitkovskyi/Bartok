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
    public List<CardBartok> hand; //  арты в руках игрока

    // ƒобавл€ет карту в руки
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();
        hand.Add(eCB);

        // ≈сли это человек, отсортировать карты по достоинству с помощью LINQ
        if (type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();

            // Ёто вызов LINQ
            cards = cards.OrderBy(cd => cd.rank).ToArray();

            hand = new List<CardBartok>(cards);
        }

        eCB.SetSortingLayerName("10"); // ѕеренести перемещаемую карту в верхний
        // слой
        eCB.eventualSortLayer = handSlotDef.layerName;

        FanHand();
        return eCB;
    }

    // ”дал€ет карту из рук
    public CardBartok RemoveCard(CardBartok cb)
    {
        if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return cb;
    }
    public void FanHand()
    {
        // startRot - угол поворота первой карты относительно оси Z
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }

        // ѕереместить все карты в новые позиции
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);
            
            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

            pos = rotQ * pos;

            // ѕрибавить координаты позиции руки игрока
            //  (внизу в центре веера карт)
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            // ≈сли это не начальна€ раздача, начать перемещение карты немедленно
            if (Bartok.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }

            // ”становить локальную позицию и поворот i-й карты в руках
            hand[i].MoveTo(pos, rotQ); // —ообщить карте, что она должна
                                       //   начать интерпол€цию
            hand[i].state = CBState.toHand;
            // «акончив перемещение, карта запишет в поле state значение
            // CBState.hand

            /* // ”становить локальную позицию и поворот i-й карты в руках
            hand[i].transform.localPosition = pos;
            hand[i].transform.rotation = rotQ;
            hand[i].state = CBState.hand;
             */

            hand[i].faceUp = (type == PlayerType.human);

            // ”становить SortOrder карт, чтобы обеспечить правильное перекрытие
            hand[i].eventualSortOrder = i * 4;
            //hand[i].SetSortOrder(i * 4);
        }
    }

    // –еализует »» дл€ игроков, управл€емых компьютером
    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn");

        // ничего не делать дл€ игрока человека
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
            // ... вз€ть карту
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }
        //  у нас есть карты, которыми можно сыграть, выбираем карту
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallBack(CardBartok tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        //  арта завершила перемещение, передать право хода
        Bartok.S.PassTurn();
    }
}
