"use client";

import { useState, useEffect } from "react";
import {
  createStore,
  getStoreById,
  updateStore,
  changeStoreActivation,
  type CreateStoreRequest,
  type UpdateStoreRequest,
} from "@/lib/api/stores";
import { getAllAreas } from "@/lib/api/areas";
import { getStoreTypes } from "@/lib/api/storeTypes";
import type { Area } from "@/types/areas";
import type { StoreType } from "@/types/storeTypes";
import { validateStore, type StoreFieldErrors } from "@/lib/validators/storeValidator";
import { hasPermission } from "@/services/authService";
import styles from "@/components/modal/FormModal.module.css";

interface StoreFormProps {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
}

export default function StoreForm({ mode, editId, onSave, onCancel }: StoreFormProps) {
  const [areas, setAreas] = useState<Area[]>([]);
  const [storeTypes, setStoreTypes] = useState<StoreType[]>([]);
  const [storeCd, setStoreCd] = useState("");
  const [form, setForm] = useState<CreateStoreRequest>({
    storeName: "",
    areaId: "",
    storeTypeId: "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<StoreFieldErrors>({});
  const [loading, setLoading] = useState(true);
  const [canDelete, setCanDelete] = useState(false);
  const [currentIsActive, setCurrentIsActive] = useState(true);
  const [changingActivation, setChangingActivation] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        if (mode === "create") {
          const [areaItems, typeItems] = await Promise.all([
            getAllAreas(),
            getStoreTypes({ isActive: "active" }),
          ]);

          if (!cancelled) {
            setAreas(areaItems);
            setStoreTypes(typeItems);
            setLoading(false);
          }
        } else {
          const [store, areaItems, typeItems] = await Promise.all([
            getStoreById(editId!),
            getAllAreas(),
            getStoreTypes(),
          ]);

          if (!cancelled) {
            setAreas(areaItems);
            setStoreTypes(typeItems);
            setStoreCd(store.storeCd);
            setForm({
              storeName: store.storeName,
              areaId: store.areaId,
              storeTypeId: store.storeTypeId,
            });
            setCurrentIsActive(store.isActive);
            setLoading(false);
          }
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "データの取得に失敗しました。");
          setLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

  useEffect(() => {
    setCanDelete(hasPermission("stores.delete"));
  }, []);

  const handleChange = (field: keyof CreateStoreRequest, value: string | boolean) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validateStore(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof StoreFieldErrors] }));
  };

  const handleSubmit = async () => {
    const errors = validateStore(form);
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    try {
      setSubmitting(true);
      setError(null);

      if (mode === "create") {
        await createStore({
          storeName: form.storeName.trim(),
          areaId: form.areaId,
          storeTypeId: form.storeTypeId,
        });
      } else {
        await updateStore(editId!, {
          storeName: form.storeName.trim(),
          areaId: form.areaId,
          storeTypeId: form.storeTypeId,
        });
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
    <div className={styles.form}>
      <div className={styles.field}>
        <label className={styles.label}>店舗コード</label>
        {mode === "create" ? (
          <p className={styles.hint}>登録時に自動採番されます。</p>
        ) : (
          <>
            <input type="text" value={storeCd} readOnly className={styles.readOnlyInput} />
            <p className={styles.hint}>店舗コードは変更できません。</p>
          </>
        )}
      </div>

      <div className={styles.field}>
        <label className={styles.label}>店舗名 *</label>
        <input
          type="text"
          value={form.storeName}
          onChange={(e) => handleChange("storeName", e.target.value)}
          className={styles.input}
        />
        <p className={styles.hint}>50文字以内で入力してください。</p>
        {fieldErrors.storeName && <p className={styles.fieldError}>{fieldErrors.storeName}</p>}
      </div>

      <div className={styles.field}>
        <label className={styles.label}>エリア *</label>
        <select
          value={form.areaId}
          onChange={(e) => handleChange("areaId", e.target.value)}
          className={styles.select}
          disabled={loading}
        >
          <option value="">選択してください</option>
          {areas.map((area) => (
            <option key={area.areaId} value={area.areaId}>
              {area.areaName} ({area.areaCd})
            </option>
          ))}
        </select>
        {fieldErrors.areaId && <p className={styles.fieldError}>{fieldErrors.areaId}</p>}
      </div>

      <div className={styles.field}>
        <label className={styles.label}>店舗種別 *</label>
        <select
          value={form.storeTypeId}
          onChange={(e) => handleChange("storeTypeId", e.target.value)}
          className={styles.select}
          disabled={loading}
        >
          <option value="">選択してください</option>
          {storeTypes.map((storeType) => (
            <option key={storeType.storeTypeId} value={storeType.storeTypeId}>
              {storeType.storeTypeName} ({storeType.storeTypeCd})
            </option>
          ))}
        </select>
        {fieldErrors.storeTypeId && <p className={styles.fieldError}>{fieldErrors.storeTypeId}</p>}
      </div>

      {error && <div className={styles.errorBox}>{error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton} disabled={submitting}>
          キャンセル
        </button>
        <button type="button" onClick={handleSubmit} className={styles.submitButton} disabled={submitting || loading}>
          {submitting ? "保存中..." : "保存"}
        </button>
      </div>

      {mode === "edit" && canDelete && (
        <fieldset className={styles.field} style={{ border: "1px solid #e2e8f0", borderRadius: "8px", padding: "12px", marginTop: "8px" }}>
          <legend style={{ fontSize: "13px", fontWeight: 600, color: "#0f172a", padding: "0 4px" }}>有効状態の変更</legend>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
            <span style={{ fontSize: "13px" }}>
              現在の状態: <strong>{currentIsActive ? "有効" : "無効"}</strong>
            </span>
            <button
              type="button"
              onClick={async () => {
                try {
                  setChangingActivation(true);
                  await changeStoreActivation(editId!, !currentIsActive);
                  setCurrentIsActive(!currentIsActive);
                } catch (err) {
                  setError(err instanceof Error ? err.message : "状態の変更に失敗しました。");
                } finally {
                  setChangingActivation(false);
                }
              }}
              disabled={changingActivation}
              className={styles.submitButton}
              style={currentIsActive ? { backgroundColor: "#dc2626" } : {}}
            >
              {changingActivation ? "変更中..." : currentIsActive ? "無効化する" : "有効化する"}
            </button>
          </div>
        </fieldset>
      )}
    </div>
  );
}
