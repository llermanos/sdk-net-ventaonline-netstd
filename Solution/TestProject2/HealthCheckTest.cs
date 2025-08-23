using Decidir;
using Decidir.Constants;
using Decidir.Model;

namespace DecidirTest
{
 
    public class HealthCheckTest
    {
        [Test]
        public void IsActive()
        {
            DecidirConnector decidir = new DecidirConnector(Ambiente.AMBIENTE_SANDBOX, "", "");
            HealthCheckResponse response = decidir.HealthCheck();
            Assert.That(response.buildTime != null);
            Assert.That(response.name != null);
            Assert.That(response.version != null);
        }
    }
}
