namespace Fincore.Application.DTO.Payment.AccountsReceivable.Responses
{
    public class ARAgingResponseDto
    {
        public int Current { get; set; }

        public int Days1To30 { get; set; }

        public int Days31To60 { get; set; }

        public int Days61To90 { get; set; }

        public int Above90Days { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal Days1To30Amount { get; set; }

        public decimal Days31To60Amount { get; set; }

        public decimal Days61To90Amount { get; set; }

        public decimal Above90DaysAmount { get; set; }
    }
}