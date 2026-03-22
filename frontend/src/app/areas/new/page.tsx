"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createArea, type CreateAreaRequest } from "../../lib/api/areas";
import { validateArea, type AreaFieldErrors } from "../../lib/validators/areaValidator";
import styles from "./page.module.css";

export default function NewAreaPage() {
  const router = useRouter();
  const [form, setForm] = useState<CreateAreaRequest>({
    areaCd: "",
    areaName: "",
    isActive: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<AreaFieldErrors>({});

  const handleChange = (field: keyof CreateAreaRequest, value: string | boolean) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validateArea(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof AreaFieldErrors] }));
  };

  const validate = () => {
    const errors = validateArea(form);
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
          <small className={styles.hint}>2文字以内で入力してください。</small>
          {fieldErrors.areaCd && <small className={styles.errorText}>{fieldErrors.areaCd}</small>}
        </label>

        <label className={styles.field}>
          <span>エリア名 *</span>
          <input value={form.areaName} onChange={(e) => handleChange("areaName", e.target.value)} className={styles.input} />
          <small className={styles.hint}>20文字以内で入力してください。</small>
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
