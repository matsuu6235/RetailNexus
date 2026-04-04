"use client";

import { useEffect, useState } from "react";
import { getStoreTypes, reorderStoreTypes } from "@/lib/api/storeTypes";
import { fallback } from "@/lib/messages";
import type { StoreType } from "@/types/storeTypes";
import { useModal } from "@/lib/hooks/useModal";
import { useDragReorder } from "@/lib/hooks/useDragReorder";
import Modal from "@/components/modal/Modal";
import StoreTypeForm from "./StoreTypeForm";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const getStoreTypeId = (item: StoreType) => item.storeTypeId;

export default function StoreTypesPage() {
  const [items, setItems] = useState<StoreType[]>([]);
  const [originalOrderIds, setOriginalOrderIds] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [codeInput, setCodeInput] = useState("");
  const [nameInput, setNameInput] = useState("");
  const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

  const [codeFilter, setCodeFilter] = useState("");
  const [nameFilter, setNameFilter] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

  const modal = useModal();
  const drag = useDragReorder({
    getId: getStoreTypeId,
    items,
    setItems,
    originalOrderIds,
    setOriginalOrderIds,
    reorderFn: reorderStoreTypes,
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getStoreTypes();
      const normalized = response.map((item, index) => ({ ...item, displayOrder: index + 1 }));
      setItems(normalized);
      setOriginalOrderIds(normalized.map(getStoreTypeId));
    } catch (e) {
      setError(e instanceof Error ? e.message : fallback.listFetchFailed("店舗種別"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
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

  const handleSave = () => {
    modal.close();
    fetchData();
  };

  const displayError = error || drag.error;

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>店舗種別マスタ</h1>
          <p className={styles.subtitle}>店舗種別の検索・編集・新規作成と表示順変更を行います。</p>
        </div>

        <button
          type="button"
          onClick={modal.openCreate}
          className={styles.primaryButton}
        >
          店舗種別新規作成
        </button>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗種別コード</span>
            <input value={codeInput} onChange={(e) => setCodeInput(e.target.value)} placeholder="店舗種別コード" className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗種別名</span>
            <input value={nameInput} onChange={(e) => setNameInput(e.target.value)} placeholder="店舗種別名" className={styles.input} />
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

      {drag.isDirty && (
        <div className={styles.noticeBox}>
          <div>
            <strong>表示順が未保存です。</strong>
            <div className={styles.noticeText}>行をドラッグして並び順を変更し、「並び順を保存」を押してください。</div>
          </div>
          <div className={styles.noticeActions}>
            <button type="button" onClick={drag.onResetOrder} className={styles.secondaryButton} disabled={drag.savingOrder}>
              元に戻す
            </button>
            <button type="button" onClick={drag.onSaveOrder} className={styles.saveButton} disabled={drag.savingOrder}>
              {drag.savingOrder ? "保存中..." : "並び順を保存"}
            </button>
          </div>
        </div>
      )}

      {loading && <p>読み込み中...</p>}
      {displayError && <div className={styles.errorBox}>{displayError}</div>}

      {!loading && !displayError && (
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
                    drag.draggingId === item.storeTypeId ? styles.draggingRow : ""
                  } ${drag.dragOverId === item.storeTypeId ? styles.dragOverRow : ""}`;

                  const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                  const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                  return (
                    <tr
                      key={item.storeTypeId}
                      className={rowClass}
                      {...drag.getRowDragProps(item.storeTypeId)}
                    >
                      <td className={`${tableStyles.td} ${styles.handleCell}`}>
                        <button
                          type="button"
                          {...drag.getDragHandleProps(item.storeTypeId)}
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
                          onClick={() => modal.openEdit(item.storeTypeId)}
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

      <Modal open={modal.modalMode !== null} title={modal.modalMode === "create" ? "店舗種別新規作成" : "店舗種別編集"} onClose={modal.close}>
        {modal.modalMode && (
          <StoreTypeForm mode={modal.modalMode} editId={modal.editId ?? undefined} onSave={handleSave} onCancel={modal.close} />
        )}
      </Modal>
    </main>
  );
}
