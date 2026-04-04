import { validation } from "@/lib/messages";

type AreaFormFields = {
  areaCd: string;
  areaName: string;
};

export type AreaFieldErrors = Partial<Record<keyof AreaFormFields, string>>;

export function validateArea(form: AreaFormFields): AreaFieldErrors {
  const errors: AreaFieldErrors = {};

  if (!form.areaCd.trim()) {
    errors.areaCd = validation.required("エリアコード");
  } else if (!/^\d+$/.test(form.areaCd.trim())) {
    errors.areaCd = validation.digitsOnly("エリアコード");
  } else if (form.areaCd.trim().length > 2) {
    errors.areaCd = validation.maxLength("エリアコード", 2);
  }

  if (!form.areaName.trim()) {
    errors.areaName = validation.required("エリア名");
  } else if (form.areaName.trim().length > 20) {
    errors.areaName = validation.maxLength("エリア名", 20);
  }

  return errors;
}
