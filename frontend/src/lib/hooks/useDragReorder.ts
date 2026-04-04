import { useState, useCallback } from "react";

function moveItem<T>(items: T[], fromId: string, toId: string, getId: (item: T) => string): T[] {
  const fromIndex = items.findIndex((item) => getId(item) === fromId);
  const toIndex = items.findIndex((item) => getId(item) === toId);
  if (fromIndex < 0 || toIndex < 0 || fromIndex === toIndex) return items;
  const next = [...items];
  const [moved] = next.splice(fromIndex, 1);
  next.splice(toIndex, 0, moved);
  return next;
}

function normalizeDisplayOrder<T extends { displayOrder: number }>(items: T[]): T[] {
  return items.map((item, index) => ({ ...item, displayOrder: index + 1 }));
}

interface UseDragReorderOptions<T> {
  getId: (item: T) => string;
  items: T[];
  setItems: React.Dispatch<React.SetStateAction<T[]>>;
  originalOrderIds: string[];
  setOriginalOrderIds: React.Dispatch<React.SetStateAction<string[]>>;
  reorderFn: (ids: string[]) => Promise<unknown>;
}

export function useDragReorder<T extends { displayOrder: number }>({
  getId,
  items,
  setItems,
  originalOrderIds,
  setOriginalOrderIds,
  reorderFn,
}: UseDragReorderOptions<T>) {
  const [draggingId, setDraggingId] = useState<string | null>(null);
  const [dragOverId, setDragOverId] = useState<string | null>(null);
  const [savingOrder, setSavingOrder] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isDirty =
    items.length > 0 &&
    items.map(getId).join(",") !== originalOrderIds.join(",");

  const onSaveOrder = useCallback(async () => {
    try {
      setSavingOrder(true);
      setError(null);
      await reorderFn(items.map(getId));
      const normalized = normalizeDisplayOrder(items);
      setItems(normalized);
      setOriginalOrderIds(normalized.map(getId));
    } catch (e) {
      setError(e instanceof Error ? e.message : "表示順の保存に失敗しました。");
    } finally {
      setSavingOrder(false);
    }
  }, [items, getId, reorderFn, setItems, setOriginalOrderIds]);

  const onResetOrder = useCallback(() => {
    const orderMap = new Map(originalOrderIds.map((id, index) => [id, index]));
    const resetItems = [...items].sort(
      (a, b) => (orderMap.get(getId(a)) ?? 0) - (orderMap.get(getId(b)) ?? 0)
    );
    setItems(normalizeDisplayOrder(resetItems));
    setDraggingId(null);
    setDragOverId(null);
  }, [items, originalOrderIds, getId, setItems]);

  const getDragHandleProps = useCallback((itemId: string) => ({
    draggable: true,
    onDragStart: (e: React.DragEvent) => {
      setDraggingId(itemId);
      e.dataTransfer.effectAllowed = "move" as const;
      e.dataTransfer.setData("text/plain", itemId);
    },
    onDragEnd: () => {
      setDraggingId(null);
      setDragOverId(null);
    },
  }), []);

  const getRowDragProps = useCallback((itemId: string) => ({
    onDragOver: (e: React.DragEvent) => {
      if (!draggingId) return;
      e.preventDefault();
      if (dragOverId !== itemId) setDragOverId(itemId);
    },
    onDrop: (e: React.DragEvent) => {
      if (!draggingId) return;
      e.preventDefault();
      if (draggingId !== itemId) {
        setItems((current) => normalizeDisplayOrder(moveItem(current, draggingId, itemId, getId)));
      }
      setDraggingId(null);
      setDragOverId(null);
    },
  }), [draggingId, dragOverId, getId, setItems]);

  return {
    draggingId,
    dragOverId,
    isDirty,
    savingOrder,
    error: error,
    onSaveOrder,
    onResetOrder,
    getDragHandleProps,
    getRowDragProps,
  };
}
