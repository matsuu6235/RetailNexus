"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getStores } from "@/lib/api/stores";
import { getAllAreas } from "@/lib/api/areas";
import { getStoreTypes } from "@/lib/api/storeTypes";
import type { Store } from "@/types/stores";
import type { Area } from "@/types/areas";
import type { StoreType } from "@/types/storeTypes";
import styles from "./page.module.css";
import tableStyles from "@/components/table/MasterTable.module.css";

const PAGE_SIZE = 20;

export default function StoresPage() {
  const router = useRouter();
  const [stores, setStores] = useState<Store[]>([]);
  const [areas, setAreas] = useState<Area[]>([]);
  const [storeTypes, setStoreTypes] = useState<StoreType[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [storeCdInput, setStoreCdInput] = useState("");
  const [storeNameInput, setStoreNameInput] = useState("");
  const [areaIdInput, setAreaIdInput] = useState("");
  const [storeTypeIdInput, setStoreTypeIdInput] = useState("");
  const [isActiveInput, setIsActiveInput] = useState<"all" | "active" | "inactive">("all");

  const [storeCdFilter, setStoreCdFilter] = useState("");
  const [storeNameFilter, setStoreNameFilter] = useState("");
  const [areaIdFilter, setAreaIdFilter] = useState("");
  const [storeTypeIdFilter, setStoreTypeIdFilter] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState<"all" | "active" | "inactive">("all");

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const [areaItems, typeItems] = await Promise.all([
          getAllAreas(),
          getStoreTypes(),
        ]);

        if (!cancelled) {
          setAreas(areaItems);
          setStoreTypes(typeItems);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "検索条件の取得に失敗しました。");
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const res = await getStores(page, PAGE_SIZE, {
          storeCd: storeCdFilter,
          storeName: storeNameFilter,
          areaId: areaIdFilter,
          storeTypeId: storeTypeIdFilter,
          isActive: isActiveFilter,
        });

        if (!cancelled) {
          setStores(res.items);
          setTotal(res.total);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "店舗一覧の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [page, storeCdFilter, storeNameFilter, areaIdFilter, storeTypeIdFilter, isActiveFilter]);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
  const start = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const end = Math.min(page * PAGE_SIZE, total);

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>店舗マスタ</h1>
          <p className={styles.subtitle}>店舗の検索・編集・新規作成を行います。</p>
        </div>

        <button type="button" onClick={() => router.push("/stores/new")} className={styles.primaryButton}>
          店舗新規作成
        </button>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗コード</span>
            <input value={storeCdInput} onChange={(e) => setStoreCdInput(e.target.value)} className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗名</span>
            <input value={storeNameInput} onChange={(e) => setStoreNameInput(e.target.value)} className={styles.input} />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>エリア</span>
            <select value={areaIdInput} onChange={(e) => setAreaIdInput(e.target.value)} className={styles.select}>
              <option value="">すべて</option>
              {areas.map((area) => (
                <option key={area.areaId} value={area.areaId}>
                  {area.areaName} ({area.areaCd})
                </option>
              ))}
            </select>
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>店舗種別</span>
            <select value={storeTypeIdInput} onChange={(e) => setStoreTypeIdInput(e.target.value)} className={styles.select}>
              <option value="">すべて</option>
              {storeTypes.map((storeType) => (
                <option key={storeType.storeTypeId} value={storeType.storeTypeId}>
                  {storeType.storeTypeName} ({storeType.storeTypeCd})
                </option>
              ))}
            </select>
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>有効状態</span>
            <select value={isActiveInput} onChange={(e) => setIsActiveInput(e.target.value as "all" | "active" | "inactive")} className={styles.select}>
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
              setStoreCdFilter(storeCdInput);
              setStoreNameFilter(storeNameInput);
              setAreaIdFilter(areaIdInput);
              setStoreTypeIdFilter(storeTypeIdInput);
              setIsActiveFilter(isActiveInput);
              setPage(1);
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
        <>
          <div className={tableStyles.wrapper}>
            <div className={tableStyles.summary}>
              <span>{total > 0 ? `${start} - ${end} / ${total} 件` : "0 件"}</span>
            </div>

            <div className={tableStyles.tableContainer}>
              <table className={tableStyles.table}>
                <thead className={tableStyles.thead}>
                  <tr>
                    <th className={tableStyles.th}>店舗コード</th>
                    <th className={tableStyles.th}>店舗名</th>
                    <th className={tableStyles.th}>エリア</th>
                    <th className={tableStyles.th}>店舗種別</th>
                    <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thStatus}`}>状態</th>
                    <th className={`${tableStyles.th} ${tableStyles.thCenter} ${tableStyles.thAction}`}>操作</th>
                  </tr>
                </thead>
                <tbody>
                  {stores.length === 0 && (
                    <tr>
                      <td colSpan={6} className={tableStyles.empty}>データがありません</td>
                    </tr>
                  )}

                  {stores.map((store, index) => {
                    const rowClass = `${tableStyles.row} ${index % 2 === 0 ? tableStyles.rowEven : tableStyles.rowOdd}`;
                    const statusClass = `${tableStyles.statusChip} ${store.isActive ? tableStyles.statusActive : tableStyles.statusInactive}`;
                    const statusDotClass = `${tableStyles.statusDot} ${store.isActive ? tableStyles.statusDotActive : tableStyles.statusDotInactive}`;

                    return (
                      <tr key={store.storeId} className={rowClass}>
                        <td className={`${tableStyles.td} ${tableStyles.tdCode}`}>{store.storeCd}</td>
                        <td className={`${tableStyles.td} ${tableStyles.tdStrong}`}>{store.storeName}</td>
                        <td className={`${tableStyles.td} ${tableStyles.tdMuted}`}>{store.areaName}</td>
                        <td className={`${tableStyles.td} ${tableStyles.tdMuted}`}>{store.storeTypeName}</td>
                        <td className={`${tableStyles.td} ${tableStyles.tdStatus}`}>
                          <span className={statusClass}>
                            <span className={statusDotClass} />
                            {store.isActive ? "有効" : "無効"}
                          </span>
                        </td>
                        <td className={`${tableStyles.td} ${tableStyles.tdAction}`}>
                          <button type="button" onClick={() => router.push(`/stores/edit?id=${store.storeId}`)} className={tableStyles.editButton}>
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

          <div className={styles.pager}>
            <span className={styles.pagerText}>ページ {page} / {totalPages}</span>
            <div className={styles.pagerButtons}>
              <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)} className={styles.pagerButton}>
                前へ
              </button>
              <button type="button" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)} className={styles.pagerButton}>
                次へ
              </button>
            </div>
          </div>
        </>
      )}
    </main>
  );
}