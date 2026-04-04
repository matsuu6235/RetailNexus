import { validation } from "@/lib/messages";

type StoreFormFields = {
  storeName: string;
  areaId: string;
  storeTypeId: string;
};

export type StoreFieldErrors = Partial<Record<keyof StoreFormFields, string>>;

export function validateStore(form: StoreFormFields): StoreFieldErrors {
  const errors: StoreFieldErrors = {};

  if (!form.storeName.trim()) {
    errors.storeName = validation.required("店舗名");
  } else if (form.storeName.trim().length > 50) {
    errors.storeName = validation.maxLength("店舗名", 50);
  }

  if (!form.areaId) {
    errors.areaId = validation.required("エリア");
  }

  if (!form.storeTypeId) {
    errors.storeTypeId = validation.required("店舗種別");
  }

  return errors;
}
