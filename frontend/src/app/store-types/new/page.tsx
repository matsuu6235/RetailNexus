"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createStoreType, type CreateStoreTypeRequest } from "@/lib/api/storeTypes";
import { validateStoreType, type StoreTypeFieldErrors } from "@/lib/validators/storeTypeValidator";
import styles from "./page.module.css";

export default function NewStoreTypePage() {
  const router = useRouter();
  const [form, setForm] = useState<CreateStoreTypeRequest>({
    storeTypeCd: "",
    storeTypeName: "",
    isActive: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<StoreTypeFieldErrors>({});

  const handleChange = (field: keyof CreateStoreTypeRequest, value: string | boolean) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validateStoreType(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof StoreTypeFieldErrors] }));
  };

  const validate = () => {
    const errors = validateStoreType(form);
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;

    try {
      setSubmitting(true);
      await createStoreType({
        storeTypeCd: form.storeTypeCd.trim(),
        storeTypeName: form.storeTypeName.trim(),
        isActive: form.isActive,
      });
      router.push("/store-types");
    } catch (err) {
      setError(err instanceof Error ? err.message : "店舗種別の作成に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>店舗種別新規作成</h1>
      <p className={styles.description}>店舗種別コードと店舗種別名を入力してください。</p>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>店舗種別コード *</span>
          <input
            value={form.storeTypeCd}
            onChange={(e) => handleChange("storeTypeCd", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>2文字以内で入力してください。</small>
          {fieldErrors.storeTypeCd && <small className={styles.errorText}>{fieldErrors.storeTypeCd}</small>}
        </label>

        <label className={styles.field}>
          <span>店舗種別名 *</span>
          <input value={form.storeTypeName} onChange={(e) => handleChange("storeTypeName", e.target.value)} className={styles.input} />
          <small className={styles.hint}>20文字以内で入力してください。</small>
          {fieldErrors.storeTypeName && <small className={styles.errorText}>{fieldErrors.storeTypeName}</small>}
        </label>

        <label className={styles.checkboxField}>
          <input type="checkbox" checked={form.isActive} onChange={(e) => handleChange("isActive", e.target.checked)} />
          <span>有効</span>
        </label>

        {error && <div className={styles.errorBox}>{error}</div>}

        <div className={styles.actions}>
          <button type="button" onClick={() => router.push("/store-types")} className={styles.cancelButton}>
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
