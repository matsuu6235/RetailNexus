namespace RetailNexus.Domain.Enums;

public static class PurchaseOrderStatusExtensions
{
    public static string ToDisplayName(this PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.Draft => "下書き",
        PurchaseOrderStatus.AwaitingApproval => "承認待ち",
        PurchaseOrderStatus.Approved => "承認済み",
        PurchaseOrderStatus.SupplierConfirmed => "仕入先確認済み",
        PurchaseOrderStatus.Preparing => "準備中",
        PurchaseOrderStatus.Shipped => "出荷済み",
        PurchaseOrderStatus.Received => "入荷済み",
        PurchaseOrderStatus.CancelRequested => "キャンセル依頼中",
        PurchaseOrderStatus.Cancelled => "キャンセル済み",
        PurchaseOrderStatus.SupplierCancelled => "仕入先キャンセル",
        _ => status.ToString()
    };
}
