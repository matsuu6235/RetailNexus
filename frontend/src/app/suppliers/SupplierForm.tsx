"use client";

import { useState } from "react";
import {
  createSupplier,
  getSupplierById,
  updateSupplier,
  changeSupplierActivation,
  type CreateSupplierRequest,
} from "@/lib/api/suppliers";
import { validateSupplier, type SupplierFieldErrors } from "@/lib/validators/supplierValidator";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import ActivationFieldset from "@/components/form/ActivationFieldset";
import styles from "@/components/modal/FormModal.module.css";

export default function SupplierForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const [supplierCode, setSupplierCode] = useState("");

  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateSupplierRequest, SupplierFieldErrors>({
      mode,
      editId,
      initialForm: { supplierName: "", phoneNumber: "", email: "" },
      entityName: "仕入先",
      validator: (f) => validateSupplier(f),
      load: async (id) => {
        if (!id) return undefined;
        const supplier = await getSupplierById(id);
        setSupplierCode(supplier.supplierCode);
        return {
          form: { supplierName: supplier.supplierName, phoneNumber: supplier.phoneNumber ?? "", email: supplier.email ?? "" },
          isActive: supplier.isActive,
        };
      },
      save: async (f) => {
        const payload = { supplierName: f.supplierName.trim(), phoneNumber: f.phoneNumber?.trim() ?? "", email: f.email?.trim() ?? "" };
        if (mode === "create") await createSupplier(payload);
        else await updateSupplier(editId!, payload);
      },
      onSave,
      activation: { permissionCode: "suppliers.delete", changeFn: changeSupplierActivation },
    });

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
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
          onChange={(e) => handleChange("supplierName", e.target.value as string)}
          className={styles.input}
        />
        <small className={styles.hint}>50文字以内で入力してください。</small>
        {fieldErrors.supplierName && <small className={styles.errorText}>{fieldErrors.supplierName}</small>}
      </label>

      <label className={styles.field}>
        <span>電話番号</span>
        <input
          value={form.phoneNumber}
          onChange={(e) => handleChange("phoneNumber", e.target.value as string)}
          className={styles.input}
        />
        {fieldErrors.phoneNumber && <small className={styles.errorText}>{fieldErrors.phoneNumber}</small>}
      </label>

      <label className={styles.field}>
        <span>メールアドレス</span>
        <input
          type="email"
          value={form.email}
          onChange={(e) => handleChange("email", e.target.value as string)}
          className={styles.input}
        />
        {fieldErrors.email && <small className={styles.errorText}>{fieldErrors.email}</small>}
      </label>

      {(error || activation.error) && <div className={styles.errorBox}>{error || activation.error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton}>
          キャンセル
        </button>
        <button type="submit" disabled={submitting} className={styles.submitButton}>
          {mode === "create" ? (submitting ? "作成中..." : "作成") : (submitting ? "更新中..." : "更新")}
        </button>
      </div>

      {mode === "edit" && activation.canDelete && (
        <ActivationFieldset currentIsActive={activation.currentIsActive} changingActivation={activation.changingActivation} toggle={activation.toggle} />
      )}
    </form>
  );
}
