type StyleMap = Record<string, string>;

export function getStatusBadgeClass(status: number, styles: StyleMap): string {
    if (status === 0) return styles.statusDraft;
    if (status === 1) return styles.statusAwaitingApproval;
    if (status === 2) return styles.statusApproved;
    if (status >= 3 && status <= 5) return styles.statusInProgress;
    if (status === 6) return styles.statusReceived;
    return styles.statusCancelled;
}
