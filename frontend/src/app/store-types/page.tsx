"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getStoreTypes, reorderStoreTypes } from "../lib/api/storeTypes";
import type { StoreType } from "../types/storeTypes";
import styles from "./page.module.css";
import tableStyles from "../components/table/MasterTable.module.css";

function moveItem(items: StoreType[], fromId: string, toId: string): StoreType[] {
  const fromIndex = items.findIndex((item) => item.storeTypeId === fromId);
  const toIndex = items.findIndex((item) => item.storeTypeId === toId);

  if (fromIndex < 0 || toIndex < 0 || fromIndex === toIndex) return items;

  const next = [...items];
  const [moved] = next.splice(fromIndex, 1);
  next.splice(toIndex, 0, moved);
  return next;
}

function normalizeDisplayOrder(items: StoreType[]): StoreType[] {
  return items.map((item, index) => ({ ...item, displayOrder: index + 1 }));
}

export default function StoreTypesPage() {
  const router = useRouter();

  const [items, setItems] = useState<StoreType[]>([]);
  const [originalOrderIds, setOriginalOrderIds] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [savingOrder, setSavingOrder] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [codeInput, setCodeInput] = useState("");
  const [nameInput, setNameInput] = useState("");
  const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

  const [codeFilter, setCodeFilter] = useState("");
  const [nameFilter, setNameFilter] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

  const [draggingId, setDraggingId] = useState<string | null>(null);
  const [dragOverId, setDragOverId] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const response = await getStoreTypes();
        const normalized = normalizeDisplayOrder(response);

        if (!cancelled) {
          setItems(normalized);
          setOriginalOrderIds(normalized.map((item) => item.storeTypeId));
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "店舗種別一覧の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const filteredItems = items.filter((item) => {
    const matchCode = !codeFilter.trim() || item.storeTypeCd.includes(codeFilter.trim());
    const matchName = !nameFilter.trim() || item.storeTypeName.includes(nameFilter.trim());
    const matchStatus =
      isActiveFilter === "all" ||
      (isActiveFilter === "active" && item.isActive) ||
      (isActiveFilter === "inactive" && !item.isActive);

    return matchCode && matchName && matchStatus;
  });

  const isDirty =
    items.length > 0 &&
    items.map((item) => item.storeTypeId).join(",") !== originalOrderIds.join(",");

  const onSaveOrder = async () => {
    try {
      setSavingOrder(true);
      setError(null);

      await reorderStoreTypes(items.map((item) => item.storeTypeId));

      const normalized = normalizeDisplayOrder(items);
      setItems(normalized);
      setOriginalOrderIds(normalized.map((item) => item.storeTypeId));
    } catch (e) {
      setError(e instanceof Error ? e.message : "表示順の保存に失敗しました。");
    } finally {
      setSavingOrder(false);
    }
  };

  const onResetOrder = () => {
    const orderMap = new Map(originalOrderIds.map((id, index) => [id, index]));
    const resetItems = [...items].sort(
      (a, b) => (orderMap.get(a.storeTypeId) ?? 0) - (orderMap.get(b.storeTypeId) ?? 0)
    );

    setItems(normalizeDisplayOrder(resetItems));
    setDraggingId(null);
    setDragOverId(null);
  };

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>店舗種別マスタ</h1>
          <p className={styles.subtitle}>店舗種別の検索・編集・新規作成と表示順変更を行います。</p>
        </div>

        <button type="button" onClick={() => router.push("/store-types/new")} className={styles.primaryButton}>
          店舗種別新規作成
        </button>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗種別コード</span>
            <input value={codeInput} onChange={(e) => setCodeInput(e.target.value)} className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗種別名</span>
            <input value={nameInput} onChange={(e) => setNameInput(e.target.value)} className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>有効状態</span>
            <select
              value={isActiveInput}
              onChange={(e) => setIsActiveInput(e.target.value as "all" | "active" | "inactive")}
              className={styles.select}
            >
              <option value="all">すべて</option>
              <option value="active">有効</option>
              <option value="inactive">無効</option>
            </select>
          </label>
        </div>

        <div className={styles.searchActions}>
          <button
            type="button"
            onClick={() => {
              setCodeFilter(codeInput);
              setNameFilter(nameInput);
              setIsActiveFilter(isActiveInput);
            }}
            className={styles.searchButton}
          >
            検索
          </button>
        </div>
      </section>

      {isDirty && (
        <div className={styles.noticeBox}>
          <div>
            <strong>表示順が未保存です。</strong>
            <div className={styles.noticeText}>行をドラッグして並び順を変更し、「並び順を保存」を押してください。</div>
          </div>
          <div className={styles.noticeActions}>
            <button type="button" onClick={onResetOrder} className={styles.secondaryButton} disabled={savingOrder}>
              元に戻す
            </button>
            <button type="button" onClick={onSaveOrder} className={styles.saveButton} disabled={savingOrder}>
              {savingOrder ? "保存中..." : "並び順を保存"}
            </button>
          </div>
        </div>
      )}

      {loading && <p>読み込み中...</p>}
      {error && <div className={styles.errorBox}>{error}</div>}

      {!loading && !error && (
        <div className={tableStyles.wrapper}>
          <div className={tableStyles.summary}>
            <span>{filteredItems.length} 件</span>
            <span className={styles.dragHint}>左端のハンドルをドラッグすると表示順を変更できます。</span>
          </div>

          <div className={tableStyles.tableContainer}>
            <table className={tableStyles.table}>
              <thead className={tableStyles.thead}>
                <tr>
                  <th className={`${tableStyles.th} ${styles.handleHeader}`}>表示順</th>
                  <th className={tableStyles.th}>店舗種別コード</th>
                  <th className={tableStyles.th}>店舗種別名</th>
                  <th className={tableStyles.th}>表示順</th>
                  <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thStatus}`}>状態</th>
                  <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                </tr>
              </thead>
              <tbody>
                {filteredItems.length === 0 && (
                  <tr>
                    <td colSpan={6} className={tableStyles.empty}>データがありません</td>
                  </tr>
                )}

                {filteredItems.map((item, index) => {
                  const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd} ${
                    draggingId === item.storeTypeId ? styles.draggingRow : ""
                  } ${dragOverId === item.storeTypeId ? styles.dragOverRow : ""}`;

                  const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                  const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                  return (
                    <tr
                      key={item.storeTypeId}
                      className={rowClass}
                      onDragOver={(e) => {
                        if (!draggingId) return;
                        e.preventDefault();
                        if (dragOverId !== item.storeTypeId) setDragOverId(item.storeTypeId);
                      }}
                      onDrop={(e) => {
                        if (!draggingId) return;
                        e.preventDefault();
                        if (draggingId !== item.storeTypeId) {
                          setItems((current) => normalizeDisplayOrder(moveItem(current, draggingId, item.storeTypeId)));
                        }
                        setDraggingId(null);
                        setDragOverId(null);
                      }}
                    >
                      <td className={`${tableStyles.td} ${styles.handleCell}`}>
                        <button
                          type="button"
                          draggable
                          onDragStart={(e) => {
                            setDraggingId(item.storeTypeId);
                            e.dataTransfer.effectAllowed = "move";
                            e.dataTransfer.setData("text/plain", item.storeTypeId);
                          }}
                          onDragEnd={() => {
                            setDraggingId(null);
                            setDragOverId(null);
                          }}
                          className={styles.dragHandleButton}
                        >
                          <span className={styles.dragHandle}>≡</span>
                        </button>
                      </td>
                      <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{item.storeTypeCd}</td>
                      <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>{item.storeTypeName}</td>
                      <td className={`${tableStyles.td} ${tableStyles.tdNumber}`}>{item.displayOrder}</td>
                      <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                        <span className={statusClass}>
                          <span className={statusDotClass} />
                          {item.isActive ? "有効" : "無効"}
                        </span>
                      </td>
                      <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                        <button
                          type="button"
                          onClick={() => router.push(`/store-types/edit?id=${item.storeTypeId}`)}
                          className={tableStyles.editButton}
                        >
                          編集
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </main>
  );
}