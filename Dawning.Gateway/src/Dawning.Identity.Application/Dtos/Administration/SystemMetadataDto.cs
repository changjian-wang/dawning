namespace Dawning.Identity.Application.Dtos.Administration
{
    public class SystemMetadataDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool NonEditable { get; set; }
        public long Timestamp { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
