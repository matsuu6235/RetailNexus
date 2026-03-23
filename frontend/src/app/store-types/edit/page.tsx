"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getStoreTypeById, updateStoreType, type UpdateStoreTypeRequest } from "@/lib/api/storeTypes";
import { validateStoreType, type StoreTypeFieldErrors } from "@/lib/validators/storeTypeValidator";
import styles from "./page.module.css";

export default function EditStoreTypePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = searchParams.get("id") ?? "";

  const [form, setForm] = useState<UpdateStoreTypeRequest>({
    storeTypeCd: "",
    storeTypeName: "",
    isActive: true,
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<StoreTypeFieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    if (!id) {
      setError("店舗種別IDが指定されていません。");
      setLoading(false);
      return;
    }

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const item = await getStoreTypeById(id);

        if (!cancelled) {
          setForm({
            storeTypeCd: item.storeTypeCd,
            storeTypeName: item.storeTypeName,
            isActive: item.isActive,
          });
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "店舗種別情報の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleChange = (field: keyof UpdateStoreTypeRequest, value: string | boolean) => {
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
    if (!id) {
      setError("店舗種別IDが指定されていません。");
      return;
    }

    try {
      setSubmitting(true);
      await updateStoreType(id, {
        storeTypeCd: form.storeTypeCd.trim(),
        storeTypeName: form.storeTypeName.trim(),
        isActive: form.isActive,
      });
      router.push("/store-types");
    } catch (err) {
      setError(err instanceof Error ? err.message : "店舗種別の更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <main className={styles.page}>読み込み中...</main>;

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>店舗種別編集</h1>
      <p className={styles.description}>店舗種別情報を更新します。</p>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>店舗種別コード *</span>
          <input value={form.storeTypeCd} onChange={(e) => handleChange("storeTypeCd", e.target.value)} className={styles.input} />
          <small className={styles.hint}>2文字以内で入力してください。</small>
          {fieldErrors.storeTypeCd && <small className={styles.errorText}>{fieldErrors.storeTypeCd}</small>}
        </label>

        <label className={styles.field}>
          <span>店舗種別名 *</span>
          <input value={form.storeTypeName} onChange={(e) => handleChange("storeTypeName", e.target.value)} className={styles.input} />
          <small className={styles.hint}>50文字以内で入力してください。</small>
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
            {submitting ? "更新中..." : "更新"}
          </button>
        </div>
      </form>
    </main>
  );
}
