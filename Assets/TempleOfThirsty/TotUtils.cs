using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Windows;

public class TotUtils : MonoBehaviour
{
    // массив типов энергий 1-го ранга
    public int[] enRang1 = new int[] {
        6, 7, 8, 9, 10,
        26, 27, 28, 29, 30,
        46, 47, 48, 49, 50,
        76, 77, 78, 79, 80,
        91,
        105,
        112,
        136};
    // массив типов энергий 2-го ранга
    public int[] enRang2 = new int[] {
        1,2,3,4,5,
        20,21,22,23,24,
        41,42,43,44,45,
        61,62,63,64,65,
        92,93,94,95,96,
        106,107,108,109,110,
        116,117,118,119,120,
        131,132,133,134,135,
        141,142,143,144,145,146,147,148};

    public string[] activities = new string[]
    {
        "Строительство Храма",
        "Строительство оборудования",
        "Сбор материалов астероидов",
        "Сбор энергии на орбите людского мира",
        "Сбор энергии на планете/спутнике",
        "Поиск артефактов",
        "Создание снаряжения",
        "Призыв Монстров",
        "Охота на Монстров",
        "Охота на Ангелов/Демонов",
        "Охрана Храма",
        "Нападение на Храм",
        "Охота на Жаждущих",
        "Рассеивание энергии",
        "Развитие людского мира"
    };

    public int[][] activitiesEnergyTypes = new int[15][]
    {
        new int[] {6, 43, 110, 116, 73, 87},
        new int[] {46, 109, 117, 134, 74, 1},
        new int[] {27, 65, 118, 119, 15, 38, 121, 130},
        new int[] {10, 49, 50, 77, 107, 11, 36, 81, 82, 83, 84, 85, 86, 102, 103, 111, 113},
        new int[] {28, 76, 23, 96, 120, 131, 51, 67, 122, 123, 126},
        new int[] {136, 106, 132, 135, 147, 14, 37, 57, 127},
        new int[] {7, 141, 142, 148, 12, 19, 52, 100},
        new int[] {48, 21, 62, 64, 39, 69, 138},
        new int[] {8, 3, 61, 95, 25, 34, 54, 59},
        new int[] {105, 5, 22, 45, 144, 35, 58, 114},
        new int[] {29, 47, 24, 42, 93, 33, 40, 55, 56, 66, 70, 115},
        new int[] {9, 20, 44, 92, 17, 18, 31, 32, 60, 124, 125, 128},
        new int[] {91, 11, 4, 63, 16, 88, 89, 104, 139, 140},
        new int[] {26, 41, 94, 53, 68, 71, 75},
        new int[] { 30, 78, 79, 80, 1, 2, 133, 143, 145, 146, 11, 13, 72, 90, 97, 98, 99, 129, 137 }
    };

    public string[] outfitStyle = new string[] { 
        "I Лёгкий",
        "II Облегчённый",
        "III Средний",
        "IV Утяжелённый",
        "V Тяжёлый",
        "VI Сверхтяжёлый"
    };

    public void GenThirsty(int count, string txt)
    {
        int[] ens = new int[count];
        int val;
        for (int i = 0; i < ens.Length; i++)
        {
            do
            {
                val = Random.Range(1, 148);
            } while (ens.Contains(val));
            ens[i] = val;
        }

        //string s = string.Join(" ", ens.OrderBy(x => x).Select(x => x.ToString()));
        //GetSceneGO(txt).GetComponent<TextMeshProUGUI>().text = s;
        string stat = PrintThirstyStatistics(ens, txt);
        GUIUtility.systemCopyBuffer = stat;
    }
    public void GenThirsty1()
    {
        GenThirsty(30, "Thirsty1");
    }
    public void GenThirsty2()
    {
        GenThirsty(15, "Thirsty2");
    }
    public void GenThirsty3()
    {
        GenThirsty(9, "Thirsty3");
    }
    public GameObject GetSceneGO(string goName, string canvasName = "Canvas")
    {
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject canvas = null;
        GameObject go = null;
        foreach (var item in gos)
        {
            if (canvasName.Length > 0 && item.name == canvasName)
            {
                canvas = item;
                break;
            }
            if (item.name == goName)
            {
                go = item;
                break;
            }

        }
        if (canvas != null)
        {
            Component[] comps = canvas.GetComponentsInChildren<Transform>();
            foreach (var item in comps)
            {
                if (item.gameObject.name == goName)
                {
                    go = item.gameObject;
                    break;
                }
            }
        }
        return go;
    }

