"use client";

import { useEffect, useState } from "react";
import { validation, fallback } from "@/lib/messages";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { createManualTransaction } from "@/lib/api/inventories";
import { getStores } from "@/lib/api/stores";
import { getProducts } from "@/lib/api/products";
import type { Store } from "@/types/stores";
import type { Product } from "@/types/products";
import type { InventoryTransactionType } from "@/types/inventories";
import styles from "./page.module.css";

const manualTypes: { value: InventoryTransactionType; label: string; sign: "+" | "-" | "±" }[] = [
    { value: 6, label: "初期在庫", sign: "+" },
    { value: 5, label: "棚卸調整", sign: "±" },
    { value: 4, label: "廃棄", sign: "-" },
];

export default function ManualTransactionPage() {
    const router = useRouter();

    const [stores, setStores] = useState<Store[]>([]);
    const [products, setProducts] = useState<Product[]>([]);

    const [storeId, setStoreId] = useState("");
    const [productId, setProductId] = useState("");
    const [transactionType, setTransactionType] = useState<string>("");
    const [quantity, setQuantity] = useState("");
    const [note, setNote] = useState("");

    const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        (async () => {
            try {
                const [storeRes, productRes] = await Promise.all([
                    getStores(1, 200, { isActive: "active" }),
                    getProducts(1, 200, { isActive: "active" }),
                ]);
                setStores(storeRes.items);
                setProducts(productRes.items);
            } catch {
                setError(fallback.masterFetchFailed);
            }
        })();
    }, []);

    const validate = (): Record<string, string> => {
        const errors: Record<string, string> = {};
        if (!storeId) errors.storeId = validation.required("店舗");
        if (!productId) errors.productId = validation.required("商品");
        if (!transactionType) errors.transactionType = validation.required("取引種別");
        if (!quantity || Number(quantity) === 0) errors.quantity = validation.required("数量");
        if (note.length > 500) errors.note = validation.maxLength("備考", 500);
        return errors;
    };

    const handleSubmit = async () => {
        const errors = validate();
        setFieldErrors(errors);
        if (Object.keys(errors).length > 0) return;

        const selectedType = Number(transactionType) as InventoryTransactionType;
        let quantityChange = Number(quantity);

        // 廃棄の場合は必ずマイナスにする
        if (selectedType === 4 && quantityChange > 0) {
            quantityChange = -quantityChange;
        }

        try {
            setSubmitting(true);
            setError(null);

            await createManualTransaction({
                storeId,
                productId,
                transactionType: selectedType,
                quantityChange,
                note: note.trim() || undefined,
            });

            router.push("/inventory-transactions");
        } catch (e) {
            setError(e instanceof Error ? e.message : fallback.operationFailed);
        } finally {
            setSubmitting(false);
        }
    };

    const selectedTypeInfo = manualTypes.find((t) => String(t.value) === transactionType);

    return (
        <main className={styles.page}>
            <h1 className={styles.title}>手動入出庫登録</h1>

            {error && <div className={styles.errorBox}>{error}</div>}

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>入出庫情報</h2>
                <div className={styles.formGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>店舗 *</span>
                        <select value={storeId} onChange={(e) => setStoreId(e.target.value)} className={styles.select}>
                            <option value="">選択してください</option>
                            {stores.map((s) => (
                                <option key={s.storeId} value={s.storeId}>{s.storeCode} - {s.storeName}</option>
                            ))}
                        </select>
                        {fieldErrors.storeId && <span className={styles.fieldError}>{fieldErrors.storeId}</span>}
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>商品 *</span>
                        <select value={productId} onChange={(e) => setProductId(e.target.value)} className={styles.select}>
                            <option value="">選択してください</option>
                            {products.map((p) => (
                                <option key={p.id} value={p.id}>{p.productCode} - {p.productName}</option>
                            ))}
                        </select>
                        {fieldErrors.productId && <span className={styles.fieldError}>{fieldErrors.productId}</span>}
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>取引種別 *</span>
                        <select value={transactionType} onChange={(e) => setTransactionType(e.target.value)} className={styles.select}>
                            <option value="">選択してください</option>
                            {manualTypes.map((t) => (
                                <option key={t.value} value={t.value}>{t.label}（{t.sign}）</option>
                            ))}
                        </select>
                        {fieldErrors.transactionType && <span className={styles.fieldError}>{fieldErrors.transactionType}</span>}
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>
                            数量 *
                            {selectedTypeInfo && (
                                <span className={styles.hint}>
                                    {selectedTypeInfo.sign === "+" && "（プラス値で入力）"}
                                    {selectedTypeInfo.sign === "-" && "（プラス値で入力 → 自動的にマイナス）"}
                                    {selectedTypeInfo.sign === "±" && "（増加: プラス / 減少: マイナスで入力）"}
                                </span>
                            )}
                        </span>
                        <input
                            type="number"
                            value={quantity}
                            onChange={(e) => setQuantity(e.target.value)}
                            step="0.01"
                            className={styles.input}
                            placeholder="数量を入力"
                        />
                        {fieldErrors.quantity && <span className={styles.fieldError}>{fieldErrors.quantity}</span>}
                    </label>

                    <label className={`${styles.field} ${styles.fieldFull}`}>
                        <span className={styles.fieldLabel}>備考</span>
                        <textarea
                            value={note}
                            onChange={(e) => setNote(e.target.value)}
                            className={styles.textarea}
                            placeholder="廃棄理由、棚卸差異の説明など"
                            maxLength={500}
                        />
                        {fieldErrors.note && <span className={styles.fieldError}>{fieldErrors.note}</span>}
                    </label>
                </div>
            </div>

            <div className={styles.actions}>
                <Link href="/inventory-transactions" className={styles.cancelButton}>キャンセル</Link>
                <button type="button" onClick={handleSubmit} disabled={submitting} className={styles.submitButton}>
                    {submitting ? "登録中..." : "登録"}
                </button>
            </div>
        </main>
    );
}
