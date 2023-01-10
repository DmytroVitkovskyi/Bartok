using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class Bartok : MonoBehaviour
{
    static public Bartok S;
    static public Player CURRENT_PLAYER;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 10f;
    public int numStartingCards = 7;
    public float drawTimeStagger = 0.1f;

    [Header("Set Dynamically")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;
    public List<Player> players;
    public CardBartok targetCard;
    public TurnPhase phase = TurnPhase.idle;

    private BartokLayout layout;
    private Transform layoutAnchor;

    private void Awake()
    {
        S = this;
    }

    private void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<BartokLayout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = UpgradeCardList(deck.cards);
        LayoutGame();
    }

    List<CardBartok> UpgradeCardList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (var item in lCD)
        {
            lCB.Add(item as CardBartok);
        }
        return lCB;
    }

    // Позиционирует все карты в draw
    public void ArrangeDrawPile()
    {
        CardBartok tCB;
        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;
            // Угол поворота начинается с 0
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4); // упорядочить от первых к последним
            tCB.state = CBState.drawpile;
        }
    }

    // Выполняет первоначальную раздачу карт в игре
    void LayoutGame()
    {
        // Создать пустой GameObject - точку привязки для раскладки
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        // Позиционировать свободные карты
        ArrangeDrawPile();

        // Настроить игроков
        Player pl;
        players = new List<Player>();
        foreach (var item in layout.slotDefs)
        {
            pl = new Player();
            pl.handSlotDef = item;
            players.Add(pl);
            pl.playerNum = item.player;
        }
        players[0].type = PlayerType.human; // 0-й игрок - человек

        CardBartok tCB;
        // Раздать игрокам по 7 карт
        for (int i = 0; i < numStartingCards; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tCB = Draw(); // Снять карту
                // Немного отложить начало перемещения карты.
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);

                players[(j + 1) % 4].AddCard(tCB);
            }
        }

        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
    }

    // Снимает верхнюю карту со стопки свободных карт и возвращает её
    public CardBartok Draw()
    {
        CardBartok cd = drawPile[0];

        if (drawPile.Count == 0)
        {
            int ndx;
            while (discardPile.Count>0)
            {
                ndx = Random.Range(0, discardPile.Count);
                drawPile.Add(discardPile[ndx]);
                discardPile.RemoveAt(ndx);
            }
            ArrangeDrawPile();
            // Показать перемещение карт в стопку свободных карт
            float t = Time.time;
            foreach (var item in drawPile)
            {
                item.transform.localPosition = layout.discardPile.pos;
                item.callbackPlayer = null;
                item.MoveTo(layout.drawPile.pos);
                item.timeStart = t;
                t += 0.02f;
                item.state = CBState.toDrawpile;
                item.eventualSortLayer = "0";
            }
        }

        drawPile.RemoveAt(0);
        return cd;
    }

    // ВРеменно используется для проверки добавления карты в руки игрока
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            players[0].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            players[1].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            players[2].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            players[3].AddCard(Draw());
        }
    }
    */

    public void DrawFirstTarget()
    {
        // Перевернуть первую целевую карту лицевой стороной вверх
        CardBartok tCB = MoveToTarget(Draw());
        // Вызвать метод CBCallback сценария Bartok, когда карта закончит
        // перемещение
        tCB.reportFinishTo = this.gameObject;
    }

    // Делает указанную карту целевой
    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null)
        {
            MoveToDiscard(targetCard);
        }

        targetCard = tCB;

        return tCB;
    }

    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

        return tCB;
    }

    // Этот обратный вызов используется последней розданной картой в начале игры
    public void CBCallback(CardBartok cb)
    {
        // Иногда желательно сообщить о вызове метода, как здесь
        Utils.tr("Bartok:CBCallback()", cb.name);
        StartGame(); // Начать игру
    }

    public void StartGame()
    {
        // Право первого хода принадлежит игроку слева от человека.
        PassTurn(1);
        //
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var item in gos)
        {
            if (item.name == "TurnLight")
            {
                item.SetActive(true);
                break;
            }
        }
    }

    public void PassTurn(int num = -1)
    {
        // Если порядковый номер игрока не указан, выбрать следующего по кругу
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;
            // проверить завершение игры и необходимость перетасовать
            //  стопку сброшенных карт
            if (CheckGameOver())
            {
                return;
            }
        }
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;

        CURRENT_PLAYER.TakeTurn();

        // Сообщить о передаче хода
        Utils.tr("Bartok:PassTurn()", "Old: " + lastPlayerNum,
            "New: " + CURRENT_PLAYER.playerNum);
    }

    // Проверяет возможность сыграть выбранной картой
    public bool ValidPlay(CardBartok cb)
    {
        // Картой можно сыграть, если она имеет такое же достоинство,
        //  как целевая карта
        if (cb.rank == targetCard.rank)
        {
            return true;
        }
        // Картой можно сыграть, если её масть совпадает с мастью целевой карты
        if (cb.suit == targetCard.suit)
        {
            return true;
        }
        // Иначе  вернуть false
        return false;
    }
    public void CardClicked(CardBartok tCB)
    {
        if (CURRENT_PLAYER.type != PlayerType.human)
        {
            return;
        }
        if (phase == TurnPhase.waiting)
        {
            return;
        }

        switch (tCB.state)
        {
            case CBState.drawpile:
                // Взять верхнюю карту, не обязательно ту,
                //   по которой выполнен щелчок.
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr("Bartok:CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;
            case CBState.hand:
                // Проверить допустимость выбранной карты
                if (ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr("Bartok:CardClicked()", "Play", tCB.name, targetCard.name + " is target");
                    phase = TurnPhase.waiting;
                }
                else
                {
                    // Игнорировать выбор недопустимой карты,
                    //  но сообщить о попытке игрока
                    Utils.tr("Bartok:CardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
                }
                break;
        }
    }

    public bool CheckGameOver()
    {
        // Проверить, нужно ли перетасовать стопку сброшенных карт и
        //   перенести её в стопку свободных карт
        if (drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (var item in discardPile)
            {
                cards.Add(item);
            }
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardList(cards);
            ArrangeDrawPile();
        }

        // Проверить победу текущего игрока
        if (CURRENT_PLAYER.hand.Count == 0)
        {
            // Играк, только что сделавший ход, победил!
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 3);
            return true;
        }
        return false;
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("__Bartok_Scene_0");
    }
}
