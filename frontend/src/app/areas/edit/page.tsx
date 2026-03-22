"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getAreaById, updateArea, type UpdateAreaRequest } from "../../lib/api/areas";
import { validateArea, type AreaFieldErrors } from "../../lib/validators/areaValidator";
import styles from "./page.module.css";

export default function EditAreaPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = searchParams.get("id") ?? "";

  const [form, setForm] = useState<UpdateAreaRequest>({
    areaCd: "",
    areaName: "",
    isActive: true,
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<AreaFieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    if (!id) {
      setError("エリアIDが指定されていません。");
      setLoading(false);
      return;
    }

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const item = await getAreaById(id);

        if (!cancelled) {
          setForm({
            areaCd: item.areaCd,
            areaName: item.areaName,
            isActive: item.isActive,
          });
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "エリア情報の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleChange = (field: keyof UpdateAreaRequest, value: string | boolean) => {
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
    if (!id) {
      setError("エリアIDが指定されていません。");
      return;
    }

    try {
      setSubmitting(true);
      await updateArea(id, {
        areaCd: form.areaCd.trim(),
        areaName: form.areaName.trim(),
        isActive: form.isActive,
      });
      router.push("/areas");
    } catch (err) {
      setError(err instanceof Error ? err.message : "エリアの更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <main className={styles.page}>読み込み中...</main>;

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>エリア編集</h1>
      <p className={styles.description}>エリア情報を更新します。</p>

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
            {submitting ? "更新中..." : "更新"}
          </button>
        </div>
      </form>
    </main>
  );
}
