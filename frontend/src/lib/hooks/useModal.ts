import { useState, useCallback } from "react";

export function useModal() {
  const [modalMode, setModalMode] = useState<"create" | "edit" | null>(null);
  const [editId, setEditId] = useState<string | null>(null);

  const openCreate = useCallback(() => {
    setModalMode("create");
    setEditId(null);
  }, []);

  const openEdit = useCallback((id: string) => {
    setModalMode("edit");
    setEditId(id);
  }, []);

  const close = useCallback(() => {
    setModalMode(null);
    setEditId(null);
  }, []);

  return { modalMode, editId, openCreate, openEdit, close };
}
