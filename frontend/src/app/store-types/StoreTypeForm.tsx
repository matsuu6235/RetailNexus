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
import styles from "@/components/modal/FormModal.module.css";

export default function StoreTypeForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateStoreTypeRequest, StoreTypeFieldErrors>({
      mode,
      editId,
      initialForm: { storeTypeCd: "", storeTypeName: "" },
      entityName: "店舗種別",
      validator: (f) => validateStoreType(f),
      load: async (id) => {
        if (!id) return undefined;
        const data = await getStoreTypeById(id);
        return {
          form: { storeTypeCd: data.storeTypeCd, storeTypeName: data.storeTypeName },
          isActive: data.isActive,
        };
      },
      save: async (f) => {
        const payload = { storeTypeCd: f.storeTypeCd.trim(), storeTypeName: f.storeTypeName.trim() };
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
          value={form.storeTypeCd}
          onChange={(e) => handleChange("storeTypeCd", e.target.value as string)}
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
