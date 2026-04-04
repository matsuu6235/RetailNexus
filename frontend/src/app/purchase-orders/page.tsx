"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import Link from "next/link";
import { getPurchaseOrders, type PurchaseOrderSearchParams } from "@/lib/api/purchaseOrders";
import { getSuppliers } from "@/lib/api/suppliers";
import { getStores } from "@/lib/api/stores";
import type { PurchaseOrderListItem, PurchaseOrderStatus } from "@/types/purchaseOrders";
import { purchaseOrderStatusLabels } from "@/types/purchaseOrders";
import type { Supplier } from "@/types/suppliers";
import type { Store } from "@/types/stores";
import { formatDate, formatYen } from "@/lib/utils/formatters";
import { getStatusBadgeClass } from "@/lib/utils/statusBadge";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const PAGE_SIZE = 20;

export default function PurchaseOrdersPage() {
    const [orders, setOrders] = useState<PurchaseOrderListItem[]>([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [stores, setStores] = useState<Store[]>([]);

    // 入力状態
    const [orderNumberInput, setOrderNumberInput] = useState("");
    const [supplierIdInput, setSupplierIdInput] = useState("");
    const [storeIdInput, setStoreIdInput] = useState("");
    const [statusInput, setStatusInput] = useState("");
    const [orderDateFromInput, setOrderDateFromInput] = useState("");
    const [orderDateToInput, setOrderDateToInput] = useState("");
    const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

    // フィルター状態
    const [filters, setFilters] = useState<PurchaseOrderSearchParams>({});

    useEffect(() => {
        (async () => {
            try {
                const [supplierRes, storeRes] = await Promise.all([
                    getSuppliers(1, 200, { isActive: "active" }),
                    getStores(1, 200, { isActive: "active" }),
                ]);
                setSuppliers(supplierRes.items);
                setStores(storeRes.items);
            } catch {
                // ドロップダウン取得失敗は無視
            }
        })();
    }, []);

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);
                const res = await getPurchaseOrders(page, PAGE_SIZE, filters);
                if (!cancelled) {
                    setOrders(res.items);
                    setTotal(res.total);
                }
            } catch (e) {
                if (!cancelled) {
                    setError(e instanceof Error ? e.message : fallback.listFetchFailed("発注"));
                }
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();

        return () => { cancelled = true; };
    }, [page, filters]);

    const handleSearch = () => {
        setFilters({
            orderNumber: orderNumberInput,
            supplierId: supplierIdInput,
            storeId: storeIdInput,
            status: statusInput,
            orderDateFrom: orderDateFromInput ? new Date(orderDateFromInput).toISOString() : undefined,
            orderDateTo: orderDateToInput ? new Date(orderDateToInput).toISOString() : undefined,
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
                    <h1 className={styles.title}>発注一覧</h1>
                    <p className={styles.subtitle}>発注の検索・新規作成・詳細確認を行います。</p>
                </div>
                <Link href="/purchase-orders/new" className={styles.primaryButton}>
                    発注新規作成
                </Link>
            </header>

            <section className={styles.searchSection}>
                <div className={styles.searchGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発注番号</span>
                        <input value={orderNumberInput} onChange={(e) => setOrderNumberInput(e.target.value)} placeholder="PO-000001" className={styles.input} />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>仕入先</span>
                        <select value={supplierIdInput} onChange={(e) => setSupplierIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {suppliers.map((s) => (
                                <option key={s.supplierId} value={s.supplierId}>{s.supplierName}</option>
                            ))}
                        </select>
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発注元</span>
                        <select value={storeIdInput} onChange={(e) => setStoreIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {stores.map((s) => (
                                <option key={s.storeId} value={s.storeId}>{s.storeName}</option>
                            ))}
                        </select>
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>ステータス</span>
                        <select value={statusInput} onChange={(e) => setStatusInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            <option value="0">下書き</option>
                            <option value="1">承認待ち</option>
                            <option value="2">承認済</option>
                            <option value="3">仕入先確認済</option>
                            <option value="4">出荷準備中</option>
                            <option value="5">出荷済</option>
                            <option value="6">入荷済</option>
                            <option value="91">キャンセル依頼済</option>
                            <option value="92">キャンセル済</option>
                            <option value="93">仕入先キャンセル</option>
                        </select>
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発注日（From）</span>
                        <input type="date" value={orderDateFromInput} onChange={(e) => setOrderDateFromInput(e.target.value)} className={styles.input} />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発注日（To）</span>
                        <input type="date" value={orderDateToInput} onChange={(e) => setOrderDateToInput(e.target.value)} className={styles.input} />
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
                                        <th className={tableStyles.th}>発注番号</th>
                                        <th className={tableStyles.th}>仕入先</th>
                                        <th className={tableStyles.th}>発注元</th>
                                        <th className={tableStyles.th}>発注日</th>
                                        <th className={tableStyles.th}>合計金額</th>
                                        <th className={`${tableStyles.th} ${tableStyles.thCenter}`}>ステータス</th>
                                        <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {orders.length === 0 && (
                                        <tr>
                                            <td colSpan={7} className={tableStyles.empty}>データがありません</td>
                                        </tr>
                                    )}
                                    {orders.map((order, index) => {
                                        const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                                        return (
                                            <tr key={order.purchaseOrderId} className={rowClass}>
                                                <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{order.orderNumber}</td>
                                                <td className={tableStyles.td}>{order.supplierName}</td>
                                                <td className={tableStyles.td}>{order.storeName}</td>
                                                <td className={tableStyles.td}>{formatDate(order.orderDate)}</td>
                                                <td className={tableStyles.td} style={{ textAlign: "right" }}>{formatYen(order.totalAmount)}</td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                                                    <span className={`${styles.statusBadge} ${getStatusBadgeClass(order.status, styles)}`}>
                                                        {purchaseOrderStatusLabels[order.status]}
                                                    </span>
                                                </td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                                                    <Link href={`/purchase-orders/${order.purchaseOrderId}`} className={tableStyles.editButton}>
                                                        詳細
                                                    </Link>
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
