"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createArea, type CreateAreaRequest } from "../../lib/api/areas";
import styles from "./page.module.css";

type FieldErrors = Partial<Record<keyof CreateAreaRequest, string>>;

export default function NewAreaPage() {
  const router = useRouter();
  const [form, setForm] = useState<CreateAreaRequest>({
    areaCd: "",
    areaName: "",
    isActive: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  const handleChange = (field: keyof CreateAreaRequest, value: string | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    setFieldErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validate = () => {
    const errors: FieldErrors = {};
    if (!form.areaCd.trim()) errors.areaCd = "エリアコードは必須です";
    if (!form.areaName.trim()) errors.areaName = "エリア名は必須です";
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;

    try {
      setSubmitting(true);
      await createArea({
        areaCd: form.areaCd.trim(),
        areaName: form.areaName.trim(),
        isActive: form.isActive,
      });
      router.push("/areas");
    } catch (err) {
      setError(err instanceof Error ? err.message : "エリアの作成に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>エリア新規作成</h1>
      <p className={styles.description}>エリアコードとエリア名を入力してください。</p>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>エリアコード *</span>
          <input value={form.areaCd} onChange={(e) => handleChange("areaCd", e.target.value)} className={styles.input} />
          {fieldErrors.areaCd && <small className={styles.errorText}>{fieldErrors.areaCd}</small>}
        </label>

        <label className={styles.field}>
          <span>エリア名 *</span>
          <input value={form.areaName} onChange={(e) => handleChange("areaName", e.target.value)} className={styles.input} />
          {fieldErrors.areaName && <small className={styles.errorText}>{fieldErrors.areaName}</small>}
        </label>

        <label className={styles.checkboxField}>
          <input type="checkbox" checked={form.isActive} onChange={(e) => handleChange("isActive", e.target.checked)} />
          <span>有効</span>
        </label>

        {error && <div className={styles.errorBox}>{error}</div>}

        <div className={styles.actions}>
          <button type="button" onClick={() => router.push("/areas")} className={styles.cancelButton}>
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