"use client";

import { useEffect, useState } from "react";
import { fallback } from "@/lib/messages";
import { getAuditLogs } from "@/lib/api/auditLogs";
import type { AuditLog } from "@/types/auditLogs";
import Modal from "@/components/modal/Modal";
import Pager from "@/components/pager/Pager";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const PAGE_SIZE = 50;

const ENTITY_OPTIONS = [
  "all", "Area", "Store", "StoreType", "Product", "ProductCategory",
  "Supplier", "User", "Role", "UserRole", "RolePermission"
];

const ACTION_OPTIONS = ["all", "Create", "Update", "Delete"];

// Action badge colors
function actionColor(action: string): string {
  switch (action) {
    case "Create": return "#16a34a";
    case "Update": return "#2563eb";
    case "Delete": return "#dc2626";
    default: return "#64748b";
  }
}

// Parse JSON values for display
function parseJson(json: string | null): Record<string, unknown> {
  if (!json) return {};
  try { return JSON.parse(json); } catch { return {}; }
}

// Format timestamp for display
function formatTimestamp(ts: string): string {
  return new Date(ts).toLocaleString("ja-JP", {
    year: "numeric", month: "2-digit", day: "2-digit",
    hour: "2-digit", minute: "2-digit", second: "2-digit"
  });
}

