"use client";

import { useState } from "react";
import {
  createStore,
  getStoreById,
  updateStore,
  changeStoreActivation,
  type CreateStoreRequest,
} from "@/lib/api/stores";
import { getAllAreas } from "@/lib/api/areas";
import { getStoreTypes } from "@/lib/api/storeTypes";
import type { Area } from "@/types/areas";
import type { StoreType } from "@/types/storeTypes";
import { validateStore, type StoreFieldErrors } from "@/lib/validators/storeValidator";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import ActivationFieldset from "@/components/form/ActivationFieldset";
import styles from "@/components/modal/FormModal.module.css";

export default function StoreForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const [areas, setAreas] = useState<Area[]>([]);
  const [storeTypes, setStoreTypes] = useState<StoreType[]>([]);
  const [storeCode, setStoreCode] = useState("");

  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateStoreRequest, StoreFieldErrors>({
      mode,
      editId,
      initialForm: { storeName: "", areaId: "", storeTypeId: "" },
      entityName: "店舗",
      validator: (f) => validateStore(f),
      load: async (id) => {
        if (id) {
          const [store, areaItems, typeItems] = await Promise.all([
            getStoreById(id),
            getAllAreas(),
            getStoreTypes(),
          ]);
          setAreas(areaItems);
          setStoreTypes(typeItems);
          setStoreCode(store.storeCode);
          return {
            form: { storeName: store.storeName, areaId: store.areaId, storeTypeId: store.storeTypeId },
            isActive: store.isActive,
          };
        }
        const [areaItems, typeItems] = await Promise.all([
          getAllAreas(),
          getStoreTypes({ isActive: "active" }),
        ]);
        setAreas(areaItems);
        setStoreTypes(typeItems);
        return undefined;
      },
      save: async (f) => {
        const payload = { storeName: f.storeName.trim(), areaId: f.areaId, storeTypeId: f.storeTypeId };
        if (mode === "create") await createStore(payload);
        else await updateStore(editId!, payload);
      },
      onSave,
      activation: { permissionCode: "stores.delete", changeFn: changeStoreActivation },
    });

  if (loading) return <p>読み込み中...</p>;

  return (
    <div className={styles.form}>
      <div className={styles.field}>
        <label className={styles.label}>店舗コード</label>
        {mode === "create" ? (
          <p className={styles.hint}>登録時に自動採番されます。</p>
        ) : (
          <>
            <input type="text" value={storeCode} readOnly className={styles.readOnlyInput} />
            <p className={styles.hint}>店舗コードは変更できません。</p>
          </>
        )}
      </div>

      <div className={styles.field}>
        <label className={styles.label}>店舗名 *</label>
        <input
          type="text"
          value={form.storeName}
          onChange={(e) => handleChange("storeName", e.target.value as string)}
          className={styles.input}
        />
        <p className={styles.hint}>50文字以内で入力してください。</p>
        {fieldErrors.storeName && <p className={styles.fieldError}>{fieldErrors.storeName}</p>}
      </div>

      <div className={styles.field}>
        <label className={styles.label}>エリア *</label>
        <select
          value={form.areaId}
          onChange={(e) => handleChange("areaId", e.target.value as string)}
          className={styles.select}
        >
          <option value="">選択してください</option>
          {areas.map((area) => (
            <option key={area.areaId} value={area.areaId}>
              {area.areaName} ({area.areaCode})
            </option>
          ))}
        </select>
        {fieldErrors.areaId && <p className={styles.fieldError}>{fieldErrors.areaId}</p>}
      </div>

      <div className={styles.field}>
        <label className={styles.label}>店舗種別 *</label>
        <select
          value={form.storeTypeId}
          onChange={(e) => handleChange("storeTypeId", e.target.value as string)}
          className={styles.select}
        >
          <option value="">選択してください</option>
          {storeTypes.map((storeType) => (
            <option key={storeType.storeTypeId} value={storeType.storeTypeId}>
              {storeType.storeTypeName} ({storeType.storeTypeCode})
            </option>
          ))}
        </select>
        {fieldErrors.storeTypeId && <p className={styles.fieldError}>{fieldErrors.storeTypeId}</p>}
      </div>

      {(error || activation.error) && <div className={styles.errorBox}>{error || activation.error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton} disabled={submitting}>
          キャンセル
        </button>
        <button type="button" onClick={() => handleSubmit()} className={styles.submitButton} disabled={submitting}>
          {submitting ? "保存中..." : "保存"}
        </button>
      </div>

      {mode === "edit" && activation.canDelete && (
        <ActivationFieldset currentIsActive={activation.currentIsActive} changingActivation={activation.changingActivation} toggle={activation.toggle} />
      )}
    </div>
  );
}
