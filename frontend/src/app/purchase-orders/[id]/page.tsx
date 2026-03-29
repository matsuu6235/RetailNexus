"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import {
    getPurchaseOrderById,
    submitForApproval,
    approvePurchaseOrder,
    rejectPurchaseOrder,
    changePurchaseOrderStatus,
    changePurchaseOrderActivation,
} from "@/lib/api/purchaseOrders";
import type { PurchaseOrder, PurchaseOrderStatus } from "@/types/purchaseOrders";
import { purchaseOrderStatusLabels } from "@/types/purchaseOrders";
import { hasPermission } from "@/services/authService";
import styles from "./page.module.css";

function getStatusBadgeClass(status: PurchaseOrderStatus): string {
    if (status === 0) return styles.statusDraft;
    if (status === 1) return styles.statusAwaitingApproval;
    if (status === 2) return styles.statusApproved;
    if (status >= 3 && status <= 5) return styles.statusInProgress;
    if (status === 6) return styles.statusReceived;
    return styles.statusCancelled;
}

function formatDate(dateStr?: string | null): string {
    if (!dateStr) return "-";
    return new Date(dateStr).toLocaleDateString("ja-JP");
}

function formatDateTime(dateStr?: string | null): string {
    if (!dateStr) return "-";
    return new Date(dateStr).toLocaleString("ja-JP");
}

function formatAmount(amount: number): string {
    return amount.toLocaleString("ja-JP", { style: "currency", currency: "JPY" });
}

export default function PurchaseOrderDetailPage() {
    const params = useParams();
    const router = useRouter();
    const id = params.id as string;

    const [order, setOrder] = useState<PurchaseOrder | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [actionLoading, setActionLoading] = useState(false);

    const canEdit = hasPermission("purchases.edit");
    const canApprove = hasPermission("purchases.approve");
    const canDelete = hasPermission("purchases.delete");

    const fetchOrder = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await getPurchaseOrderById(id);
            setOrder(data);
        } catch (e) {
            setError(e instanceof Error ? e.message : "発注の取得に失敗しました。");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchOrder(); }, [id]);

    const handleAction = async (action: () => Promise<PurchaseOrder | void>) => {
        try {
            setActionLoading(true);
            setError(null);
            const result = await action();
            if (result) setOrder(result);
            else await fetchOrder();
        } catch (e) {
            setError(e instanceof Error ? e.message : "操作に失敗しました。");
        } finally {
            setActionLoading(false);
        }
    };

    if (loading) return <main className={styles.page}><p>読み込み中...</p></main>;
    if (!order) return <main className={styles.page}><div className={styles.errorBox}>{error || "発注が見つかりません。"}</div></main>;

    const status = order.status;

    return (
        <main className={styles.page}>
            <div className={styles.headerRow}>
                <div>
                    <h1 className={styles.title}>発注詳細</h1>
                    <p className={styles.orderNumber}>{order.orderNumber}</p>
                </div>
                <Link href="/purchase-orders" className={styles.backLink}>一覧に戻る</Link>
            </div>

            {error && <div className={styles.errorBox}>{error}</div>}

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>発注情報</h2>
                <div className={styles.infoGrid}>
                    <div>
                        <div className={styles.infoLabel}>ステータス</div>
                        <div className={styles.infoValue}>
                            <span className={`${styles.statusBadge} ${getStatusBadgeClass(status)}`}>
                                {purchaseOrderStatusLabels[status]}
                            </span>
                        </div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>仕入先</div>
                        <div className={styles.infoValue}>{order.supplierName}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>発注元</div>
                        <div className={styles.infoValue}>{order.storeName}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>発注日</div>
                        <div className={styles.infoValue}>{formatDate(order.orderDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>希望到着日</div>
                        <div className={styles.infoValue}>{formatDate(order.desiredDeliveryDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>納品予定日</div>
                        <div className={styles.infoValue}>{formatDate(order.expectedDeliveryDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>入荷日</div>
                        <div className={styles.infoValue}>{formatDate(order.receivedDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>承認者</div>
                        <div className={styles.infoValue}>{order.approvedByName || "-"}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>承認日時</div>
                        <div className={styles.infoValue}>{formatDateTime(order.approvedAt)}</div>
                    </div>
                    {order.note && (
                        <div style={{ gridColumn: "1 / -1" }}>
                            <div className={styles.infoLabel}>備考</div>
                            <div className={styles.infoValue}>{order.note}</div>
                        </div>
                    )}
                </div>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>発注明細</h2>
                <div className={styles.detailScroll}>
                <table className={styles.detailTable}>
                    <thead>
                        <tr>
                            <th>商品コード</th>
                            <th>商品名</th>
                            <th style={{ textAlign: "right" }}>数量</th>
                            <th style={{ textAlign: "right" }}>仕入単価</th>
                            <th style={{ textAlign: "right" }}>小計</th>
                        </tr>
                    </thead>
                    <tbody>
                        {order.details.map((d) => (
                            <tr key={d.purchaseOrderDetailId}>
                                <td>{d.productCode}</td>
                                <td>{d.productName}</td>
                                <td style={{ textAlign: "right" }}>{d.quantity.toLocaleString()}</td>
                                <td style={{ textAlign: "right" }}>{formatAmount(d.unitPrice)}</td>
                                <td style={{ textAlign: "right" }}>{formatAmount(d.subTotal)}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                </div>
                <div className={styles.totalRow}>
                    合計金額: {formatAmount(order.totalAmount)}
                </div>
            </div>

            <div className={styles.actionBar}>
                {/* 下書き → 承認申請 */}
                {status === 0 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => submitForApproval(id))}>
                        承認申請
                    </button>
                )}

                {/* 承認待ち → 承認 / 差戻し */}
                {status === 1 && canApprove && (
                    <>
                        <button type="button" className={`${styles.actionButton} ${styles.btnSuccess}`} disabled={actionLoading}
                            onClick={() => handleAction(() => approvePurchaseOrder(id))}>
                            承認
                        </button>
                        <button type="button" className={`${styles.actionButton} ${styles.btnWarning}`} disabled={actionLoading}
                            onClick={() => handleAction(() => rejectPurchaseOrder(id))}>
                            差戻し
                        </button>
                    </>
                )}

                {/* 承認済 → 仕入先確認済 */}
                {status === 2 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changePurchaseOrderStatus(id, 3))}>
                        仕入先確認済にする
                    </button>
                )}

                {/* 仕入先確認済 → 出荷準備中 */}
                {status === 3 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changePurchaseOrderStatus(id, 4))}>
                        出荷準備中にする
                    </button>
                )}

                {/* 出荷準備中 → 出荷済 */}
                {status === 4 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changePurchaseOrderStatus(id, 5))}>
                        出荷済にする
                    </button>
                )}

                {/* 出荷済 → 入荷済 */}
                {status === 5 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnSuccess}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changePurchaseOrderStatus(id, 6))}>
                        入荷済にする
                    </button>
                )}

                {/* キャンセル依頼（承認済〜出荷準備中の間） */}
                {status >= 2 && status <= 4 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnDanger}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changePurchaseOrderStatus(id, 91))}>
                        キャンセル依頼
                    </button>
                )}

                {/* 下書き・差戻し時のみ編集可能 */}
                {status === 0 && canEdit && (
                    <Link href={`/purchase-orders/${id}/edit`} className={styles.editLink}>
                        編集
                    </Link>
                )}

            </div>
        </main>
    );
}
