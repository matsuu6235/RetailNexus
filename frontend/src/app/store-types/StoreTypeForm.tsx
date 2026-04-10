"use client";

import {
  createStoreType,
  getStoreTypeById,
  updateStoreType,
  changeStoreTypeActivation,
  type CreateStoreTypeRequest,
} from "@/lib/api/storeTypes";
import { validateStoreType, type StoreTypeFieldErrors } from "@/lib/validators/storeTypeValidator";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import ActivationFieldset from "@/components/form/ActivationFieldset";
import styles from "@/components/modal/FormModal.module.css";

export default function StoreTypeForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateStoreTypeRequest, StoreTypeFieldErrors>({
      mode,
      editId,
      initialForm: { storeTypeCode: "", storeTypeName: "" },
      entityName: "店舗種別",
      validator: (f) => validateStoreType(f),
      load: async (id) => {
        if (!id) return undefined;
        const data = await getStoreTypeById(id);
        return {
          form: { storeTypeCode: data.storeTypeCode, storeTypeName: data.storeTypeName },
          isActive: data.isActive,
        };
      },
      save: async (f) => {
        const payload = { storeTypeCode: f.storeTypeCode.trim(), storeTypeName: f.storeTypeName.trim() };
        if (mode === "create") await createStoreType(payload);
        else await updateStoreType(editId!, payload);
      },
      onSave,
      activation: { permissionCode: "store-types.delete", changeFn: changeStoreTypeActivation },
    });

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <div className={styles.field}>
        <label>
          店舗種別コード <span>*</span>
        </label>
        <input
          type="text"
          value={form.storeTypeCode}
          onChange={(e) => handleChange("storeTypeCode", e.target.value as string)}
          disabled={mode === "edit"}
          className={mode === "edit" ? styles.readOnlyInput : styles.input}
          readOnly={mode === "edit"}
        />
        {fieldErrors.storeTypeCode && <span className={styles.errorText}>{fieldErrors.storeTypeCode}</span>}
        {mode === "create" && <span className={styles.hint}>2文字以内で入力してください。</span>}
      </div>

      <div className={styles.field}>
        <label>
          店舗種別名 <span>*</span>
        </label>
        <input
          type="text"
          value={form.storeTypeName}
          onChange={(e) => handleChange("storeTypeName", e.target.value as string)}
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
        <ActivationFieldset currentIsActive={activation.currentIsActive} changingActivation={activation.changingActivation} toggle={activation.toggle} />
      )}
    </form>
  );
}
