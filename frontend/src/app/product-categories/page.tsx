"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
    getAllProductCategories,
    reorderProductCategories,
} from "../lib/api/productCategories";
import type { ProductCategory } from "../types/productCategories";
import styles from "./page.module.css";
import tableStyles from "../components/table/MasterTable.module.css";

function moveItem(items: ProductCategory[], fromId: string, toId: string): ProductCategory[] {
    const fromIndex = items.findIndex((item) => item.productCategoryId === fromId);
    const toIndex = items.findIndex((item) => item.productCategoryId === toId);

    if (fromIndex < 0 || toIndex < 0 || fromIndex === toIndex) {
        return items;
    }

    const next = [...items];
    const [moved] = next.splice(fromIndex, 1);
    next.splice(toIndex, 0, moved);

    return next;
}

function normalizeDisplayOrder(items: ProductCategory[]): ProductCategory[] {
    return items.map((item, index) => ({
        ...item,
        displayOrder: index + 1,
    }));
}

export default function ProductCategoriesPage() {
    const router = useRouter();

    const [items, setItems] = useState<ProductCategory[]>([]);
    const [originalOrderIds, setOriginalOrderIds] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [savingOrder, setSavingOrder] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [codeInput, setCodeInput] = useState("");
    const [nameInput, setNameInput] = useState("");
    const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

    const [codeFilter, setCodeFilter] = useState("");
    const [nameFilter, setNameFilter] = useState("");
    const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

    const [draggingId, setDraggingId] = useState<string | null>(null);
    const [dragOverId, setDragOverId] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);

                const response = await getAllProductCategories();
                const normalized = normalizeDisplayOrder(response);

                if (!cancelled) {
                    setItems(normalized);
                    setOriginalOrderIds(normalized.map((item) => item.productCategoryId));
                }
            } catch (e) {
                if (!cancelled) {
                    setError(e instanceof Error ? e.message : "商品カテゴリ一覧の取得に失敗しました。");
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        })();

        return () => {
            cancelled = true;
        };
    }, []);

    const filteredItems = items.filter((item) => {
        const matchCode =
            !codeFilter.trim() || item.productCategoryCd.includes(codeFilter.trim());
        const matchName =
            !nameFilter.trim() || item.productCategoryName.includes(nameFilter.trim());
        const matchStatus =
            isActiveFilter === "all" ||
            (isActiveFilter === "active" && item.isActive) ||
            (isActiveFilter === "inactive" && !item.isActive);

        return matchCode && matchName && matchStatus;
    });

    const isDirty =
        items.length > 0 &&
        items.map((item) => item.productCategoryId).join(",") !== originalOrderIds.join(",");

    const onSaveOrder = async () => {
        try {
            setSavingOrder(true);
            setError(null);

            await reorderProductCategories(items.map((item) => item.productCategoryId));

            const normalized = normalizeDisplayOrder(items);
            setItems(normalized);
            setOriginalOrderIds(normalized.map((item) => item.productCategoryId));
        } catch (e) {
            setError(e instanceof Error ? e.message : "表示順の保存に失敗しました。");
        } finally {
            setSavingOrder(false);
        }
    };

    const onResetOrder = () => {
        const orderMap = new Map(originalOrderIds.map((id, index) => [id, index]));
        const resetItems = [...items].sort((a, b) => {
            return (orderMap.get(a.productCategoryId) ?? 0) - (orderMap.get(b.productCategoryId) ?? 0);
        });

        setItems(normalizeDisplayOrder(resetItems));
        setDraggingId(null);
        setDragOverId(null);
    };

    return (
        <main className={styles.page}>
            <header className={styles.header}>
                <div>
                    <h1 className={styles.title}>商品カテゴリマスタ</h1>
                    <p className={styles.subtitle}>
                        商品カテゴリの検索・編集・新規登録を行います。
                    </p>
                </div>

                <button
                    type="button"
                    onClick={() => router.push("/product-categories/new")}
                    className={styles.primaryButton}
                >
                    商品カテゴリ新規作成
                </button>
            </header>

            <section className={styles.searchSection}>
                <div className={styles.searchGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>商品カテゴリコード</span>
                        <input
                            value={codeInput}
                            onChange={(e) => setCodeInput(e.target.value)}
                            placeholder="商品カテゴリコード"
                            className={styles.input}
                        />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>商品カテゴリ名</span>
                        <input
                            value={nameInput}
                            onChange={(e) => setNameInput(e.target.value)}
                            placeholder="商品カテゴリ名"
                            className={styles.input}
                        />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>状態</span>
                        <select
                            value={isActiveInput}
                            onChange={(e) => setIsActiveInput(e.target.value as "all" | "active" | "inactive")}
                            className={styles.select}
                        >
                            <option value="all">すべて</option>
                            <option value="active">有効</option>
                            <option value="inactive">無効</option>
                        </select>
                    </label>
                </div>

                <div className={styles.searchActions}>
                    <button
                        type="button"
                        onClick={() => {
                            setCodeFilter(codeInput);
                            setNameFilter(nameInput);
                            setIsActiveFilter(isActiveInput);
                        }}
                        className={styles.searchButton}
                    >
                        検索
                    </button>
                </div>
            </section>

            {isDirty && (
                <div className={styles.noticeBox}>
                    <div>
                        <strong>表示順が未保存です。</strong>
                        <div className={styles.noticeText}>
                            並び替えアイコンをドラッグして順序を変更した後、「並び順を保存」を押してください。
                        </div>
                    </div>
                    <div className={styles.noticeActions}>
                        <button
                            type="button"
                            onClick={onResetOrder}
                            className={styles.secondaryButton}
                            disabled={savingOrder}
                        >
                            元に戻す
                        </button>
                        <button
                            type="button"
                            onClick={onSaveOrder}
                            className={styles.saveButton}
                            disabled={savingOrder}
                        >
                            {savingOrder ? "保存中..." : "並び順を保存"}
                        </button>
                    </div>
                </div>
            )}

            {loading && <p>読み込み中...</p>}
            {error && <div className={styles.errorBox}>{error}</div>}

            {!loading && !error && (
                <div className={tableStyles.wrapper}>
                    <div className={tableStyles.summary}>
                        <span>{filteredItems.length} 件</span>
                        <span className={styles.dragHint}>「並び替え」アイコンをドラッグして表示順を変更できます</span>
                    </div>

                    <div className={tableStyles.tableContainer}>
                        <table className={tableStyles.table}>
                            <thead className={tableStyles.thead}>
                                <tr>
                                    <th className={`${tableStyles.th} ${styles.handleHeader}`}>
                                        <span className={styles.handleHeaderLabel}>並び替え</span>
                                    </th>
                                    <th className={tableStyles.th}>商品カテゴリコード</th>
                                    <th className={tableStyles.th}>商品カテゴリ名</th>
                                    <th className={tableStyles.th}>表示順</th>
                                    <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thStatus}`}>状態</th>
                                    <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredItems.length === 0 && (
                                    <tr>
                                        <td colSpan={6} className={tableStyles.empty}>
                                            データがありません
                                        </td>
                                    </tr>
                                )}

                                {filteredItems.map((item, index) => {
                                    const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd
                                        } ${draggingId === item.productCategoryId ? styles.draggingRow : ""} ${dragOverId === item.productCategoryId ? styles.dragOverRow : ""
                                        }`;

                                    const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive
                                        }`;

                                    const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive
                                        }`;

                                    return (
                                        <tr
                                            key={item.productCategoryId}
                                            className={rowClass}
                                            onDragOver={(e) => {
                                                if (!draggingId) {
                                                    return;
                                                }

                                                e.preventDefault();

                                                if (dragOverId !== item.productCategoryId) {
                                                    setDragOverId(item.productCategoryId);
                                                }
                                            }}
                                            onDrop={(e) => {
                                                if (!draggingId) {
                                                    return;
                                                }

                                                e.preventDefault();

                                                const fromId = draggingId;
                                                const toId = item.productCategoryId;

                                                if (fromId === toId) {
                                                    setDraggingId(null);
                                                    setDragOverId(null);
                                                    return;
                                                }

                                                setItems((current) => normalizeDisplayOrder(moveItem(current, fromId, toId)));
                                                setDraggingId(null);
                                                setDragOverId(null);
                                            }}
                                        >
                                            <td className={`${tableStyles.td} ${styles.handleCell}`}>
                                                <button
                                                    type="button"
                                                    draggable
                                                    onDragStart={(e) => {
                                                        setDraggingId(item.productCategoryId);
                                                        e.dataTransfer.effectAllowed = "move";
                                                        e.dataTransfer.setData("text/plain", item.productCategoryId);
                                                    }}
                                                    onDragEnd={() => {
                                                        setDraggingId(null);
                                                        setDragOverId(null);
                                                    }}
                                                    className={styles.dragHandleButton}
                                                    title="ドラッグして並び替え"
                                                    aria-label={`${item.productCategoryName} をドラッグして並び替え`}
                                                >
                                                    <span className={styles.dragHandle}>≡</span>
                                                </button>
                                            </td>
                                            <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>
                                                {item.productCategoryCd}
                                            </td>
                                            <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>
                                                {item.productCategoryName}
                                            </td>
                                            <td className={`${tableStyles.td} ${tableStyles.tdNumber}`}>
                                                {item.displayOrder}
                                            </td>
                                            <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                                                <span className={statusClass}>
                                                    <span className={statusDotClass} />
                                                    {item.isActive ? "有効" : "無効"}
                                                </span>
                                            </td>
                                            <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                                                <button
                                                    type="button"
                                                    onClick={() =>
                                                        router.push(`/product-categories/edit?id=${item.productCategoryId}`)
                                                    }
                                                    className={tableStyles.editButton}
                                                >
                                                    編集
                                                </button>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </main>
    );
}