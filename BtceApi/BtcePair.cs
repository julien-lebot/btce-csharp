using System;

namespace BtcE
{
	public enum BtcePair
	{
		btc_usd = 1,
		btc_rur = 17,
		btc_eur = 19,
		ltc_btc = 10,
		ltc_usd = 14,
		ltc_rur = 21,
        ltc_eur = 27,
		nmc_btc = 13,
        nmc_usd = 28,
		nvc_btc = 22,
        nvc_usd = 29,
		usd_rur = 18,
        eur_usd = 20,
		trc_btc = 23,
        ppc_btc = 24,
        ppc_usd = 31,
        ftc_btc = 25,
        xpm_btc = 30,
        Unknown
	}

	public class BtcePairHelper
	{
		public static BtcePair FromString(string s)
        {
			BtcePair ret = BtcePair.Unknown;
			Enum.TryParse<BtcePair>(s.ToLowerInvariant(), out ret);
			return ret;
		}
		public static string ToString(BtcePair v)
        {
			return Enum.GetName(typeof(BtcePair), v).ToLowerInvariant();
		}
	}
}
