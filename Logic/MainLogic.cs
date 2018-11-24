using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using Bittrex.Net;
using BTC.Models;
using System.Collections.Specialized;

namespace BTC.Logic
{
    public class InitialLogic
    {
        public static void Start()
        {
            var cxt = new Models.DBC();
            var timespan = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            //if (Math.Abs(cxt.TradeRecords.OrderByDescending(t=>t.timestamp).First().timestamp-timespan)>300)
            //{
            //    //cxt.TradeRecords.RemoveRange(cxt.TradeRecords.Take(cxt.TradeRecords.Count()));
            //    cxt.SaveChanges();
            //}
            BittrexDefaults.SetDefaultApiCredentials("1ac24640c15e46ccb852fcefb056dd7e", "f877c5c08edf4e95bd2cd96e72fa633b");

            JobScheduler.Main();
        }
    }
    
    public class MainLogic : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {

            var ticker = 6;
            var cxt = new Models.DBC();
            if (DateTime.Now.Minute % 10 == 0)
            {
                WebRequest req = WebRequest.Create("http://btc2017.azurewebsites.net/BTC");
                req.GetResponse();
            }
            //if (DateTime.Now.Minute % 60 == 0 && cxt.TradeRecords.Count() > 100 * ticker)
            //{
            //    cxt.TradeRecords.RemoveRange(cxt.TradeRecords.Take(cxt.TradeRecords.Count() - 60 * ticker));
            //    cxt.SaveChanges();
            //}

            var tradeHistory = cxt.TradeRecords;
            var config = cxt.Configs.First();

            CoinCheck.CoinCheck client = new CoinCheck.CoinCheck("GC0BLtAyNwTGfT1g", "GjvJPfn6HLLxl8J9iE7FsqiBUA01NO1T");
            BittrexClient client2 = new BittrexClient();

            var newTradeRecord = new Models.TradeRecord();

            string newTrade_CC;
            Bittrex.Net.Objects.BittrexApiResult<Bittrex.Net.Objects.BittrexPrice> newTrade_BT;
            bool ifFailed = false;

            do
            {
                if (ifFailed)
                {
                    Task.Delay(500).Wait();
                }
                newTrade_CC = client.Ticker.All().Content;
                ifFailed = true;
            } while (newTrade_CC == "");

            ifFailed = false;

            do
            {
                if (ifFailed)
                {
                    Task.Delay(500).Wait();
                }
                newTrade_BT = client2.GetTicker("USDT-BCC");
                ifFailed = true;
            } while (newTrade_BT.Success == false);

            ifFailed = false;


            newTradeRecord.timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            newTradeRecord.last = JsonConvert.DeserializeObject<Models.TradeRecord>(newTrade_CC).last;
            newTradeRecord.last_BT = newTrade_BT.Result.Last;


            List<double> rateHistory_CC = tradeHistory.Select(t => t.last).ToList().Skip(Math.Max(0, tradeHistory.Count() - 30 * ticker + 1)).ToList();
            rateHistory_CC.Add(newTradeRecord.last);
            List<double> rateHistory_BT = tradeHistory.Select(t => t.last_BT).ToList().Skip(Math.Max(0, tradeHistory.Count() - 30 * ticker + 1)).ToList();
            rateHistory_BT.Add(newTradeRecord.last_BT);
            List<long> timestampHistory = tradeHistory.Select(t => t.timestamp).ToList().Skip(Math.Max(0, tradeHistory.Count() - 30 * ticker + 1)).ToList();
            timestampHistory.Add(newTradeRecord.timestamp);


            newTradeRecord.ShortTrend_CC = (EvalYIntercept(rateHistory_CC.ToList().Skip(Math.Max(0, rateHistory_CC.Count() - ticker)).ToArray()));
            newTradeRecord.LongTrend_CC = (EvalYIntercept(rateHistory_CC.ToList().Skip(Math.Max(0, rateHistory_CC.Count() - 5 * ticker)).ToArray()));
            newTradeRecord.Before_BT = MathNet.Numerics.Statistics.Statistics.Mean(rateHistory_BT.ToList().Skip(Math.Max(0, rateHistory_BT.Count() -3 * ticker / 2)).Take(Math.Min(rateHistory_BT.Count(), ticker)).ToArray());
            newTradeRecord.After_BT = MathNet.Numerics.Statistics.Statistics.Mean(rateHistory_BT.ToList().Skip(Math.Max(0, rateHistory_BT.Count() - ticker / 2)).ToArray());
            newTradeRecord.ShortSlope_BT = (EvalSlope(rateHistory_BT.ToList().Skip(Math.Max(0, rateHistory_BT.Count() - ticker)).ToArray()));

