using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball3DHardAgent1 : Agent
{
    [Header("Specific to Ball3DHard")]
    public Chart chart;

    public float bitcoins;
    public float bitcoinsMax;
    public float cryptos;
    public float cryptosMax;
    public float avarageCost;

    float nomralizedChartValue = 0.5f;
    float normalizedBitcoins;
    float normalizedCryptos;

    class MyTrade
    {
        public float Amount;    // getradete BTC Menge
        public float Cost;  // BTC Preis
    }

    List<MyTrade> MyTrades = new List<MyTrade>(0);


    public override void CollectObservations()
    {
        nomralizedChartValue = (chart.ChartValue - chart.ChartValueChange24hMin) / (chart.ChartValueChange24hMax - chart.ChartValueChange24hMin);
        AddVectorObs(nomralizedChartValue);

        bitcoinsMax = Mathf.Max(bitcoinsMax, bitcoins);
        if (bitcoinsMax == 0)
            normalizedBitcoins = 0f;
        else
            normalizedBitcoins = bitcoins / bitcoinsMax;
        AddVectorObs(normalizedBitcoins);

        cryptosMax = Mathf.Max(cryptosMax, cryptos);
        if (cryptosMax == 0)
            normalizedCryptos = 0f;
        else
            normalizedCryptos = cryptos / cryptosMax;
        AddVectorObs(normalizedCryptos);

        //AddVectorObs(avarageCost);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous && chart.Ready)
        {
            float action_trade = Mathf.Clamp(vectorAction[0], -1f, 1f);
            float action_valueBitcoin = Mathf.Clamp(vectorAction[1], 0f, bitcoins);
            float action_valueCrypto = Mathf.Clamp(vectorAction[2], 0f, cryptos);
            
            // Buy Cryptos
            if (action_trade > 0)
                if (action_valueBitcoin > 0)
                {
                    if (nomralizedChartValue > 0.5)
                    {
                        float a, b;
                        a = Mathf.InverseLerp(0.5f, 1f, nomralizedChartValue);
                        b = Mathf.InverseLerp(0f, 1f, normalizedBitcoins);

                        AddReward(0.1f * a * b);
                    }

                    chart.BuyCrypto(action_valueBitcoin, chart.ChartValue, this);
                    MyTrade trade = new MyTrade();
                    trade.Amount = action_valueBitcoin / chart.ChartValue;
                    trade.Cost = chart.ChartValue;
                    MyTrades.Add(trade);
                    GetAvarageTradesCost();
                }

            // Do nothing
            if (action_trade > 0)
            {
                if (nomralizedChartValue > 0.8f)
                    AddReward(0.15f * Mathf.InverseLerp(0.5f, 0f, normalizedBitcoins));

                if (nomralizedChartValue < 0.2f)
                    AddReward(0.15f * Mathf.InverseLerp(0.5f, 0f, normalizedCryptos));
            }

            // Buy Bitcoins
            if (action_trade < 0)
                if (action_valueCrypto > 0 && avarageCost > 0)
                {
                    if (nomralizedChartValue < 0.5)
                    {
                        float a, b;
                        a = Mathf.InverseLerp(0.5f, 1f, nomralizedChartValue);
                        b = Mathf.InverseLerp(0f, 1f, normalizedCryptos);

                        AddReward(0.1f * a * b);
                    }

                    chart.SellCrypto(action_valueCrypto, chart.ChartValue, this);
                    UpdateTrades(action_valueCrypto);


                }
        }
    }

    public override void AgentReset()
    {

    }

    void GetAvarageTradesCost()
    {
        if (MyTrades.Count == 0)
        {
            avarageCost = 0f;
            return;
        }

        float avarage = 0f;
        for (int i = 0; i < MyTrades.Count; i++)
        {
            avarage += MyTrades[i].Cost;
        }
        avarageCost = avarage / MyTrades.Count;
    }

    void UpdateTrades(float value)
    {
        float amount = value;
        float lowestCost = 9000f;
        int index = 0;

        while (amount > 0)
        {
            if (MyTrades.Count == 0)
                return;

            for (int i = 0; i < MyTrades.Count; i++)
            {
                if (MyTrades[i].Cost < lowestCost)
                {
                    lowestCost += MyTrades[i].Cost;
                    index = i;
                }
            }

            if (MyTrades[index].Amount > amount)
            {
                MyTrades[index].Amount -= amount;
            }
            else
            {
                amount -= MyTrades[index].Amount;
                MyTrades.Remove(MyTrades[index]);
                GetAvarageTradesCost();
            }
        }
    }
}
