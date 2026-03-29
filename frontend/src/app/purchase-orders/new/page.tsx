"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { createPurchaseOrder } from "@/lib/api/purchaseOrders";
import { getSuppliers } from "@/lib/api/suppliers";
import { getStores } from "@/lib/api/stores";
import { getProducts } from "@/lib/api/products";
import type { Supplier } from "@/types/suppliers";
import type { Store } from "@/types/stores";
import type { Product } from "@/types/products";
import {
    validatePurchaseOrderHeader,
    validateDetail,
    type PurchaseOrderFormFields,
    type PurchaseOrderFieldErrors,
    type DetailFormFields,
    type DetailFieldErrors,
} from "@/lib/validators/purchaseOrderValidator";
import styles from "./page.module.css";

type DetailRow = DetailFormFields & { key: number };

const today = new Date().toISOString().split("T")[0];

export default function PurchaseOrderNewPage() {
    const router = useRouter();

    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [stores, setStores] = useState<Store[]>([]);
    const [products, setProducts] = useState<Product[]>([]);

    const [form, setForm] = useState<PurchaseOrderFormFields>({
        supplierId: "",
        storeId: "",
        orderDate: today,
        desiredDeliveryDate: "",
        note: "",
    });
    const [fieldErrors, setFieldErrors] = useState<PurchaseOrderFieldErrors>({});

    const [details, setDetails] = useState<DetailRow[]>([
        { key: 1, productId: "", quantity: "", unitPrice: "" },
    ]);
    const [detailErrors, setDetailErrors] = useState<Record<number, DetailFieldErrors>>({});
    const [nextKey, setNextKey] = useState(2);

    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        (async () => {
            try {
                const [supplierRes, storeRes, productRes] = await Promise.all([
                    getSuppliers(1, 200, { isActive: "active" }),
                    getStores(1, 200, { isActive: "active" }),
                    getProducts(1, 200, { isActive: "active" }),
                ]);
                setSuppliers(supplierRes.items);
                setStores(storeRes.items);
                setProducts(productRes.items);
            } catch {
                setError("マスタデータの取得に失敗しました。");
            }
        })();
    }, []);

    const handleHeaderChange = (field: keyof PurchaseOrderFormFields, value: string) => {
        const updated = { ...form, [field]: value };
        setForm(updated);
        const errors = validatePurchaseOrderHeader(updated);
        setFieldErrors((prev) => ({ ...prev, [field]: errors[field] }));
    };

    const handleDetailChange = (key: number, field: keyof DetailFormFields, value: string) => {
        let updates: Partial<DetailFormFields> = { [field]: value };

        // 商品選択時に重複チェック＋仕入単価を自動セット
        if (field === "productId" && value) {
            const isDuplicate = details.some((d) => d.key !== key && d.productId === value);
            if (isDuplicate) {
                setDetailErrors((prev) => ({
                    ...prev,
                    [key]: { ...prev[key], productId: "この商品は既に追加されています。数量を変更してください。" },
                }));
                return;
            }
            const product = products.find((p) => p.id === value);
            if (product) {
                updates.unitPrice = String(product.cost);
            }
        }

        setDetails((prev) =>
            prev.map((d) => (d.key === key ? { ...d, ...updates } : d))
        );
        const row = details.find((d) => d.key === key);
        if (row) {
            const updated = { ...row, ...updates };
            const errors = validateDetail(updated);
            setDetailErrors((prev) => ({ ...prev, [key]: { ...prev[key], [field]: errors[field] } }));
        }
    };

    const addDetailRow = () => {
        setDetails((prev) => [...prev, { key: nextKey, productId: "", quantity: "", unitPrice: "" }]);
        setNextKey((k) => k + 1);
    };

    const removeDetailRow = (key: number) => {
        setDetails((prev) => prev.filter((d) => d.key !== key));
        setDetailErrors((prev) => {
            const copy = { ...prev };
            delete copy[key];
            return copy;
        });
    };

    const getSubTotal = (row: DetailRow): number => {
        const qty = Number(row.quantity);
        const price = Number(row.unitPrice);
        if (isNaN(qty) || isNaN(price)) return 0;
        return qty * price;
    };

    const totalAmount = details.reduce((sum, d) => sum + getSubTotal(d), 0);

    // 重複商品IDを検出
    const duplicateProductIds = new Set<string>();
    const seenProductIds = new Map<string, number>();
    for (const d of details) {
        if (!d.productId) continue;
        if (seenProductIds.has(d.productId)) {
            duplicateProductIds.add(d.productId);
        }
        seenProductIds.set(d.productId, d.key);
    }

    const handleSubmit = async () => {
        const headerErrors = validatePurchaseOrderHeader(form);
        setFieldErrors(headerErrors);

        const allDetailErrors: Record<number, DetailFieldErrors> = {};
        let hasDetailError = false;
        for (const d of details) {
            const errors = validateDetail(d);
            allDetailErrors[d.key] = errors;
            if (Object.keys(errors).length > 0) hasDetailError = true;
        }
        setDetailErrors(allDetailErrors);

        if (details.length === 0) {
            setError("明細を1行以上入力してください。");
            return;
        }

        if (Object.keys(headerErrors).length > 0 || hasDetailError) return;

        try {
            setSubmitting(true);
            setError(null);

            const res = await createPurchaseOrder({
                supplierId: form.supplierId,
                storeId: form.storeId,
                orderDate: new Date(form.orderDate).toISOString(),
                desiredDeliveryDate: form.desiredDeliveryDate ? new Date(form.desiredDeliveryDate).toISOString() : null,
                note: form.note.trim() || undefined,
                details: details.map((d) => ({
                    productId: d.productId,
                    quantity: Number(d.quantity),
                    unitPrice: Number(d.unitPrice),
                })),
            });

            router.push(`/purchase-orders/${res.purchaseOrderId}`);
        } catch (e) {
            setError(e instanceof Error ? e.message : "発注の作成に失敗しました。");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <main className={styles.page}>
            <h1 className={styles.title}>発注新規作成</h1>

            {error && <div className={styles.errorBox}>{error}</div>}

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>発注ヘッダ</h2>
                <div className={styles.formGrid}>
                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>仕入先 *</span>
                        <select value={form.supplierId} onChange={(e) => handleHeaderChange("supplierId", e.target.value)} className={styles.select}>
                            <option value="">選択してください</option>
                            {suppliers.map((s) => (
                                <option key={s.supplierId} value={s.supplierId}>{s.supplierCode} - {s.supplierName}</option>
                            ))}
                        </select>
                        {fieldErrors.supplierId && <span className={styles.fieldError}>{fieldErrors.supplierId}</span>}
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発注元 *</span>
                        <select value={form.storeId} onChange={(e) => handleHeaderChange("storeId", e.target.value)} className={styles.select}>
                            <option value="">選択してください</option>
                            {stores.map((s) => (
                                <option key={s.storeId} value={s.storeId}>{s.storeName}</option>
                            ))}
                        </select>
                        {fieldErrors.storeId && <span className={styles.fieldError}>{fieldErrors.storeId}</span>}
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>発注日 *</span>
                        <input type="date" value={form.orderDate} min={today} onChange={(e) => handleHeaderChange("orderDate", e.target.value)} className={styles.input} />
                        {fieldErrors.orderDate && <span className={styles.fieldError}>{fieldErrors.orderDate}</span>}
                    </label>

                    <label className={styles.field}>
                        <span className={styles.fieldLabel}>希望到着日</span>
                        <input type="date" value={form.desiredDeliveryDate} min={today} onChange={(e) => handleHeaderChange("desiredDeliveryDate", e.target.value)} className={styles.input} />
                    </label>

                    <label className={`${styles.field} ${styles.fieldFull}`}>
                        <span className={styles.fieldLabel}>備考</span>
                        <textarea value={form.note} onChange={(e) => handleHeaderChange("note", e.target.value)} className={styles.textarea} placeholder="備考があれば入力" />
                        {fieldErrors.note && <span className={styles.fieldError}>{fieldErrors.note}</span>}
                    </label>
                </div>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>発注明細</h2>
                <div className={styles.detailScroll}>
                <table className={styles.detailTable}>
                    <thead>
                        <tr>
                            <th style={{ width: "40%" }}>商品</th>
                            <th style={{ width: "15%" }}>数量</th>
                            <th style={{ width: "18%" }}>仕入単価</th>
                            <th style={{ width: "17%" }}>小計</th>
                            <th style={{ width: "10%" }}></th>
                        </tr>
                    </thead>
                    <tbody>
                        {details.map((row) => {
                            const isDuplicate = row.productId ? duplicateProductIds.has(row.productId) : false;
                            return (
                            <tr key={row.key} className={isDuplicate ? styles.duplicateRow : undefined}>
                                <td>
                                    <select value={row.productId} onChange={(e) => handleDetailChange(row.key, "productId", e.target.value)}>
                                        <option value="">選択</option>
                                        {products.map((p) => (
                                            <option key={p.id} value={p.id}>{p.productCode} - {p.productName}</option>
                                        ))}
                                    </select>
                                    {isDuplicate && <div className={styles.fieldError}>この商品は重複しています</div>}
                                    {!isDuplicate && detailErrors[row.key]?.productId && <div className={styles.fieldError}>{detailErrors[row.key].productId}</div>}
                                </td>
                                <td>
                                    <input type="number" className={styles.numericInput} value={row.quantity} onChange={(e) => handleDetailChange(row.key, "quantity", e.target.value)} min="1" />
                                    {detailErrors[row.key]?.quantity && <div className={styles.fieldError}>{detailErrors[row.key].quantity}</div>}
                                </td>
                                <td>
                                    <input type="number" className={styles.numericInput} value={row.unitPrice} onChange={(e) => handleDetailChange(row.key, "unitPrice", e.target.value)} min="0" step="0.01" />
                                    {detailErrors[row.key]?.unitPrice && <div className={styles.fieldError}>{detailErrors[row.key].unitPrice}</div>}
                                </td>
                                <td style={{ textAlign: "right" }}>
                                    {getSubTotal(row).toLocaleString("ja-JP", { style: "currency", currency: "JPY" })}
                                </td>
                                <td>
                                    <button type="button" onClick={() => removeDetailRow(row.key)} className={styles.removeButton}>削除</button>
                                </td>
                            </tr>
                            );
                        })}
                    </tbody>
                </table>
                </div>

                <button type="button" onClick={addDetailRow} className={styles.addButton}>+ 明細行を追加</button>

                <div className={styles.totalRow}>
                    合計金額: {totalAmount.toLocaleString("ja-JP", { style: "currency", currency: "JPY" })}
                </div>
            </div>

            <div className={styles.actions}>
                <Link href="/purchase-orders" className={styles.cancelButton}>キャンセル</Link>
                <button type="button" onClick={handleSubmit} disabled={submitting} className={styles.submitButton}>
                    {submitting ? "作成中..." : "下書き保存"}
                </button>
            </div>
        </main>
    );
}
