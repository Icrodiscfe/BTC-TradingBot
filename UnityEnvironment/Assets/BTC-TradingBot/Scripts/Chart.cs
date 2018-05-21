using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class Chart : MonoBehaviour {

    public float ChartChange;
    public float ChartValue = 50f;
    public float ChartValueChange24h;
    public float ChartValueChange24hMin;
    public float ChartValueChange24hMax;
    public float ChartValueChange1h;
    public float PortfolioTotal;
    public float PortfolioBTC;
    public float PortfolioCRYPTOS;
    public int TraderCount;
    public bool Ready;

    public List<Ball3DHardAgent1> MyAgents;

    public List<Text> TextList;
    List<Ball3DHardAgent1> listTrader = new List<Ball3DHardAgent1>(0);
    
    float startTime = 0f;
    List<float> ListChartValue = new List<float>();


	void Start () {
		
	}

    float ChartTimer = 0f;

    void Update () {
        ChartTimer += Time.deltaTime;

        if (ChartTimer >= 1f)
        {
            ChartChange = Random.Range(-1.0f, 1.0f);
            if (Input.GetKey(KeyCode.DownArrow))
                ChartChange = Random.Range(-1.0f, 0.0f);
            if (Input.GetKey(KeyCode.UpArrow))
                ChartChange = Random.Range(0.0f, 1.0f);

            ChartValue += ChartChange;
            ChartTimer = 0f;

            ListChartValue.Add(ChartValue);

            if (ListChartValue.Count >= 10)
            {
                if (ListChartValue.Count > 10)
                    ListChartValue.RemoveAt(0);

                float min = 9999f;
                float max = 0f;

                foreach (float i in ListChartValue)
                {
                    min = Mathf.Min(min, i);
                    max = Mathf.Max(max, i);
                }
                ChartValueChange24hMin = min;
                ChartValueChange24hMax = max;

                ChartValueChange24h = 100 / ListChartValue[0] * (ListChartValue[9] - ListChartValue[0]);
                ChartValueChange1h = 100 / ListChartValue[8] * (ListChartValue[9] - ListChartValue[8]);

                foreach (Ball3DHardAgent1 Agent in MyAgents)
                {
                    Agent.RequestDecision();
                }
            }
            

            //StartCoroutine(GetText());
        }

        if (ChartValue <= 30)
            ChartValue = 30;

        var value = (ChartValue - ChartValueChange24hMin) / (ChartValueChange24hMax - ChartValueChange24hMin);

        TextList[0].text = value.ToString();
        TextList[1].text = ChartValue.ToString();
        TextList[2].text = ChartValueChange24h.ToString();
        TextList[3].text = PortfolioBTC.ToString();
        TextList[4].text = PortfolioCRYPTOS.ToString();
        TextList[5].text = PortfolioTotal.ToString();
        TextList[6].text = ChartValueChange1h.ToString();

        if (startTime >= 0)
            startTime -= Time.deltaTime;

        Ready = startTime < 0;

        PortfolioBTC = 0;
        PortfolioCRYPTOS = 0;

        TraderCount = listTrader.Count;

        for (int i = 0; i < listTrader.Count; i++)
        {
            PortfolioBTC += listTrader[i].bitcoins;
            PortfolioCRYPTOS += listTrader[i].cryptos;
        }

        PortfolioTotal = PortfolioBTC + PortfolioCRYPTOS * ChartValue;

        //CCP_API_Request myObject = new CCP_API_Request();
        //myObject = JsonUtility.FromJson<CCP_API_Request>(json);
    }

    public void BuyCrypto(float amount, float cost, Ball3DHardAgent1 trader)
    {
        if (!listTrader.Contains(trader))
            listTrader.Add(trader);

        trader.cryptos += amount / cost;
        trader.bitcoins -= amount;

        LogTrade();
    }

    public void SellCrypto(float amount, float cost, Ball3DHardAgent1 trader)
    {
        if (!listTrader.Contains(trader))
            listTrader.Add(trader);

        trader.cryptos -= amount;
        trader.bitcoins += amount * cost;

        LogTrade();
    }

    void LogTrade ()
    {
        File.AppendAllText(Application.dataPath + "/MyLogfile.txt", ChartValue + ";" + PortfolioTotal + "\r\n");
    }
    
    public class CCP_API_Request
    {
        public string symbol;   // "ETH"
        public float price_btc; // "0.0841251"
        public float percent_change_1h; // "-0.38"
        public float percent_change_24h;    // "1.23"
        public float percent_change_7d; // "2.81"
        public int last_updated;    // "1526718258"
    }

    IEnumerator GetText()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.binance.com/api/v3/ticker/price?symbol=ETHBTC"))
        {
            yield return www.Send();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
            }
        }
    }
}