            var lastTimestamp = newTradeRecord.timestamp;

            var lastTenMinutes = rateHistory_CC.Skip(Math.Max(0, rateHistory_CC.Count() - 10 * ticker));
            double lastTenMinutesRangeRatio = (lastTenMinutes.Max(t => t) - lastTenMinutes.Min(t => t) - MathNet.Numerics.Statistics.Statistics.InterquartileRange(lastTenMinutes)) * config.Multiplier / newTradeRecord.last;

            var lastTenMinutesBCC = rateHistory_BT.Skip(Math.Max(0, rateHistory_BT.Count() - 10 * ticker));
            double lastTenMinutesRangeRatioBCC = (lastTenMinutesBCC.Max(t => t) - lastTenMinutesBCC.Min(t => t)) / newTradeRecord.last_BT;

            //Trace.WriteLine(Math.Round(lastTenMinutesRangeRatioBCC, 2).ToString());

            if (lastTenMinutesRangeRatioBCC < config.Threshold_Low)
            {
                config.Switch = false;
                cxt.TradeRecords.Add(newTradeRecord);
                try
                {
                    cxt.SaveChanges();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
                
                return;
            }
            else if (lastTenMinutesRangeRatioBCC > config.Threshold_High || config.Switch == true)
            {
                if (config.Switch == false)
                {
                    config.Switch = true;
                    cxt.SaveChanges();
                }

                if (timestampHistory.Skip(Math.Max(0, timestampHistory.Count() - 10 * ticker)).All(t => t > newTradeRecord.timestamp - 1000) && newTradeRecord.timestamp % 30 > 19)
                {
                    //WriteLine("timestamp: " + newTradeRecord.timestamp);
                    int lag = 0;
                    if (timestampHistory.All(t => t > newTradeRecord.timestamp - 3000))
                    {

                        lag = GetLag(rateHistory_CC, rateHistory_BT);
                    }
                    //Trace.WriteLine("Lag: " + (ticker - 6));
                    Trace.WriteLine("A-B: " + Math.Round((newTradeRecord.After_BT - newTradeRecord.Before_BT), 2).ToString() + "  Th: " + Math.Round(lastTenMinutesRangeRatio * newTradeRecord.last_BT,2));
                    //Trace.WriteLine("Last*0.025: " + newTradeRecord.last_BT * 0.025);
                    //Trace.WriteLine("ShortSlope_BT: " + newTradeRecord.ShortSlope_BT);

                    string positionString;

                    positionString = GetPosition(client);

                    TradeDecision(lastTenMinutesRangeRatio, lag, client, newTradeRecord, positionString);
                    return;
                }
            }

        }

