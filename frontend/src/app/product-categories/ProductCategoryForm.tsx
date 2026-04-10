"use client";

import {
  createProductCategory,
  getProductCategoryById,
  updateProductCategory,
  changeProductCategoryActivation,
  type CreateProductCategoryRequest,
} from "@/lib/api/productCategories";
import { validateProductCategory, type ProductCategoryFieldErrors } from "@/lib/validators/productCategoryValidator";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import ActivationFieldset from "@/components/form/ActivationFieldset";
import styles from "@/components/modal/FormModal.module.css";

export default function ProductCategoryForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateProductCategoryRequest, ProductCategoryFieldErrors>({
      mode,
      editId,
      initialForm: { productCategoryCode: "", categoryAbbreviation: "", productCategoryName: "" },
      entityName: "商品カテゴリ",
      validator: (f) => validateProductCategory(f),
      load: async (id) => {
        if (!id) return undefined;
        const item = await getProductCategoryById(id);
        return {
          form: {
            productCategoryCode: item.productCategoryCode,
            categoryAbbreviation: item.categoryAbbreviation,
            productCategoryName: item.productCategoryName,
          },
          isActive: item.isActive,
        };
      },
      save: async (f) => {
        const payload = {
          productCategoryCode: f.productCategoryCode.trim(),
          categoryAbbreviation: f.categoryAbbreviation.trim(),
          productCategoryName: f.productCategoryName.trim(),
        };
        if (mode === "create") await createProductCategory(payload);
        else await updateProductCategory(editId!, payload);
      },
      onSave,
      activation: { permissionCode: "product-categories.delete", changeFn: changeProductCategoryActivation },
    });

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>商品カテゴリコード *</span>
        <input value={form.productCategoryCode} onChange={(e) => handleChange("productCategoryCode", e.target.value as string)} className={styles.input} />
        <small className={styles.hint}>数字3文字以内で入力してください。</small>
        {fieldErrors.productCategoryCode && <small className={styles.errorText}>{fieldErrors.productCategoryCode}</small>}
      </label>

      <label className={styles.field}>
        <span>カテゴリ略称 *</span>
        <input value={form.categoryAbbreviation} onChange={(e) => handleChange("categoryAbbreviation", e.target.value as string)} className={styles.input} />
        <small className={styles.hint}>英字2〜5文字で入力してください。例: FD → 商品コード FD-000001</small>
        {fieldErrors.categoryAbbreviation && <small className={styles.errorText}>{fieldErrors.categoryAbbreviation}</small>}
      </label>

      <label className={styles.field}>
        <span>商品カテゴリ名 *</span>
        <input value={form.productCategoryName} onChange={(e) => handleChange("productCategoryName", e.target.value as string)} className={styles.input} />
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
        <ActivationFieldset currentIsActive={activation.currentIsActive} changingActivation={activation.changingActivation} toggle={activation.toggle} />
      )}
    </form>
  );
}
