using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Common
{
    public enum OrderKind
    {
        Market, Limit
    }

    public enum TradeDirection
    {
        Buy, Sell
    }

    public enum PriceKind
    {
        Bid, Ask, Unspecified
    }

    public static partial class Extensions
    {
        public static string ToString(this OrderKind kind)
        {
            switch (kind)
            {
                case OrderKind.Limit:
                    return "Limit";
                case OrderKind.Market:
                    return "Market";
                default:
                    throw new ImpossibleException();
            }
        }

        public static string ToString(this TradeDirection direction)
        {
            switch (direction)
            {
                case TradeDirection.Buy:
                    return "Buy";
                case TradeDirection.Sell:
                    return "Sell";
                default:
                    throw new ImpossibleException();
            }
        }

        public static string ToString(this PriceKind direction)
        {
            switch (direction)
            {
                case PriceKind.Ask:
                    return "Ask";
                case PriceKind.Bid:
                    return "Bid";
                case PriceKind.Unspecified:
                    return "Unspecified";
                default:
                    throw new ImpossibleException();
            }
        }
    }
}
