using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    /// <summary>
    /// Class used to instantiate proxy objects using Pandora's proxy info. This can be only used with exchange operations.
    /// </summary>
    public static class PandoraProxy
    {
        public const string Address = "http://proxy.davincicodes.net";
        public const int Port = 808;
        public const string Username = "ProxyUser";
        private const string Password = "gaQe36U2wnDu90dmH5mnwZZ+wQSrXaW80MkolglIcFHRnngF4Qi90h6XR597c0e2y02xeVmcQ6Tp1EBcUX7rCxgingAdEp3JlLJ4T3Ok6Ce4la+Jh/cSdOjNu6kBqC3jZ6TbHMN7R3sUjqoAoeG4zLgn06MDzcmpSyq+/KqCDEs=";
        private const string PrivKey = "PFJTQUtleVZhbHVlPjxNb2R1bHVzPjFrTWpOYlpkbnBzUE1wdkhtdmdWYXpNdDdvcjQrSVlQWWJ6bStpTVVSM1d4WXUwRUtVckM5UXpwd1ZsRXVvbXljTlVteCt0T1RlZzJJT0M5RUxMbDRwWEpvUExGYlNDclg2SUFlQ0FVUmZWWXJmSVk0anVRYWVYOUNHbllLc3NUdGhsTVY3M0xQSk9RcEFoVjNMUk1MVGoydmIraEszRDhBTlBZY1d2am1mOD08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjRCdUpiM1Y0V2xXUkhUaHB6OFRHQ2Z3V2dHZXcwTmxlb2hrdkJzQmpKY3BBT2lTd2xtUjBYNXV2QUZjYk1ISGlqZ2JLQmRvVEFtK2JjSmFUQjZCNlV3PT08L1A+PFE+OU1EdEdpc200UWdWbnpJcTRoazV2bUlvU1NsOXJXdEhVdTlJMnl4WU5xVU1tQjd2VlJxN0sxNkZsdVp2VExrcVk5TFlyVkt5MEZuK0hFcSs4RURrSlE9PTwvUT48RFA+Z0ExYjkxTHc1UFQxaVBPK0Noak9hOUFkNzFyUVBsV2w3QXRsd243VmFEZHJnWTJMVlRPckJ5SkcyWFBzTmo4c2k2aTNjS2FOckJ1bmZxcWE4b1d3dnc9PTwvRFA+PERRPm9xanhLdG9RM2E5RXgyZE03clM3RnBjZmxQZjVLVjdKcVFtVUR1MWF0djViNzJGbng5U2JIY2lVMmVQTmhsenRHaWlQamlrTzQ3OWJUU3d6MTNJWTdRPT08L0RRPjxJbnZlcnNlUT5ubkw3cGtjY3ZodGt3SnRtdXhzeFdMSGhQR1NyOUIyNlRZZ3hoeDlTTFBwSWZhT0x6d1Y1cEZkYWRpenIzam91SklRRjI4aHcwdGIrYkVWTXZscWRjZz09PC9JbnZlcnNlUT48RD5EaGc2TTQ2Q0VtekNoSzFCRUYyR2ZJS2xpd2pOcWxKRFVSdDRtYUYvZEMzMnFTVWgvaHk0RTNVSExERnFUSUpzdGNYSHpjOUVXNVZOZU9uZnkybEJVYXQ1K1MxcEtZN2ozSDI1MTZLek9QZzgwZDE1ZzlEaTJ0KzV4REkwd2lQN1M4ajJUQVZZL0dNc0VPWlYxNzYwVS8wVVJKTVVBRy8yTFRDNnUvRWVpU1U9PC9EPjwvUlNBS2V5VmFsdWU+";

        /// <summary>
        /// Generate an ApiProxy to be used with exchange libraries
        /// </summary>
        /// <returns>Pandora's proxy apiproxy object</returns>
        public static ApiProxy GetApiProxy()
        {
            return new ApiProxy(Address, Port, Username, DecryptPassword(Password, PrivKey));
        }

        /// <summary>
        /// Generate a WebProxy using Pandora's Proxy info
        /// </summary>
        /// <param name="aIsSSL">True if connection is made trough https. Default: false</param>
        /// <returns>Pandora's Proxy webproxy object</returns>
        public static WebProxy GetWebProxy(bool aIsSSL = false)
        {
            ICredentials lCredentials = new NetworkCredential(Username, DecryptPassword(Password, PrivKey));
            WebProxy lProxy = new WebProxy(string.Format("{0}://{1}:{2}/", aIsSSL ? "https" : "http", Address, Port), false, null, lCredentials);
            return lProxy;
        }

        /// <summary>
        /// Using asymetric encryptions, decrypts a string by given a private key
        /// </summary>
        /// <param name="aPassword">Data to decrypt</param>
        /// <param name="aPrivkey">Private key used to decrypt</param>
        /// <returns>Decrypted data string</returns>
        private static string DecryptPassword(string aPassword, string aPrivkey)
        {
            var lByteKey = Convert.FromBase64String(aPrivkey);
            var lPrivKey = Encoding.ASCII.GetString(lByteKey);
            var lDecrypter = new RSACryptoServiceProvider();
            lDecrypter.FromXmlString(lPrivKey);
            byte[] lBytePassword = Convert.FromBase64String(aPassword);
            byte[] lDecryptedPass = lDecrypter.Decrypt(lBytePassword, true);
            return Encoding.ASCII.GetString(lDecryptedPass);
        }
    }
}