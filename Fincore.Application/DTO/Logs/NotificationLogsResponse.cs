namespace Fincore.Application.DTO.Logs
{
    public class NotificationLogResponseDto
    {
        public long NotificationLogId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; }
    }
}