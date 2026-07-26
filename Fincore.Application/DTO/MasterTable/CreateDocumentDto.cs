using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.MasterTable
{
    public class CreateDocumentDto
    {
        [Required(ErrorMessage = "DocumentTypeId Is Requied")]
        public int DocumentTypeId { get; set; }
        [Required(ErrorMessage = "UserId Is Requied")]
        public int UserId { get; set; }
        [Required(ErrorMessage ="Entity Is Requied")]
        
        public int? EntityId { get; set; }
        [Required(ErrorMessage = "MasterTypeId Is Requied")]
        public int? MasterTypeId { get; set; }
       
        public IFormFile? FilePath { get; set; }
    }
}