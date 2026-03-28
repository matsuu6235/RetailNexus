type ProductFormFields = {
  janCode: string;
  productName: string;
  price: number;
  cost: number;
  productCategoryCode: string;
};

type UpdateProductFormFields = ProductFormFields;

export type ProductFieldErrors = Partial<Record<keyof ProductFormFields, string>>;
export type UpdateProductFieldErrors = Partial<Record<keyof UpdateProductFormFields, string>>;

export function validateProduct(form: ProductFormFields): ProductFieldErrors {
  const errors: ProductFieldErrors = {};

  if (form.janCode) {
    if (!/^\d+$/.test(form.janCode.trim())) {
      errors.janCode = "JANコードは数字のみ入力できます。";
    } else if (form.janCode.trim().length !== 13) {
      errors.janCode = "JANコードは13桁で入力してください。";
    }
  }

  if (!form.productName.trim()) {
    errors.productName = "商品名は必須です。";
  } else if (form.productName.trim().length > 200) {
    errors.productName = "商品名は200文字以内で入力してください。";
  }

  if (form.price < 0) {
    errors.price = "売価は0以上で入力してください。";
  }

  if (form.cost < 0) {
    errors.cost = "原価は0以上で入力してください。";
  }

  if (!form.productCategoryCode.trim()) {
    errors.productCategoryCode = "カテゴリは必須です。";
  }

  return errors;
}

export function validateUpdateProduct(form: UpdateProductFormFields): UpdateProductFieldErrors {
  return validateProduct(form);
}
