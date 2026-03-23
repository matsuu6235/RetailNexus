"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import {
    getSupplierById,
    updateSupplier,
    type UpdateSupplierRequest,
} from "@/lib/api/suppliers";
import { validateSupplier, type SupplierFieldErrors } from "@/lib/validators/supplierValidator";
import styles from "./page.module.css";

export default function EditSupplierPage() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const supplierId = searchParams.get("id") ?? "";

    const [form, setForm] = useState<UpdateSupplierRequest>({
        supplierCode: "",
        supplierName: "",
        phoneNumber: "",
        email: "",
        isActive: true,
    });

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<SupplierFieldErrors>({});

    useEffect(() => {
        let cancelled = false;

        if (!supplierId) {
            setError("仕入先IDが指定されていません。");
            setLoading(false);
            return () => {
                cancelled = true;
            };
        }

        (async () => {
            try {
                setLoading(true);
                setError(null);

                const supplier = await getSupplierById(supplierId);

                if (!cancelled) {
                    setForm({
                        supplierCode: supplier.supplierCode,
                        supplierName: supplier.supplierName,
                        phoneNumber: supplier.phoneNumber ?? "",
                        email: supplier.email ?? "",
                        isActive: supplier.isActive,
                    });
                }
            } catch (e) {
                if (!cancelled) {
                    setError(e instanceof Error ? e.message : "仕入先情報の取得に失敗しました。");
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
    }, [supplierId]);

    const handleChange = (field: keyof UpdateSupplierRequest, value: string | boolean) => {
        const updatedForm = { ...form, [field]: value };
        setForm(updatedForm);
        const errors = validateSupplier(updatedForm);
        setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof SupplierFieldErrors] }));
    };

    const validate = () => {
        const errors = validateSupplier(form);
        setFieldErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!validate()) return;
        if (!supplierId) {
            setError("仕入先IDが指定されていません。");
            return;
        }

        try {
            setSubmitting(true);
            await updateSupplier(supplierId, {
                supplierCode: form.supplierCode.trim(),
                supplierName: form.supplierName.trim(),
                phoneNumber: form.phoneNumber?.trim() || "",
                email: form.email?.trim() || "",
                isActive: form.isActive,
            });
            router.push("/suppliers");
        } catch (err) {
            setError(err instanceof Error ? err.message : "仕入先更新に失敗しました。");
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return <main className={styles.page}>読み込み中...</main>;
    }

    return (
        <main className={styles.page}>
            <h1 className={styles.title}>仕入先編集</h1>
            <p className={styles.description}>仕入先情報を更新します。</p>

            <form onSubmit={onSubmit} className={styles.form}>
                <label className={styles.field}>
                    <span>仕入先コード *</span>
                    <input
                        value={form.supplierCode}
                        onChange={(e) => handleChange("supplierCode", e.target.value)}
                        className={styles.input}
                    />
                    <small className={styles.hint}>30文字以内で入力してください。</small>
                    {fieldErrors.supplierCode && <small className={styles.errorText}>{fieldErrors.supplierCode}</small>}
                </label>

                <label className={styles.field}>
                    <span>仕入先名 *</span>
                    <input
                        value={form.supplierName}
                        onChange={(e) => handleChange("supplierName", e.target.value)}
                        className={styles.input}
                    />
                    <small className={styles.hint}>100文字以内で入力してください。</small>
                    {fieldErrors.supplierName && <small className={styles.errorText}>{fieldErrors.supplierName}</small>}
                </label>

                <label className={styles.field}>
                    <span>電話番号</span>
                    <input
                        value={form.phoneNumber}
                        onChange={(e) => handleChange("phoneNumber", e.target.value)}
                        className={styles.input}
                    />
                    <small className={styles.hint}>20文字以内で入力してください。</small>
                    {fieldErrors.phoneNumber && <small className={styles.errorText}>{fieldErrors.phoneNumber}</small>}
                </label>

                <label className={styles.field}>
                    <span>メールアドレス</span>
                    <input
                        type="email"
                        value={form.email}
                        onChange={(e) => handleChange("email", e.target.value)}
                        className={styles.input}
                    />
                    <small className={styles.hint}>255文字以内で入力してください。</small>
                    {fieldErrors.email && <small className={styles.errorText}>{fieldErrors.email}</small>}
                </label>

                <label className={styles.checkboxField}>
                    <input
                        type="checkbox"
                        checked={form.isActive}
                        onChange={(e) => handleChange("isActive", e.target.checked)}
                    />
                    <span>有効</span>
                </label>

                {error && <div className={styles.errorBox}>{error}</div>}

                <div className={styles.actions}>
                    <button type="button" onClick={() => router.push("/suppliers")} className={styles.cancelButton}>
                        キャンセル
                    </button>
                    <button type="submit" disabled={submitting} className={styles.submitButton}>
                        {submitting ? "更新中..." : "更新"}
                    </button>
                </div>
            </form>
        </main>
    );
}
