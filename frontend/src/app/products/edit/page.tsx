"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getProductById, updateProduct, type UpdateProductRequest } from "@/lib/api/products";
import { getAllProductCategories } from "@/lib/api/productCategories";
import type { ProductCategory } from "@/types/productCategories";
import { validateUpdateProduct, type UpdateProductFieldErrors } from "@/lib/validators/productValidator";
import styles from "./page.module.css";

export default function EditProductPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const productId = searchParams.get("id") ?? "";

  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [productCode, setProductCode] = useState("");
  const [form, setForm] = useState<UpdateProductRequest>({
    janCode: "",
    productName: "",
    price: 0,
    cost: 0,
    productCategoryCode: "",
    isActive: true,
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<UpdateProductFieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    if (!productId) {
      setError("商品IDが指定されていません。");
      setLoading(false);
      return () => { cancelled = true; };
    }

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const [product, categoryItems] = await Promise.all([
          getProductById(productId),
          getAllProductCategories({ isActive: "active" }),
        ]);

        if (!cancelled) {
          setProductCode(product.productCode);
          setForm({
            janCode: product.janCode,
            productName: product.productName,
            price: product.price,
            cost: product.cost,
            productCategoryCode: product.productCategoryCode,
            isActive: product.isActive,
          });
          setCategories(categoryItems);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "商品情報の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    })();

    return () => { cancelled = true; };
  }, [productId]);

  const handleChange = (field: keyof UpdateProductRequest, value: string | boolean) => {
    const numericValue = field === "price" || field === "cost" ? (value === "" ? 0 : Number(value)) : value;
    const updatedForm = { ...form, [field]: numericValue };
    setForm(updatedForm);
    const errors = validateUpdateProduct(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof UpdateProductFieldErrors] }));
  };

  const validate = () => {
    const errors = validateUpdateProduct(form);
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;
    if (!productId) {
      setError("商品IDが指定されていません。");
      return;
    }

    try {
      setSubmitting(true);
      await updateProduct(productId, {
        janCode: form.janCode.trim(),
        productName: form.productName.trim(),
        price: form.price,
        cost: form.cost,
        productCategoryCode: form.productCategoryCode,
        isActive: form.isActive,
      });
      router.push("/products");
    } catch (err) {
      setError(err instanceof Error ? err.message : "商品更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <main className={styles.page}>読み込み中...</main>;
  }

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>商品編集</h1>
      <p className={styles.description}>商品情報を更新します。</p>

      <form onSubmit={onSubmit} className={styles.form}>
        <div className={styles.field}>
          <span>商品コード</span>
          <input
            value={productCode}
            readOnly
            className={styles.input}
            style={{ backgroundColor: "#f5f5f5" }}
          />
          <small className={styles.hint}>商品コードは変更できません。</small>
        </div>

        <label className={styles.field}>
          <span>JAN</span>
          <input
            value={form.janCode}
            onChange={(e) => handleChange("janCode", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>13桁の数字で入力してください。</small>
          {fieldErrors.janCode && <small className={styles.errorText}>{fieldErrors.janCode}</small>}
        </label>

        <label className={styles.field}>
          <span>商品名 *</span>
          <input
            value={form.productName}
            onChange={(e) => handleChange("productName", e.target.value)}
            className={styles.input}
          />
          <small className={styles.hint}>200文字以内で入力してください。</small>
          {fieldErrors.productName && <small className={styles.errorText}>{fieldErrors.productName}</small>}
        </label>

        <label className={styles.field}>
          <span>売価 *</span>
          <input
            type="number"
            min={0}
            value={form.price}
            onFocus={(e) => e.target.select()}
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
            onFocus={(e) => e.target.select()}
            onChange={(e) => handleChange("cost", e.target.value)}
            className={styles.input}
          />
          {fieldErrors.cost && <small className={styles.errorText}>{fieldErrors.cost}</small>}
        </label>

        <label className={styles.field}>
          <span>カテゴリ *</span>
          <select
            value={form.productCategoryCode}
            onChange={(e) => handleChange("productCategoryCode", e.target.value)}
            className={styles.select}
          >
            <option value="">選択してください</option>
            {categories.map((c) => (
              <option key={c.productCategoryId} value={c.productCategoryCd}>
                {c.productCategoryName} ({c.productCategoryCd})
              </option>
            ))}
          </select>
          {fieldErrors.productCategoryCode && <small className={styles.errorText}>{fieldErrors.productCategoryCode}</small>}
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
          <button type="button" onClick={() => router.push("/products")} className={styles.cancelButton}>
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
