using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BtcE;
using BtcE.Utils;
using QuickGraph;
using QuickGraph.Algorithms;

namespace ApiTest
{
    class TradeEdge
    {
        private decimal _buyFee;
        public decimal BuyFee
        {
            get
            {
                Update();
                return (_buyFee);
            }
        }

        private decimal _sellFee;
        public decimal SellFee
        {
            get
            {
                Update();
                return (_sellFee);
            }
        }

        public Ticker Ticker { get; private set; }
        public BtcePair Pair { get; private set; }

        public TradeEdge(BtcePair pair)
        {
            Pair = pair;
            Update(force:true);
        }

        public void Update(bool? force = false)
        {
            bool shouldUpdate = Ticker == null ? true : (DateTime.Now - UnixTime.ConvertToDateTime(Ticker.ServerTime).ToLocalTime() > TimeSpan.FromSeconds(60)) ||
                                                        (force.HasValue && force.Value);
            if (shouldUpdate)
            {
                Ticker = BtceApi.GetTicker(Pair);
                _buyFee = BtceApi.GetFee(true, Ticker.Buy, Pair);
                _sellFee = BtceApi.GetFee(false, Ticker.Sell, Pair);
            }
        }
    }

    class CurrencyNetwork : BidirectionalGraph<BtceCurrency, STaggedEdge<BtceCurrency, TradeEdge>>
    {
        public CurrencyNetwork()
        {
            UpdateNetwork();
        }

        public void UpdateNetwork()
        {
            Clear();

            foreach (BtcePair pair in (BtcePair[])Enum.GetValues(typeof(BtcePair)))
            {
                if (pair != BtcePair.Unknown)
                {
                    string[] currencies = BtcePairHelper.ToString(pair).Split('_');
                    AddVerticesAndEdge(new STaggedEdge<BtceCurrency, TradeEdge>(BtceCurrencyHelper.FromString(currencies[0]), BtceCurrencyHelper.FromString(currencies[1]), new TradeEdge(pair)));
                }
            }
        }

        public IEnumerable<STaggedEdge<BtceCurrency, TradeEdge>> ShortestPath(BtceCurrency source, BtceCurrency destination)
        {
            var solutions = this.ShortestPathsBellmanFord((tradeEdge) =>
                {
                    return ((double)(tradeEdge.Tag.Ticker.Sell + tradeEdge.Tag.SellFee));
                }, source);
            IEnumerable<STaggedEdge<BtceCurrency, TradeEdge>> path;
            if (solutions(destination, out path))
                return (path);
            else
                return (null);
        }
    }

	class Program
    {
        CurrencyNetwork net = new CurrencyNetwork();

        public void PrintShortesMoneyPath(BtceCurrency source, BtceCurrency destination)
        {
            Console.WriteLine("Shortest money path between " + source.ToString() + " and " + destination.ToString());
            var shortest = net.ShortestPath(source, destination);
            if (shortest != null)
            {
                decimal startValue = 1;
                foreach (var e in shortest)
                {
                    decimal endValue = startValue * e.Tag.Ticker.Sell - e.Tag.SellFee;
                    Console.WriteLine(startValue + " " + e.Source.ToString() + " -> " + endValue + " " + e.Target.ToString());
                    startValue = endValue;
                }
            }
        }

		static void Main(string[] args)
        {
            Program p = new Program();
            p.PrintShortesMoneyPath(BtceCurrency.nvc, BtceCurrency.eur);
            p.PrintShortesMoneyPath(BtceCurrency.ltc, BtceCurrency.eur);
            p.PrintShortesMoneyPath(BtceCurrency.btc, BtceCurrency.eur);

			//var ticker = BtceApi.GetTicker(BtcePair.ltc_eur);
			//var trades = BtceApi.GetTrades(BtcePair.ltc_eur);
            //var btcusdDepth = BtceApi.GetDepth(BtcePair.ltc_eur);
            //var fee = BtceApi.GetFee(BtcePair.ltc_eur);

            //var btceApi = new BtceApi("api_key", "api_secret");
			//var info = btceApi.GetInfo();
			//var transHistory = btceApi.GetTransHistory();
			//var tradeHistory = btceApi.GetTradeHistory();
			//var orderList = btceApi.GetOrderList();
		}
	}
}
