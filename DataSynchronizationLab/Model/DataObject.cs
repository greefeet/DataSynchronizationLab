namespace DataSynchronizationLab.Model
{
    public class DataObject : IHashObject
    {
        public string Key { get; set; }
        public int ValueInt { get; set; }
        public string ValueString { get; set; }
        public override int GetHashCode()
        {
            return new { Key, ValueInt, ValueString }.GetHashCode();
        }
    }
}
