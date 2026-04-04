import { validation } from "@/lib/messages";

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
      errors.janCode = validation.digitsOnly("JANコード");
    } else if (form.janCode.trim().length !== 13) {
      errors.janCode = validation.exactLength("JANコード", 13);
    }
  }

  if (!form.productName.trim()) {
    errors.productName = validation.required("商品名");
  } else if (form.productName.trim().length > 200) {
    errors.productName = validation.maxLength("商品名", 200);
  }

  if (form.price < 0) {
    errors.price = validation.minValue("売価", 0);
  }

  if (form.cost < 0) {
    errors.cost = validation.minValue("原価", 0);
  }

  if (!form.productCategoryCode.trim()) {
    errors.productCategoryCode = validation.required("カテゴリ");
  }

  return errors;
}

export function validateUpdateProduct(form: UpdateProductFormFields): UpdateProductFieldErrors {
  return validateProduct(form);
}
