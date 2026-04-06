"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import { useParams } from "next/navigation";
import Link from "next/link";
import {
    getStoreRequestById,
    submitStoreRequestForApproval,
    approveStoreRequest,
    rejectStoreRequest,
    changeStoreRequestStatus,
    changeStoreRequestActivation,
} from "@/lib/api/storeRequests";
import type { StoreRequest } from "@/types/storeRequests";
import { hasPermission } from "@/services/authService";
import { formatDate, formatDateTime } from "@/lib/utils/formatters";
import StatusStepBar from "@/components/status-step-bar/StatusStepBar";
import styles from "../../purchase-orders/[id]/page.module.css";

const storeRequestSteps = [
    { status: 0, label: "下書き" },
    { status: 1, label: "承認待ち" },
    { status: 2, label: "承認済" },
    { status: 3, label: "確認済" },
    { status: 4, label: "出荷準備中" },
    { status: 5, label: "出荷済" },
    { status: 6, label: "入荷済" },
];

export default function StoreRequestDetailPage() {
    const params = useParams();
    const id = params.id as string;

    const [request, setRequest] = useState<StoreRequest | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [actionLoading, setActionLoading] = useState(false);

    const canEdit = hasPermission("store-requests.edit");
    const canApprove = hasPermission("store-requests.approve");
    const canDelete = hasPermission("store-requests.delete");

    const fetchRequest = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await getStoreRequestById(id);
            setRequest(data);
        } catch (e) {
            setError(e instanceof Error ? e.message : fallback.fetchFailed("発送依頼"));
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchRequest(); }, [id]);

    const handleAction = async (action: () => Promise<StoreRequest | void>) => {
        try {
            setActionLoading(true);
            setError(null);
            const result = await action();
            if (result) setRequest(result);
            else await fetchRequest();
        } catch (e) {
            setError(e instanceof Error ? e.message : fallback.operationFailed);
        } finally {
            setActionLoading(false);
        }
    };

    if (loading) return <main className={styles.page}><p>読み込み中...</p></main>;
    if (!request) return <main className={styles.page}><div className={styles.errorBox}>{error || "発送依頼が見つかりません。"}</div></main>;

    const status = request.status;

    return (
        <main className={styles.page}>
            <div className={styles.headerRow}>
                <div>
                    <h1 className={styles.title}>発送依頼詳細</h1>
                    <p className={styles.orderNumber}>{request.requestNumber}</p>
                </div>
                <Link href="/store-requests" className={styles.backLink}>一覧に戻る</Link>
            </div>

            {error && <div className={styles.errorBox}>{error}</div>}

            <StatusStepBar steps={storeRequestSteps} currentStatus={status} />

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>依頼情報</h2>
                <div className={styles.infoGrid}>
                    <div>
                        <div className={styles.infoLabel}>依頼元</div>
                        <div className={styles.infoValue}>{request.fromStoreName}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>依頼先</div>
                        <div className={styles.infoValue}>{request.toStoreName}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>依頼日</div>
                        <div className={styles.infoValue}>{formatDate(request.requestDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>希望到着日</div>
                        <div className={styles.infoValue}>{formatDate(request.desiredDeliveryDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>到着予定日</div>
                        <div className={styles.infoValue}>{formatDate(request.expectedDeliveryDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>出荷日</div>
                        <div className={styles.infoValue}>{formatDate(request.shippedDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>入荷日</div>
                        <div className={styles.infoValue}>{formatDate(request.receivedDate)}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>承認者</div>
                        <div className={styles.infoValue}>{request.approvedByName || "-"}</div>
                    </div>
                    <div>
                        <div className={styles.infoLabel}>承認日時</div>
                        <div className={styles.infoValue}>{formatDateTime(request.approvedAt)}</div>
                    </div>
                    {request.note && (
                        <div style={{ gridColumn: "1 / -1" }}>
                            <div className={styles.infoLabel}>備考</div>
                            <div className={styles.infoValue}>{request.note}</div>
                        </div>
                    )}
                </div>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>依頼明細</h2>
                <div className={styles.detailScroll}>
                <table className={styles.detailTable}>
                    <thead>
                        <tr>
                            <th>商品コード</th>
                            <th>商品名</th>
                            <th style={{ textAlign: "right" }}>数量</th>
                        </tr>
                    </thead>
                    <tbody>
                        {request.details.map((d) => (
                            <tr key={d.storeRequestDetailId}>
                                <td>{d.productCode}</td>
                                <td>{d.productName}</td>
                                <td style={{ textAlign: "right" }}>{d.quantity.toLocaleString()}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                </div>
            </div>

            <div className={styles.actionBar}>
                {status === 0 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => submitStoreRequestForApproval(id))}>
                        承認申請
                    </button>
                )}
                {status === 1 && canApprove && (
                    <>
                        <button type="button" className={`${styles.actionButton} ${styles.btnSuccess}`} disabled={actionLoading}
                            onClick={() => handleAction(() => approveStoreRequest(id))}>
                            承認
                        </button>
                        <button type="button" className={`${styles.actionButton} ${styles.btnWarning}`} disabled={actionLoading}
                            onClick={() => handleAction(() => rejectStoreRequest(id))}>
                            差戻し
                        </button>
                    </>
                )}
                {status === 2 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changeStoreRequestStatus(id, 3))}>
                        確認済にする
                    </button>
                )}
                {status === 3 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changeStoreRequestStatus(id, 4))}>
                        出荷準備中にする
                    </button>
                )}
                {status === 4 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnPrimary}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changeStoreRequestStatus(id, 5))}>
                        出荷済にする
                    </button>
                )}
                {status === 5 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnSuccess}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changeStoreRequestStatus(id, 6))}>
                        入荷済にする
                    </button>
                )}
                {status >= 2 && status <= 4 && canEdit && (
                    <button type="button" className={`${styles.actionButton} ${styles.btnDanger}`} disabled={actionLoading}
                        onClick={() => handleAction(() => changeStoreRequestStatus(id, 91))}>
                        キャンセル依頼
                    </button>
                )}
                {status === 0 && canEdit && (
                    <Link href={`/store-requests/${id}/edit`} className={styles.editLink}>編集</Link>
                )}
            </div>
        </main>
    );
}
