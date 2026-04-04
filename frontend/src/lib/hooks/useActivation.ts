import { useEffect, useState, useCallback } from "react";
import { hasPermission } from "@/services/authService";
import { fallback } from "@/lib/messages";

interface UseActivationOptions {
  permissionCode: string;
  initialIsActive?: boolean;
  changeFn: (id: string, isActive: boolean) => Promise<unknown>;
  editId?: string;
}

export function useActivation({ permissionCode, initialIsActive = true, changeFn, editId }: UseActivationOptions) {
  const [canDelete, setCanDelete] = useState(false);
  const [currentIsActive, setCurrentIsActive] = useState(initialIsActive);
  const [changingActivation, setChangingActivation] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setCanDelete(hasPermission(permissionCode));
  }, [permissionCode]);

  useEffect(() => {
    setCurrentIsActive(initialIsActive);
  }, [initialIsActive]);

  const toggle = useCallback(async () => {
    if (!editId) return;
    try {
      setChangingActivation(true);
      setError(null);
      await changeFn(editId, !currentIsActive);
      setCurrentIsActive(!currentIsActive);
    } catch (err) {
      setError(err instanceof Error ? err.message : fallback.activationFailed);
    } finally {
      setChangingActivation(false);
    }
  }, [editId, currentIsActive, changeFn]);

  return { canDelete, currentIsActive, changingActivation, error, toggle };
}
