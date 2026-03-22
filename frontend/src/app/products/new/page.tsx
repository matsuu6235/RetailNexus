"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createProduct, type CreateProductRequest } from "../../lib/api/products";
import { getAllProductCategories } from "../../lib/api/productCategories";
import type { ProductCategory } from "../../types/productCategories";
import { validateProduct, type ProductFieldErrors } from "../../lib/validators/productValidator";
import styles from "./page.module.css";

export default function NewProductPage() {
  const router = useRouter();
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [loadingCategories, setLoadingCategories] = useState(true);

  const [form, setForm] = useState<CreateProductRequest>({
    productCode: "",
    janCode: "",
    productName: "",
    price: 0,
    cost: 0,
    productCategoryCode: "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<ProductFieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const items = await getAllProductCategories({ isActive: "active" });

        if (!cancelled) {
          setCategories(items);
          if (items.length > 0) {
            setForm((prev) => ({
              ...prev,
              productCategoryCode: prev.productCategoryCode || items[0].productCategoryCd,
            }));
          }
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "カテゴリ一覧の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) {
          setLoadingCategories(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const handleChange = (field: keyof CreateProductRequest, value: string) => {
    const numericValue = field === "price" || field === "cost" ? (value === "" ? 0 : Number(value)) : value;
    const updatedForm = { ...form, [field]: numericValue };
    setForm(updatedForm);
    const errors = validateProduct(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof ProductFieldErrors] }));
  };

  const validate = () => {
    const errors = validateProduct(form);
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;

    try {
      setSubmitting(true);
      await createProduct({
        productCode: form.productCode.trim(),
        janCode: form.janCode.trim(),
        productName: form.productName.trim(),
        price: form.price,
        cost: form.cost,
        productCategoryCode: form.productCategoryCode,
      });
      router.push("/products");
    } catch (err) {
      setError(err instanceof Error ? err.message : "商品作成に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>商品新規作成</h1>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>SKU *</span>
          <input
            value={form.productCode}
            onChange={(e) => handleChange("productCode", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>50文字以内で入力してください。</small>
          {fieldErrors.productCode && <small className={styles.errorText}>{fieldErrors.productCode}</small>}
        </label>

        <label className={styles.field}>
          <span>JAN</span>
          <input
            value={form.janCode}
            onChange={(e) => handleChange("janCode", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>13文字以内で入力してください。</small>
          {fieldErrors.janCode && <small className={styles.errorText}>{fieldErrors.janCode}</small>}
        </label>

        <label className={styles.field}>
          <span>商品名 *</span>
          <input
            value={form.productName}
            onChange={(e) => handleChange("productName", e.target.value)}
            className={styles.input}
          />
          {fieldErrors.productName && <small className={styles.errorText}>{fieldErrors.productName}</small>}
        </label>

        <label className={styles.field}>
          <span>売価 *</span>
          <input
            type="number"
            min={0}
            value={form.price}
            onChange={(e) => handleChange("price", e.target.value)}
            className={styles.input}
          />
          {fieldErrors.price && <small className={styles.errorText}>{fieldErrors.price}</small>}
        </label>

        <label className={styles.field}>
          <span>原価 *</span>
          <input
            type="number"
            min={0}
            value={form.cost}
            onChange={(e) => handleChange("cost", e.target.value)}
            className={styles.input}
          />
          {fieldErrors.cost && <small className={styles.errorText}>{fieldErrors.cost}</small>}
        </label>

        <label className={styles.field}>
          <span>カテゴリ</span>
          <select
            value={form.productCategoryCode}
            onChange={(e) => handleChange("productCategoryCode", e.target.value)}
            disabled={loadingCategories}
            className={styles.select}
          >
            {categories.length === 0 && <option value="">カテゴリなし</option>}
            {categories.map((c) => (
              <option key={c.productCategoryId} value={c.productCategoryCd}>
                {c.productCategoryName} ({c.productCategoryCd})
              </option>
            ))}
          </select>
          {fieldErrors.productCategoryCode && <small className={styles.errorText}>{fieldErrors.productCategoryCode}</small>}
        </label>

        {error && <div className={styles.errorBox}>{error}</div>}

        <div className={styles.actions}>
          <button
            type="button"
            onClick={() => router.push("/products")}
            className={styles.cancelButton}
          >
            キャンセル
          </button>
          <button
            type="submit"
            disabled={submitting || loadingCategories}
            className={styles.submitButton}
          >
            {submitting ? "作成中..." : "作成"}
          </button>
        </div>
      </form>
    </main>
  );
}
