namespace Fincore.Application.DTO.Logs
{
    public class AuditLogResponseDto
    {
        public long AuditLogId { get; set; }

        public string EntityName { get; set; }

        public long EntityId { get; set; }

        public string OperationType { get; set; }

        public string OldData { get; set; }

        public string NewData { get; set; }

        public int AuditBy { get; set; }

        public string? AuditByName { get; set; }

        public DateTime AuditAt { get; set; }
    }
}