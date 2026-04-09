import { validation } from "@/lib/messages";

type ProductCategoryFormFields = {
  productCategoryCode: string;
  categoryAbbreviation: string;
  productCategoryName: string;
};

export type ProductCategoryFieldErrors = Partial<Record<keyof ProductCategoryFormFields, string>>;

export function validateProductCategory(form: ProductCategoryFormFields): ProductCategoryFieldErrors {
  const errors: ProductCategoryFieldErrors = {};

  if (!form.productCategoryCode.trim()) {
    errors.productCategoryCode = validation.required("商品カテゴリコード");
  } else if (!/^\d+$/.test(form.productCategoryCode.trim())) {
    errors.productCategoryCode = validation.digitsOnly("商品カテゴリコード");
  } else if (form.productCategoryCode.trim().length > 3) {
    errors.productCategoryCode = validation.maxLength("商品カテゴリコード", 3);
  }

  if (!form.categoryAbbreviation.trim()) {
    errors.categoryAbbreviation = validation.required("カテゴリ略称");
  } else if (!/^[A-Za-z]{2,5}$/.test(form.categoryAbbreviation.trim())) {
    errors.categoryAbbreviation = validation.alphaRange("カテゴリ略称", 2, 5);
  }

  if (!form.productCategoryName.trim()) {
    errors.productCategoryName = validation.required("商品カテゴリ名");
  } else if (form.productCategoryName.trim().length > 30) {
    errors.productCategoryName = validation.maxLength("商品カテゴリ名", 30);
  }

  return errors;
}
