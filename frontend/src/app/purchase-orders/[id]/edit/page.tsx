"use client";

import { useEffect, useState } from "react";
import { validation, fallback } from "@/lib/messages";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { getPurchaseOrderById, updatePurchaseOrder } from "@/lib/api/purchaseOrders";
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
import { useDetailRows } from "@/lib/hooks/useDetailRows";
import newStyles from "../../new/page.module.css";

type EditDetailFormFields = DetailFormFields & { purchaseOrderDetailId?: string };
type DetailRow = EditDetailFormFields & { key: number };

const today = new Date().toISOString().split("T")[0];

export default function PurchaseOrderEditPage() {
    const params = useParams();
    const router = useRouter();
    const id = params.id as string;

    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [stores, setStores] = useState<Store[]>([]);
    const [products, setProducts] = useState<Product[]>([]);

    const [form, setForm] = useState<PurchaseOrderFormFields>({
        supplierId: "",
        storeId: "",
        orderDate: "",
        desiredDeliveryDate: "",
        note: "",
    });
    const [fieldErrors, setFieldErrors] = useState<PurchaseOrderFieldErrors>({});

    const { details, setDetails, detailErrors, setDetailErrors, duplicateProductIds, handleDetailChange, addDetailRow, removeDetailRow } = useDetailRows<EditDetailFormFields, DetailFieldErrors>({
        emptyRow: { productId: "", quantity: "", unitPrice: "" },
        validateRow: validateDetail,
        onProductSelect: (productId) => {
            const product = products.find((p) => p.id === productId);
            return product ? { unitPrice: String(product.cost) } : {};
        },
    });

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        (async () => {
            try {
                const [order, supplierRes, storeRes, productRes] = await Promise.all([
                    getPurchaseOrderById(id),
                    getSuppliers(1, 200, { isActive: "active" }),
                    getStores(1, 200, { isActive: "active" }),
                    getProducts(1, 200, { isActive: "active" }),
                ]);

                setSuppliers(supplierRes.items);
                setStores(storeRes.items);
                setProducts(productRes.items);

                setForm({
                    supplierId: order.supplierId,
                    storeId: order.storeId,
                    orderDate: order.orderDate.split("T")[0],
                    desiredDeliveryDate: order.desiredDeliveryDate ? order.desiredDeliveryDate.split("T")[0] : "",
                    note: order.note || "",
                });

                const loadedDetails: DetailRow[] = order.details.map((d, i) => ({
                    key: i + 1,
                    purchaseOrderDetailId: d.purchaseOrderDetailId,
                    productId: d.productId,
                    quantity: String(d.quantity),
                    unitPrice: String(d.unitPrice),
                }));
                setDetails(loadedDetails);
            } catch (e) {
                setError(e instanceof Error ? e.message : fallback.fetchFailed("データ"));
            } finally {
                setLoading(false);
            }
        })();
    }, [id]);

    const handleHeaderChange = (field: keyof PurchaseOrderFormFields, value: string) => {
        const updated = { ...form, [field]: value };
        setForm(updated);
        const errors = validatePurchaseOrderHeader(updated);
        setFieldErrors((prev) => ({ ...prev, [field]: errors[field] }));
    };

    const getSubTotal = (row: DetailRow): number => {
        const qty = Number(row.quantity);
        const price = Number(row.unitPrice);
        if (isNaN(qty) || isNaN(price)) return 0;
        return qty * price;
    };

    const totalAmount = details.reduce((sum, d) => sum + getSubTotal(d), 0);

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
            setError(validation.listMinCount("明細"));
            return;
        }

        if (Object.keys(headerErrors).length > 0 || hasDetailError) return;

        try {
            setSubmitting(true);
            setError(null);

            await updatePurchaseOrder(id, {
                supplierId: form.supplierId,
                storeId: form.storeId,
                orderDate: new Date(form.orderDate).toISOString(),
                desiredDeliveryDate: form.desiredDeliveryDate ? new Date(form.desiredDeliveryDate).toISOString() : null,
                note: form.note.trim() || undefined,
                details: details.map((d) => ({
                    purchaseOrderDetailId: d.purchaseOrderDetailId || null,
                    productId: d.productId,
                    quantity: Number(d.quantity),
                    unitPrice: Number(d.unitPrice),
                })),
            });

            router.push(`/purchase-orders/${id}`);
        } catch (e) {
            setError(e instanceof Error ? e.message : fallback.updateFailed("発注"));
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) return <main className={newStyles.page}><p>読み込み中...</p></main>;

    return (
        <main className={newStyles.page}>
            <h1 className={newStyles.title}>発注編集</h1>

            {error && <div className={newStyles.errorBox}>{error}</div>}

            <div className={newStyles.section}>
                <h2 className={newStyles.sectionTitle}>発注ヘッダ</h2>
                <div className={newStyles.formGrid}>
                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>仕入先 *</span>
                        <select value={form.supplierId} onChange={(e) => handleHeaderChange("supplierId", e.target.value)} className={newStyles.select}>
                            <option value="">選択してください</option>
                            {suppliers.map((s) => (
                                <option key={s.supplierId} value={s.supplierId}>{s.supplierCode} - {s.supplierName}</option>
                            ))}
                        </select>
                        {fieldErrors.supplierId && <span className={newStyles.fieldError}>{fieldErrors.supplierId}</span>}
                    </label>

                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>発注元 *</span>
                        <select value={form.storeId} onChange={(e) => handleHeaderChange("storeId", e.target.value)} className={newStyles.select}>
                            <option value="">選択してください</option>
                            {stores.map((s) => (
                                <option key={s.storeId} value={s.storeId}>{s.storeName}</option>
                            ))}
                        </select>
                        {fieldErrors.storeId && <span className={newStyles.fieldError}>{fieldErrors.storeId}</span>}
                    </label>

                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>発注日 *</span>
                        <input type="date" value={form.orderDate} min={today} onChange={(e) => handleHeaderChange("orderDate", e.target.value)} className={newStyles.input} />
                        {fieldErrors.orderDate && <span className={newStyles.fieldError}>{fieldErrors.orderDate}</span>}
                    </label>

                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>希望到着日</span>
                        <input type="date" value={form.desiredDeliveryDate} min={today} onChange={(e) => handleHeaderChange("desiredDeliveryDate", e.target.value)} className={newStyles.input} />
                    </label>

                    <label className={`${newStyles.field} ${newStyles.fieldFull}`}>
                        <span className={newStyles.fieldLabel}>備考</span>
                        <textarea value={form.note} onChange={(e) => handleHeaderChange("note", e.target.value)} className={newStyles.textarea} placeholder="備考があれば入力" />
                        {fieldErrors.note && <span className={newStyles.fieldError}>{fieldErrors.note}</span>}
                    </label>
                </div>
            </div>

            <div className={newStyles.section}>
                <h2 className={newStyles.sectionTitle}>発注明細</h2>
                <div className={newStyles.detailScroll}>
                <table className={newStyles.detailTable}>
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
                            <tr key={row.key} className={isDuplicate ? newStyles.duplicateRow : undefined}>
                                <td>
                                    <select value={row.productId} onChange={(e) => handleDetailChange(row.key, "productId", e.target.value)}>
                                        <option value="">選択</option>
                                        {products.map((p) => (
                                            <option key={p.id} value={p.id}>{p.productCode} - {p.productName}</option>
                                        ))}
                                    </select>
                                    {isDuplicate && <div className={newStyles.fieldError}>この商品は重複しています</div>}
                                    {!isDuplicate && detailErrors[row.key]?.productId && <div className={newStyles.fieldError}>{detailErrors[row.key].productId}</div>}
                                </td>
                                <td>
                                    <input type="number" value={row.quantity} onChange={(e) => handleDetailChange(row.key, "quantity", e.target.value)} min="1" />
                                    {detailErrors[row.key]?.quantity && <div className={newStyles.fieldError}>{detailErrors[row.key].quantity}</div>}
                                </td>
                                <td>
                                    <input type="number" value={row.unitPrice} onChange={(e) => handleDetailChange(row.key, "unitPrice", e.target.value)} min="0" step="0.01" />
                                    {detailErrors[row.key]?.unitPrice && <div className={newStyles.fieldError}>{detailErrors[row.key].unitPrice}</div>}
                                </td>
                                <td style={{ textAlign: "right" }}>
                                    {getSubTotal(row).toLocaleString("ja-JP", { style: "currency", currency: "JPY" })}
                                </td>
                                <td>
                                    <button type="button" onClick={() => removeDetailRow(row.key)} className={newStyles.removeButton}>削除</button>
                                </td>
                            </tr>
                            );
                        })}
                    </tbody>
                </table>
                </div>

                <button type="button" onClick={addDetailRow} className={newStyles.addButton}>+ 明細行を追加</button>

                <div className={newStyles.totalRow}>
                    合計金額: {totalAmount.toLocaleString("ja-JP", { style: "currency", currency: "JPY" })}
                </div>
            </div>

            <div className={newStyles.actions}>
                <Link href={`/purchase-orders/${id}`} className={newStyles.cancelButton}>キャンセル</Link>
                <button type="button" onClick={handleSubmit} disabled={submitting} className={newStyles.submitButton}>
                    {submitting ? "保存中..." : "保存"}
                </button>
            </div>
        </main>
    );
}
