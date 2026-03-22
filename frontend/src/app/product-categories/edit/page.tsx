"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import {
  getProductCategoryById,
  updateProductCategory,
  type UpdateProductCategoryRequest,
} from "../../lib/api/productCategories";
import { validateProductCategory, type ProductCategoryFieldErrors } from "../../lib/validators/productCategoryValidator";
import styles from "./page.module.css";

export default function EditProductCategoryPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = searchParams.get("id") ?? "";

  const [form, setForm] = useState<UpdateProductCategoryRequest>({
    productCategoryCd: "",
    productCategoryName: "",
    isActive: true,
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<ProductCategoryFieldErrors>({});

  useEffect(() => {
    let cancelled = false;

    if (!id) {
      setError("商品カテゴリIDが指定されていません。");
      setLoading(false);
      return;
    }

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const item = await getProductCategoryById(id);

        if (!cancelled) {
          setForm({
            productCategoryCd: item.productCategoryCd,
            productCategoryName: item.productCategoryName,
            isActive: item.isActive,
          });
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "商品カテゴリの取得に失敗しました。");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleChange = (
    field: keyof UpdateProductCategoryRequest,
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

    if (!id) {
      setError("商品カテゴリIDが指定されていません。");
      return;
    }

    try {
      setSubmitting(true);
      await updateProductCategory(id, {
        productCategoryCd: form.productCategoryCd.trim(),
        productCategoryName: form.productCategoryName.trim(),
        isActive: form.isActive,
      });
      router.push("/product-categories");
    } catch (err) {
      setError(err instanceof Error ? err.message : "商品カテゴリの更新に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <main className={styles.page}>読み込み中...</main>;
  }

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>商品カテゴリ編集</h1>
      <p className={styles.description}>
        商品カテゴリ情報を更新します。表示順は一覧画面でドラッグして変更します。
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
            {submitting ? "更新中..." : "更新"}
          </button>
        </div>
      </form>
    </main>
  );
}
