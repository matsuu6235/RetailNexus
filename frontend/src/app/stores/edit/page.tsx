"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getStoreById, updateStore, type UpdateStoreRequest } from "@/lib/api/stores";
import { getAllAreas } from "@/lib/api/areas";
import { getStoreTypes } from "@/lib/api/storeTypes";
import type { Area } from "@/types/areas";
import type { StoreType } from "@/types/storeTypes";
import { validateStore, type StoreFieldErrors } from "@/lib/validators/storeValidator";
import styles from "./page.module.css";

export default function EditStorePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const storeId = searchParams.get("id") ?? "";

  const [areas, setAreas] = useState<Area[]>([]);
  const [storeTypes, setStoreTypes] = useState<StoreType[]>([]);
  const [form, setForm] = useState<UpdateStoreRequest>({
    storeCd: "",
    storeName: "",
    areaId: "",
    storeTypeId: "",
    isActive: true,
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<StoreFieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    if (!storeId) {
      setError("店舗IDが指定されていません。");
      setLoading(false);
      return;
    }

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const [store, areaItems, typeItems] = await Promise.all([
          getStoreById(storeId),
          getAllAreas(),
          getStoreTypes(),
        ]);

        if (!cancelled) {
          setAreas(areaItems);
          setStoreTypes(typeItems);
          setForm({
            storeCd: store.storeCd,
            storeName: store.storeName,
            areaId: store.areaId,
            storeTypeId: store.storeTypeId,
            isActive: store.isActive,
          });
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "店舗情報の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [storeId]);

  const handleChange = (field: keyof UpdateStoreRequest, value: string | boolean) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validateStore(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof StoreFieldErrors] }));
  };

  const validate = () => {
    const errors = validateStore(form);
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!validate()) return;
    if (!storeId) {
      setError("店舗IDが指定されていません。");
      return;
    }

    try {
      setSubmitting(true);
      await updateStore(storeId, {
        storeCd: form.storeCd.trim(),
        storeName: form.storeName.trim(),
        areaId: form.areaId,
        storeTypeId: form.storeTypeId,
        isActive: form.isActive,
      });
      router.push("/stores");
    } catch (err) {
      setError(err instanceof Error ? err.message : "店舗の更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <main className={styles.page}>読み込み中...</main>;

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>店舗編集</h1>
      <p className={styles.description}>店舗情報を更新します。</p>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>店舗コード *</span>
          <input value={form.storeCd} onChange={(e) => handleChange("storeCd", e.target.value)} className={styles.input} />
          <small className={styles.hint}>6文字以内で入力してください。</small>
          {fieldErrors.storeCd && <small className={styles.errorText}>{fieldErrors.storeCd}</small>}
        </label>

        <label className={styles.field}>
          <span>店舗名 *</span>
          <input value={form.storeName} onChange={(e) => handleChange("storeName", e.target.value)} className={styles.input} />
          <small className={styles.hint}>100文字以内で入力してください。</small>
          {fieldErrors.storeName && <small className={styles.errorText}>{fieldErrors.storeName}</small>}
        </label>

        <label className={styles.field}>
          <span>エリア *</span>
          <select value={form.areaId} onChange={(e) => handleChange("areaId", e.target.value)} className={styles.select}>
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
          <select value={form.storeTypeId} onChange={(e) => handleChange("storeTypeId", e.target.value)} className={styles.select}>
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
          <button type="submit" disabled={submitting} className={styles.submitButton}>
            {submitting ? "更新中..." : "更新"}
          </button>
        </div>
      </form>
    </main>
  );
}
