export type StoreRequestFormFields = {
  fromStoreId: string;
  toStoreId: string;
  requestDate: string;
  desiredDeliveryDate: string;
  note: string;
};

export type DetailFormFields = {
  productId: string;
  quantity: string;
};

export type StoreRequestFieldErrors = Partial<Record<keyof StoreRequestFormFields, string>>;

export type DetailFieldErrors = Partial<Record<keyof DetailFormFields, string>>;

export function validateStoreRequestHeader(form: StoreRequestFormFields): StoreRequestFieldErrors {
  const errors: StoreRequestFieldErrors = {};

  if (!form.fromStoreId) {
    errors.fromStoreId = "依頼元は必須です。";
  }

  if (!form.toStoreId) {
    errors.toStoreId = "依頼先は必須です。";
  }

  if (form.fromStoreId && form.toStoreId && form.fromStoreId === form.toStoreId) {
    errors.toStoreId = "依頼元と依頼先は異なる店舗を選択してください。";
  }

  if (!form.requestDate) {
    errors.requestDate = "依頼日は必須です。";
  }

  if (form.note && form.note.length > 500) {
    errors.note = "備考は500文字以内で入力してください。";
  }

  return errors;
}

export function validateStoreRequestDetail(detail: DetailFormFields): DetailFieldErrors {
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

  return errors;
}