        private void TradeDecision(double ratio,int lag,CoinCheck.CoinCheck client, TradeRecord newTradeRecord, string positionString)
        {
            string amount = "";
            Task.Run(() =>
            {
                var cxt = new Models.DBC();
                switch (BchStatus(ratio,newTradeRecord))
                {
                    case 2:
                        {
                            if (lag > 0)
                            {
                                Task.Delay(lag * 10000).Wait();
                            }
                            List<Models.Position> activePosition = JsonConvert.DeserializeObject<Models.PositionList>(positionString).data.Where(t => t.status == "open" && t.side == "buy").ToList();

                            if (activePosition.Count() > 0)
                            {
                                CloseLongPosition(client, activePosition);

                                WaitBalanceClear(client);
                            }

                            amount = CheckBalance(client);

                            OpenShortPosition(client, double.Parse(JObject.Parse(JObject.Parse(amount).GetValue("margin_available").ToString()).GetValue("jpy").ToString()) * 5 / (newTradeRecord.last * 1.01));
                            newTradeRecord.BchStatus = 2;
                            break;
                        }
                    case 1:
                        {
                            if (lag > 0)
                            {
                                Task.Delay(lag * 10000).Wait();
                            }
                            positionString = GetPosition(client);

                            List<Position> activePosition = JsonConvert.DeserializeObject<PositionList>(positionString).data.Where(t => t.status == "open" && t.side == "buy").ToList();


                            if (activePosition.Count() > 0)
                            {
                                CloseLongPosition(client, activePosition);
                            }
                            newTradeRecord.BchStatus = 1;
                            break;

                        }
                    case -2:
                        {
                            if (lag > 0)
                            {
                                Task.Delay(lag * 10000).Wait();
                            }
                            List<Position> activePosition = JsonConvert.DeserializeObject<PositionList>(positionString).data.Where(t => t.status == "open" && t.side == "sell").ToList();

                            if (activePosition.Count() > 0)
                            {
                                CloseShortPosition(client, activePosition);

                                WaitBalanceClear(client);
                            }

                            amount = CheckBalance(client);

                            OpenLongPosition(client, double.Parse(JObject.Parse(JObject.Parse(amount).GetValue("margin_available").ToString()).GetValue("jpy").ToString()) * 5 / (newTradeRecord.last * 1.01));
                            newTradeRecord.BchStatus = -1;
                            break;
                        }
                    case -1:
                        {
                            if (lag > 0)
                            {
                                Task.Delay(lag * 10000).Wait();
                            }
                            positionString = GetPosition(client);

                            List<Position> activePosition = JsonConvert.DeserializeObject<PositionList>(positionString).data.Where(t => t.status == "open" && t.side == "sell").ToList();


                            if (activePosition.Count() > 0)
                            {
                                CloseShortPosition(client, activePosition);
                            }
                            newTradeRecord.BchStatus = -2;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                cxt.TradeRecords.Add(newTradeRecord);
                try
                {
                    cxt.SaveChanges();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            });
        }

        private int GetLag(List<double> cc, List<double> bt)
        {
            var index = -1;
            double min = 1;
            double value = 1;
            List<double> ccArray = new List<double>();
            List<double> btArray = new List<double>();
            for (int i = 0; i < 15; i++)
            {
                cc = cc.Skip(i+3).ToList();
                bt = bt.Take(bt.Count() - 3-i).ToList();
                int obtained = 0;
                while (obtained <= cc.Count()-3)
                {
                    ccArray.Add(MathNet.Numerics.Statistics.Statistics.Mean(cc.Skip(obtained).Take(3)));
                    btArray.Add(MathNet.Numerics.Statistics.Statistics.Mean(bt.Skip(obtained).Take(3)));
                    obtained += 3;
                }
                value = MathNet.Numerics.Statistics.Correlation.Pearson(ccArray, btArray);
                if (value <= min)
                {
                    min = value;
                    index = i;
                }
            }

            if (min<-0.6)
            {
                return index;
            }
            else
            {
                return 0;
            }
        }

        private void OpenLongPosition(CoinCheck.CoinCheck client, double amount)
        {
            client.Order.Create(new Dictionary<string, string>
                    {
                        {"amount", amount.ToString() },
                        {"order_type", "leverage_buy"},
                        {"pair", "btc_jpy"}
                    });
            Trace.WriteLine("long_new");
        }

        private void OpenShortPosition(CoinCheck.CoinCheck client, double amount)
        {
            client.Order.Create(new Dictionary<string, string>
                    {
                        {"amount", amount.ToString() },
                        {"order_type", "leverage_sell"},
                        {"pair", "btc_jpy"}
                    });
            Trace.WriteLine("short_new");
        }
        
        private void CloseLongPosition(CoinCheck.CoinCheck client, List<Position> activePosition)
        {
            foreach (var item in activePosition)
            {
                client.Order.Create(new Dictionary<string, string>
                            {
                                {"amount", item.amount.ToString()},
                                {"order_type", "close_long"},
                                {"pair", "btc_jpy"},
                                {"position_id", item.id.ToString() }
                            });
                Trace.WriteLine("long_closed");
            }
        }

        private void CloseShortPosition(CoinCheck.CoinCheck client, List<Position> activePosition)
        {
            foreach (var item in activePosition)
            {
                client.Order.Create(new Dictionary<string, string>
                            {
                                {"amount", item.amount.ToString()},
                                {"order_type", "close_short"},
                                {"pair", "btc_jpy"},
                                {"position_id", item.id.ToString() }
                            });
                Trace.WriteLine("short_closed");
            }
        }

        private string CheckBalance(CoinCheck.CoinCheck client)
        {
            bool ifFailed = false;
            string balanceString;
            do
            {
                if (ifFailed)
                {
                    Task.Delay(600).Wait(); ;
                }
                balanceString = client.Account.LeverageBalance().Content;
                ifFailed = true;
            } while (balanceString == "");

            return balanceString;
        }

        private void WaitBalanceClear(CoinCheck.CoinCheck client)
        {
            string amount = "";
            string amountAvailable = "";
            do
            {
                Task.Delay(1000).Wait(); ;
                amount = JObject.Parse(JObject.Parse(client.Account.LeverageBalance().Content).GetValue("margin").ToString()).GetValue("jpy").ToString();
                amountAvailable = JObject.Parse(JObject.Parse(client.Account.LeverageBalance().Content).GetValue("margin_available").ToString()).GetValue("jpy").ToString();
            } while (amount != amountAvailable || amount == "" || amountAvailable == "");
        }

       

        private string GetPosition(CoinCheck.CoinCheck client)
        {
            bool ifFailed = false;
            string positionString;
            do
            {
                if (ifFailed)
                {
                    Task.Delay(500).Wait(); ;
                }
                positionString = client.Leverage.Positions().Content;
                ifFailed = true;
            } while (positionString == null);

            return positionString;
        }

        private int BchStatus(double ratio,TradeRecord newTradeRecord)
        {
            if (newTradeRecord.After_BT - newTradeRecord.Before_BT > newTradeRecord.last_BT * ratio && newTradeRecord.ShortSlope_BT > 0)
            {
                return 2;
            }
            else if (newTradeRecord.After_BT - newTradeRecord.Before_BT < -newTradeRecord.last_BT * ratio && newTradeRecord.ShortSlope_BT < 0)
            {
                return -2;
            }
            else if (newTradeRecord.After_BT - newTradeRecord.Before_BT > 0)
            {
                return 1;
            }
            else if (newTradeRecord.After_BT - newTradeRecord.Before_BT < 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public double EvalYIntercept(double[] yArray)
        {
            if (yArray.Length > 1)
            {

            var xArray = new double[yArray.Length];
            for (int i = 0; i < xArray.Length; i++)
            {
                    xArray[i] = xArray.Length - i + 1;
            }

            var ds = new CommonLib.Numerical.XYDataSet(xArray, yArray);
            return ds.YIntercept;
            }
            else
            {
                return 0;
            }
        }

        public double EvalSlope(double[] yArray)
        {
            if (yArray.Length>1)
            {

            var xArray = new double[yArray.Length];
            for (int i = 0; i < xArray.Length; i++)
            {
                xArray[i] = i;
            }

            var ds = new CommonLib.Numerical.XYDataSet(xArray, yArray);
            return ds.Slope;
            }
            else
            {
                return 0;
            }
        }

        public bool IfGoldenCross(double[] sArray, double[] mArray)
        {
            if (sArray[3] - mArray[3] > mArray[3] * 0.0015)
            {
                Trace.WriteLine("Golden");
                return true;
            }
            return false;
        }

        public bool IfGoldenCrossW(double[] sArray, double[] mArray)
        {
            if (sArray[3] - mArray[3] > 0)
            {
                Trace.WriteLine("Golden");
                return true;
            }
            return false;
        }

        public bool IfDeadCross(double[] sArray, double[] mArray)
        {
            if (sArray[3] - mArray[3] < -mArray[3] * 0.0015)
            {
                Trace.WriteLine("Dead");
                return true;
            }
            return false;
        }

        public bool IfDeadCrossW(double[] sArray, double[] mArray)
        {
            if (sArray[3] - mArray[3] < 0)
            {
                Trace.WriteLine("Dead");
                return true;
            }
            return false;
        }
    }

    public class JobScheduler
    {
        public static void Main()
        {
            RunProgram().GetAwaiter().GetResult();
        }
        private static async Task RunProgram()
        {
            NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<MainLogic>().Build();

            ITrigger trigger = TriggerBuilder.Create().WithDailyTimeIntervalSchedule
                (t => t.WithIntervalInSeconds(10).OnEveryDay()).Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}