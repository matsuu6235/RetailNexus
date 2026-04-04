import { useState, useMemo, useCallback } from "react";

interface UseDetailRowsOptions<TFields, TErrors> {
  emptyRow: TFields;
  validateRow: (row: TFields) => TErrors;
  onProductSelect?: (productId: string, row: TFields) => Partial<TFields>;
}

type DetailRow<TFields> = TFields & { key: number };

export function useDetailRows<
  TFields extends { productId: string },
  TErrors extends Record<string, string | undefined>,
>({ emptyRow, validateRow, onProductSelect }: UseDetailRowsOptions<TFields, TErrors>) {
  const [details, setDetails] = useState<DetailRow<TFields>[]>([
    { ...emptyRow, key: 1 },
  ]);
  const [detailErrors, setDetailErrors] = useState<Record<number, TErrors>>({});
  const [nextKey, setNextKey] = useState(2);

  const duplicateProductIds = useMemo(() => {
    const dupes = new Set<string>();
    const seen = new Map<string, number>();
    for (const d of details) {
      if (!d.productId) continue;
      if (seen.has(d.productId)) dupes.add(d.productId);
      seen.set(d.productId, d.key);
    }
    return dupes;
  }, [details]);

  const handleDetailChange = useCallback(
    (key: number, field: keyof TFields, value: string) => {
      let updates: Partial<TFields> = { [field]: value } as Partial<TFields>;

      if (field === "productId" && value) {
        const isDuplicate = details.some((d) => d.key !== key && d.productId === value);
        if (isDuplicate) {
          setDetailErrors((prev) => ({
            ...prev,
            [key]: { ...prev[key], productId: "この商品は既に追加されています。数量を変更してください。" } as TErrors,
          }));
          return;
        }
        if (onProductSelect) {
          const extra = onProductSelect(value, details.find((d) => d.key === key)!);
          updates = { ...updates, ...extra };
        }
      }

      setDetails((prev) => prev.map((d) => (d.key === key ? { ...d, ...updates } : d)));

      const row = details.find((d) => d.key === key);
      if (row) {
        const updated = { ...row, ...updates };
        const errors = validateRow(updated);
        setDetailErrors((prev) => ({
          ...prev,
          [key]: { ...prev[key], [field]: errors[field as keyof TErrors] } as TErrors,
        }));
      }
    },
    [details, validateRow, onProductSelect],
  );

  const addDetailRow = useCallback(() => {
    setDetails((prev) => [...prev, { ...emptyRow, key: nextKey }]);
    setNextKey((k) => k + 1);
  }, [emptyRow, nextKey]);

  const removeDetailRow = useCallback((key: number) => {
    setDetails((prev) => prev.filter((d) => d.key !== key));
    setDetailErrors((prev) => {
      const copy = { ...prev };
      delete copy[key];
      return copy;
    });
  }, []);

  return {
    details,
    setDetails,
    detailErrors,
    setDetailErrors,
    duplicateProductIds,
    handleDetailChange,
    addDetailRow,
    removeDetailRow,
  };
}
