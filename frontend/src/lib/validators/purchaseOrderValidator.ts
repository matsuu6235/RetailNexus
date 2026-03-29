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
    errors.supplierId = "仕入先は必須です。";
  }

  if (!form.storeId) {
    errors.storeId = "発注元は必須です。";
  }

  if (!form.orderDate) {
    errors.orderDate = "発注日は必須です。";
  }

  if (form.note && form.note.length > 500) {
    errors.note = "備考は500文字以内で入力してください。";
  }

  return errors;
}

export function validateDetail(detail: DetailFormFields): DetailFieldErrors {
  const errors: DetailFieldErrors = {};

  if (!detail.productId) {
    errors.productId = "商品は必須です。";
  }

  const qty = Number(detail.quantity);
  if (!detail.quantity || isNaN(qty)) {
    errors.quantity = "数量は必須です。";
  } else if (qty <= 0 || !Number.isInteger(qty)) {
    errors.quantity = "数量は1以上の整数で入力してください。";
  }

  const price = Number(detail.unitPrice);
  if (detail.unitPrice === "" || isNaN(price)) {
    errors.unitPrice = "単価は必須です。";
  } else if (price < 0) {
    errors.unitPrice = "単価は0以上で入力してください。";
  }

  return errors;
}
