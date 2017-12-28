namespace DataSynchronizationLab.Model
{
    public class LinkHashObject : ILinkRowKey
    {
        public string RowKey { get; set; }
        public string PreviousRowKey { get; set; }
    }
}
