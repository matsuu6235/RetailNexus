"use client";

import { useEffect, useState } from "react";
import {
    getAllProductCategories,
    reorderProductCategories,
} from "@/lib/api/productCategories";
import type { ProductCategory } from "@/types/productCategories";
import { useModal } from "@/lib/hooks/useModal";
import { useDragReorder } from "@/lib/hooks/useDragReorder";
import Modal from "@/components/modal/Modal";
import ProductCategoryForm from "./ProductCategoryForm";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const getCategoryId = (item: ProductCategory) => item.productCategoryId;

export default function ProductCategoriesPage() {
    const [items, setItems] = useState<ProductCategory[]>([]);
    const [originalOrderIds, setOriginalOrderIds] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [codeInput, setCodeInput] = useState("");
    const [nameInput, setNameInput] = useState("");
    const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

    const [codeFilter, setCodeFilter] = useState("");
    const [nameFilter, setNameFilter] = useState("");
    const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

    const modal = useModal();
    const drag = useDragReorder({
        getId: getCategoryId,
        items,
        setItems,
        originalOrderIds,
        setOriginalOrderIds,
        reorderFn: reorderProductCategories,
    });

    const fetchData = async () => {
        try {
            setLoading(true);
            setError(null);
            const response = await getAllProductCategories();
            const normalized = response.map((item, index) => ({ ...item, displayOrder: index + 1 }));
            setItems(normalized);
            setOriginalOrderIds(normalized.map(getCategoryId));
        } catch (e) {
            setError(e instanceof Error ? e.message : "商品カテゴリ一覧の取得に失敗しました。");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
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

    const handleSave = () => {
        modal.close();
        fetchData();
    };

    const displayError = error || drag.error;

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
                    onClick={modal.openCreate}
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

            {drag.isDirty && (
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
                            onClick={drag.onResetOrder}
                            className={styles.secondaryButton}
                            disabled={drag.savingOrder}
                        >
                            元に戻す
                        </button>
                        <button
                            type="button"
                            onClick={drag.onSaveOrder}
                            className={styles.saveButton}
                            disabled={drag.savingOrder}
                        >
                            {drag.savingOrder ? "保存中..." : "並び順を保存"}
                        </button>
                    </div>
                </div>
            )}

            {loading && <p>読み込み中...</p>}
            {displayError && <div className={styles.errorBox}>{displayError}</div>}

            {!loading && !displayError && (
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
                                        } ${drag.draggingId === item.productCategoryId ? styles.draggingRow : ""} ${drag.dragOverId === item.productCategoryId ? styles.dragOverRow : ""
                                        }`;

                                    const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive
                                        }`;

                                    const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive
                                        }`;

                                    return (
                                        <tr
                                            key={item.productCategoryId}
                                            className={rowClass}
                                            {...drag.getRowDragProps(item.productCategoryId)}
                                        >
                                            <td className={`${tableStyles.td} ${styles.handleCell}`}>
                                                <button
                                                    type="button"
                                                    {...drag.getDragHandleProps(item.productCategoryId)}
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
                                                    onClick={() => modal.openEdit(item.productCategoryId)}
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

            <Modal
                open={modal.modalMode !== null}
                title={modal.modalMode === "create" ? "商品カテゴリ新規作成" : "商品カテゴリ編集"}
                onClose={modal.close}
            >
                {modal.modalMode && (
                    <ProductCategoryForm
                        mode={modal.modalMode}
                        editId={modal.editId ?? undefined}
                        onSave={handleSave}
                        onCancel={modal.close}
                    />
                )}
            </Modal>
        </main>
    );
}
