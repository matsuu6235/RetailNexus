"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import { useModal } from "@/lib/hooks/useModal";
import { getUsers } from "@/lib/api/users";
import type { User } from "@/types/users";
import Modal from "@/components/modal/Modal";
import UserForm from "./UserForm";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

export default function UsersPage() {
  const [items, setItems] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [loginIdInput, setLoginIdInput] = useState("");
  const [nameInput, setNameInput] = useState("");
  const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

  const [loginIdFilter, setLoginIdFilter] = useState("");
  const [nameFilter, setNameFilter] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

  const modal = useModal();

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      const users = await getUsers();
      setItems(users);
    } catch (e) {
      setError(e instanceof Error ? e.message : fallback.listFetchFailed("ユーザー"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const filteredItems = items.filter((item) => {
    const matchLoginId = !loginIdFilter.trim() || item.loginId.includes(loginIdFilter.trim());
    const matchName = !nameFilter.trim() || item.userName.includes(nameFilter.trim());
    const matchStatus =
      isActiveFilter === "all" ||
      (isActiveFilter === "active" && item.isActive) ||
      (isActiveFilter === "inactive" && !item.isActive);
    return matchLoginId && matchName && matchStatus;
  });

  const handleSave = () => {
    modal.close();
    fetchData();
  };

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>ユーザー管理</h1>
          <p className={styles.subtitle}>ユーザーの検索・編集・新規作成を行います。</p>
        </div>

        <button
          type="button"
          onClick={modal.openCreate}
          className={styles.primaryButton}
        >
          ユーザー新規作成
        </button>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>ログインID</span>
            <input value={loginIdInput} onChange={(e) => setLoginIdInput(e.target.value)} placeholder="ログインID" className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>ユーザー名</span>
            <input value={nameInput} onChange={(e) => setNameInput(e.target.value)} placeholder="ユーザー名" className={styles.input} />
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
              setLoginIdFilter(loginIdInput);
              setNameFilter(nameInput);
              setIsActiveFilter(isActiveInput);
            }}
            className={styles.searchButton}
          >
            検索
          </button>
        </div>
      </section>

      {loading && <p>読み込み中...</p>}
      {error && <div className={styles.errorBox}>{error}</div>}

      {!loading && !error && (
        <div className={tableStyles.wrapper}>
          <div className={tableStyles.summary}>
            <span>{filteredItems.length} 件</span>
          </div>

          <div className={tableStyles.tableContainer}>
            <table className={tableStyles.table}>
              <thead className={tableStyles.thead}>
                <tr>
                  <th className={tableStyles.th}>ログインID</th>
                  <th className={tableStyles.th}>ユーザー名</th>
                  <th className={tableStyles.th}>メールアドレス</th>
                  <th className={tableStyles.th}>ロール</th>
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
                  const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                  const statusClass = `${tableStyles.statusChip} ${item.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                  const statusDotClass = `${tableStyles.statusDot} ${item.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                  return (
                    <tr key={item.userId} className={rowClass}>
                      <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{item.loginId}</td>
                      <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>{item.userName}</td>
                      <td className={tableStyles.td}>{item.email ?? ""}</td>
                      <td className={tableStyles.td}>
                        {item.roles.map((r) => (
                          <span key={r.roleId} className={styles.roleBadge}>{r.roleName}</span>
                        ))}
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
                          onClick={() => modal.openEdit(item.userId)}
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

      <Modal open={modal.modalMode !== null} title={modal.modalMode === "create" ? "ユーザー新規作成" : "ユーザー編集"} onClose={modal.close}>
        {modal.modalMode && (
          <UserForm
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
