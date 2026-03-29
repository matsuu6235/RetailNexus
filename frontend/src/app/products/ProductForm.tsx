"use client";

import { useState, useEffect } from "react";
import {
  createProduct,
  getProductById,
  updateProduct,
  changeProductActivation,
  type CreateProductRequest,
  type UpdateProductRequest,
} from "@/lib/api/products";
import { getAllProductCategories } from "@/lib/api/productCategories";
import type { ProductCategory } from "@/types/productCategories";
import {
  validateProduct,
  validateUpdateProduct,
  type ProductFieldErrors,
  type UpdateProductFieldErrors,
} from "@/lib/validators/productValidator";
import { hasPermission } from "@/services/authService";
import styles from "@/components/modal/FormModal.module.css";

interface ProductFormProps {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
}

export default function ProductForm({ mode, editId, onSave, onCancel }: ProductFormProps) {
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [productCode, setProductCode] = useState("");
  const [form, setForm] = useState<{
    janCode: string;
    productName: string;
    price: number;
    cost: number;
    productCategoryCode: string;
  }>({ janCode: "", productName: "", price: 0, cost: 0, productCategoryCode: "" });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<ProductFieldErrors | UpdateProductFieldErrors>({});
  const [loading, setLoading] = useState(true);
  const [canDelete, setCanDelete] = useState(false);
  const [currentIsActive, setCurrentIsActive] = useState(true);
  const [changingActivation, setChangingActivation] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        if (mode === "create") {
          const cats = await getAllProductCategories({ isActive: "active" });
          if (!cancelled) {
            setCategories(cats);
            setLoading(false);
          }
        } else {
          const [product, cats] = await Promise.all([
            getProductById(editId!),
            getAllProductCategories({ isActive: "active" }),
          ]);
          if (!cancelled) {
            setProductCode(product.productCode);
            setForm({
              janCode: product.janCode ?? "",
              productName: product.productName,
              price: product.price,
              cost: product.cost,
              productCategoryCode: product.productCategoryCode,
            });
            setCurrentIsActive(product.isActive);
            setCategories(cats);
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
    setCanDelete(hasPermission("products.delete"));
  }, []);

  const handleChange = (field: string, value: string | boolean) => {
    const numericValue = field === "price" || field === "cost" ? (value === "" ? 0 : Number(value)) : value;
    const updatedForm = { ...form, [field]: numericValue };
    setForm(updatedForm);
    if (mode === "create") {
      const errors = validateProduct(updatedForm);
      setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof ProductFieldErrors] }));
    } else {
      const errors = validateUpdateProduct(updatedForm);
      setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof UpdateProductFieldErrors] }));
    }
  };

  const handleSubmit = async () => {
    setError(null);

    if (mode === "create") {
      const errors = validateProduct(form);
      if (Object.keys(errors).length > 0) {
        setFieldErrors(errors);
        return;
      }
    } else {
      const errors = validateUpdateProduct(form);
      if (Object.keys(errors).length > 0) {
        setFieldErrors(errors);
        return;
      }
    }

    try {
      setSubmitting(true);
      if (mode === "create") {
        await createProduct({
          janCode: form.janCode.trim(),
          productName: form.productName.trim(),
          price: form.price,
          cost: form.cost,
          productCategoryCode: form.productCategoryCode,
        });
      } else {
        await updateProduct(editId!, {
          janCode: form.janCode.trim(),
          productName: form.productName.trim(),
          price: form.price,
          cost: form.cost,
          productCategoryCode: form.productCategoryCode,
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
      <label className={styles.field}>
        <span className={styles.label}>商品コード</span>
        {mode === "create" ? (
          <span className={styles.hint}>登録時にカテゴリに基づいて自動採番されます。</span>
        ) : (
          <>
            <input
              type="text"
              value={productCode}
              readOnly
              className={styles.readOnlyInput}
            />
            <span className={styles.hint}>商品コードは変更できません。</span>
          </>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>JAN</span>
        <input
          type="text"
          value={form.janCode}
          onChange={(e) => handleChange("janCode", e.target.value)}
          className={styles.input}
        />
        <span className={styles.hint}>13桁の数字で入力してください。</span>
        {fieldErrors.janCode && <span className={styles.errorText}>{fieldErrors.janCode}</span>}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>商品名 *</span>
        <input
          type="text"
          value={form.productName}
          onChange={(e) => handleChange("productName", e.target.value)}
          className={styles.input}
        />
        <span className={styles.hint}>200文字以内で入力してください。</span>
        {fieldErrors.productName && <span className={styles.errorText}>{fieldErrors.productName}</span>}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>売価 *</span>
        <input
          type="number"
          min={0}
          value={form.price}
          onChange={(e) => handleChange("price", e.target.value)}
          onFocus={(e) => e.target.select()}
          className={styles.input}
        />
        {fieldErrors.price && <span className={styles.errorText}>{fieldErrors.price}</span>}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>原価 *</span>
        <input
          type="number"
          min={0}
          value={form.cost}
          onChange={(e) => handleChange("cost", e.target.value)}
          onFocus={(e) => e.target.select()}
          className={styles.input}
        />
        {fieldErrors.cost && <span className={styles.errorText}>{fieldErrors.cost}</span>}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>{mode === "create" ? "カテゴリ" : "カテゴリ *"}</span>
        <select
          value={form.productCategoryCode}
          onChange={(e) => handleChange("productCategoryCode", e.target.value)}
          className={styles.input}
          disabled={loading}
        >
          <option value="">選択してください</option>
          {categories.map((c) => (
            <option key={c.productCategoryId} value={c.productCategoryCd}>
              {c.productCategoryName} ({c.productCategoryCd})
            </option>
          ))}
        </select>
        {fieldErrors.productCategoryCode && (
          <span className={styles.errorText}>{fieldErrors.productCategoryCode}</span>
        )}
      </label>

      {error && <div className={styles.errorBox}>{error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton} disabled={submitting}>
          キャンセル
        </button>
        <button type="button" onClick={handleSubmit} className={styles.submitButton} disabled={submitting}>
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
                  await changeProductActivation(editId!, !currentIsActive);
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
