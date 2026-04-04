export function formatDate(dateStr?: string | null): string {
    if (!dateStr) return "-";
    return new Date(dateStr).toLocaleDateString("ja-JP");
}

export function formatDateTime(dateStr?: string | null): string {
    if (!dateStr) return "-";
    return new Date(dateStr).toLocaleString("ja-JP");
}

export function formatYen(amount: number): string {
    return amount.toLocaleString("ja-JP", { style: "currency", currency: "JPY" });
}
