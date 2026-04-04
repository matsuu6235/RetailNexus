import { validation } from "@/lib/messages";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

type SupplierFormFields = {
  supplierName: string;
  phoneNumber?: string;
  email?: string;
};

export type SupplierFieldErrors = Partial<Record<keyof SupplierFormFields, string>>;

export function validateSupplier(form: SupplierFormFields): SupplierFieldErrors {
  const errors: SupplierFieldErrors = {};

  if (!form.supplierName.trim()) {
    errors.supplierName = validation.required("仕入先名");
  } else if (form.supplierName.trim().length > 50) {
    errors.supplierName = validation.maxLength("仕入先名", 50);
  }

  if (form.phoneNumber) {
    if (!/^[\d-]+$/.test(form.phoneNumber)) {
      errors.phoneNumber = validation.phoneFormat("電話番号");
    } else if (form.phoneNumber.length > 20) {
      errors.phoneNumber = validation.maxLength("電話番号", 20);
    }
  }

  if (form.email) {
    if (form.email.length > 255) {
      errors.email = validation.maxLength("メールアドレス", 255);
    } else if (!EMAIL_REGEX.test(form.email)) {
      errors.email = validation.emailFormat;
    }
  }

  return errors;
}
