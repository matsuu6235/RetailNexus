import { useEffect, useState } from "react";
import { useActivation } from "@/lib/hooks/useActivation";
import { fallback } from "@/lib/messages";

export type MasterFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

type UseMasterFormOptions<TForm, TFieldErrors extends Partial<Record<string, string>>> = {
  mode: "create" | "edit";
  editId?: string;
  initialForm: TForm;
  entityName: string;
  validator: (form: TForm, mode: "create" | "edit") => TFieldErrors;
  load: (editId?: string) => Promise<{ form: TForm; isActive: boolean } | undefined>;
  save: (form: TForm) => Promise<void>;
  onSave: () => void;
  activation: {
    permissionCode: string;
    changeFn: (id: string, isActive: boolean) => Promise<unknown>;
  };
};

export function useMasterForm<TForm, TFieldErrors extends Partial<Record<string, string>>>({
  mode,
  editId,
  initialForm,
  entityName,
  validator,
  load,
  save,
  onSave,
  activation: activationConfig,
}: UseMasterFormOptions<TForm, TFieldErrors>) {
  const [form, setForm] = useState<TForm>(initialForm);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<TFieldErrors>({} as TFieldErrors);
  const [fetchedIsActive, setFetchedIsActive] = useState(true);

  const activation = useActivation({
    permissionCode: activationConfig.permissionCode,
    initialIsActive: fetchedIsActive,
    changeFn: activationConfig.changeFn,
    editId,
  });

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await load(mode === "edit" ? editId : undefined);

        if (!cancelled && result) {
          setForm(result.form);
          setFetchedIsActive(result.isActive);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : fallback.fetchFailed(entityName));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

  const handleChange = (field: keyof TForm & string, value: TForm[keyof TForm]) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validator(updatedForm, mode);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof TFieldErrors] }));
  };

  const handleSubmit = async (e?: React.FormEvent) => {
    e?.preventDefault();
    setError(null);

    const errors = validator(form, mode);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;

    try {
      setSubmitting(true);
      await save(form);
      onSave();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : mode === "create"
            ? fallback.createFailed(entityName)
            : fallback.updateFailed(entityName)
      );
    } finally {
      setSubmitting(false);
    }
  };

  return {
    form,
    setForm,
    loading,
    submitting,
    error,
    fieldErrors,
    activation,
    handleChange,
    handleSubmit,
  };
}
