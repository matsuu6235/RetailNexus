import { validation } from "@/lib/messages";

type StoreTypeFormFields = {
  storeTypeCd: string;
  storeTypeName: string;
};

export type StoreTypeFieldErrors = Partial<Record<keyof StoreTypeFormFields, string>>;

export function validateStoreType(form: StoreTypeFormFields): StoreTypeFieldErrors {
  const errors: StoreTypeFieldErrors = {};

  if (!form.storeTypeCd.trim()) {
    errors.storeTypeCd = validation.required("店舗種別コード");
  } else if (!/^\d+$/.test(form.storeTypeCd.trim())) {
    errors.storeTypeCd = validation.digitsOnly("店舗種別コード");
  } else if (form.storeTypeCd.trim().length > 2) {
    errors.storeTypeCd = validation.maxLength("店舗種別コード", 2);
  }

  if (!form.storeTypeName.trim()) {
    errors.storeTypeName = validation.required("店舗種別名");
  } else if (form.storeTypeName.trim().length > 20) {
    errors.storeTypeName = validation.maxLength("店舗種別名", 20);
  }

  return errors;
}
