type AreaFormFields = {
  areaCd: string;
  areaName: string;
};

export type AreaFieldErrors = Partial<Record<keyof AreaFormFields, string>>;

export function validateArea(form: AreaFormFields): AreaFieldErrors {
  const errors: AreaFieldErrors = {};

  if (!form.areaCd.trim()) {
    errors.areaCd = "エリアコードは必須です。";
  } else if (!/^\d+$/.test(form.areaCd.trim())) {
    errors.areaCd = "エリアコードは数字のみ入力できます。";
  } else if (form.areaCd.trim().length > 2) {
    errors.areaCd = "エリアコードは2文字以内で入力してください。";
  }

  if (!form.areaName.trim()) {
    errors.areaName = "エリア名は必須です。";
  } else if (form.areaName.trim().length > 20) {
    errors.areaName = "エリア名は20文字以内で入力してください。";
  }

  return errors;
}
