"use client";

import { getAreaById, createArea, updateArea, changeAreaActivation, type CreateAreaRequest } from "@/lib/api/areas";
import { validateArea, type AreaFieldErrors } from "@/lib/validators/areaValidator";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import styles from "@/components/modal/FormModal.module.css";

export default function AreaForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateAreaRequest, AreaFieldErrors>({
      mode,
      editId,
      initialForm: { areaCode: "", areaName: "" },
      entityName: "エリア",
      validator: (f) => validateArea(f),
      load: async (id) => {
        if (!id) return undefined;
        const item = await getAreaById(id);
        return { form: { areaCode: item.areaCode, areaName: item.areaName }, isActive: item.isActive };
      },
      save: async (f) => {
        const payload = { areaCode: f.areaCode.trim(), areaName: f.areaName.trim() };
        if (mode === "create") await createArea(payload);
        else await updateArea(editId!, payload);
      },
      onSave,
      activation: { permissionCode: "areas.delete", changeFn: changeAreaActivation },
    });

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>エリアコード *</span>
        <input value={form.areaCode} onChange={(e) => handleChange("areaCode", e.target.value as string)} className={styles.input} />
        <small className={styles.hint}>2文字以内で入力してください。</small>
        {fieldErrors.areaCode && <small className={styles.errorText}>{fieldErrors.areaCode}</small>}
      </label>

      <label className={styles.field}>
        <span>エリア名 *</span>
        <input value={form.areaName} onChange={(e) => handleChange("areaName", e.target.value as string)} className={styles.input} />
        <small className={styles.hint}>20文字以内で入力してください。</small>
        {fieldErrors.areaName && <small className={styles.errorText}>{fieldErrors.areaName}</small>}
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
