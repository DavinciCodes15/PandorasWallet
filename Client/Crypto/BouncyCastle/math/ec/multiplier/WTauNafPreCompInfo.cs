namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Math.EC.Multiplier
{
	/**
     * Class holding precomputation data for the WTNAF (Window
     * <code>&#964;</code>-adic Non-Adjacent Form) algorithm.
     */
	internal class WTauNafPreCompInfo
		: PreCompInfo
	{
		/**
         * Array holding the precomputed <code>AbstractF2mPoint</code>s used for the
         * WTNAF multiplication in <code>
         * {@link Pandora.Client.Crypto.Currencies.BouncyCastle.math.ec.multiplier.WTauNafMultiplier.multiply()
         * WTauNafMultiplier.multiply()}</code>.
         */
		protected AbstractF2mPoint[] m_preComp;

		public virtual AbstractF2mPoint[] PreComp
		{
			get
			{
				return m_preComp;
			}
			set
			{
				this.m_preComp = value;
			}
		}
	}
}
