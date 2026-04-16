"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import { getInventories, type InventorySearchParams } from "@/lib/api/inventories";
import { getAreas } from "@/lib/api/areas";
import { getStores } from "@/lib/api/stores";
import { getProductCategories } from "@/lib/api/productCategories";
import type { InventoryListItem } from "@/types/inventories";
import type { Area } from "@/types/areas";
import type { Store } from "@/types/stores";
import type { ProductCategory } from "@/types/productCategories";
import Pager from "@/components/pager/Pager";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const PAGE_SIZE = 20;

export default function InventoriesPage() {
    const [items, setItems] = useState<InventoryListItem[]>([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [areas, setAreas] = useState<Area[]>([]);
    const [stores, setStores] = useState<Store[]>([]);
    const [allStores, setAllStores] = useState<Store[]>([]);
    const [categories, setCategories] = useState<ProductCategory[]>([]);

    const [areaIdInput, setAreaIdInput] = useState("");
    const [storeIdInput, setStoreIdInput] = useState("");
    const [categoryIdInput, setCategoryIdInput] = useState("");
    const [productCodeInput, setProductCodeInput] = useState("");
    const [stockStatusInput, setStockStatusInput] = useState("all");

    const [filters, setFilters] = useState<InventorySearchParams>({});

    useEffect(() => {
        (async () => {
            try {
                const [areaRes, storeRes, catRes] = await Promise.all([
                    getAreas(1, 200, { isActive: "active" }),
                    getStores(1, 200, { isActive: "active" }),
                    getProductCategories(1, 200, { isActive: "active" }),
                ]);
                setAreas(areaRes.items);
                setAllStores(storeRes.items);
                setStores(storeRes.items);
                setCategories(catRes.items);
            } catch {
                // ignore
            }
        })();
    }, []);

    useEffect(() => {
        if (areaIdInput) {
            setStores(allStores.filter((s) => s.areaId === areaIdInput));
        } else {
            setStores(allStores);
        }
        setStoreIdInput("");
    }, [areaIdInput, allStores]);

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);
                const res = await getInventories(page, PAGE_SIZE, filters);
                if (!cancelled) {
                    setItems(res.items);
                    setTotal(res.total);
                }
            } catch (e) {
                if (!cancelled) {
                    setError(e instanceof Error ? e.message : fallback.listFetchFailed("在庫"));
                }
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();

        return () => { cancelled = true; };
    }, [page, filters]);

    const handleSearch = () => {
        setFilters({
            areaId: areaIdInput || undefined,
            storeId: storeIdInput || undefined,
            productCategoryId: categoryIdInput || undefined,
            productCode: productCodeInput || undefined,
            stockStatus: stockStatusInput,
        });
        setPage(1);
    };

    const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
    const start = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
    const end = Math.min(page * PAGE_SIZE, total);

    const getQuantityClass = (qty: number): string => {
        if (qty < 0) return styles.quantityNegative;
        if (qty === 0) return styles.quantityZero;
        return "";
    };

    return (
        <main className={styles.page}>
            <header className={styles.header}>
                <div>
                    <h1 className={styles.title}>在庫一覧</h1>
                    <p className={styles.subtitle}>全店舗の在庫状況を確認します。</p>
                </div>
            </header>

            <form className={styles.searchSection} onSubmit={(e) => { e.preventDefault(); handleSearch(); }}>
                <div className={styles.searchGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>エリア</span>
                        <select value={areaIdInput} onChange={(e) => setAreaIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {areas.map((a) => (
                                <option key={a.areaId} value={a.areaId}>{a.areaName}</option>
                            ))}
                        </select>
                    </label>

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
                        <span className={styles.fieldLabel}>商品カテゴリ</span>
                        <select value={categoryIdInput} onChange={(e) => setCategoryIdInput(e.target.value)} className={styles.select}>
                            <option value="">すべて</option>
                            {categories.map((c) => (
                                <option key={c.productCategoryId} value={c.productCategoryId}>{c.productCategoryName}</option>
                            ))}
                        </select>
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>商品コード</span>
                        <input value={productCodeInput} onChange={(e) => setProductCodeInput(e.target.value)} placeholder="部分一致" className={styles.input} />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>在庫状態</span>
                        <select value={stockStatusInput} onChange={(e) => setStockStatusInput(e.target.value)} className={styles.select}>
                            <option value="all">すべて</option>
                            <option value="inStock">在庫あり</option>
                            <option value="outOfStock">在庫切れ</option>
                        </select>
                    </label>
                </div>

                <div className={styles.searchActions}>
                    <button
                        type="button"
                        onClick={() => {
                            setAreaIdInput(""); setStoreIdInput(""); setCategoryIdInput(""); setProductCodeInput(""); setStockStatusInput("all");
                            setFilters({}); setPage(1);
                        }}
                        className={styles.clearButton}
                    >
                        クリア
                    </button>
                    <button type="submit" className={styles.searchButton}>検索</button>
                </div>
            </form>

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
                                        <th className={tableStyles.th}>エリア</th>
                                        <th className={tableStyles.th}>店舗</th>
                                        <th className={tableStyles.th}>商品コード</th>
                                        <th className={tableStyles.th}>商品名</th>
                                        <th className={tableStyles.th} style={{ textAlign: "right" }}>在庫数</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.length === 0 && (
                                        <tr>
                                            <td colSpan={5} className={tableStyles.empty}>データがありません</td>
                                        </tr>
                                    )}
                                    {items.map((item, index) => {
                                        const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                                        return (
                                            <tr key={item.inventoryId} className={rowClass}>
                                                <td className={tableStyles.td}>{item.areaName}</td>
                                                <td className={tableStyles.td}>{item.storeName}</td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{item.productCode}</td>
                                                <td className={tableStyles.td}>{item.productName}</td>
                                                <td className={`${tableStyles.td} ${getQuantityClass(item.quantity)}`} style={{ textAlign: "right" }}>
                                                    {item.quantity.toLocaleString()}
                                                </td>
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
