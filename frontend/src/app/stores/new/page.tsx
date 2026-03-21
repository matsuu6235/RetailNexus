"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createStore, type CreateStoreRequest } from "../../lib/api/stores";
import { getAllAreas } from "../../lib/api/areas";
import { getStoreTypes } from "../../lib/api/storeTypes";
import type { Area } from "../../types/areas";
import type { StoreType } from "../../types/storeTypes";
import styles from "./page.module.css";

type FieldErrors = Partial<Record<keyof CreateStoreRequest, string>>;

export default function NewStorePage() {
  const router = useRouter();
  const [areas, setAreas] = useState<Area[]>([]);
  const [storeTypes, setStoreTypes] = useState<StoreType[]>([]);
  const [loadingMasters, setLoadingMasters] = useState(true);

  const [form, setForm] = useState<CreateStoreRequest>({
    storeCd: "",
    storeName: "",
    areaId: "",
    storeTypeId: "",
    isActive: true,
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const [areaItems, typeItems] = await Promise.all([getAllAreas(), getStoreTypes({ isActive: "active" })]);

        if (!cancelled) {
          setAreas(areaItems);
          setStoreTypes(typeItems);
          setForm((prev) => ({
            ...prev,
            areaId: prev.areaId || areaItems[0]?.areaId || "",
            storeTypeId: prev.storeTypeId || typeItems[0]?.storeTypeId || "",
          }));
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "参照マスタの取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoadingMasters(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const handleChange = (field: keyof CreateStoreRequest, value: string | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    setFieldErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validate = () => {
    const errors: FieldErrors = {};
    if (!form.storeCd.trim()) errors.storeCd = "店舗コードは必須です";
    if (!form.storeName.trim()) errors.storeName = "店舗名は必須です";
    if (!form.areaId) errors.areaId = "エリアは必須です";
    if (!form.storeTypeId) errors.storeTypeId = "店舗種別は必須です";
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;

    try {
      setSubmitting(true);
      await createStore({
        storeCd: form.storeCd.trim(),
        storeName: form.storeName.trim(),
        areaId: form.areaId,
        storeTypeId: form.storeTypeId,
        isActive: form.isActive,
      });
      router.push("/stores");
    } catch (err) {
      setError(err instanceof Error ? err.message : "店舗の作成に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>店舗新規作成</h1>
      <p className={styles.description}>店舗コード、店舗名、所属エリア、店舗種別を入力してください。</p>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>店舗コード *</span>
          <input value={form.storeCd} onChange={(e) => handleChange("storeCd", e.target.value)} className={styles.input} />
          {fieldErrors.storeCd && <small className={styles.errorText}>{fieldErrors.storeCd}</small>}
        </label>

        <label className={styles.field}>
          <span>店舗名 *</span>
          <input value={form.storeName} onChange={(e) => handleChange("storeName", e.target.value)} className={styles.input} />
          {fieldErrors.storeName && <small className={styles.errorText}>{fieldErrors.storeName}</small>}
        </label>

        <label className={styles.field}>
          <span>エリア *</span>
          <select value={form.areaId} onChange={(e) => handleChange("areaId", e.target.value)} className={styles.select} disabled={loadingMasters}>
            <option value="">選択してください</option>
            {areas.map((area) => (
              <option key={area.areaId} value={area.areaId}>
                {area.areaName} ({area.areaCd})
              </option>
            ))}
          </select>
          {fieldErrors.areaId && <small className={styles.errorText}>{fieldErrors.areaId}</small>}
        </label>

        <label className={styles.field}>
          <span>店舗種別 *</span>
          <select value={form.storeTypeId} onChange={(e) => handleChange("storeTypeId", e.target.value)} className={styles.select} disabled={loadingMasters}>
            <option value="">選択してください</option>
            {storeTypes.map((storeType) => (
              <option key={storeType.storeTypeId} value={storeType.storeTypeId}>
                {storeType.storeTypeName} ({storeType.storeTypeCd})
              </option>
            ))}
          </select>
          {fieldErrors.storeTypeId && <small className={styles.errorText}>{fieldErrors.storeTypeId}</small>}
        </label>

        <label className={styles.checkboxField}>
          <input type="checkbox" checked={form.isActive} onChange={(e) => handleChange("isActive", e.target.checked)} />
          <span>有効</span>
        </label>

        {error && <div className={styles.errorBox}>{error}</div>}

        <div className={styles.actions}>
          <button type="button" onClick={() => router.push("/stores")} className={styles.cancelButton}>
            キャンセル
          </button>
          <button type="submit" disabled={submitting || loadingMasters} className={styles.submitButton}>
            {submitting ? "作成中..." : "作成"}
          </button>
        </div>
      </form>
    </main>
  );
}