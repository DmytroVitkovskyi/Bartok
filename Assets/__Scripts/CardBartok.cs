using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 
public enum CBState
{
    toDrawpile,
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card
{
    // Статические переменные совместно используются всеми экземплярами CardBartok
    static public float MOVE_DURATION = 0.5f;
    static public string MOVE_EASING = Easing.InOut;
    static public float CARD_HEIGHT = 3.5f;
    static public float CARD_WIDTH = 2f;

    [Header("Set Dynamically: CardBartok")]
    public CBState state = CBState.drawpile;

    // Поля с информацией, необходимой для перемещения и поворачивания карты
    public List<Vector3> bezierPts;
    public List<Quaternion> bezierRots;
    public float timeStart, timeDuration;

    // По завершении перемещения карты будет вызываться
    // reportFinishTo.SendMessage()
    public GameObject reportFinishTo = null;

    // Запускает перемещение карты в новое местоположение с заданным
    //  поворотом
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        // Создать новые списки для интерполяции.
        // Траектории перемещения и поворота определяются двумя точками каждая.
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition); // Текущее местоположение
        bezierPts.Add(ePos);  // Новое местоположение

        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);
        bezierRots.Add(eRot);

        if (timeStart == 0)
        {
            timeStart = Time.time;
        }
        // timeDuration всегда получает одно и то же значение, но потом
        //    это можно исправить
        timeDuration = MOVE_DURATION;
        state = CBState.to;
    }

    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }
    private void Update()
    {
        switch (state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.toDrawpile:
            case CBState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);
                if (u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                }
                else if (u >= 1)
                {
                    uC = 1;
                    // Перевести из состояния to... в соответствующее
                    //   следующее состояние
                    if (state == CBState.toHand) state = CBState.hand;
                    if (state == CBState.toTarget) state = CBState.target;
                    if (state == CBState.toDrawpile) state = CBState.drawpile;
                    if (state == CBState.to) state = CBState.idle;

                    // Переместить в конечное местоположение
                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierRots.Count - 1];

                    // Сбросить timeStart в 0, чтобы в следующий раз 
                    //   можно было установить текущее время
                    timeStart = 0;

                    if (reportFinishTo != null)
                    {
                        reportFinishTo.SendMessage("CBCallBack", this);
                        reportFinishTo = null;
                    }
                    else
                    { // Если ничего вызывать не надо
                        // Оставить как есть.
                    }
                }
                else
                { // Нормальный режим интерполяции (0 <= u < 1)
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;
                }
                break;
        }
    }
}
