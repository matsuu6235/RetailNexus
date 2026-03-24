type ProductCategoryFormFields = {
  productCategoryCd: string;
  categoryAbbreviation: string;
  productCategoryName: string;
};

export type ProductCategoryFieldErrors = Partial<Record<keyof ProductCategoryFormFields, string>>;

export function validateProductCategory(form: ProductCategoryFormFields): ProductCategoryFieldErrors {
  const errors: ProductCategoryFieldErrors = {};

  if (!form.productCategoryCd.trim()) {
    errors.productCategoryCd = "商品カテゴリコードは必須です。";
  } else if (!/^\d+$/.test(form.productCategoryCd.trim())) {
    errors.productCategoryCd = "商品カテゴリコードは数字のみ入力できます。";
  } else if (form.productCategoryCd.trim().length > 3) {
    errors.productCategoryCd = "商品カテゴリコードは3文字以内で入力してください。";
  }

  if (!form.categoryAbbreviation.trim()) {
    errors.categoryAbbreviation = "カテゴリ略称は必須です。";
  } else if (!/^[A-Za-z]{2,5}$/.test(form.categoryAbbreviation.trim())) {
    errors.categoryAbbreviation = "カテゴリ略称は英字2〜5文字で入力してください。";
  }

  if (!form.productCategoryName.trim()) {
    errors.productCategoryName = "商品カテゴリ名は必須です。";
  } else if (form.productCategoryName.trim().length > 30) {
    errors.productCategoryName = "商品カテゴリ名は30文字以内で入力してください。";
  }

  return errors;
}
