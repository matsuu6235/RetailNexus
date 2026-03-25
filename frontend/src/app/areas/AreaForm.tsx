"use client";

import { useEffect, useState } from "react";
import { createArea, getAreaById, updateArea, type CreateAreaRequest, type UpdateAreaRequest } from "@/lib/api/areas";
import { validateArea, type AreaFieldErrors } from "@/lib/validators/areaValidator";
import styles from "@/components/modal/FormModal.module.css";

type AreaFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

export default function AreaForm({ mode, editId, onSave, onCancel }: AreaFormProps) {
  const [form, setForm] = useState<CreateAreaRequest>({
    areaCd: "",
    areaName: "",
    isActive: true,
  });
  const [loading, setLoading] = useState(mode === "edit");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<AreaFieldErrors>({});

  useEffect(() => {
    if (mode !== "edit" || !editId) return;

    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const item = await getAreaById(editId);

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
  }, [mode, editId]);

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

      if (mode === "create") {
        await createArea({
          areaCd: form.areaCd.trim(),
          areaName: form.areaName.trim(),
          isActive: form.isActive,
        });
      } else {
        await updateArea(editId!, {
          areaCd: form.areaCd.trim(),
          areaName: form.areaName.trim(),
          isActive: form.isActive,
        });
      }

      onSave();
    } catch (err) {
      setError(err instanceof Error ? err.message : mode === "create" ? "エリアの作成に失敗しました。" : "エリアの更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <p>読み込み中...</p>;

  return (
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
        <button type="button" onClick={onCancel} className={styles.cancelButton}>
          キャンセル
        </button>
        <button type="submit" disabled={submitting} className={styles.submitButton}>
          {mode === "create" ? (submitting ? "作成中..." : "作成") : (submitting ? "更新中..." : "更新")}
        </button>
      </div>
    </form>
  );
}
