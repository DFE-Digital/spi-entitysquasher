namespace Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders
{
    public interface IEntityAdapterSettingsProvider
    {
        /// <summary>
        /// Base address for GIAS adapter
        /// </summary>
        string GiasBaseUrl { get; }
        
        /// <summary>
        /// Base address for UKRLP adapter
        /// </summary>
        string UkrlpBaseUrl { get; }
        
        /// <summary>
        /// Base address for Rates adapter
        /// </summary>
        string RatesBaseUrl { get; }
        
        /// <summary>
        /// Base address for iStore adapter
        /// </summary>
        string IStoreBaseUrl { get; }
        
        /// <summary>
        /// EAPIM Subscription key for adapters
        /// </summary>
        string SubscriptionKey { get; }
    }
}