namespace BetterWidgets.Tests.Attributes
{
    public class TestGuid : Attribute
    {
        private readonly Guid guid;

        public TestGuid() => guid = Guid.NewGuid();
    }
}
