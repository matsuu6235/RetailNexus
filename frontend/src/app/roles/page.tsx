"use client";

import { useEffect, useState } from "react";
import { getRoles } from "@/lib/api/roles";
import type { Role } from "@/types/roles";
import Modal from "@/components/modal/Modal";
import RoleForm from "./RoleForm";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

export default function RolesPage() {
  const [items, setItems] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [modalMode, setModalMode] = useState<"create" | "edit" | null>(null);
  const [editId, setEditId] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      const roles = await getRoles();
      setItems(roles);
    } catch (e) {
      setError(e instanceof Error ? e.message : "ロール一覧の取得に失敗しました。");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleModalClose = () => {
    setModalMode(null);
    setEditId(null);
  };

  const handleSave = () => {
    handleModalClose();
    fetchData();
  };

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>ロール管理</h1>
          <p className={styles.subtitle}>ロールの作成・編集・権限設定を行います。</p>
        </div>

        <button
          type="button"
          onClick={() => {
            setModalMode("create");
            setEditId(null);
          }}
          className={styles.primaryButton}
        >
          ロール新規作成
        </button>
      </header>

      {loading && <p>読み込み中...</p>}
      {error && <div className={styles.errorBox}>{error}</div>}

      {!loading && !error && (
        <div className={tableStyles.wrapper}>
          <div className={tableStyles.summary}>
            <span>{items.length} 件</span>
          </div>

          <div className={tableStyles.tableContainer}>
            <table className={tableStyles.table}>
              <thead className={tableStyles.thead}>
                <tr>
                  <th className={tableStyles.th}>ロール名</th>
                  <th className={tableStyles.th}>説明</th>
                  <th className={tableStyles.th}>権限数</th>
                  <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thStatus}`}>状態</th>
                  <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 && (
                  <tr>
                    <td colSpan={5} className={tableStyles.empty}>データがありません</td>
                  </tr>
                )}

                {items.map((item, index) => {
                  const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                  const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                  const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                  return (
                    <tr key={item.roleId} className={rowClass}>
                      <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>{item.roleName}</td>
                      <td className={tableStyles.td}>{item.description ?? ""}</td>
                      <td className={tableStyles.td}>
                        <span className={styles.permCount}>{item.permissions.length} 件</span>
                      </td>
                      <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                        <span className={statusClass}>
                          <span className={statusDotClass} />
                          {item.isActive ? "有効" : "無効"}
                        </span>
                      </td>
                      <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                        <button
                          type="button"
                          onClick={() => {
                            setModalMode("edit");
                            setEditId(item.roleId);
                          }}
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

      <Modal open={modalMode !== null} title={modalMode === "create" ? "ロール新規作成" : "ロール編集"} onClose={handleModalClose}>
        {modalMode && (
          <RoleForm
            mode={modalMode}
            editId={editId ?? undefined}
            onSave={handleSave}
            onCancel={handleModalClose}
          />
        )}
      </Modal>
    </main>
  );
}
