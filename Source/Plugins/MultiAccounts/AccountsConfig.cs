using System.Collections.Generic;
using Bricklayer.Core.Common;

namespace Bricklayer.Plugins.MultiAccounts
{
    public class AccountsConfig : IConfig
    {
        /// <summary>
        /// Accounts and their passwords.
        /// </summary>
        public Dictionary<string, string> Accounts;

        public IConfig GenerateDefaultConfig()
        {
            return new AccountsConfig
            {
                // Add some testing accounts.
                // These accounts are not allowed to sign into the website or forums (public usergroup).
                Accounts = new Dictionary<string, string>
                {
                    {"Bricklayer_Test_1", "bricklayer"},
                    {"Bricklayer_Test_2", "bricklayer"},
                    {"Bricklayer_Test_3", "bricklayer"},
                    {"Bricklayer_Test_4", "bricklayer"},
                    {"Bricklayer_Test_5", "bricklayer"}
                }
            };
        }
    }
}