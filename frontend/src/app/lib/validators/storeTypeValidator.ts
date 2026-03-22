type StoreTypeFormFields = {
  storeTypeCd: string;
  storeTypeName: string;
};

export type StoreTypeFieldErrors = Partial<Record<keyof StoreTypeFormFields, string>>;

export function validateStoreType(form: StoreTypeFormFields): StoreTypeFieldErrors {
  const errors: StoreTypeFieldErrors = {};

  if (!form.storeTypeCd.trim()) {
    errors.storeTypeCd = "店舗種別コードは必須です。";
  } else if (!/^\d+$/.test(form.storeTypeCd.trim())) {
    errors.storeTypeCd = "店舗種別コードは数字のみ入力できます。";
  } else if (form.storeTypeCd.trim().length > 2) {
    errors.storeTypeCd = "店舗種別コードは2文字以内で入力してください。";
  }

  if (!form.storeTypeName.trim()) {
    errors.storeTypeName = "店舗種別名は必須です。";
  } else if (form.storeTypeName.trim().length > 50) {
    errors.storeTypeName = "店舗種別名は50文字以内で入力してください。";
  }

  return errors;
}
