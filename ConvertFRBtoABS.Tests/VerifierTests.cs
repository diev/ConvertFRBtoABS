#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov 2021
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConvertFRBtoABS.Tests
{
    [TestClass]
    public class VerifierTests
    {
        [TestMethod]
        public void VerifyLS04()
        {
            string prompt = "Test";

            string doc_LS = "40702810000000001565";
            string doc_BIC = "044030702";
            string doc_KS = "30101810600000000702";

            Verifier ver = new Verifier(prompt + "9", "Счет плат.");

            Assert.IsTrue(ver.LSKey(doc_LS, doc_BIC, doc_KS));
        }

        [TestMethod]
        public void VerifyLS01a()
        {
            string prompt = "Test";

            string doc_LS = "03214643000000017200";
            string doc_BIC = "014030106";
            string doc_KS = "40102810945370000005";

            Verifier ver = new Verifier(prompt + "9", "Счет плат.");

            Assert.IsTrue(ver.LSKey(doc_LS, doc_BIC, doc_KS));
        }

        [TestMethod]
        public void VerifyLS01b()
        {
            string prompt = "Test";

            string doc_LS = "03224643400000007200";
            string doc_BIC = "014030106";
            string doc_KS = "40102810945370000005";

            Verifier ver = new Verifier(prompt + "9", "Счет плат.");

            Assert.IsTrue(ver.LSKey(doc_LS, doc_BIC, doc_KS));
        }
    }
}
