using OnlineVoting.Models.Dtos.Request;

public static class InvoiceCacheKeys
{
    private const string Prefix = "onlinevoting:v1:invoice";

    public static string GetInvoice(string invoiceId)
    {
        return $"{Prefix}:id:{invoiceId}";
    }

    public static string GetInvoices(InvoiceRequest request)
    {
        return $"{Prefix}:page:{request.PageNumber}:size:{request.PageSize}:status:{request.InvoiceStatusId}:student:{request.StudentId}:application:{request.PositionApplicationId}:search:{request.SearchTerm}";
    }
}