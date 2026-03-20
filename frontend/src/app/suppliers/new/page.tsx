"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createSupplier, type CreateSupplierRequest } from "../../lib/api/suppliers";
import { formatPhoneNumber, normalizePhoneNumber } from "../../lib/utils/phoneNumber";
import styles from "./page.module.css";

type FieldErrors = Partial<Record<keyof CreateSupplierRequest, string>>;

export default function NewSupplierPage() {
    const router = useRouter();
    const [form, setForm] = useState<CreateSupplierRequest>({
        supplierCode: "",
        supplierName: "",
        phoneNumber: "",
        email: "",
        isActive: true,
    });
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

    const handleChange = (field: keyof CreateSupplierRequest, value: string | boolean) => {
        setForm((prev) => ({ ...prev, [field]: value }));
        setFieldErrors((prev) => ({ ...prev, [field]: undefined }));
    };

    const validate = () => {
        const errors: FieldErrors = {};
        if (!form.supplierCode.trim()) errors.supplierCode = "仕入先コードは必須です";
        if (!form.supplierName.trim()) errors.supplierName = "仕入先名は必須です";
        setFieldErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!validate()) return;

        try {
            setSubmitting(true);
            await createSupplier({
                supplierCode: form.supplierCode.trim(),
                supplierName: form.supplierName.trim(),
                phoneNumber: normalizePhoneNumber(form.phoneNumber ?? ""),
                email: form.email?.trim() ?? "",
                isActive: form.isActive,
            });
            router.push("/suppliers");
        } catch (err) {
            setError(err instanceof Error ? err.message : "仕入先作成に失敗しました。");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <main className={styles.page}>
            <h1 className={styles.title}>仕入先新規作成</h1>
            <p className={styles.description}>仕入先コードと仕入先名は必須です。</p>

            <form onSubmit={onSubmit} className={styles.form}>
                <label className={styles.field}>
                    <span>仕入先コード *</span>
                    <input
                        value={form.supplierCode}
                        onChange={(e) => handleChange("supplierCode", e.target.value)}
                        className={styles.input}
                    />
                    {fieldErrors.supplierCode && <small className={styles.errorText}>{fieldErrors.supplierCode}</small>}
                </label>

                <label className={styles.field}>
                    <span>仕入先名 *</span>
                    <input
                        value={form.supplierName}
                        onChange={(e) => handleChange("supplierName", e.target.value)}
                        className={styles.input}
                    />
                    {fieldErrors.supplierName && <small className={styles.errorText}>{fieldErrors.supplierName}</small>}
                </label>

                <label className={styles.field}>
                    <span>電話番号</span>
                    <input
                        value={formatPhoneNumber(form.phoneNumber)}
                        onChange={(e) => handleChange("phoneNumber", normalizePhoneNumber(e.target.value))}
                        className={styles.input}
                    />
                </label>

                <label className={styles.field}>
                    <span>メールアドレス</span>
                    <input
                        type="email"
                        value={form.email}
                        onChange={(e) => handleChange("email", e.target.value)}
                        className={styles.input}
                    />
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
                        {submitting ? "作成中..." : "作成"}
                    </button>
                </div>
            </form>
        </main>
    );
}