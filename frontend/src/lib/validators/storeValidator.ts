type StoreFormFields = {
  storeName: string;
  areaId: string;
  storeTypeId: string;
};

export type StoreFieldErrors = Partial<Record<keyof StoreFormFields, string>>;

export function validateStore(form: StoreFormFields): StoreFieldErrors {
  const errors: StoreFieldErrors = {};

  if (!form.storeName.trim()) {
    errors.storeName = "店舗名は必須です。";
  } else if (form.storeName.trim().length > 50) {
    errors.storeName = "店舗名は50文字以内で入力してください。";
  }

  if (!form.areaId) {
    errors.areaId = "エリアは必須です。";
  }

  if (!form.storeTypeId) {
    errors.storeTypeId = "店舗種別は必須です。";
  }

  return errors;
}
