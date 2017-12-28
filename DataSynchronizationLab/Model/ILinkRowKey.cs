namespace DataSynchronizationLab.Model
{
    public interface ILinkRowKey
    {
        string RowKey { get; set; }
        string PreviousRowKey { get; set; }
    }
}