export default function AuditLogsPage() {
  const [items, setItems] = useState<AuditLog[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Input state
  const [fromInput, setFromInput] = useState("");
  const [toInput, setToInput] = useState("");
  const [userNameInput, setUserNameInput] = useState("");
  const [actionInput, setActionInput] = useState("all");
  const [entityNameInput, setEntityNameInput] = useState("all");

  // Applied filter state
  const [fromFilter, setFromFilter] = useState("");
  const [toFilter, setToFilter] = useState("");
  const [userNameFilter, setUserNameFilter] = useState("");
  const [actionFilter, setActionFilter] = useState("all");
  const [entityNameFilter, setEntityNameFilter] = useState("all");

  // Detail modal
  const [selectedItem, setSelectedItem] = useState<AuditLog | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await getAuditLogs(page, PAGE_SIZE, {
        from: fromFilter ? new Date(fromFilter).toISOString() : undefined,
        to: toFilter ? new Date(toFilter).toISOString() : undefined,
        userName: userNameFilter || undefined,
        action: actionFilter,
        entityName: entityNameFilter,
      });
      setItems(res.items);
      setTotal(res.total);
    } catch (e) {
      setError(e instanceof Error ? e.message : fallback.listFetchFailed("監査ログ"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [page, fromFilter, toFilter, userNameFilter, actionFilter, entityNameFilter]);

  const handleSearch = () => {
    setFromFilter(fromInput);
    setToFilter(toInput);
    setUserNameFilter(userNameInput);
    setActionFilter(actionInput);
    setEntityNameFilter(entityNameInput);
    setPage(1);
  };

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>監査ログ</h1>
          <p className={styles.subtitle}>データの変更履歴を確認できます。</p>
        </div>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>開始日時</span>
            <input type="datetime-local" value={fromInput} onChange={(e) => setFromInput(e.target.value)} className={styles.input} />
          </label>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>終了日時</span>
            <input type="datetime-local" value={toInput} onChange={(e) => setToInput(e.target.value)} className={styles.input} />
          </label>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>ユーザー名</span>
            <input value={userNameInput} onChange={(e) => setUserNameInput(e.target.value)} placeholder="ユーザー名" className={styles.input} />
          </label>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>操作種別</span>
            <select value={actionInput} onChange={(e) => setActionInput(e.target.value)} className={styles.select}>
              <option value="all">すべて</option>
              {ACTION_OPTIONS.filter(a => a !== "all").map(a => <option key={a} value={a}>{a}</option>)}
            </select>
          </label>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>エンティティ</span>
            <select value={entityNameInput} onChange={(e) => setEntityNameInput(e.target.value)} className={styles.select}>
              <option value="all">すべて</option>
              {ENTITY_OPTIONS.filter(e => e !== "all").map(e => <option key={e} value={e}>{e}</option>)}
            </select>
          </label>
        </div>
        <div className={styles.searchActions}>
          <button
            type="button"
            onClick={() => {
              setFromInput(""); setToInput(""); setUserNameInput(""); setActionInput("all"); setEntityNameInput("all");
              setFromFilter(""); setToFilter(""); setUserNameFilter(""); setActionFilter("all"); setEntityNameFilter("all");
              setPage(1);
            }}
            className={styles.clearButton}
          >
            クリア
          </button>
          <button type="button" onClick={handleSearch} className={styles.searchButton}>検索</button>
        </div>
      </section>

      {loading && <p>読み込み中...</p>}
      {error && <div className={styles.errorBox}>{error}</div>}

      {!loading && !error && (
        <div className={tableStyles.wrapper}>
          <div className={tableStyles.summary}>
            <span>{total} 件</span>
          </div>

          <div className={tableStyles.tableContainer} style={{ maxHeight: "500px", overflowY: "auto" }}>
            <table className={tableStyles.table}>
              <thead className={tableStyles.thead} style={{ position: "sticky", top: 0, zIndex: 1 }}>
                <tr>
                  <th className={tableStyles.th}>日時</th>
                  <th className={tableStyles.th} style={{ minWidth: "120px" }}>ユーザー</th>
                  <th className={tableStyles.th}>操作</th>
                  <th className={tableStyles.th}>エンティティ</th>
                  <th className={tableStyles.th}>変更内容</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 && (
                  <tr><td colSpan={5} className={tableStyles.empty}>データがありません</td></tr>
                )}
                {items.map((item, index) => {
                  const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                  const oldVals = parseJson(item.oldValues);
                  const newVals = parseJson(item.newValues);
                  const changedKeys = [...new Set([...Object.keys(oldVals), ...Object.keys(newVals)])];

                  return (
                    <tr key={item.auditLogId} className={rowClass} onClick={() => setSelectedItem(item)} style={{ cursor: "pointer" }}>
                      <td className={tableStyles.td} style={{ whiteSpace: "nowrap", fontSize: "12px" }}>{formatTimestamp(item.timestamp)}</td>
                      <td className={tableStyles.td}>{item.userName}</td>
                      <td className={tableStyles.td}>
                        <span style={{ display: "inline-block", padding: "2px 8px", borderRadius: "4px", fontSize: "12px", fontWeight: 600, color: "#fff", backgroundColor: actionColor(item.action) }}>
                          {item.action}
                        </span>
                      </td>
                      <td className={tableStyles.td}>{item.entityName}</td>
                      <td className={tableStyles.td}>
                        <span style={{ fontSize: "12px", color: "#64748b" }}>
                          {changedKeys.length > 0 ? changedKeys.join(", ") : "-"}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <Pager page={page} totalPages={totalPages} onPageChange={setPage} />
        </div>
      )}

      <Modal open={selectedItem !== null} title="監査ログ詳細" onClose={() => setSelectedItem(null)}>
        {selectedItem && <AuditLogDetail item={selectedItem} />}
      </Modal>
    </main>
  );
}

function AuditLogDetail({ item }: { item: AuditLog }) {
  const oldVals = parseJson(item.oldValues);
  const newVals = parseJson(item.newValues);
  const changedKeys = [...new Set([...Object.keys(oldVals), ...Object.keys(newVals)])];

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
      <dl style={{ display: "grid", gridTemplateColumns: "120px 1fr", gap: "8px 12px", fontSize: "14px", margin: 0 }}>
        <dt style={{ fontWeight: 600, color: "#64748b" }}>日時</dt>
        <dd style={{ margin: 0 }}>{formatTimestamp(item.timestamp)}</dd>
        <dt style={{ fontWeight: 600, color: "#64748b" }}>ユーザー</dt>
        <dd style={{ margin: 0 }}>{item.userName}</dd>
        <dt style={{ fontWeight: 600, color: "#64748b" }}>操作</dt>
        <dd style={{ margin: 0 }}>
          <span style={{ display: "inline-block", padding: "2px 8px", borderRadius: "4px", fontSize: "12px", fontWeight: 600, color: "#fff", backgroundColor: actionColor(item.action) }}>
            {item.action}
          </span>
        </dd>
        <dt style={{ fontWeight: 600, color: "#64748b" }}>エンティティ</dt>
        <dd style={{ margin: 0 }}>{item.entityName}</dd>
        <dt style={{ fontWeight: 600, color: "#64748b" }}>レコードID</dt>
        <dd style={{ margin: 0, fontFamily: "monospace", fontSize: "12px" }}>{item.entityId}</dd>
      </dl>

      {changedKeys.length > 0 && (
        <div>
          <h3 style={{ fontSize: "14px", fontWeight: 600, marginBottom: "8px" }}>変更内容</h3>
          <table style={{ width: "100%", fontSize: "13px", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <th style={{ textAlign: "left", padding: "6px 8px", borderBottom: "2px solid #e2e8f0", fontWeight: 600 }}>項目</th>
                <th style={{ textAlign: "left", padding: "6px 8px", borderBottom: "2px solid #e2e8f0", fontWeight: 600 }}>変更前</th>
                <th style={{ textAlign: "left", padding: "6px 8px", borderBottom: "2px solid #e2e8f0", fontWeight: 600 }}>変更後</th>
              </tr>
            </thead>
            <tbody>
              {changedKeys.map(key => (
                <tr key={key} style={{ borderBottom: "1px solid #f1f5f9" }}>
                  <td style={{ padding: "6px 8px", fontWeight: 500 }}>{key}</td>
                  <td style={{ padding: "6px 8px", color: "#dc2626" }}>{oldVals[key] !== undefined ? String(oldVals[key]) : "-"}</td>
                  <td style={{ padding: "6px 8px", color: "#16a34a" }}>{newVals[key] !== undefined ? String(newVals[key]) : "-"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {changedKeys.length === 0 && (
        <p style={{ fontSize: "13px", color: "#64748b" }}>変更内容はありません。</p>
      )}
    </div>
  );
}
