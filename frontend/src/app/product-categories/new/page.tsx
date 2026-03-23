"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  createProductCategory,
  type CreateProductCategoryRequest,
} from "@/lib/api/productCategories";
import { validateProductCategory, type ProductCategoryFieldErrors } from "@/lib/validators/productCategoryValidator";
import styles from "./page.module.css";

export default function NewProductCategoryPage() {
  const router = useRouter();
  const [form, setForm] = useState<CreateProductCategoryRequest>({
    productCategoryCd: "",
    productCategoryName: "",
    isActive: true,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<ProductCategoryFieldErrors>({});

  const handleChange = (
    field: keyof CreateProductCategoryRequest,
    value: string | boolean
  ) => {
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

    if (!validate()) {
      return;
    }

    try {
      setSubmitting(true);
      await createProductCategory({
        productCategoryCd: form.productCategoryCd.trim(),
        productCategoryName: form.productCategoryName.trim(),
        isActive: form.isActive,
      });
      router.push("/product-categories");
    } catch (err) {
      setError(err instanceof Error ? err.message : "商品カテゴリの登録に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>商品カテゴリ新規作成</h1>
      <p className={styles.description}>
        商品カテゴリコードと商品カテゴリ名を入力してください。表示順は一覧画面でドラッグして変更します。
      </p>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>商品カテゴリコード *</span>
          <input
            value={form.productCategoryCd}
            onChange={(e) => handleChange("productCategoryCd", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>30文字以内で入力してください。</small>
          {fieldErrors.productCategoryCd && (
            <small className={styles.errorText}>{fieldErrors.productCategoryCd}</small>
          )}
        </label>

        <label className={styles.field}>
          <span>商品カテゴリ名 *</span>
          <input
            value={form.productCategoryName}
            onChange={(e) => handleChange("productCategoryName", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>100文字以内で入力してください。</small>
          {fieldErrors.productCategoryName && (
            <small className={styles.errorText}>{fieldErrors.productCategoryName}</small>
          )}
        </label>

        <label className={styles.checkboxField}>
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={(e) => handleChange("isActive", e.target.checked)}
          />
          <span>有効</span>
        </label>

        {error && <div className={styles.errorBox}>{error}</div>}

        <div className={styles.actions}>
          <button
            type="button"
            onClick={() => router.push("/product-categories")}
            className={styles.cancelButton}
          >
            キャンセル
          </button>
          <button type="submit" disabled={submitting} className={styles.submitButton}>
            {submitting ? "登録中..." : "登録"}
          </button>
        </div>
      </form>
    </main>
  );
}
