"use client";

import { useEffect, useState } from "react";
import {
  createSupplier,
  getSupplierById,
  updateSupplier,
  type CreateSupplierRequest,
  type UpdateSupplierRequest,
} from "@/lib/api/suppliers";
import { validateSupplier, type SupplierFieldErrors } from "@/lib/validators/supplierValidator";
import styles from "@/components/modal/FormModal.module.css";

type SupplierFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

export default function SupplierForm({ mode, editId, onSave, onCancel }: SupplierFormProps) {
  const [form, setForm] = useState<CreateSupplierRequest>({
    supplierName: "",
    phoneNumber: "",
    email: "",
    isActive: true,
  });
  const [supplierCode, setSupplierCode] = useState("");
  const [loading, setLoading] = useState(mode === "edit");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<SupplierFieldErrors>({});

  useEffect(() => {
    if (mode !== "edit" || !editId) return;

    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const supplier = await getSupplierById(editId);

        if (!cancelled) {
          setSupplierCode(supplier.supplierCode);
          setForm({
            supplierName: supplier.supplierName,
            phoneNumber: supplier.phoneNumber ?? "",
            email: supplier.email ?? "",
            isActive: supplier.isActive,
          });
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "仕入先情報の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

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

      if (mode === "create") {
        await createSupplier({
          supplierName: form.supplierName.trim(),
          phoneNumber: form.phoneNumber?.trim() ?? "",
          email: form.email?.trim() ?? "",
          isActive: form.isActive,
        });
      } else {
        await updateSupplier(editId!, {
          supplierName: form.supplierName.trim(),
          phoneNumber: form.phoneNumber?.trim() || "",
          email: form.email?.trim() || "",
          isActive: form.isActive,
        });
      }

      onSave();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : mode === "create"
            ? "仕入先の作成に失敗しました。"
            : "仕入先の更新に失敗しました。"
      );
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={onSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>仕入先コード</span>
        {mode === "create" ? (
          <small className={styles.hint}>登録時に自動採番されます。</small>
        ) : (
          <>
            <input value={supplierCode} readOnly className={styles.readOnlyInput} />
            <small className={styles.hint}>仕入先コードは変更できません。</small>
          </>
        )}
      </label>

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
