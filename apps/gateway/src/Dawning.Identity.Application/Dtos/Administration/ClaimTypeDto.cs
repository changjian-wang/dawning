namespace Dawning.Identity.Application.Dtos.Administration
{
    public class ClaimTypeDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool NonEditable { get; set; }
        public long Timestamp { get; set; }
    }
}
