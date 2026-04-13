"use client";

import { useEffect, useState } from "react";
import { useModal } from "@/lib/hooks/useModal";
import { getSuppliers } from "@/lib/api/suppliers";
import { fallback } from "@/lib/messages";
import type { Supplier } from "@/types/suppliers";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";
import { formatPhoneNumber } from "@/lib/utils/phoneNumber";
import Modal from "@/components/modal/Modal";
import Pager from "@/components/pager/Pager";
import SupplierForm from "./SupplierForm";

const PAGE_SIZE = 20;

export default function SuppliersPage() {
    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [supplierCodeInput, setSupplierCodeInput] = useState("");
    const [supplierNameInput, setSupplierNameInput] = useState("");
    const [phoneNumberInput, setPhoneNumberInput] = useState("");
    const [emailInput, setEmailInput] = useState("");
    const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

    const [supplierCodeFilter, setSupplierCodeFilter] = useState("");
    const [supplierNameFilter, setSupplierNameFilter] = useState("");
    const [phoneNumberFilter, setPhoneNumberFilter] = useState("");
    const [emailFilter, setEmailFilter] = useState("");
    const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

    const modal = useModal();
    const [refreshKey, setRefreshKey] = useState(0);

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);

                const res = await getSuppliers(page, PAGE_SIZE, {
                    supplierCode: supplierCodeFilter,
                    supplierName: supplierNameFilter,
                    phoneNumber: phoneNumberFilter,
                    email: emailFilter,
                    isActive: isActiveFilter,
                });

                if (!cancelled) {
                    setSuppliers(res.items);
                    setTotal(res.total);
                }
            } catch (e) {
                if (!cancelled) {
                    setError(e instanceof Error ? e.message : fallback.listFetchFailed("仕入先"));
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
    }, [page, supplierCodeFilter, supplierNameFilter, phoneNumberFilter, emailFilter, isActiveFilter, refreshKey]);

    const handleSave = () => {
        modal.close();
        setRefreshKey((k) => k + 1);
    };

    const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
    const start = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
    const end = Math.min(page * PAGE_SIZE, total);

    return (
        <main className={styles.page}>
            <header className={styles.header}>
                <div>
                    <h1 className={styles.title}>仕入先一覧</h1>
                    <p className={styles.subtitle}>仕入先マスタの検索・編集・新規登録を行います。</p>
                </div>

                <button
                    type="button"
                    onClick={modal.openCreate}
                    className={styles.primaryButton}
                >
                    仕入先新規作成
                </button>
            </header>

            <section className={styles.searchSection}>
                <div className={styles.searchGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>仕入先コード</span>
                        <input
                            value={supplierCodeInput}
                            onChange={(e) => setSupplierCodeInput(e.target.value)}
                            placeholder="仕入先コード"
                            className={styles.input}
                        />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>仕入先名</span>
                        <input
                            value={supplierNameInput}
                            onChange={(e) => setSupplierNameInput(e.target.value)}
                            placeholder="仕入先名"
                            className={styles.input}
                        />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>電話番号</span>
                        <input
                            value={phoneNumberInput}
                            onChange={(e) => setPhoneNumberInput(e.target.value)}
                            placeholder="電話番号"
                            className={styles.input}
                        />
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>メールアドレス</span>
                        <input
                            value={emailInput}
                            onChange={(e) => setEmailInput(e.target.value)}
                            placeholder="メールアドレス"
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
                            setSupplierCodeFilter(supplierCodeInput);
                            setSupplierNameFilter(supplierNameInput);
                            setPhoneNumberFilter(phoneNumberInput);
                            setEmailFilter(emailInput);
                            setIsActiveFilter(isActiveInput);
                            setPage(1);
                        }}
                        className={styles.searchButton}
                    >
                        検索
                    </button>
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
                                        <th className={tableStyles.th}>仕入先コード</th>
                                        <th className={tableStyles.th}>仕入先名</th>
                                        <th className={tableStyles.th}>電話番号</th>
                                        <th className={tableStyles.th}>メールアドレス</th>
                                        <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thStatus}`}>状態</th>
                                        <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {suppliers.length === 0 && (
                                        <tr>
                                            <td colSpan={6} className={tableStyles.empty}>
                                                データがありません
                                            </td>
                                        </tr>
                                    )}

                                    {suppliers.map((supplier, index) => {
                                        const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                                        const statusClass = `${tableStyles.statusChip} ${supplier.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                                        const statusDotClass = `${tableStyles.statusDot} ${supplier.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                                        return (
                                            <tr key={supplier.supplierId} className={rowClass}>
                                                <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>
                                                    {supplier.supplierCode}
                                                </td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>{supplier.supplierName}</td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdMuted}`}>
                                                    {supplier.phoneNumber ? formatPhoneNumber(supplier.phoneNumber) : "-"}
                                                </td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdMuted}`}>{supplier.email || "-"}</td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                                                    <span className={statusClass}>
                                                        <span className={statusDotClass} />
                                                        {supplier.isActive ? "有効" : "無効"}
                                                    </span>
                                                </td>
                                                <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                                                    <button
                                                        type="button"
                                                        onClick={() => modal.openEdit(supplier.supplierId)}
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

                    <Pager page={page} totalPages={totalPages} onPageChange={setPage} />
                </>
            )}

            <Modal open={modal.modalMode !== null} title={modal.modalMode === "create" ? "仕入先新規作成" : "仕入先編集"} onClose={modal.close}>
                {modal.modalMode && (
                    <SupplierForm mode={modal.modalMode} editId={modal.editId ?? undefined} onSave={handleSave} onCancel={modal.close} />
                )}
            </Modal>
        </main>
    );
}
