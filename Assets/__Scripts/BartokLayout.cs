using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

// ����� SlotDef �� ��������� MonoBehaviour, ������� ��� ���� �� ���������
//  ��������� ��������� ���� �� C#
[System.Serializable] // ������� ��������� SlotDef ������� � ���������� Unity
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>(); // �� ������������ � Bartok
    public float rot; // ������� � ����������� �� ������
    public string type = "slot";
    public Vector2 stagger;
    public int player;  // ���������� ����� ������
    public Vector3 pos;  // ����������� �� ������ x,y � multiplier
}

public class BartokLayout : MonoBehaviour
{
    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml; // ������������ ��� ��������� ������� � xml
    public Vector2 multiplier; // �������� � ���������
    // ������ �� SlotDef
    public List<SlotDef> slotDefs; // ������ SlotDef ��� �������
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;

    // ������ ����� BartokLayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText); // ��������� XML
        xml = xmlr.xml["xml"][0]; // ���������� xml ��� ��������� ������� � XML

        // ��������� ���������, ������������ ���������� ����� �������
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), NumberStyles.Float, new CultureInfo("en-US"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), NumberStyles.Float, new CultureInfo("en-US"));

        // ��������� �����
        SlotDef tSD;
        // slotsX ������������ ��� ��������� ������� � ��������� <slot>
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();
            if (slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                tSD.type = "slot";
            }
            tSD.x = float.Parse(slotsX[i].att("x"), NumberStyles.Float, new CultureInfo("en-US"));
            tSD.y = float.Parse(slotsX[i].att("y"), NumberStyles.Float, new CultureInfo("en-US"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch (tSD.type)
            {
                case "slot":
                    // �����
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), NumberStyles.Float, new CultureInfo("en-US"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "target":
                    target = tSD;
                    break;
                case "hand":
                    tSD.player = int.Parse(slotsX[i].att("player")); 
                    tSD.rot = int.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}
