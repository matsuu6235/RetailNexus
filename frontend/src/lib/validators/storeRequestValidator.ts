import { validation } from "@/lib/messages";

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
    errors.fromStoreId = validation.required("依頼元");
  }

  if (!form.toStoreId) {
    errors.toStoreId = validation.required("依頼先");
  }

  if (form.fromStoreId && form.toStoreId && form.fromStoreId === form.toStoreId) {
    errors.toStoreId = validation.sameStore;
  }

  if (!form.requestDate) {
    errors.requestDate = validation.required("依頼日");
  }

  if (form.note && form.note.length > 500) {
    errors.note = validation.maxLength("備考", 500);
  }

  return errors;
}

export function validateStoreRequestDetail(detail: DetailFormFields): DetailFieldErrors {
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

  return errors;
}