    public void My_test()
    {
        string s = "ddd";
        GUIUtility.systemCopyBuffer = s;
    }

    public int ThirstyEnergyRang(int enId)
    {
        if (enRang1.Contains(enId)) return 1;
        if (enRang2.Contains(enId)) return 2;
        return 3;
    }

    public int[] ThirstyCalcAmountEnRang(int[] ens)
    {
        int[] enrangs = new int[3] { 0, 0, 0 };
        for (int i = 0; i < ens.Length; i++)
        {
            switch (ThirstyEnergyRang(ens[i]))
            {
                case 1:
                    enrangs[0] += 1;
                    break;
                case 2:
                    enrangs[1] += 1;
                    break;
                case 3:
                    enrangs[2] += 1;
                    break;
            }
        }

        return enrangs;
    }

    public int ThirstyOutfitStyle(int bs)
    {
        if (bs <= 20) return 1;
        if (bs <= 30) return 2;
        if (bs <= 40) return 3;
        if (bs <= 50) return 4;
        if (bs <= 60) return 5;
        return 6;
    }

    public int ThirstyRank(int[] ens)
    {
        if (ens.Length == 9) return 3;
        if (ens.Length == 15) return 2;
        return 1;
    }

    public int ThirstyBaseStrength(int[] ens)
    {
        int baseStr = 0;
        for (int i = 0; i < ens.Length; i++)
        {
            switch(ThirstyEnergyRang(ens[i]))
            {
                case 1:
                    baseStr += 4;
                    break;
                case 2:
                    baseStr += 2;
                    break;
                case 3:
                    baseStr += 1;
                    break;
            }
        }
        return baseStr;
    }

    public int [] ThirstyActivities(int[] ens, ref List<int> acRank)
    {
        List<int> ac = new List<int>();
        int val = 0;
        for (int i = 0; i < ens.Length; i++)
        {
            for (int j = 0; j < activitiesEnergyTypes.Length; j++)
            {
                val = (activitiesEnergyTypes[j].Contains(ens[i])) ? j : 0;
                if(val != 0) break;
            }
            if (!ac.Contains(val))
            {
                ac.Add(val);
                acRank.Add(1);
            }
            else
            {
                int ndx = ac.IndexOf(val);
                acRank[ndx] += 1; 
            }
        }
        return ac.ToArray();
    }
    /// <summary>
    /// Вывод статистики Жаждущего в компонент UI
    /// </summary>
    /// <param name="ens">массив типов энергий</param>
    /// <param name="txtUI">компонент в UI для вывода</param>
    public string PrintThirstyStatistics(int[] ens, string txtUI)
    {
        /*
         * Name:
         * Rank:
         * Base strength:
         * Outfit style:
         * Activities []:
         */
        
        string stat = "Empty";
        string name = string.Join(" ", ens.OrderBy(x => x).Select(x => x.ToString()));
        int rank = ThirstyRank(ens);
        int bs = ThirstyBaseStrength(ens);
        string os = outfitStyle[ThirstyOutfitStyle(bs)-1];
        
        List<int> acRanks = new List<int>();
        int[] ac = ThirstyActivities(ens, ref acRanks);
        List<string> acStr = new List<string>();
        for (int i = 0; i < ac.Length; i++)
        {
            acStr.Add($"{activities[ac[i]]}: {acRanks[i]}");
        }

        string[] statHeader = new string[] { $"Name: {name}", $"Rank: {rank}", 
        $"Base strength: {bs}", $"Outfit style: {os}", $"Activities [{ac.Length}]:\n"};

        stat = string.Join("\n", statHeader);
        stat += string.Join("\n", acStr);

        GetSceneGO(txtUI).GetComponent<TextMeshProUGUI>().text = stat;
        return stat;
    }

}
