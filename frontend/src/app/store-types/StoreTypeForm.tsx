"use client";

import { useState, useEffect } from "react";
import {
  createStoreType,
  getStoreTypeById,
  updateStoreType,
  type CreateStoreTypeRequest,
  type UpdateStoreTypeRequest,
} from "@/lib/api/storeTypes";
import { validateStoreType, type StoreTypeFieldErrors } from "@/lib/validators/storeTypeValidator";
import styles from "@/components/modal/FormModal.module.css";

type StoreTypeFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

export default function StoreTypeForm({ mode, editId, onSave, onCancel }: StoreTypeFormProps) {
  const [form, setForm] = useState<CreateStoreTypeRequest>({
    storeTypeCd: "",
    storeTypeName: "",
    isActive: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<StoreTypeFieldErrors>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (mode !== "edit" || !editId) return;

    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        const data = await getStoreTypeById(editId);
        if (!cancelled) {
          setForm({
            storeTypeCd: data.storeTypeCd,
            storeTypeName: data.storeTypeName,
            isActive: data.isActive,
          });
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "データの取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

  const handleChange = (field: keyof CreateStoreTypeRequest, value: string | boolean) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validateStoreType(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof StoreTypeFieldErrors] }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const errors = validateStoreType(form);
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    try {
      setSubmitting(true);

      const payload = {
        storeTypeCd: form.storeTypeCd.trim(),
        storeTypeName: form.storeTypeName.trim(),
        isActive: form.isActive,
      };

      if (mode === "edit") {
        await updateStoreType(editId!, payload as UpdateStoreTypeRequest);
      } else {
        await createStoreType(payload);
      }

      onSave();
    } catch (e) {
      setError(e instanceof Error ? e.message : "保存に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <p>読み込み中...</p>;
  }

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <div className={styles.field}>
        <label>
          店舗種別コード <span>*</span>
        </label>
        <input
          type="text"
          value={form.storeTypeCd}
          onChange={(e) => handleChange("storeTypeCd", e.target.value)}
          disabled={mode === "edit"}
          className={mode === "edit" ? styles.readOnlyInput : styles.input}
          readOnly={mode === "edit"}
        />
        {fieldErrors.storeTypeCd && <span className={styles.errorText}>{fieldErrors.storeTypeCd}</span>}
        {mode === "create" && <span className={styles.hint}>2文字以内で入力してください。</span>}
      </div>

      <div className={styles.field}>
        <label>
          店舗種別名 <span>*</span>
        </label>
        <input
          type="text"
          value={form.storeTypeName}
          onChange={(e) => handleChange("storeTypeName", e.target.value)}
          className={styles.input}
        />
        {fieldErrors.storeTypeName && <span className={styles.errorText}>{fieldErrors.storeTypeName}</span>}
        <span className={styles.hint}>20文字以内で入力してください。</span>
      </div>

      <div className={styles.checkboxField}>
        <input
          type="checkbox"
          id="isActive"
          checked={form.isActive}
          onChange={(e) => handleChange("isActive", e.target.checked)}
        />
        <label htmlFor="isActive">有効</label>
      </div>

      {error && <div className={styles.errorBox}>{error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton} disabled={submitting}>
          キャンセル
        </button>
        <button type="submit" className={styles.submitButton} disabled={submitting}>
          {submitting ? "保存中..." : "保存"}
        </button>
      </div>
    </form>
  );
}
