using Xunit;

namespace tests
{
    public class extendPistons_should
    {
        [Fact]
        public void extendPistons_should_run()
        {
            var osi = new osi.IngameScript();
            // osi.isInventoryFull();

            Assert.False(osi, "Inventory should not be full");
        }
    }
}