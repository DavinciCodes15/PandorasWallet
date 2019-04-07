using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using static Pandora.Client.PandorasWallet.ServerAccess.PandorasServer;

namespace Pandora.Client.PandorasWallet.Tests
{
    [TestClass]
    public class HashNameTests
    {
        [TestMethod]
        public void HashUsernameandEmail()
        {
            string lUserInstanceData = string.Concat("maan221292", "maan221292@gmail.com").ToLower();

            //List<string> lPosibilities = Permute(lUserInstanceData);
            string InstanceId = HashUtility.CreateMD5(lUserInstanceData);
        }

        public List<string> Permute(string s)
        {
            List<string> listPermutations = new List<string>();

            char[] array = s.ToLower().ToCharArray();
            int iterations = (1 << array.Length) - 1;

            for (int i = 0; i <= 1000; i++)
            {
                for (int j = 0; j < array.Length; j++)
                    array[j] = (i & (1 << j)) != 0
                                  ? char.ToUpper(array[j])
                                  : char.ToLower(array[j]);
                listPermutations.Add(new string(array));
            }
            return listPermutations;
        }
    }
}