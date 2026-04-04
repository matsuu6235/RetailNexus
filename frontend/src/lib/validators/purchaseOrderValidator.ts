import { validation } from "@/lib/messages";

export type PurchaseOrderFormFields = {
  supplierId: string;
  storeId: string;
  orderDate: string;
  desiredDeliveryDate: string;
  note: string;
};

export type DetailFormFields = {
  productId: string;
  quantity: string;
  unitPrice: string;
};

export type PurchaseOrderFieldErrors = Partial<Record<keyof PurchaseOrderFormFields, string>>;

export type DetailFieldErrors = Partial<Record<keyof DetailFormFields, string>>;

export function validatePurchaseOrderHeader(form: PurchaseOrderFormFields): PurchaseOrderFieldErrors {
  const errors: PurchaseOrderFieldErrors = {};

  if (!form.supplierId) {
    errors.supplierId = validation.required("仕入先");
  }

  if (!form.storeId) {
    errors.storeId = validation.required("発注元");
  }

  if (!form.orderDate) {
    errors.orderDate = validation.required("発注日");
  }

  if (form.note && form.note.length > 500) {
    errors.note = validation.maxLength("備考", 500);
  }

  return errors;
}

export function validateDetail(detail: DetailFormFields): DetailFieldErrors {
  const errors: DetailFieldErrors = {};

  if (!detail.productId) {
    errors.productId = validation.required("商品");
  }

  const qty = Number(detail.quantity);
  if (!detail.quantity || isNaN(qty)) {
    errors.quantity = validation.required("数量");
  } else if (qty <= 0 || !Number.isInteger(qty)) {
    errors.quantity = validation.greaterThan("数量", 1);
  }

  const price = Number(detail.unitPrice);
  if (detail.unitPrice === "" || isNaN(price)) {
    errors.unitPrice = validation.required("単価");
  } else if (price < 0) {
    errors.unitPrice = validation.minValue("単価", 0);
  }

  return errors;
}
