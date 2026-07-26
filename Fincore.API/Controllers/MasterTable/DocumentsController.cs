using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/[Action]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService repo;

        public DocumentsController(IDocumentService repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> FetchDocuments(int page = 1, int pageSize = 10)
        {
            var data = await repo.GetAll(page, pageSize);
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> DocumentGetById(int id)
        {
            var data = await repo.DocumentGetById(id);

            if (!data.success)
            {
                return NotFound(data);
            }

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddDocument([FromForm] CreateDocumentDto dto)
        {
            var data = await repo.AddDocument(dto);

            if (!data.success)
            {
                return BadRequest(data);
            }

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> UpadateDocument(int id, [FromForm] UpdateDocumentDto dto)
        {
            var data = await repo.UpdateDocument(id, dto);
            return Ok(data);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var data = await repo.DeleteDocument(id);

            if (!data.success)
            {
                return NotFound(data);
            }

            return Ok(data);
        }
    }
}