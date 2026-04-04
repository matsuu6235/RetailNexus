"use client";

import { useState, useEffect } from "react";
import {
  createStoreType,
  getStoreTypeById,
  updateStoreType,
  changeStoreTypeActivation,
  type CreateStoreTypeRequest,
  type UpdateStoreTypeRequest,
} from "@/lib/api/storeTypes";
import { validateStoreType, type StoreTypeFieldErrors } from "@/lib/validators/storeTypeValidator";
import { useActivation } from "@/lib/hooks/useActivation";
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
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<StoreTypeFieldErrors>({});
  const [loading, setLoading] = useState(false);
  const [fetchedIsActive, setFetchedIsActive] = useState(true);
  const activation = useActivation({ permissionCode: "store-types.delete", initialIsActive: fetchedIsActive, changeFn: changeStoreTypeActivation, editId });

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
          });
          setFetchedIsActive(data.isActive);
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

      {(error || activation.error) && <div className={styles.errorBox}>{error || activation.error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton} disabled={submitting}>
          キャンセル
        </button>
        <button type="submit" className={styles.submitButton} disabled={submitting}>
          {submitting ? "保存中..." : "保存"}
        </button>
      </div>

      {mode === "edit" && activation.canDelete && (
        <fieldset className={styles.field} style={{ border: "1px solid #e2e8f0", borderRadius: "8px", padding: "12px", marginTop: "8px" }}>
          <legend style={{ fontSize: "13px", fontWeight: 600, color: "#0f172a", padding: "0 4px" }}>有効状態の変更</legend>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
            <span style={{ fontSize: "13px" }}>
              現在の状態: <strong>{activation.currentIsActive ? "有効" : "無効"}</strong>
            </span>
            <button
              type="button"
              onClick={activation.toggle}
              disabled={activation.changingActivation}
              className={styles.submitButton}
              style={activation.currentIsActive ? { backgroundColor: "#dc2626" } : {}}
            >
              {activation.changingActivation ? "変更中..." : activation.currentIsActive ? "無効化する" : "有効化する"}
            </button>
          </div>
        </fieldset>
      )}
    </form>
  );
}
