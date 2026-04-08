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
import styles from "@/components/modal/FormModal.module.css";

export default function ProductCategoryForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<CreateProductCategoryRequest, ProductCategoryFieldErrors>({
      mode,
      editId,
      initialForm: { productCategoryCd: "", categoryAbbreviation: "", productCategoryName: "" },
      entityName: "商品カテゴリ",
      validator: (f) => validateProductCategory(f),
      load: async (id) => {
        if (!id) return undefined;
        const item = await getProductCategoryById(id);
        return {
          form: {
            productCategoryCd: item.productCategoryCd,
            categoryAbbreviation: item.categoryAbbreviation,
            productCategoryName: item.productCategoryName,
          },
          isActive: item.isActive,
        };
      },
      save: async (f) => {
        const payload = {
          productCategoryCd: f.productCategoryCd.trim(),
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
        <input value={form.productCategoryCd} onChange={(e) => handleChange("productCategoryCd", e.target.value as string)} className={styles.input} />
        <small className={styles.hint}>数字3文字以内で入力してください。</small>
        {fieldErrors.productCategoryCd && <small className={styles.errorText}>{fieldErrors.productCategoryCd}</small>}
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
