using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Logs
{
    public class AuditLogDto
    {
        public string EntityName { get; set; } = string.Empty;

        public long EntityId { get; set; }

        public string OperationType { get; set; } = string.Empty;

        public string? OldData { get; set; }

        public string? NewData { get; set; }

        public int AuditBy { get; set; }

        public DateTime AuditAt { get; set; }
    }
}