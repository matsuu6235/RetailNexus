"use client";

import { useState } from "react";
import {
  createProduct,
  getProductById,
  updateProduct,
  changeProductActivation,
} from "@/lib/api/products";
import { getAllProductCategories } from "@/lib/api/productCategories";
import type { ProductCategory } from "@/types/productCategories";
import {
  validateProduct,
  validateUpdateProduct,
  type ProductFieldErrors,
  type UpdateProductFieldErrors,
} from "@/lib/validators/productValidator";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import ActivationFieldset from "@/components/form/ActivationFieldset";
import styles from "@/components/modal/FormModal.module.css";

type ProductFormData = {
  janCode: string;
  productName: string;
  price: number;
  cost: number;
  productCategoryCode: string;
};

type CombinedFieldErrors = ProductFieldErrors & UpdateProductFieldErrors;

export default function ProductForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [productCode, setProductCode] = useState("");

  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<ProductFormData, CombinedFieldErrors>({
      mode,
      editId,
      initialForm: { janCode: "", productName: "", price: 0, cost: 0, productCategoryCode: "" },
      entityName: "商品",
      validator: (f, m) => (m === "create" ? validateProduct(f) : validateUpdateProduct(f)) as CombinedFieldErrors,
      load: async (id) => {
        if (id) {
          const [product, cats] = await Promise.all([
            getProductById(id),
            getAllProductCategories({ isActive: "active" }),
          ]);
          setCategories(cats);
          setProductCode(product.productCode);
          return {
            form: {
              janCode: product.janCode ?? "",
              productName: product.productName,
              price: product.price,
              cost: product.cost,
              productCategoryCode: product.productCategoryCode,
            },
            isActive: product.isActive,
          };
        }
        const cats = await getAllProductCategories({ isActive: "active" });
        setCategories(cats);
        return undefined;
      },
      save: async (f) => {
        const payload = {
          janCode: f.janCode.trim(),
          productName: f.productName.trim(),
          price: f.price,
          cost: f.cost,
          productCategoryCode: f.productCategoryCode,
        };
        if (mode === "create") await createProduct(payload);
        else await updateProduct(editId!, payload);
      },
      onSave,
      activation: { permissionCode: "products.delete", changeFn: changeProductActivation },
    });

  const handleNumericChange = (field: "price" | "cost", value: string) => {
    handleChange(field, (value === "" ? 0 : Number(value)) as ProductFormData[typeof field]);
  };

  if (loading) return <p>読み込み中...</p>;

  return (
    <div className={styles.form}>
      <label className={styles.field}>
        <span className={styles.label}>商品コード</span>
        {mode === "create" ? (
          <span className={styles.hint}>登録時にカテゴリに基づいて自動採番されます。</span>
        ) : (
          <>
            <input type="text" value={productCode} readOnly className={styles.readOnlyInput} />
            <span className={styles.hint}>商品コードは変更できません。</span>
          </>
        )}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>JAN</span>
        <input
          type="text"
          value={form.janCode}
          onChange={(e) => handleChange("janCode", e.target.value as string)}
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
          onChange={(e) => handleChange("productName", e.target.value as string)}
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
          onChange={(e) => handleNumericChange("price", e.target.value)}
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
          onChange={(e) => handleNumericChange("cost", e.target.value)}
          onFocus={(e) => e.target.select()}
          className={styles.input}
        />
        {fieldErrors.cost && <span className={styles.errorText}>{fieldErrors.cost}</span>}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>{mode === "create" ? "カテゴリ" : "カテゴリ *"}</span>
        <select
          value={form.productCategoryCode}
          onChange={(e) => handleChange("productCategoryCode", e.target.value as string)}
          className={styles.input}
        >
          <option value="">選択してください</option>
          {categories.map((c) => (
            <option key={c.productCategoryId} value={c.productCategoryCode}>
              {c.productCategoryName} ({c.productCategoryCode})
            </option>
          ))}
        </select>
        {fieldErrors.productCategoryCode && (
          <span className={styles.errorText}>{fieldErrors.productCategoryCode}</span>
        )}
      </label>

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
