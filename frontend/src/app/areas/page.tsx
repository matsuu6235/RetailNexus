"use client";

import { useEffect, useState } from "react";
import { getAllAreas, reorderAreas } from "@/lib/api/areas";
import { fallback } from "@/lib/messages";
import type { Area } from "@/types/areas";
import { useModal } from "@/lib/hooks/useModal";
import { useDragReorder } from "@/lib/hooks/useDragReorder";
import Modal from "@/components/modal/Modal";
import AreaForm from "./AreaForm";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const getAreaId = (item: Area) => item.areaId;

export default function AreasPage() {
  const [items, setItems] = useState<Area[]>([]);
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
    getId: getAreaId,
    items,
    setItems,
    originalOrderIds,
    setOriginalOrderIds,
    reorderFn: reorderAreas,
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getAllAreas();
      const normalized = response.map((item, index) => ({ ...item, displayOrder: index + 1 }));
      setItems(normalized);
      setOriginalOrderIds(normalized.map(getAreaId));
    } catch (e) {
      setError(e instanceof Error ? e.message : fallback.listFetchFailed("エリア"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const filteredItems = items.filter((item) => {
    const matchCode = !codeFilter.trim() || item.areaCd.includes(codeFilter.trim());
    const matchName = !nameFilter.trim() || item.areaName.includes(nameFilter.trim());
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
          <h1 className={styles.title}>エリアマスタ</h1>
          <p className={styles.subtitle}>エリアの検索・編集・新規作成と表示順変更を行います。</p>
        </div>

        <button
          type="button"
          onClick={modal.openCreate}
          className={styles.primaryButton}
        >
          エリア新規作成
        </button>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>エリアコード</span>
            <input value={codeInput} onChange={(e) => setCodeInput(e.target.value)} placeholder="エリアコード" className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>エリア名</span>
            <input value={nameInput} onChange={(e) => setNameInput(e.target.value)} placeholder="エリア名" className={styles.input} />
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
                  <th className={tableStyles.th}>エリアコード</th>
                  <th className={tableStyles.th}>エリア名</th>
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
                    drag.draggingId === item.areaId ? styles.draggingRow : ""
                  } ${drag.dragOverId === item.areaId ? styles.dragOverRow : ""}`;

                  const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                  const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                  return (
                    <tr
                      key={item.areaId}
                      className={rowClass}
                      {...drag.getRowDragProps(item.areaId)}
                    >
                      <td className={`${tableStyles.td} ${styles.handleCell}`}>
                        <button
                          type="button"
                          {...drag.getDragHandleProps(item.areaId)}
                          className={styles.dragHandleButton}
                        >
                          <span className={styles.dragHandle}>≡</span>
                        </button>
                      </td>
                      <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{item.areaCd}</td>
                      <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>{item.areaName}</td>
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
                          onClick={() => modal.openEdit(item.areaId)}
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

      <Modal open={modal.modalMode !== null} title={modal.modalMode === "create" ? "エリア新規作成" : "エリア編集"} onClose={modal.close}>
        {modal.modalMode && (
          <AreaForm
            mode={modal.modalMode}
            editId={modal.editId ?? undefined}
            onSave={handleSave}
            onCancel={modal.close}
          />
        )}
      </Modal>
    </main>
  );
}
