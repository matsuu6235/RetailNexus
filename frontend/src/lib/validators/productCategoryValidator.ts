type ProductCategoryFormFields = {
  productCategoryCd: string;
  productCategoryName: string;
};

export type ProductCategoryFieldErrors = Partial<Record<keyof ProductCategoryFormFields, string>>;

export function validateProductCategory(form: ProductCategoryFormFields): ProductCategoryFieldErrors {
  const errors: ProductCategoryFieldErrors = {};

  if (!form.productCategoryCd.trim()) {
    errors.productCategoryCd = "商品カテゴリコードは必須です。";
  } else if (!/^\d+$/.test(form.productCategoryCd.trim())) {
    errors.productCategoryCd = "商品カテゴリコードは数字のみ入力できます。";
  } else if (form.productCategoryCd.trim().length > 30) {
    errors.productCategoryCd = "商品カテゴリコードは30文字以内で入力してください。";
  }

  if (!form.productCategoryName.trim()) {
    errors.productCategoryName = "商品カテゴリ名は必須です。";
  } else if (form.productCategoryName.trim().length > 100) {
    errors.productCategoryName = "商品カテゴリ名は100文字以内で入力してください。";
  }

  return errors;
}
