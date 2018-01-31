namespace Query
{
    [TableName("EXAMES")]
    public class ExameMetadata : IDbMetadata
    {
        [TableColumn("EXAMES_ID")]
        public int Id { get; set; }

        [TableColumn("DESCRIPTION")]
        public string Description { get; set; }
    }
}
