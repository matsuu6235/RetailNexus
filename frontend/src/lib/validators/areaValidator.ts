import { validation } from "@/lib/messages";

type AreaFormFields = {
  areaCode: string;
  areaName: string;
};

export type AreaFieldErrors = Partial<Record<keyof AreaFormFields, string>>;

export function validateArea(form: AreaFormFields): AreaFieldErrors {
  const errors: AreaFieldErrors = {};

  if (!form.areaCode.trim()) {
    errors.areaCode = validation.required("エリアコード");
  } else if (!/^\d+$/.test(form.areaCode.trim())) {
    errors.areaCode = validation.digitsOnly("エリアコード");
  } else if (form.areaCode.trim().length > 2) {
    errors.areaCode = validation.maxLength("エリアコード", 2);
  }

  if (!form.areaName.trim()) {
    errors.areaName = validation.required("エリア名");
  } else if (form.areaName.trim().length > 20) {
    errors.areaName = validation.maxLength("エリア名", 20);
  }

  return errors;
}
