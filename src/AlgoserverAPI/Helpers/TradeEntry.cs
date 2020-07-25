using System;
using System.Linq;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Helpers
{

    public class InfoPanelData
    {
        public string objective { get; set; }
        public string status { get; set; }
        public string suggestedrisk { get; set; }
        public string positionsize { get; set; }
        public string pas { get; set; }
        public string macrotrend { get; set; }
        public string n_currencySymbol { get; set; }
    }

    public class TradeEntryResult
    {
        public decimal algo_TP2 { get; internal set; }
        public decimal algo_TP1_high { get; internal set; }
        public decimal algo_TP1_low { get; internal set; }
        public decimal algo_Entry_high { get; internal set; }
        public decimal algo_Entry_low { get; internal set; }
        public decimal algo_Entry { get; internal set; }
        public decimal algo_Stop { get; internal set; }
        public decimal algo_Risk { get; internal set; }
        public InfoPanelData algo_Info { get; internal set; }
    }

    public static class TradeEntry
    {
        public static TradeEntryResult Calculate(InputDataContainer container, Levels levels, SupportAndResistanceResult sar)
        {
            var isForex = container.Type == "forex";
            var dailyData = container.CloseD;
            var buyMax = 0m;
            var buyEntry = buyMax;
            var buyEntryIs100 = false;

            var Increment = levels.Level128.Increment; var AbsTop = levels.Level128.AbsTop;
            var EightEight = levels.Level128.EightEight; var FourEight = levels.Level128.FourEight; var ZeroEight = levels.Level128.ZeroEight;
            var EightEight1 = levels.Level32.EightEight; var FourEight1 = levels.Level32.FourEight; var ZeroEight1 = levels.Level32.ZeroEight;
            var EightEight2 = levels.Level16.EightEight; var FourEight2 = levels.Level16.FourEight; var ZeroEight2 = levels.Level16.ZeroEight;
            var EightEight3 = levels.Level8.EightEight; var FourEight3 = levels.Level8.FourEight; var ZeroEight3 = levels.Level8.ZeroEight;

            var hma1 = TechCalculations.Hma(dailyData, 200);
            var s1 = hma1.LastOrDefault();

            var hma21 = TechCalculations.Hma(dailyData, 50);
            var s21 = hma21.LastOrDefault();

            var dsma5 = TechCalculations.Sun(container.CloseD, 5);
            var smaTolBuy = (s1 * (1 - (isForex ? 0.001m : 0.01m)));
            var sma21buy = s21 > s1;

            if (sar.ValidNeu100 && FourEight > smaTolBuy && FourEight > buyEntry && sma21buy)
            {
                if (FourEight < dsma5)
                {
                    buyEntry = FourEight;
                    buyEntryIs100 = true;
                }
            }
            if (sar.ValidSup100 && ZeroEight > smaTolBuy && ZeroEight > buyEntry && sma21buy)
            {
                buyEntry = ZeroEight;
                buyEntryIs100 = true;
            }
            if (sar.ValidSup75a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && sma21buy)
            {
                buyEntry = ZeroEight;
            }
            if (sar.ValidSup75b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && sma21buy)
            {
                buyEntry = ZeroEight1;
            }

            //Find long TP1
            var buyTP1 = decimal.MaxValue;
            if (sar.ValidRes100 && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP1 && sma21buy) { buyTP1 = EightEight; }
            if (sar.ValidRes75a && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP1 && sma21buy) { buyTP1 = EightEight; }
            if (sar.ValidRes75b && EightEight1 > smaTolBuy && EightEight1 > buyEntry && EightEight1 < buyTP1 && sma21buy) { buyTP1 = EightEight1; }
            if (sar.ValidRes50a && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP1 && sma21buy) { buyTP1 = EightEight; }
            if (sar.ValidRes50b && EightEight1 > smaTolBuy && EightEight1 > buyEntry && EightEight1 < buyTP1 && sma21buy) { buyTP1 = EightEight1; }
            if (sar.ValidRes50c && EightEight2 > smaTolBuy && EightEight2 > buyEntry && EightEight2 < buyTP1 && sma21buy) { buyTP1 = EightEight2; }
            if (sar.ValidRes25a && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP1 && sma21buy) { buyTP1 = EightEight; }
            if (sar.ValidRes25b && EightEight1 > smaTolBuy && EightEight1 > buyEntry && EightEight1 < buyTP1 && sma21buy) { buyTP1 = EightEight1; }
            if (sar.ValidRes25c && EightEight2 > smaTolBuy && EightEight2 > buyEntry && EightEight2 < buyTP1 && sma21buy) { buyTP1 = EightEight2; }
            if (sar.ValidRes25d && EightEight3 > smaTolBuy && EightEight3 > buyEntry && EightEight3 < buyTP1 && sma21buy) { buyTP1 = EightEight3; }
            if (sar.ValidNeu100 && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP1 && sma21buy) { buyTP1 = FourEight; }
            if (sar.ValidNeu75a && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP1 && sma21buy) { buyTP1 = FourEight; }
            if (sar.ValidNeu75b && FourEight1 > smaTolBuy && FourEight1 > buyEntry && FourEight1 < buyTP1 && sma21buy) { buyTP1 = FourEight1; }
            if (sar.ValidNeu50a && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP1 && sma21buy) { buyTP1 = FourEight; }
            if (sar.ValidNeu50b && FourEight1 > smaTolBuy && FourEight1 > buyEntry && FourEight1 < buyTP1 && sma21buy) { buyTP1 = FourEight1; }
            if (sar.ValidNeu50c && FourEight2 > smaTolBuy && FourEight2 > buyEntry && FourEight2 < buyTP1 && sma21buy) { buyTP1 = FourEight2; }
            if (sar.ValidNeu25a && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP1 && sma21buy) { buyTP1 = FourEight; }
            if (sar.ValidNeu25b && FourEight1 > smaTolBuy && FourEight1 > buyEntry && FourEight1 < buyTP1 && sma21buy) { buyTP1 = FourEight1; }
            if (sar.ValidNeu25c && FourEight2 > smaTolBuy && FourEight2 > buyEntry && FourEight2 < buyTP1 && sma21buy) { buyTP1 = FourEight2; }
            if (sar.ValidNeu25d && FourEight3 > smaTolBuy && FourEight3 > buyEntry && FourEight3 < buyTP1 && sma21buy) { buyTP1 = FourEight3; }
            if (sar.ValidSup100 && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP1 && sma21buy) { buyTP1 = ZeroEight; }
            if (sar.ValidSup75a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP1 && sma21buy) { buyTP1 = ZeroEight; }
            if (sar.ValidSup75b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && ZeroEight1 < buyTP1 && sma21buy) { buyTP1 = ZeroEight1; }
            if (sar.ValidSup50a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP1 && sma21buy) { buyTP1 = ZeroEight; }
            if (sar.ValidSup50b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && ZeroEight1 < buyTP1 && sma21buy) { buyTP1 = ZeroEight1; }
            if (sar.ValidSup50c && ZeroEight2 > smaTolBuy && ZeroEight2 > buyEntry && ZeroEight2 < buyTP1 && sma21buy) { buyTP1 = ZeroEight2; }
            if (sar.ValidSup25a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP1 && sma21buy) { buyTP1 = ZeroEight; }
            if (sar.ValidSup25b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && ZeroEight1 < buyTP1 && sma21buy) { buyTP1 = ZeroEight1; }
            if (sar.ValidSup25c && ZeroEight2 > smaTolBuy && ZeroEight2 > buyEntry && ZeroEight2 < buyTP1 && sma21buy) { buyTP1 = ZeroEight2; }
            if (sar.ValidSup25d && ZeroEight3 > smaTolBuy && ZeroEight3 > buyEntry && ZeroEight3 < buyTP1 && sma21buy) { buyTP1 = ZeroEight3; }

            //Find long TP2
            var buyTP2 = decimal.MaxValue;
            if (sar.ValidRes100 && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP2 && EightEight > buyTP1 && sma21buy) { buyTP2 = EightEight; }
            if (sar.ValidRes75a && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP2 && EightEight > buyTP1 && sma21buy) { buyTP2 = EightEight; }
            if (sar.ValidRes75b && EightEight1 > smaTolBuy && EightEight1 > buyEntry && EightEight1 < buyTP2 && EightEight1 > buyTP1 && sma21buy) { buyTP2 = EightEight1; }
            if (sar.ValidRes50a && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP2 && EightEight > buyTP1 && sma21buy) { buyTP2 = EightEight; }
            if (sar.ValidRes50b && EightEight1 > smaTolBuy && EightEight1 > buyEntry && EightEight1 < buyTP2 && EightEight1 > buyTP1 && sma21buy) { buyTP2 = EightEight1; }
            if (sar.ValidRes50c && EightEight2 > smaTolBuy && EightEight2 > buyEntry && EightEight2 < buyTP2 && EightEight2 > buyTP1 && sma21buy) { buyTP2 = EightEight2; }
            if (sar.ValidRes25a && EightEight > smaTolBuy && EightEight > buyEntry && EightEight < buyTP2 && EightEight > buyTP1 && sma21buy) { buyTP2 = EightEight; }
            if (sar.ValidRes25b && EightEight1 > smaTolBuy && EightEight1 > buyEntry && EightEight1 < buyTP2 && EightEight1 > buyTP1 && sma21buy) { buyTP2 = EightEight1; }
            if (sar.ValidRes25c && EightEight2 > smaTolBuy && EightEight2 > buyEntry && EightEight2 < buyTP2 && EightEight2 > buyTP1 && sma21buy) { buyTP2 = EightEight2; }
            if (sar.ValidRes25d && EightEight3 > smaTolBuy && EightEight3 > buyEntry && EightEight3 < buyTP2 && EightEight3 > buyTP1 && sma21buy) { buyTP2 = EightEight3; }
            if (sar.ValidNeu100 && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP2 && FourEight > buyTP1 && sma21buy) { buyTP2 = FourEight; }
            if (sar.ValidNeu75a && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP2 && FourEight > buyTP1 && sma21buy) { buyTP2 = FourEight; }
            if (sar.ValidNeu75b && FourEight1 > smaTolBuy && FourEight1 > buyEntry && FourEight1 < buyTP2 && FourEight1 > buyTP1 && sma21buy) { buyTP2 = FourEight1; }
            if (sar.ValidNeu50a && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP2 && FourEight > buyTP1 && sma21buy) { buyTP2 = FourEight; }
            if (sar.ValidNeu50b && FourEight1 > smaTolBuy && FourEight1 > buyEntry && FourEight1 < buyTP2 && FourEight1 > buyTP1 && sma21buy) { buyTP2 = FourEight1; }
            if (sar.ValidNeu50c && FourEight2 > smaTolBuy && FourEight2 > buyEntry && FourEight2 < buyTP2 && FourEight2 > buyTP1 && sma21buy) { buyTP2 = FourEight2; }
            if (sar.ValidNeu25a && FourEight > smaTolBuy && FourEight > buyEntry && FourEight < buyTP2 && FourEight > buyTP1 && sma21buy) { buyTP2 = FourEight; }
            if (sar.ValidNeu25b && FourEight1 > smaTolBuy && FourEight1 > buyEntry && FourEight1 < buyTP2 && FourEight1 > buyTP1 && sma21buy) { buyTP2 = FourEight1; }
            if (sar.ValidNeu25c && FourEight2 > smaTolBuy && FourEight2 > buyEntry && FourEight2 < buyTP2 && FourEight2 > buyTP1 && sma21buy) { buyTP2 = FourEight2; }
            if (sar.ValidNeu25d && FourEight3 > smaTolBuy && FourEight3 > buyEntry && FourEight3 < buyTP2 && FourEight3 > buyTP1 && sma21buy) { buyTP2 = FourEight3; }
            if (sar.ValidSup100 && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP2 && ZeroEight > buyTP1 && sma21buy) { buyTP2 = ZeroEight; }
            if (sar.ValidSup75a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP2 && ZeroEight > buyTP1 && sma21buy) { buyTP2 = ZeroEight; }
            if (sar.ValidSup75b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && ZeroEight1 < buyTP2 && ZeroEight1 > buyTP1 && sma21buy) { buyTP2 = ZeroEight1; }
            if (sar.ValidSup50a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP2 && ZeroEight > buyTP1 && sma21buy) { buyTP2 = ZeroEight; }
            if (sar.ValidSup50b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && ZeroEight1 < buyTP2 && ZeroEight1 > buyTP1 && sma21buy) { buyTP2 = ZeroEight1; }
            if (sar.ValidSup50c && ZeroEight2 > smaTolBuy && ZeroEight2 > buyEntry && ZeroEight2 < buyTP2 && ZeroEight2 > buyTP1 && sma21buy) { buyTP2 = ZeroEight2; }
            if (sar.ValidSup25a && ZeroEight > smaTolBuy && ZeroEight > buyEntry && ZeroEight < buyTP2 && ZeroEight > buyTP1 && sma21buy) { buyTP2 = ZeroEight; }
            if (sar.ValidSup25b && ZeroEight1 > smaTolBuy && ZeroEight1 > buyEntry && ZeroEight1 < buyTP2 && ZeroEight1 > buyTP1 && sma21buy) { buyTP2 = ZeroEight1; }
            if (sar.ValidSup25c && ZeroEight2 > smaTolBuy && ZeroEight2 > buyEntry && ZeroEight2 < buyTP2 && ZeroEight2 > buyTP1 && sma21buy) { buyTP2 = ZeroEight2; }
            if (sar.ValidSup25d && ZeroEight3 > smaTolBuy && ZeroEight3 > buyEntry && ZeroEight3 < buyTP2 && ZeroEight3 > buyTP1 && sma21buy) { buyTP2 = ZeroEight3; }

            if (buyTP1 != decimal.MaxValue && buyTP2 == decimal.MaxValue)
            {
                buyTP2 = buyEntry + ((buyTP1 - buyEntry) / 2);
            }

            if (buyTP1 == decimal.MaxValue) {
                buyTP1 = decimal.Zero;
            }
            
            if (buyTP2 == decimal.MaxValue) {
                buyTP2 = decimal.Zero;
            }
            
            if (buyEntry == decimal.MaxValue) {
                buyEntry = decimal.Zero;
            }


            //Find sell entry under dsma200
            var smaTolSell = (s1 * (1 + (isForex ? 0.001m : 0.01m)));
            var sma21sell = (s21 < s1);
            var sellMax = decimal.MaxValue;
            var sellEntry = sellMax;
            var sellEntryIs100 = false;

            if (sar.ValidRes100 && EightEight < smaTolSell && EightEight < sellEntry && sma21sell)
            {
                sellEntry = EightEight;
                sellEntryIs100 = true;
            }
            if (sar.ValidRes75a && EightEight < smaTolSell && EightEight < sellEntry && sma21sell)
            {
                sellEntry = EightEight;
            }
            if (sar.ValidRes75b && EightEight1 < smaTolSell && EightEight1 < sellEntry && sma21sell)
            {
                sellEntry = EightEight1;
            }
            if (sar.ValidNeu100 && FourEight < smaTolSell && FourEight < sellEntry && sma21sell)
            {
                if (FourEight > dsma5)
                {
                    sellEntry = FourEight;
                    sellEntryIs100 = true;
                }
            }

            //Find sell TP1
            var sellTP1 = 0m;
            if (sar.ValidRes100 && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP1 && sma21sell) { sellTP1 = EightEight; }
            if (sar.ValidRes75a && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP1 && sma21sell) { sellTP1 = EightEight; }
            if (sar.ValidRes75b && EightEight1 < smaTolSell && EightEight1 < sellEntry && EightEight1 > sellTP1 && sma21sell) { sellTP1 = EightEight1; }
            if (sar.ValidRes50a && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP1 && sma21sell) { sellTP1 = EightEight; }
            if (sar.ValidRes50b && EightEight1 < smaTolSell && EightEight1 < sellEntry && EightEight1 > sellTP1 && sma21sell) { sellTP1 = EightEight1; }
            if (sar.ValidRes50c && EightEight2 < smaTolSell && EightEight2 < sellEntry && EightEight2 > sellTP1 && sma21sell) { sellTP1 = EightEight2; }
            if (sar.ValidRes25a && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP1 && sma21sell) { sellTP1 = EightEight; }
            if (sar.ValidRes25b && EightEight1 < smaTolSell && EightEight1 < sellEntry && EightEight1 > sellTP1 && sma21sell) { sellTP1 = EightEight1; }
            if (sar.ValidRes25c && EightEight2 < smaTolSell && EightEight2 < sellEntry && EightEight2 > sellTP1 && sma21sell) { sellTP1 = EightEight2; }
            if (sar.ValidRes25d && EightEight3 < smaTolSell && EightEight3 < sellEntry && EightEight3 > sellTP1 && sma21sell) { sellTP1 = EightEight3; }
            if (sar.ValidNeu100 && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP1 && sma21sell) { sellTP1 = FourEight; }
            if (sar.ValidNeu75a && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP1 && sma21sell) { sellTP1 = FourEight; }
            if (sar.ValidNeu75b && FourEight1 < smaTolSell && FourEight1 < sellEntry && FourEight1 > sellTP1 && sma21sell) { sellTP1 = FourEight1; }
            if (sar.ValidNeu50a && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP1 && sma21sell) { sellTP1 = FourEight; }
            if (sar.ValidNeu50b && FourEight1 < smaTolSell && FourEight1 < sellEntry && FourEight1 > sellTP1 && sma21sell) { sellTP1 = FourEight1; }
            if (sar.ValidNeu50c && FourEight2 < smaTolSell && FourEight2 < sellEntry && FourEight2 > sellTP1 && sma21sell) { sellTP1 = FourEight2; }
            if (sar.ValidNeu25a && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP1 && sma21sell) { sellTP1 = FourEight; }
            if (sar.ValidNeu25b && FourEight1 < smaTolSell && FourEight1 < sellEntry && FourEight1 > sellTP1 && sma21sell) { sellTP1 = FourEight1; }
            if (sar.ValidNeu25c && FourEight2 < smaTolSell && FourEight2 < sellEntry && FourEight2 > sellTP1 && sma21sell) { sellTP1 = FourEight2; }
            if (sar.ValidNeu25d && FourEight3 < smaTolSell && FourEight3 < sellEntry && FourEight3 > sellTP1 && sma21sell) { sellTP1 = FourEight3; }
            if (sar.ValidSup100 && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP1 && sma21sell) { sellTP1 = ZeroEight; }
            if (sar.ValidSup75a && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP1 && sma21sell) { sellTP1 = ZeroEight; }
            if (sar.ValidSup75b && ZeroEight1 < smaTolSell && ZeroEight1 < sellEntry && ZeroEight1 > sellTP1 && sma21sell) { sellTP1 = ZeroEight1; }
            if (sar.ValidSup50a && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP1 && sma21sell) { sellTP1 = ZeroEight; }
            if (sar.ValidSup50b && ZeroEight1 < smaTolSell && ZeroEight1 < sellEntry && ZeroEight1 > sellTP1 && sma21sell) { sellTP1 = ZeroEight1; }
            if (sar.ValidSup50c && ZeroEight2 < smaTolSell && ZeroEight2 < sellEntry && ZeroEight2 > sellTP1 && sma21sell) { sellTP1 = ZeroEight2; }
            if (sar.ValidSup25a && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP1 && sma21sell) { sellTP1 = ZeroEight; }
            if (sar.ValidSup25b && ZeroEight1 < smaTolSell && ZeroEight1 < sellEntry && ZeroEight1 > sellTP1 && sma21sell) { sellTP1 = ZeroEight1; }
            if (sar.ValidSup25c && ZeroEight2 < smaTolSell && ZeroEight2 < sellEntry && ZeroEight2 > sellTP1 && sma21sell) { sellTP1 = ZeroEight2; }
            if (sar.ValidSup25d && ZeroEight3 < smaTolSell && ZeroEight3 < sellEntry && ZeroEight3 > sellTP1 && sma21sell) { sellTP1 = ZeroEight3; }

            //Find sell TP2
            var sellTP2 = 0m;
            if (sar.ValidRes100 && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP2 && EightEight < sellTP1 && sma21sell) { sellTP2 = EightEight; }
            if (sar.ValidRes75a && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP2 && EightEight < sellTP1 && sma21sell) { sellTP2 = EightEight; }
            if (sar.ValidRes75b && EightEight1 < smaTolSell && EightEight1 < sellEntry && EightEight1 > sellTP2 && EightEight1 < sellTP1 && sma21sell) { sellTP2 = EightEight1; }
            if (sar.ValidRes50a && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP2 && EightEight < sellTP1 && sma21sell) { sellTP2 = EightEight; }
            if (sar.ValidRes50b && EightEight1 < smaTolSell && EightEight1 < sellEntry && EightEight1 > sellTP2 && EightEight1 < sellTP1 && sma21sell) { sellTP2 = EightEight1; }
            if (sar.ValidRes50c && EightEight2 < smaTolSell && EightEight2 < sellEntry && EightEight2 > sellTP2 && EightEight2 < sellTP1 && sma21sell) { sellTP2 = EightEight2; }
            if (sar.ValidRes25a && EightEight < smaTolSell && EightEight < sellEntry && EightEight > sellTP2 && EightEight < sellTP1 && sma21sell) { sellTP2 = EightEight; }
            if (sar.ValidRes25b && EightEight1 < smaTolSell && EightEight1 < sellEntry && EightEight1 > sellTP2 && EightEight1 < sellTP1 && sma21sell) { sellTP2 = EightEight1; }
            if (sar.ValidRes25c && EightEight2 < smaTolSell && EightEight2 < sellEntry && EightEight2 > sellTP2 && EightEight2 < sellTP1 && sma21sell) { sellTP2 = EightEight2; }
            if (sar.ValidRes25d && EightEight3 < smaTolSell && EightEight3 < sellEntry && EightEight3 > sellTP2 && EightEight3 < sellTP1 && sma21sell) { sellTP2 = EightEight3; }
            if (sar.ValidNeu100 && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP2 && FourEight < sellTP1 && sma21sell) { sellTP2 = FourEight; }
            if (sar.ValidNeu75a && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP2 && FourEight < sellTP1 && sma21sell) { sellTP2 = FourEight; }
            if (sar.ValidNeu75b && FourEight1 < smaTolSell && FourEight1 < sellEntry && FourEight1 > sellTP2 && FourEight1 < sellTP1 && sma21sell) { sellTP2 = FourEight1; }
            if (sar.ValidNeu50a && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP2 && FourEight < sellTP1 && sma21sell) { sellTP2 = FourEight; }
            if (sar.ValidNeu50b && FourEight1 < smaTolSell && FourEight1 < sellEntry && FourEight1 > sellTP2 && FourEight1 < sellTP1 && sma21sell) { sellTP2 = FourEight1; }
            if (sar.ValidNeu50c && FourEight2 < smaTolSell && FourEight2 < sellEntry && FourEight2 > sellTP2 && FourEight2 < sellTP1 && sma21sell) { sellTP2 = FourEight2; }
            if (sar.ValidNeu25a && FourEight < smaTolSell && FourEight < sellEntry && FourEight > sellTP2 && FourEight < sellTP1 && sma21sell) { sellTP2 = FourEight; }
            if (sar.ValidNeu25b && FourEight1 < smaTolSell && FourEight1 < sellEntry && FourEight1 > sellTP2 && FourEight1 < sellTP1 && sma21sell) { sellTP2 = FourEight1; }
            if (sar.ValidNeu25c && FourEight2 < smaTolSell && FourEight2 < sellEntry && FourEight2 > sellTP2 && FourEight2 < sellTP1 && sma21sell) { sellTP2 = FourEight2; }
            if (sar.ValidNeu25d && FourEight3 < smaTolSell && FourEight3 < sellEntry && FourEight3 > sellTP2 && FourEight3 < sellTP1 && sma21sell) { sellTP2 = FourEight3; }
            if (sar.ValidSup100 && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP2 && ZeroEight < sellTP1 && sma21sell) { sellTP2 = ZeroEight; }
            if (sar.ValidSup75a && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP2 && ZeroEight < sellTP1 && sma21sell) { sellTP2 = ZeroEight; }
            if (sar.ValidSup75b && ZeroEight1 < smaTolSell && ZeroEight1 < sellEntry && ZeroEight1 > sellTP2 && ZeroEight1 < sellTP1 && sma21sell) { sellTP2 = ZeroEight1; }
            if (sar.ValidSup50a && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP2 && ZeroEight < sellTP1 && sma21sell) { sellTP2 = ZeroEight; }
            if (sar.ValidSup50b && ZeroEight1 < smaTolSell && ZeroEight1 < sellEntry && ZeroEight1 > sellTP2 && ZeroEight1 < sellTP1 && sma21sell) { sellTP2 = ZeroEight1; }
            if (sar.ValidSup50c && ZeroEight2 < smaTolSell && ZeroEight2 < sellEntry && ZeroEight2 > sellTP2 && ZeroEight2 < sellTP1 && sma21sell) { sellTP2 = ZeroEight2; }
            if (sar.ValidSup25a && ZeroEight < smaTolSell && ZeroEight < sellEntry && ZeroEight > sellTP2 && ZeroEight < sellTP1 && sma21sell) { sellTP2 = ZeroEight; }
            if (sar.ValidSup25b && ZeroEight1 < smaTolSell && ZeroEight1 < sellEntry && ZeroEight1 > sellTP2 && ZeroEight1 < sellTP1 && sma21sell) { sellTP2 = ZeroEight1; }
            if (sar.ValidSup25c && ZeroEight2 < smaTolSell && ZeroEight2 < sellEntry && ZeroEight2 > sellTP2 && ZeroEight2 < sellTP1 && sma21sell) { sellTP2 = ZeroEight2; }
            if (sar.ValidSup25d && ZeroEight3 < smaTolSell && ZeroEight3 < sellEntry && ZeroEight3 > sellTP2 && ZeroEight3 < sellTP1 && sma21sell) { sellTP2 = ZeroEight3; }

            if (sellTP1 != 0 && sellTP2 == 0)
            {
                sellTP2 = sellEntry - ((sellEntry - sellTP1) / 2);
            }

            if (sellTP1 == decimal.MaxValue) {
                sellTP1 = decimal.Zero;
            }
            
            if (sellTP2 == decimal.MaxValue) {
                sellTP2 = decimal.Zero;
            }
            
            if (sellEntry == decimal.MaxValue) {
                sellEntry = decimal.Zero;
            }


            //N formation and hit TP1 within x candles? (true means cancel trade below)/
            var checkBuy100_d = false;
            var checkBuy75t_d = false;
            var checkSell100_d = false;
            var checkSell75t_d = false;
            var checkBuy100_4hr = false;
            var checkBuy75t_4hr = false;
            var checkSell100_4hr = false;
            var checkSell75t_4hr = false;
            var diMo = false;
            var sj_allow = new bool[] { true, true };
            var timeframe_isintraday = container.TimeframePeriod == Periodicity.HOUR || container.TimeframePeriod == Periodicity.MINUTE;
            var timeframe_isdaily = container.TimeframePeriod == Periodicity.DAY;
            var timeframe_isweekly = container.TimeframePeriod == Periodicity.WEEK;
            var timeframe_ismonthly = container.TimeframePeriod == Periodicity.MONTH;
            var timeframe_multiplier = container.TimeframeInterval;
            var timeframe_isdwm = timeframe_isdaily || timeframe_isweekly || timeframe_ismonthly;
            var greaterThan4hr = (!timeframe_isintraday) || (timeframe_multiplier > 240);
            var aRS = ((diMo && isForex) || (!diMo)) && (timeframe_isdwm || timeframe_multiplier >= 240) ? true : false; //allowRenderStrat


            //trade cancelled checks
            var _byEntry100_can = (aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && buyEntryIs100 && greaterThan4hr) && (checkBuy100_d || !sj_allow[1]);
            var _byEntry75_can = (aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && (!buyEntryIs100) && greaterThan4hr) && (checkBuy75t_d || !sj_allow[1]);
            var _byEntry100_4hr_can = (aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && buyEntryIs100 && (!greaterThan4hr)) && (!sj_allow[1]);
            var _byEntry75_4hr_can = (aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && (!buyEntryIs100) && (!greaterThan4hr)) && (!sj_allow[1]);

            var _slEntry100_can = (aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && sellEntryIs100 && greaterThan4hr) && (checkSell100_d || !sj_allow[1]);
            var _slEntry75_can = (aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && (!sellEntryIs100) && greaterThan4hr) && (checkSell75t_d || !sj_allow[1]);
            var _slEntry100_4hr_can = (aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && sellEntryIs100 && (!greaterThan4hr)) && (!sj_allow[1]);
            var _slEntry75_4hr_can = (aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && (!sellEntryIs100) && (!greaterThan4hr)) && (!sj_allow[1]);

            var _buyEntry100 = aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && buyEntryIs100 && greaterThan4hr && (!checkBuy100_d) && sj_allow[1] ? buyEntry : decimal.Zero;
            var _buyEntry75 = aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && (!buyEntryIs100) && greaterThan4hr && (!checkBuy75t_d) && sj_allow[1] ? buyEntry : decimal.Zero;
            var _buyEntry1004hr = aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && buyEntryIs100 && (!greaterThan4hr) && (!checkBuy100_4hr) && sj_allow[1] ? buyEntry : decimal.Zero;
            var _buyEntry754hr = aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && buyTP2 != decimal.Zero && (!buyEntryIs100) && (!greaterThan4hr) && (!checkBuy75t_4hr) && sj_allow[1] ? buyEntry : decimal.Zero;
            var _buyEntryExists = decimal.Zero != _buyEntry100 || decimal.Zero != _buyEntry75 || decimal.Zero != _buyEntry1004hr || decimal.Zero != _buyEntry754hr;
            var _buyTP1 = aRS && buyEntry != buyMax && buyTP1 != decimal.Zero && _buyEntryExists ? Math.Min(buyTP1, buyTP2) : decimal.Zero;
            var _buyTP2 = aRS && buyEntry != buyMax && buyTP2 != decimal.Zero && _buyEntryExists ? Math.Max(buyTP1, buyTP2) : decimal.Zero;

            var _sellEntry100 = aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && sellEntryIs100 && greaterThan4hr && (!checkSell100_d) && sj_allow[1] ? sellEntry : decimal.Zero;
            var _sellEntry75 = aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && (!sellEntryIs100) && greaterThan4hr && (!checkSell75t_d) && sj_allow[1] ? sellEntry : decimal.Zero;
            var _sellEntry1004hr = aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && sellEntryIs100 && (!greaterThan4hr) && (!checkSell100_4hr) && sj_allow[1] ? sellEntry : decimal.Zero;
            var _sellEntry754hr = aRS && sellEntry != sellMax && sellTP1 != 0 && sellTP2 != 0 && (!sellEntryIs100) && (!greaterThan4hr) && (!checkSell75t_4hr) && sj_allow[1] ? sellEntry : decimal.Zero;
            var _sellEntryExists = decimal.Zero != _sellEntry100 || decimal.Zero != _sellEntry75 || decimal.Zero != _sellEntry1004hr || decimal.Zero != _sellEntry754hr;
            var _sellTP1 = aRS && sellEntry != sellMax && sellTP1 != 0 && _sellEntryExists ? Math.Max(sellTP1, sellTP2) : decimal.Zero;
            var _sellTP2 = aRS && sellEntry != sellMax && sellTP2 != 0 && _sellEntryExists ? Math.Min(sellTP1, sellTP2) : decimal.Zero;

            //Shift
            var shift = false;
            if (_sellEntryExists)
            {
                if ((!(sar.ValidRes100 || sar.ValidRes75a || sar.ValidRes75b)) && (sar.ValidNeu100 || sar.ValidNeu75a || sar.ValidNeu75b))
                {
                    shift = true;
                }
            }
            else
            {
                if (_buyEntryExists)
                {
                    if ((!(sar.ValidSup100 || sar.ValidSup75a || sar.ValidSup75b)) && (sar.ValidNeu100 || sar.ValidNeu75a || sar.ValidNeu75b))
                    {
                        shift = true;
                    }
                }
            }

            //No trade zone
            var daily = timeframe_isdaily;
            var noTradeZone = sellEntry == decimal.Zero && buyEntry == decimal.Zero;
            var noTradeZone2 = !(decimal.Zero == _buyEntry100 || decimal.Zero == _buyEntry75 || decimal.Zero == _sellEntry100 || decimal.Zero == _sellEntry75 ||
                                 decimal.Zero == _buyEntry1004hr || decimal.Zero == _buyEntry754hr || decimal.Zero == _sellEntry1004hr || decimal.Zero == _sellEntry754hr);

            //algo objective text
            var algoObjectiveText = "None";
            var isN100 = false;
            if (decimal.Zero != _buyEntry100 || decimal.Zero != _buyEntry75 || decimal.Zero != _buyEntry1004hr || decimal.Zero != _buyEntry754hr)
            { //long

                if (sar.ValidNeu100 || sar.ValidNeu75a || sar.ValidNeu75b)
                {
                    algoObjectiveText = "Long middle range";
                    isN100 = true;
                }
                else
                {
                    if (sar.ValidSup100 || sar.ValidSup75a || sar.ValidSup75b)
                        algoObjectiveText = "Long bottom range";
                    isN100 = false;
                }
            }
            else
            {
                if (decimal.Zero != _sellEntry100 || decimal.Zero != _sellEntry75 || decimal.Zero != _sellEntry1004hr || decimal.Zero != _sellEntry754hr)
                { //short
                    if (sar.ValidNeu100 || sar.ValidNeu75a || sar.ValidNeu75b)
                    {
                        algoObjectiveText = "Short middle range";
                        isN100 = true;
                    }
                    else
                    {
                        if (sar.ValidRes100 || sar.ValidRes75a || sar.ValidRes75b)
                        {
                            algoObjectiveText = "Short top range";
                            isN100 = false;
                        }
                    }
                }
            }

            var currencySymbol = isForex ? "lots" : "units";
            var accountSize = container.InputAccountSize * container.UsdRatio;
            var suggestedRisk = container.InputRisk;
            var sLR = container.InputStoplossRatio; //input(1.7, "Stop loss ratio")
            var lHLH = container.InputDetectLowHigh; //input(false, "Detect last low/high and use as stop") //lastHighLowHeikin
            var splitPositions = container.InputSplitPositions; //input(3, "Number of Split Positions")

            if (suggestedRisk == decimal.Zero)
            {
                suggestedRisk = ((_buyEntry100 != decimal.Zero || _sellEntry100 != decimal.Zero) ? (isN100 ? 3.5m : 5m) : (_buyEntry75 != decimal.Zero || _sellEntry75 != decimal.Zero) ? 3.5m : (_buyEntry1004hr != decimal.Zero || _sellEntry1004hr != decimal.Zero) ? 2m : (_buyEntry754hr != decimal.Zero || _sellEntry754hr != decimal.Zero) ? 1.5m : 0);
            }

            var isLong = _buyEntry100 != decimal.Zero || _buyEntry75 != decimal.Zero || _buyEntry1004hr != decimal.Zero || _buyEntry754hr != decimal.Zero ? true : false;
            var isShort = _sellEntry100 != decimal.Zero || _sellEntry75 != decimal.Zero || _sellEntry1004hr != decimal.Zero || _sellEntry754hr != decimal.Zero ? true : false;
            var stopLossPosBuyPH = buyEntry - ((_buyTP2 - buyEntry) / sLR);
            var stopLossPosSellPH = sellEntry + ((sellEntry - _sellTP2) / sLR);
            var stopLossPosBuy = lHLH ? CheckHaLow(100, buyEntry, container) : stopLossPosBuyPH;
            var stopLossPosSell = lHLH ? CheckHaHigh(100, sellEntry, container) : stopLossPosSellPH;

            var positionValue = 0m;
            if (isLong || isShort)
            {
                if (isLong)
                {
                    if (isForex)
                    {
                        positionValue = (buyEntry * ((accountSize * (suggestedRisk / 100)) / Math.Abs(buyEntry - stopLossPosBuy))) / 100000; //forex
                    }
                    else
                    {
                        positionValue = (accountSize * (suggestedRisk / 100)) / Math.Abs(buyEntry - stopLossPosBuy); //stocks
                    }
                }
                else
                {
                    if (isShort)
                    {
                        if (isForex)
                        {
                            positionValue = (sellEntry * ((accountSize * (suggestedRisk / 100)) / Math.Abs(sellEntry - stopLossPosSell))) / 100000; //forex
                        }
                        else
                        {
                            positionValue = (accountSize * (suggestedRisk / 100)) / Math.Abs(sellEntry - stopLossPosSell); //stocks
                        }
                    }
                }
            }

            var sttLblTxt = "";
            if (noTradeZone || noTradeZone2)
            {
                sttLblTxt = sttLblTxt + "No trade zone";
            }
            else
            {
                if (!(noTradeZone || noTradeZone2))
                {
                    sttLblTxt = sttLblTxt + "Trade found";
                }
            }
            if (_byEntry100_can || _byEntry75_can || _byEntry100_4hr_can || _byEntry75_4hr_can || _slEntry100_can || _slEntry75_can || _slEntry100_4hr_can || _slEntry75_4hr_can)
            {
                sttLblTxt = sttLblTxt + "Trade Cancelled";
            }

            if (timeframe_isintraday) {
                sttLblTxt = "N/A below 1D TF";
            }

            var syminfo_currency = container.Currency;

            var infoPanelData = new InfoPanelData
            {
                objective = algoObjectiveText,
                status = sttLblTxt,
                suggestedrisk = suggestedRisk.ToString(),
                positionsize = ((container.Type == "cfd" || container.Type == "metals" || container.Type == "crypto") ? "Size depends on your broker." : ((syminfo_currency == "GBX" ? Math.Round(positionValue) : Math.Round(positionValue * 100) / 100).ToString() + " | Split: " + (syminfo_currency == "GBX" ? Math.Round(positionValue / splitPositions) : Math.Round((positionValue / splitPositions) * 100) / 100).ToString())),
                pas = "Market is natural.",
                macrotrend = (s21 > s1 ? "Uptrending" : "Downtrending"),
                n_currencySymbol = currencySymbol
            };

            var stopLossEntry100 = 0m;
            var stopLossEntry75 = 0m;
            var stopLossEntry1004hr = 0m;
            var stopLossEntry754hr = 0m;
            var stopLossEntry100org = 0m;
            var stopLossEntry75org = 0m;
            var stopLossEntry1004hrorg = 0m;
            var stopLossEntry754hrorg = 0m;


            if (_buyEntry100 != decimal.Zero)
            {
                stopLossEntry100org = _buyEntry100 - ((_buyTP2 - _buyEntry100) / 2);
                stopLossEntry100 = lHLH ? CheckHaLow(10, _buyEntry100, container) : _buyEntry100 - ((_buyTP2 - _buyEntry100) / sLR);
            }
            if (_buyEntry75 != decimal.Zero)
            {
                stopLossEntry75org = _buyEntry75 - ((_buyTP2 - _buyEntry75) / 2);
                stopLossEntry75 = lHLH ? CheckHaLow(10, _buyEntry75, container) : _buyEntry75 - ((_buyTP2 - _buyEntry75) / sLR);
            }
            if (_sellEntry100 != decimal.Zero)
            {
                stopLossEntry100org = _sellEntry100 + ((_sellEntry100 - _sellTP2) / 2);
                stopLossEntry100 = lHLH ? CheckHaHigh(10, _sellEntry100, container) : _sellEntry100 + ((_sellEntry100 - _sellTP2) / sLR);
            }
            if (_sellEntry75 != decimal.Zero)
            {
                stopLossEntry75org = _sellEntry75 + ((_sellEntry75 - _sellTP2) / 2);
                stopLossEntry75 = lHLH ? CheckHaHigh(10, _sellEntry75, container) : _sellEntry75 + ((_sellEntry75 - _sellTP2) / sLR);
            }
            if (_buyEntry1004hr != decimal.Zero)
            {
                stopLossEntry1004hrorg = _buyEntry1004hr - ((_buyTP2 - _buyEntry1004hr) / 2);
                stopLossEntry1004hr = lHLH ? CheckHaLow(10, _buyEntry1004hr, container) : _buyEntry1004hr - ((_buyTP2 - _buyEntry1004hr) / sLR);
            }
            if (_buyEntry754hr != decimal.Zero)
            {
                stopLossEntry754hrorg = _buyEntry754hr - ((_buyTP2 - _buyEntry754hr) / 2);
                stopLossEntry754hr = lHLH ? CheckHaLow(10, _buyEntry754hr, container) : _buyEntry754hr - ((_buyTP2 - _buyEntry754hr) / sLR);
            }
            if (_sellEntry1004hr != decimal.Zero)
            {
                stopLossEntry1004hrorg = _sellEntry1004hr + ((_sellEntry1004hr - _sellTP2) / 2);
                stopLossEntry1004hr = lHLH ? CheckHaHigh(10, _sellEntry1004hr, container) : _sellEntry1004hr + ((_sellEntry1004hr - _sellTP2) / sLR);
            }
            if (_sellEntry754hr != decimal.Zero)
            {
                stopLossEntry754hrorg = _sellEntry754hr + ((_sellEntry754hr - _sellTP2) / 2);
                stopLossEntry754hr = lHLH ? CheckHaHigh(10, _sellEntry754hr, container) : _sellEntry754hr + ((_sellEntry754hr - _sellTP2) / sLR);
            }

            var allowShift = false;

            //Buys
            if (aRS && _buyEntry100 != decimal.Zero)
            {
                _buyEntry100 = shift && allowShift ? stopLossEntry100org : _buyEntry100;
            }
            else
            {
                if (aRS && _buyEntry75 != decimal.Zero)
                {
                    _buyEntry75 = shift && allowShift ? stopLossEntry75org : _buyEntry75;
                }
            }
            if (aRS && _buyEntry1004hr != decimal.Zero)
            {
                _buyEntry1004hr = shift && allowShift ? stopLossEntry100org : _buyEntry1004hr;
            }
            else
            {
                if (aRS && _buyEntry754hr != decimal.Zero)
                {
                    _buyEntry754hr = shift && allowShift ? stopLossEntry754hrorg : _buyEntry754hr;
                }
            }
            if (aRS && _buyEntry100 != decimal.Zero)
            {
                stopLossEntry100 = shift && allowShift ? _buyEntry100 - ((_buyTP1 - _buyEntry100) / sLR) : stopLossEntry100;
            }
            else
            {
                if (aRS && _buyEntry75 != decimal.Zero)
                {
                    stopLossEntry75 = shift && allowShift ? _buyEntry75 - ((_buyTP1 - _buyEntry75) / sLR) : stopLossEntry75;
                }
            }
            if (aRS && _buyEntry1004hr != decimal.Zero)
            {
                stopLossEntry1004hr = shift && allowShift ? _buyEntry1004hr - ((_buyTP1 - _buyEntry1004hr) / sLR) : stopLossEntry1004hr;
            }
            else
            {
                if (aRS && _buyEntry754hr != decimal.Zero)
                {
                    stopLossEntry754hr = shift && allowShift ? _buyEntry754hr - ((_buyTP1 - _buyEntry754hr) / sLR) : stopLossEntry754hr;
                }
            }
            //Sells
            if (aRS && _sellEntry100 != decimal.Zero)
            {
                _sellEntry100 = shift && allowShift ? stopLossEntry100org : _sellEntry100;
            }
            else
            {
                if (aRS && _sellEntry75 != decimal.Zero)
                {
                    _sellEntry75 = shift && allowShift ? stopLossEntry75org : _sellEntry75;
                }
            }
            if (aRS && _sellEntry1004hr != decimal.Zero)
            {
                _sellEntry1004hr = shift && allowShift ? stopLossEntry1004hrorg : _sellEntry1004hr;
            }
            else
            {
                if (aRS && _sellEntry754hr != decimal.Zero)
                {
                    _sellEntry754hr = shift && allowShift ? stopLossEntry754hrorg : _sellEntry754hr;
                }
            }
            if (aRS && _sellEntry100 != decimal.Zero)
            {
                stopLossEntry100 = shift && allowShift ? _sellEntry100 + ((_sellEntry100 - _sellTP1) / sLR) : stopLossEntry100;
            }
            else
            {
                if (aRS && _sellEntry75 != decimal.Zero)
                {
                    stopLossEntry75 = shift && allowShift ? _sellEntry75 + ((_sellEntry75 - _sellTP1) / sLR) : stopLossEntry75;
                }
            }
            if (aRS && _sellEntry1004hr != decimal.Zero)
            {
                stopLossEntry1004hr = shift && allowShift ? _sellEntry1004hr + ((_sellEntry1004hr - _sellTP1) / sLR) : stopLossEntry1004hr;
            }
            else
            {
                if (aRS && _sellEntry754hr != decimal.Zero)
                {
                    stopLossEntry754hr = shift && allowShift ? _sellEntry754hr + ((_sellEntry754hr - _sellTP1) / sLR) : stopLossEntry754hr;
                }
            }

            var return_TP1 = new decimal[]{_buyTP1, _sellTP1}.FirstOrDefault(_ => _ != decimal.Zero);
            var return_TP2 = new decimal[]{_buyTP2, _sellTP2}.FirstOrDefault(_ => _ != decimal.Zero);
            var return_Entry = new decimal[]{_buyEntry100, _buyEntry1004hr, _buyEntry75, _buyEntry754hr, _sellEntry100, _sellEntry1004hr, _sellEntry75, _sellEntry754hr}.FirstOrDefault(_ => _ != decimal.Zero);
            var return_Stop = new decimal[]{stopLossEntry100, stopLossEntry1004hr, stopLossEntry75, stopLossEntry754hr}.FirstOrDefault(_ => _ != decimal.Zero);

             var avgrange = AverageRange(100, 1, container);

            var return_Entry_high = return_Entry + (avgrange * 0.125m);
            var return_Entry_low = return_Entry - (avgrange * 0.125m);
            var return_TP1_high = return_TP1 + (avgrange * 0.0625m);
            var return_TP1_low = return_TP1 - (avgrange * 0.0625m);

            var alogRandomizedShift = GetRandomizedShift(return_Entry);

            if (return_Entry == decimal.Zero) {
                return_TP1 = decimal.Zero;
                return_TP2 = decimal.Zero;
                return_Stop = decimal.Zero;
                return_Entry_high = decimal.Zero;
                return_Entry_low = decimal.Zero;
                return_TP1_high = decimal.Zero;
                return_TP1_low = decimal.Zero;
                alogRandomizedShift = decimal.Zero;
            }

            var returnData = new TradeEntryResult {
                 // xmode
                // ee = EightEight,
                // ee1 = EightEight1,
                // ee2 = EightEight2,
                // ee3 = EightEight3,
                // fe = FourEight,
                // fe1 = FourEight1,
                // fe2 = FourEight2,
                // fe3 = FourEight3,
                // ze = ZeroEight,
                // ze1 = ZeroEight1,
                // ze2 = ZeroEight2,
                // ze3 = ZeroEight3,
                // vr100 = sar.ValidRes100,
                // vr75a = sar.ValidRes75a,
                // vr75b = sar.ValidRes75b,
                // vn100 = sar.ValidNeu100,
                // vn75a = sar.ValidNeu75a,
                // vn75b = sar.ValidNeu75b,
                // vs100 = sar.ValidSup100,
                // vs75a = sar.ValidSup75a,
                // vs75b = sar.ValidSup75b,
                // vscs = sar.Validscs,
                // vscs2 = sar.Validscs2,
                // vexttp = sar.Validexttp,
                // vexttp2 = sar.Validexttp2,
                // m18 = sar.Minus18,
                // m28 = sar.Minus28,
                // p18 = sar.Plus18,
                // p28 = sar.Plus28,
                // algo
                algo_TP2 = return_TP2 + alogRandomizedShift,
                algo_TP1_high = return_TP1_high + alogRandomizedShift,
                algo_TP1_low = return_TP1_low + alogRandomizedShift,
                algo_Entry_high = return_Entry_low + alogRandomizedShift,
                algo_Entry_low = return_Entry_high + alogRandomizedShift,
                algo_Entry = return_Entry + alogRandomizedShift,
                algo_Stop = return_Stop + alogRandomizedShift,
                // algo_Info = mrgTxt,
                algo_Risk = suggestedRisk,
                algo_Info = infoPanelData,
                // id = id
            };
  
            return returnData;

        }
        private static decimal GetHaClose(int candle, InputDataContainer container)
        {
            return (container.Open[candle] + container.High[candle] + container.Low[candle] + container.Close[candle]) / 4;
        }

        private static decimal GetHaOpen(int candle, InputDataContainer container)
        {
            return (container.Open[candle] + container.Close[candle]) / 2;
        }

        private static decimal GetHaHigh(int candle, InputDataContainer container)
        {
            return Math.Max(Math.Max(container.High[candle], (container.Open[candle + 1] + container.Close[candle + 1]) / 2), (container.Open[candle] + container.High[candle] + container.Low[candle] + container.Close[candle]) / 4);
        }

        private static decimal GetHaLow(int candle, InputDataContainer container)
        {
            return Math.Min(Math.Min(container.Low[candle], (container.Open[candle + 1] + container.Close[candle + 1]) / 2), (container.Open[candle] + container.High[candle] + container.Low[candle] + container.Close[candle]) / 4);
        }

        private static decimal CheckHaHigh(int lookback, decimal entry, InputDataContainer container)
        {
            var y = 0m;
            for (var i = 0; i < lookback; i++)
            {
                if (GetHaClose(i, container) > GetHaOpen(i, container) && GetHaClose(i, container) > entry)
                {
                    y = GetHaHigh(i, container);
                    break;
                }
            }
            return y;
        }

        private static decimal CheckHaLow(int lookback, decimal entry, InputDataContainer container)
        {
            var y = 0m;
            for (var i = 0; i < lookback; i++)
            {
                if (GetHaClose(i, container) < GetHaOpen(i, container) && GetHaClose(i, container) < entry)
                {
                    y = GetHaLow(i, container);
                    break;
                }
            }
            return y;
        }

        private static decimal AverageRange(int period, decimal scale_multiplier, InputDataContainer container)
        {
            var substracted = TechCalculations.SubtractArrays(container.HighD, container.LowD);
            var avgRng = TechCalculations.Sun(substracted, period) * scale_multiplier;
            return avgRng;
        }  
        
        private static decimal GetRandomizedShift(decimal price)
        {
            Random random = new Random();  
            // from 0 to 10
            int randomShift = random.Next(0, 10); 
            // positive or negative shift
            int randomShiftDirection = random.Next(0, 10) >= 5 ? -1 : 1;
            // 0.001% of price
            var minShift = price / 100000;
            // from -0.01% to 0.01% shift from price
            return minShift * randomShift * randomShiftDirection;
        }

    }

}
