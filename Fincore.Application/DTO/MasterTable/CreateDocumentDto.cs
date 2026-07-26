using Microsoft.AspNetCore.Http;

namespace Fincore.Application.DTO.MasterTable
{
    public class CreateDocumentDto
    {
        public int DocumentTypeId { get; set; }

        public int UserId { get; set; }

        public int? EntityId { get; set; }

        public int? MasterTypeId { get; set; }

        public IFormFile? FilePath { get; set; }
    }
}