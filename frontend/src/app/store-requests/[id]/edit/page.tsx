"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { getStoreRequestById, updateStoreRequest } from "@/lib/api/storeRequests";
import { getStores } from "@/lib/api/stores";
import { getProducts } from "@/lib/api/products";
import type { Store } from "@/types/stores";
import type { Product } from "@/types/products";
import {
    validateStoreRequestHeader,
    validateStoreRequestDetail,
    type StoreRequestFormFields,
    type StoreRequestFieldErrors,
    type DetailFormFields,
    type DetailFieldErrors,
} from "@/lib/validators/storeRequestValidator";
import newStyles from "../../../purchase-orders/new/page.module.css";

type DetailRow = DetailFormFields & { key: number; storeRequestDetailId?: string };

const today = new Date().toISOString().split("T")[0];

export default function StoreRequestEditPage() {
    const params = useParams();
    const router = useRouter();
    const id = params.id as string;

    const [stores, setStores] = useState<Store[]>([]);
    const [products, setProducts] = useState<Product[]>([]);

    const [form, setForm] = useState<StoreRequestFormFields>({
        fromStoreId: "",
        toStoreId: "",
        requestDate: "",
        desiredDeliveryDate: "",
        note: "",
    });
    const [fieldErrors, setFieldErrors] = useState<StoreRequestFieldErrors>({});

    const [details, setDetails] = useState<DetailRow[]>([]);
    const [detailErrors, setDetailErrors] = useState<Record<number, DetailFieldErrors>>({});
    const [nextKey, setNextKey] = useState(1);

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        (async () => {
            try {
                const [request, storeRes, productRes] = await Promise.all([
                    getStoreRequestById(id),
                    getStores(1, 200, { isActive: "active" }),
                    getProducts(1, 200, { isActive: "active" }),
                ]);

                setStores(storeRes.items);
                setProducts(productRes.items);

                setForm({
                    fromStoreId: request.fromStoreId,
                    toStoreId: request.toStoreId,
                    requestDate: request.requestDate.split("T")[0],
                    desiredDeliveryDate: request.desiredDeliveryDate ? request.desiredDeliveryDate.split("T")[0] : "",
                    note: request.note || "",
                });

                const loadedDetails: DetailRow[] = request.details.map((d, i) => ({
                    key: i + 1,
                    storeRequestDetailId: d.storeRequestDetailId,
                    productId: d.productId,
                    quantity: String(d.quantity),
                }));
                setDetails(loadedDetails);
                setNextKey(loadedDetails.length + 1);
            } catch (e) {
                setError(e instanceof Error ? e.message : "データの取得に失敗しました。");
            } finally {
                setLoading(false);
            }
        })();
    }, [id]);

    const handleHeaderChange = (field: keyof StoreRequestFormFields, value: string) => {
        const updated = { ...form, [field]: value };
        setForm(updated);
        const errors = validateStoreRequestHeader(updated);
        setFieldErrors((prev) => ({ ...prev, [field]: errors[field] }));
    };

    const handleDetailChange = (key: number, field: keyof DetailFormFields, value: string) => {
        const updates: Partial<DetailFormFields> = { [field]: value };

        if (field === "productId" && value) {
            const isDuplicate = details.some((d) => d.key !== key && d.productId === value);
            if (isDuplicate) {
                setDetailErrors((prev) => ({
                    ...prev,
                    [key]: { ...prev[key], productId: "この商品は既に追加されています。数量を変更してください。" },
                }));
                return;
            }
        }

        setDetails((prev) => prev.map((d) => (d.key === key ? { ...d, ...updates } : d)));
        const row = details.find((d) => d.key === key);
        if (row) {
            const updated = { ...row, ...updates };
            const errors = validateStoreRequestDetail(updated);
            setDetailErrors((prev) => ({ ...prev, [key]: { ...prev[key], [field]: errors[field] } }));
        }
    };

    const addDetailRow = () => {
        setDetails((prev) => [...prev, { key: nextKey, productId: "", quantity: "" }]);
        setNextKey((k) => k + 1);
    };

    const removeDetailRow = (key: number) => {
        setDetails((prev) => prev.filter((d) => d.key !== key));
        setDetailErrors((prev) => { const copy = { ...prev }; delete copy[key]; return copy; });
    };

    // 重複商品ID検出
    const duplicateProductIds = new Set<string>();
    const seenProductIds = new Map<string, number>();
    for (const d of details) {
        if (!d.productId) continue;
        if (seenProductIds.has(d.productId)) duplicateProductIds.add(d.productId);
        seenProductIds.set(d.productId, d.key);
    }

    const handleSubmit = async () => {
        const headerErrors = validateStoreRequestHeader(form);
        setFieldErrors(headerErrors);

        const allDetailErrors: Record<number, DetailFieldErrors> = {};
        let hasDetailError = false;
        for (const d of details) {
            const errors = validateStoreRequestDetail(d);
            allDetailErrors[d.key] = errors;
            if (Object.keys(errors).length > 0) hasDetailError = true;
        }
        setDetailErrors(allDetailErrors);

        if (details.length === 0) { setError("明細を1行以上入力してください。"); return; }
        if (Object.keys(headerErrors).length > 0 || hasDetailError) return;

        try {
            setSubmitting(true);
            setError(null);

            await updateStoreRequest(id, {
                fromStoreId: form.fromStoreId,
                toStoreId: form.toStoreId,
                requestDate: new Date(form.requestDate).toISOString(),
                desiredDeliveryDate: form.desiredDeliveryDate ? new Date(form.desiredDeliveryDate).toISOString() : null,
                note: form.note.trim() || undefined,
                details: details.map((d) => ({
                    storeRequestDetailId: d.storeRequestDetailId || null,
                    productId: d.productId,
                    quantity: Number(d.quantity),
                })),
            });

            router.push(`/store-requests/${id}`);
        } catch (e) {
            setError(e instanceof Error ? e.message : "発送依頼の更新に失敗しました。");
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) return <main className={newStyles.page}><p>読み込み中...</p></main>;

    return (
        <main className={newStyles.page}>
            <h1 className={newStyles.title}>発送依頼編集</h1>

            {error && <div className={newStyles.errorBox}>{error}</div>}

            <div className={newStyles.section}>
                <h2 className={newStyles.sectionTitle}>依頼ヘッダ</h2>
                <div className={newStyles.formGrid}>
                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>依頼元 *</span>
                        <select value={form.fromStoreId} onChange={(e) => handleHeaderChange("fromStoreId", e.target.value)} className={newStyles.select}>
                            <option value="">選択してください</option>
                            {stores.map((s) => <option key={s.storeId} value={s.storeId}>{s.storeName}</option>)}
                        </select>
                        {fieldErrors.fromStoreId && <span className={newStyles.fieldError}>{fieldErrors.fromStoreId}</span>}
                    </label>
                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>依頼先 *</span>
                        <select value={form.toStoreId} onChange={(e) => handleHeaderChange("toStoreId", e.target.value)} className={newStyles.select}>
                            <option value="">選択してください</option>
                            {stores.map((s) => <option key={s.storeId} value={s.storeId}>{s.storeName}</option>)}
                        </select>
                        {fieldErrors.toStoreId && <span className={newStyles.fieldError}>{fieldErrors.toStoreId}</span>}
                    </label>
                    <label className={newStyles.field}>
                        <span className={newStyles.fieldLabel}>依頼日 *</span>
                        <input type="date" value={form.requestDate} min={today} onChange={(e) => handleHeaderChange("requestDate", e.target.value)} className={newStyles.input} />
                        {fieldErrors.requestDate && <span className={newStyles.fieldError}>{fieldErrors.requestDate}</span>}
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
                <h2 className={newStyles.sectionTitle}>依頼明細</h2>
                <div className={newStyles.detailScroll}>
                <table className={newStyles.detailTable}>
                    <thead>
                        <tr>
                            <th style={{ width: "55%" }}>商品</th>
                            <th style={{ width: "25%" }}>数量</th>
                            <th style={{ width: "20%" }}></th>
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
                                        {products.map((p) => <option key={p.id} value={p.id}>{p.productCode} - {p.productName}</option>)}
                                    </select>
                                    {isDuplicate && <div className={newStyles.fieldError}>この商品は重複しています</div>}
                                    {!isDuplicate && detailErrors[row.key]?.productId && <div className={newStyles.fieldError}>{detailErrors[row.key].productId}</div>}
                                </td>
                                <td>
                                    <input type="number" value={row.quantity} onChange={(e) => handleDetailChange(row.key, "quantity", e.target.value)} min="1" />
                                    {detailErrors[row.key]?.quantity && <div className={newStyles.fieldError}>{detailErrors[row.key].quantity}</div>}
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
            </div>

            <div className={newStyles.actions}>
                <Link href={`/store-requests/${id}`} className={newStyles.cancelButton}>キャンセル</Link>
                <button type="button" onClick={handleSubmit} disabled={submitting} className={newStyles.submitButton}>
                    {submitting ? "保存中..." : "保存"}
                </button>
            </div>
        </main>
    );
}
