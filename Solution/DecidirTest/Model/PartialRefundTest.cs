using Decidir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DecidirTest.Model
{
    [TestClass]
    public class PartialRefundTest
    {
        [TestMethod]
        public void toJsonTest()
        {
            PartialRefund refund = new PartialRefund();
            refund.amount = 1000;

            string result = PartialRefund.toJson(refund);

            Assert.AreEqual("{\"amount\":1000}", result);
        }
    }
}
