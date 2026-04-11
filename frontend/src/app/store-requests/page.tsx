"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import Link from "next/link";
import { getStoreRequests, type StoreRequestSearchParams } from "@/lib/api/storeRequests";
import { getStores } from "@/lib/api/stores";
import type { StoreRequestListItem, StoreRequestStatus } from "@/types/storeRequests";
import { storeRequestStatusLabels } from "@/types/storeRequests";
import type { Store } from "@/types/stores";
import { formatDate } from "@/lib/utils/formatters";
import { getStatusBadgeClass } from "@/lib/utils/statusBadge";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const PAGE_SIZE = 20;

export default function StoreRequestsPage() {
    const [requests, setRequests] = useState<StoreRequestListItem[]>([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [stores, setStores] = useState<Store[]>([]);

    const [requestNumberInput, setRequestNumberInput] = useState("");
    const [fromStoreIdInput, setFromStoreIdInput] = useState("");
    const [toStoreIdInput, setToStoreIdInput] = useState("");
    const [statusInput, setStatusInput] = useState("");
    const [requestDateFromInput, setRequestDateFromInput] = useState("");
    const [requestDateToInput, setRequestDateToInput] = useState("");
    const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

    const [filters, setFilters] = useState<StoreRequestSearchParams>({});

    useEffect(() => {
        (async () => {
            try {
                const storeRes = await getStores(1, 200, { isActive: "active" });
                setStores(storeRes.items);
            } catch { /* ignore */ }
        })();
    }, []);

    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                setLoading(true);
                setError(null);
                const res = await getStoreRequests(page, PAGE_SIZE, filters);
                if (!cancelled) { setRequests(res.items); setTotal(res.total); }
            } catch (e) {
                if (!cancelled) setError(e instanceof Error ? e.message : fallback.listFetchFailed("発送依頼"));
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();
        return () => { cancelled = true; };
    }, [page, filters]);

    const handleSearch = () => {
        setFilters({
            requestNumber: requestNumberInput,
            fromStoreId: fromStoreIdInput,
            toStoreId: toStoreIdInput,
            status: statusInput,
            requestDateFrom: requestDateFromInput ? new Date(requestDateFromInput).toISOString() : undefined,
            requestDateTo: requestDateToInput ? new Date(requestDateToInput).toISOString() : undefined,
            isActive: isActiveInput,
        });
        setPage(1);
    };

    const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
    const start = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
    const end = Math.min(page * PAGE_SIZE, total);

    return (
        <main className={styles.page}>
            <header className={styles.header}>
                <div>
                    <h1 className={styles.title}>発送依頼一覧</h1>
                    <p className={styles.subtitle}>発送依頼の検索・新規作成・詳細確認を行います。</p>
                </div>
                <Link href="/store-requests/new" className={styles.primaryButton}>発送依頼新規作成</Link>
            </header>

            <section className={styles.searchSection}>
                <div className={styles.searchGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>依頼番号</span>
                        <input value={requestNumberInput} onChange={(e) => setRequestNumberInput(e.target.value)} placeholder="SR-000001" className={styles.input} />
                    </label>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>依頼元</span>
                        <select value={fromStoreIdInput} onChange={(e) => setFromStoreIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {stores.map((s) => <option key={s.storeId} value={s.storeId}>{s.storeName}</option>)}
                        </select>
                    </label>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>依頼先</span>
                        <select value={toStoreIdInput} onChange={(e) => setToStoreIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {stores.map((s) => <option key={s.storeId} value={s.storeId}>{s.storeName}</option>)}
                        </select>
                    </label>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>ステータス</span>
                        <select value={statusInput} onChange={(e) => setStatusInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            <option value="0">下書き</option>
                            <option value="1">承認待ち</option>
                            <option value="2">承認済</option>
                            <option value="3">確認済</option>
                            <option value="4">出荷準備中</option>
                            <option value="5">出荷済</option>
                            <option value="6">入荷済</option>
                            <option value="91">キャンセル依頼済</option>
                            <option value="92">キャンセル済</option>
                            <option value="93">却下</option>
                        </select>
                    </label>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>依頼日（From）</span>
                        <input type="date" value={requestDateFromInput} onChange={(e) => setRequestDateFromInput(e.target.value)} className={styles.input} />
                    </label>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>依頼日（To）</span>
                        <input type="date" value={requestDateToInput} onChange={(e) => setRequestDateToInput(e.target.value)} className={styles.input} />
                    </label>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>状態</span>
                        <select value={isActiveInput} onChange={(e) => setIsActiveInput(e.target.value as "all" | "active" | "inactive")} className={styles.select}>
                            <option value="all">すべて</option>
                            <option value="active">有効</option>
                            <option value="inactive">無効</option>
                        </select>
                    </label>
                </div>
                <div className={styles.searchActions}>
                    <button type="button" onClick={handleSearch} className={styles.searchButton}>検索</button>
                </div>
            </section>

            {loading && <p>読み込み中...</p>}
            {error && <div className={styles.errorBox}>{error}</div>}

            {!loading && !error && (
                <>
                    <div className={tableStyles.wrapper}>
                        <div className={tableStyles.summary}>
                            <span>{total > 0 ? `${start} - ${end} / ${total} 件` : "0 件"}</span>
                        </div>
                        <div className={tableStyles.tableContainer}>
                            <table className={tableStyles.table}>
                                <thead className={tableStyles.thead}>
                                    <tr>
                                        <th className={tableStyles.th}>依頼番号</th>
                                        <th className={tableStyles.th}>依頼元</th>
                                        <th className={tableStyles.th}>依頼先</th>
                                        <th className={tableStyles.th}>依頼日</th>
                                        <th className={`${tableStyles.th} ${tableStyles.thCenter}`}>ステータス</th>
                                        <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {requests.length === 0 && (
                                        <tr><td colSpan={6} className={tableStyles.empty}>データがありません</td></tr>
                                    )}
                                    {requests.map((req, index) => {
                                        const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                                        return (
                                            <tr key={req.storeRequestId} className={rowClass}>
                                                <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{req.requestNumber}</td>
                                                <td className={tableStyles.td}>{req.fromStoreName}</td>
                                                <td className={tableStyles.td}>{req.toStoreName}</td>
                                                <td className={tableStyles.td}>{formatDate(req.requestDate)}</td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                                                    <span className={`${styles.statusBadge} ${getStatusBadgeClass(req.status, styles)}`}>
                                                        {storeRequestStatusLabels[req.status]}
                                                    </span>
                                                </td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                                                    <Link href={`/store-requests/${req.requestNumber}`} className={tableStyles.editButton}>詳細</Link>
                                                </td>
                                            </tr>
                                        );
                                    })}
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div className={styles.pager}>
                        <span className={styles.pagerText}>ページ {page} / {totalPages}</span>
                        <div className={styles.pagerButtons}>
                            <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)} className={styles.pagerButton}>前へ</button>
                            <button type="button" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)} className={styles.pagerButton}>次へ</button>
                        </div>
                    </div>
                </>
            )}
        </main>
    );
}
