const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

type SupplierFormFields = {
  supplierCode: string;
  supplierName: string;
  phoneNumber?: string;
  email?: string;
};

export type SupplierFieldErrors = Partial<Record<keyof SupplierFormFields, string>>;

export function validateSupplier(form: SupplierFormFields): SupplierFieldErrors {
  const errors: SupplierFieldErrors = {};

  if (!form.supplierCode.trim()) {
    errors.supplierCode = "仕入先コードは必須です。";
  } else if (form.supplierCode.trim().length > 30) {
    errors.supplierCode = "仕入先コードは30文字以内で入力してください。";
  }

  if (!form.supplierName.trim()) {
    errors.supplierName = "仕入先名は必須です。";
  } else if (form.supplierName.trim().length > 100) {
    errors.supplierName = "仕入先名は100文字以内で入力してください。";
  }

  if (form.phoneNumber) {
    if (!/^[\d-]+$/.test(form.phoneNumber)) {
      errors.phoneNumber = "電話番号は数字とハイフン（-）のみ入力できます。";
    } else if (form.phoneNumber.length > 20) {
      errors.phoneNumber = "電話番号は20文字以内で入力してください。";
    }
  }

  if (form.email) {
    if (form.email.length > 255) {
      errors.email = "メールアドレスは255文字以内で入力してください。";
    } else if (!EMAIL_REGEX.test(form.email)) {
      errors.email = "メールアドレスの形式が正しくありません。";
    }
  }

  return errors;
}
