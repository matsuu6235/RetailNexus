"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createSupplier, type CreateSupplierRequest } from "@/lib/api/suppliers";
import { validateSupplier, type SupplierFieldErrors } from "@/lib/validators/supplierValidator";
import styles from "./page.module.css";

export default function NewSupplierPage() {
    const router = useRouter();
    const [form, setForm] = useState<CreateSupplierRequest>({
        supplierName: "",
        phoneNumber: "",
        email: "",
        isActive: true,
    });
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<SupplierFieldErrors>({});

    const handleChange = (field: keyof CreateSupplierRequest, value: string | boolean) => {
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

        try {
            setSubmitting(true);
            await createSupplier({
                supplierName: form.supplierName.trim(),
                phoneNumber: form.phoneNumber?.trim() ?? "",
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
                <div className={styles.field}>
                    <span>仕入先コード</span>
                    <small className={styles.hint}>登録時に自動採番されます。</small>
                </div>

                <label className={styles.field}>
                    <span>仕入先名 *</span>
                    <input
                        value={form.supplierName}
                        onChange={(e) => handleChange("supplierName", e.target.value)}
                        className={styles.input}
                    />
                    <small className={styles.hint}>50文字以内で入力してください。</small>
                    {fieldErrors.supplierName && <small className={styles.errorText}>{fieldErrors.supplierName}</small>}
                </label>

                <label className={styles.field}>
                    <span>電話番号</span>
                    <input
                        value={form.phoneNumber}
                        onChange={(e) => handleChange("phoneNumber", e.target.value)}
                        className={styles.input}
                    />
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
                        {submitting ? "作成中..." : "作成"}
                    </button>
                </div>
            </form>
        </main>
    );
}
