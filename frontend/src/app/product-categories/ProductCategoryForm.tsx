"use client";

import { useEffect, useState } from "react";
import {
  createProductCategory,
  getProductCategoryById,
  updateProductCategory,
  changeProductCategoryActivation,
  type CreateProductCategoryRequest,
  type UpdateProductCategoryRequest,
} from "@/lib/api/productCategories";
import { validateProductCategory, type ProductCategoryFieldErrors } from "@/lib/validators/productCategoryValidator";
import { useActivation } from "@/lib/hooks/useActivation";
import styles from "@/components/modal/FormModal.module.css";

type ProductCategoryFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

export default function ProductCategoryForm({ mode, editId, onSave, onCancel }: ProductCategoryFormProps) {
  const [form, setForm] = useState<CreateProductCategoryRequest>({
    productCategoryCd: "",
    categoryAbbreviation: "",
    productCategoryName: "",
  });
  const [loading, setLoading] = useState(mode === "edit");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<ProductCategoryFieldErrors>({});
  const [fetchedIsActive, setFetchedIsActive] = useState(true);
  const activation = useActivation({ permissionCode: "product-categories.delete", initialIsActive: fetchedIsActive, changeFn: changeProductCategoryActivation, editId });

  useEffect(() => {
    if (mode !== "edit" || !editId) return;

    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const item = await getProductCategoryById(editId);

        if (!cancelled) {
          setForm({
            productCategoryCd: item.productCategoryCd,
            categoryAbbreviation: item.categoryAbbreviation,
            productCategoryName: item.productCategoryName,
          });
          setFetchedIsActive(item.isActive);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "商品カテゴリ情報の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

  const handleChange = (field: keyof CreateProductCategoryRequest, value: string | boolean) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validateProductCategory(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof ProductCategoryFieldErrors] }));
  };

  const validate = () => {
    const errors = validateProductCategory(form);
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
        await createProductCategory({
          productCategoryCd: form.productCategoryCd.trim(),
          categoryAbbreviation: form.categoryAbbreviation.trim(),
          productCategoryName: form.productCategoryName.trim(),
        });
      } else {
        await updateProductCategory(editId!, {
          productCategoryCd: form.productCategoryCd.trim(),
          categoryAbbreviation: form.categoryAbbreviation.trim(),
          productCategoryName: form.productCategoryName.trim(),
        });
      }

      onSave();
    } catch (err) {
      setError(err instanceof Error ? err.message : mode === "create" ? "商品カテゴリの作成に失敗しました。" : "商品カテゴリの更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={onSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>商品カテゴリコード *</span>
        <input value={form.productCategoryCd} onChange={(e) => handleChange("productCategoryCd", e.target.value)} className={styles.input} />
        <small className={styles.hint}>数字3文字以内で入力してください。</small>
        {fieldErrors.productCategoryCd && <small className={styles.errorText}>{fieldErrors.productCategoryCd}</small>}
      </label>

      <label className={styles.field}>
        <span>カテゴリ略称 *</span>
        <input value={form.categoryAbbreviation} onChange={(e) => handleChange("categoryAbbreviation", e.target.value)} className={styles.input} />
        <small className={styles.hint}>英字2〜5文字で入力してください。例: FD → 商品コード FD-000001</small>
        {fieldErrors.categoryAbbreviation && <small className={styles.errorText}>{fieldErrors.categoryAbbreviation}</small>}
      </label>

      <label className={styles.field}>
        <span>商品カテゴリ名 *</span>
        <input value={form.productCategoryName} onChange={(e) => handleChange("productCategoryName", e.target.value)} className={styles.input} />
        <small className={styles.hint}>30文字以内で入力してください。</small>
        {fieldErrors.productCategoryName && <small className={styles.errorText}>{fieldErrors.productCategoryName}</small>}
      </label>

      {(error || activation.error) && <div className={styles.errorBox}>{error || activation.error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton}>
          キャンセル
        </button>
        <button type="submit" disabled={submitting} className={styles.submitButton}>
          {mode === "create" ? (submitting ? "登録中..." : "登録") : (submitting ? "更新中..." : "更新")}
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
