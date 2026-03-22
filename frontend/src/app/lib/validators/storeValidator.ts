type StoreFormFields = {
  storeCd: string;
  storeName: string;
  areaId: string;
  storeTypeId: string;
};

export type StoreFieldErrors = Partial<Record<keyof StoreFormFields, string>>;

export function validateStore(form: StoreFormFields): StoreFieldErrors {
  const errors: StoreFieldErrors = {};

  if (!form.storeCd.trim()) {
    errors.storeCd = "店舗コードは必須です。";
  } else if (!/^\d+$/.test(form.storeCd.trim())) {
    errors.storeCd = "店舗コードは数字のみ入力できます。";
  } else if (form.storeCd.trim().length > 6) {
    errors.storeCd = "店舗コードは6文字以内で入力してください。";
  }

  if (!form.storeName.trim()) {
    errors.storeName = "店舗名は必須です。";
  } else if (form.storeName.trim().length > 100) {
    errors.storeName = "店舗名は100文字以内で入力してください。";
  }

  if (!form.areaId) {
    errors.areaId = "エリアは必須です。";
  }

  if (!form.storeTypeId) {
    errors.storeTypeId = "店舗種別は必須です。";
  }

  return errors;
}
