namespace Fincore.Application.DTO.Logs
{
    public class ApprovalLogResponseDto
    {
        public int ApprovalLogId { get; set; }

        public string EntityName { get; set; }

        public long EntityId { get; set; }

        public int ApproverId { get; set; }

        public string ApproverName { get; set; }

        public string Status { get; set; }

        public string Remarks { get; set; }

        public DateTime ActionDate { get; set; }
    }
}