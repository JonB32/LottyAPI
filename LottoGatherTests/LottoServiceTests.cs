using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LottoGather;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LottoGather.Tests
{
    [TestClass()]
    public class LottoServiceTests
    {
        //Simply call the main class; as long as there isn't a failure
        //the encapsilated methods are good
        [TestMethod()]
        public void MainTest()
        {
            int status;
            LottoService main = new LottoService();

            status = main.Start(null);

            Assert.AreEqual<int>(0, status, "LottoService ran successfully.");
        }
    }
}
