namespace RetailNexus.Domain.Enums;

public static class StoreRequestStatusExtensions
{
    public static string ToDisplayName(this StoreRequestStatus status) => status switch
    {
        StoreRequestStatus.Draft => "下書き",
        StoreRequestStatus.AwaitingApproval => "承認待ち",
        StoreRequestStatus.Approved => "承認済み",
        StoreRequestStatus.Confirmed => "確認済み",
        StoreRequestStatus.Preparing => "準備中",
        StoreRequestStatus.Shipped => "出荷済み",
        StoreRequestStatus.Received => "入荷済み",
        StoreRequestStatus.CancelRequested => "キャンセル依頼中",
        StoreRequestStatus.Cancelled => "キャンセル済み",
        StoreRequestStatus.Rejected => "却下",
        _ => status.ToString()
    };
}
