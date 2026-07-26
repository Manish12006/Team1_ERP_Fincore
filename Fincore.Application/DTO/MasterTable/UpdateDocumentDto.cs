using Microsoft.AspNetCore.Http;

public class UpdateDocumentDto
{
    public int DocumentTypeId { get; set; }
    public int UserId { get; set; }
    public int? EntityId { get; set; }
    public int? MasterTypeId { get; set; }

    public IFormFile? FilePath { get; set; }
}