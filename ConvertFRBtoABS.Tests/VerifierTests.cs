// Copyright (c) 2021 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Verifier ver = new Verifier(prompt + "9.1", "Счет плат.");

            Assert.IsTrue(ver.LSKey(doc_LS, doc_BIC, doc_KS));
        }

        [TestMethod]
        public void VerifyLS01()
        {
            string prompt = "Test";

            string doc_LS = "03214643000000017200";
            string doc_BIC = "014030106";
            string doc_KS = "40102810945370000005";

            Verifier ver = new Verifier(prompt + "9.2", "Счет плат.");

            Assert.IsTrue(ver.LSKey(doc_LS, doc_BIC, doc_KS));
        }

        [TestMethod]
        public void VerifyLS01a()
        {
            string prompt = "Test";

            string doc_KS = "03214643000000017200";
            string doc_BIC = "014030106";
            string doc_LS = "40102810945370000005";

            Verifier ver = new Verifier(prompt + "9.2", "Счет плат.");

            Assert.IsTrue(ver.LSKey(doc_LS, doc_BIC, doc_KS));
        }
    }
}
