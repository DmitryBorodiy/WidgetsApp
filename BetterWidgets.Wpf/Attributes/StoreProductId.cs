namespace BetterWidgets.Attributes
{
    public class StoreProductId : Attribute
    {
        public string Id { get; set; }

        public StoreProductId(string id)
        {
            Id = id;
        }
    }
}
