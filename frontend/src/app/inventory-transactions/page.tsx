"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import Link from "next/link";
import { getInventoryTransactions, type InventoryTransactionSearchParams } from "@/lib/api/inventories";
import { getStores } from "@/lib/api/stores";
import type { InventoryTransactionListItem, InventoryTransactionType } from "@/types/inventories";
import { inventoryTransactionTypeLabels } from "@/types/inventories";
import type { Store } from "@/types/stores";
import { formatDateTime } from "@/lib/utils/formatters";
import { hasPermission } from "@/services/authService";
import Pager from "@/components/pager/Pager";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const PAGE_SIZE = 20;

const manualTransactionTypes: { value: string; label: string }[] = [
    { value: "1", label: "発注入荷" },
    { value: "2", label: "出荷（出庫）" },
    { value: "3", label: "入荷（入庫）" },
    { value: "4", label: "廃棄" },
    { value: "5", label: "棚卸調整" },
    { value: "6", label: "初期在庫" },
];

export default function InventoryTransactionsPage() {
    const [items, setItems] = useState<InventoryTransactionListItem[]>([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [stores, setStores] = useState<Store[]>([]);

    const [storeIdInput, setStoreIdInput] = useState("");
    const [transactionTypeInput, setTransactionTypeInput] = useState("");
    const [dateFromInput, setDateFromInput] = useState("");
    const [dateToInput, setDateToInput] = useState("");

    const [filters, setFilters] = useState<InventoryTransactionSearchParams>({});

    const canEdit = hasPermission("inventory.edit");

    useEffect(() => {
        (async () => {
            try {
                const storeRes = await getStores(1, 200, { isActive: "active" });
                setStores(storeRes.items);
            } catch {
                // ignore
            }
        })();
    }, []);

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);
                const res = await getInventoryTransactions(page, PAGE_SIZE, filters);
                if (!cancelled) {
                    setItems(res.items);
                    setTotal(res.total);
                }
            } catch (e) {
                if (!cancelled) {
                    setError(e instanceof Error ? e.message : fallback.listFetchFailed("在庫変動履歴"));
                }
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();

        return () => { cancelled = true; };
    }, [page, filters]);

    const handleSearch = () => {
        setFilters({
            storeId: storeIdInput || undefined,
            transactionType: transactionTypeInput || undefined,
            dateFrom: dateFromInput ? new Date(dateFromInput).toISOString() : undefined,
            dateTo: dateToInput ? new Date(dateToInput).toISOString() : undefined,
        });
        setPage(1);
    };

    const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
    const start = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
    const end = Math.min(page * PAGE_SIZE, total);

    const getChangeClass = (qty: number): string => {
        if (qty > 0) return styles.positive;
        if (qty < 0) return styles.negative;
        return "";
    };

    return (
        <main className={styles.page}>
            <header className={styles.header}>
                <div>
                    <h1 className={styles.title}>在庫変動履歴</h1>
                    <p className={styles.subtitle}>在庫の入出庫履歴を確認します。</p>
                </div>
                {canEdit && (
                    <Link href="/inventory-transactions/new" className={styles.primaryButton}>
                        手動入出庫登録
                    </Link>
                )}
            </header>

            <section className={styles.searchSection}>
                <div className={styles.searchGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>店舗</span>
                        <select value={storeIdInput} onChange={(e) => setStoreIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {stores.map((s) => (
                                <option key={s.storeId} value={s.storeId}>{s.storeName}</option>
                            ))}
                        </select>
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>取引種別</span>
                        <select value={transactionTypeInput} onChange={(e) => setTransactionTypeInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {manualTransactionTypes.map((t) => (
                                <option key={t.value} value={t.value}>{t.label}</option>
                            ))}
                        </select>
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発生日（From）</span>
                        <input type="date" value={dateFromInput} onChange={(e) => setDateFromInput(e.target.value)} className={styles.input} />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発生日（To）</span>
                        <input type="date" value={dateToInput} onChange={(e) => setDateToInput(e.target.value)} className={styles.input} />
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
                                        <th className={tableStyles.th}>発生日時</th>
                                        <th className={tableStyles.th}>店舗</th>
                                        <th className={tableStyles.th}>商品コード</th>
                                        <th className={tableStyles.th}>商品名</th>
                                        <th className={tableStyles.th}>取引種別</th>
                                        <th className={tableStyles.th} style={{ textAlign: "right" }}>増減数</th>
                                        <th className={tableStyles.th} style={{ textAlign: "right" }}>変動後在庫</th>
                                        <th className={tableStyles.th}>参照番号</th>
                                        <th className={tableStyles.th}>備考</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.length === 0 && (
                                        <tr>
                                            <td colSpan={9} className={tableStyles.empty}>データがありません</td>
                                        </tr>
                                    )}
                                    {items.map((item, index) => {
                                        const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                                        return (
                                            <tr key={item.inventoryTransactionId} className={rowClass}>
                                                <td className={tableStyles.td} style={{ whiteSpace: "nowrap" }}>{formatDateTime(item.occurredAt)}</td>
                                                <td className={tableStyles.td}>{item.storeName}</td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{item.productCode}</td>
                                                <td className={tableStyles.td}>{item.productName}</td>
                                                <td className={tableStyles.td}>
                                                    <span className={styles.typeBadge}>
                                                        {inventoryTransactionTypeLabels[item.transactionType]}
                                                    </span>
                                                </td>
                                                <td className={`${tableStyles.td} ${getChangeClass(item.quantityChange)}`} style={{ textAlign: "right" }}>
                                                    {item.quantityChange > 0 ? "+" : ""}{item.quantityChange.toLocaleString()}
                                                </td>
                                                <td className={tableStyles.td} style={{ textAlign: "right" }}>
                                                    {item.quantityAfter.toLocaleString()}
                                                </td>
                                                <td className={tableStyles.td}>{item.referenceNumber || "-"}</td>
                                                <td className={tableStyles.td}>{item.note || "-"}</td>
                                            </tr>
                                        );
                                    })}
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <Pager page={page} totalPages={totalPages} onPageChange={setPage} />
                </>
            )}
        </main>
    );
}
