using Decidir;
using Decidir.Constants;
using Decidir.Model;

namespace DecidirTest
{

    public class TokenTest
    {
        [Test]
        public void GetTokenTest()
        {
            DecidirConnector decidir = new DecidirConnector(Ambiente.AMBIENTE_SANDBOX, "566f2c897b5e4bfaa0ec2452f5d67f13", "b192e4cb99564b84bf5db5550112adea");
            try
            {
                var res = decidir.GetTokenByCardTokenBsa(new CardTokenBsa
                {
                    public_token = Guid.NewGuid().ToString(),
                    card_holder_identification = new CardHolderIdentification
                    {
                        type = "dni",
                        number = "25123456"
                    },
                     fraud_detection= new FraudDetectionBSA
                     {
                          device_unique_identifier= "123456789",    
                     },
                    card_holder_name = "John Doe",
                    issue_date = "20250823",
                    merchant_id = "1",
                    payment_mode = "gateway",

                });

                Assert.That(res != null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
