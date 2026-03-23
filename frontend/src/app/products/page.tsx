"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getProducts } from "@/lib/api/products";
import { getAllProductCategories } from "@/lib/api/productCategories";
import type { Product } from "@/types/products";
import type { ProductCategory } from "@/types/productCategories";
import styles from "./page.module.css";

const PAGE_SIZE = 20;

function formatYen(value: number) {
  return new Intl.NumberFormat("ja-JP", {
    style: "currency",
    currency: "JPY",
  }).format(value);
}

export default function ProductsPage() {
  const router = useRouter();
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [skuInput, setSkuInput] = useState("");
  const [janInput, setJanInput] = useState("");
  const [nameInput, setNameInput] = useState("");
  const [categoryCodeInput, setCategoryCodeInput] = useState("");
  const [activeInput, setActiveInput] = useState<"all" | "active" | "inactive">("all");

  const [skuFilter, setSkuFilter] = useState("");
  const [janFilter, setJanFilter] = useState("");
  const [nameFilter, setNameFilter] = useState("");
  const [categoryCodeFilter, setCategoryCodeFilter] = useState("");
  const [activeFilter, setActiveFilter] = useState<"all" | "active" | "inactive">("all");

  const moveToPage = (nextPage: number) => {
    setPage(nextPage);
    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const [productRes, categoryRes] = await Promise.all([
          getProducts(page, PAGE_SIZE, {
            productCode: skuFilter,
            janCode: janFilter,
            productName: nameFilter,
            productCategoryCode: categoryCodeFilter,
            isActive: activeFilter,
          }),
          getAllProductCategories(),
        ]);

        if (!cancelled) {
          const categoryMap = new Map(
            categoryRes.map((category) => [category.productCategoryCd, category.productCategoryName])
          );

          setProducts(
            productRes.items.map((product) => ({
              ...product,
              categoryName: categoryMap.get(product.productCategoryCode) ?? null,
            }))
          );
          setTotal(productRes.total);
          setCategories(categoryRes);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "商品一覧の取得に失敗しました。");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [page, skuFilter, nameFilter, janFilter, categoryCodeFilter, activeFilter]);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
  const hasData = products.length > 0;
  const start = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const end = Math.min(page * PAGE_SIZE, total);

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>商品一覧</h1>
          <p className={styles.subtitle}>商品マスタの閲覧・編集・新規登録を行います。</p>
        </div>

        <button type="button" onClick={() => router.push("/products/new")} className={styles.primaryButton}>
          商品新規作成
        </button>
      </header>

      <section className={styles.searchSection}>
        <div className={styles.searchGrid}>
          <label className={styles.field}>
            <span className={styles.fieldLabel}>SKU</span>
            <input
              value={skuInput}
              onChange={(e) => setSkuInput(e.target.value)}
              placeholder="SKU"
              className={styles.input}
            />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>JAN</span>
            <input
              value={janInput}
              onChange={(e) => setJanInput(e.target.value)}
              placeholder="JAN"
              className={styles.input}
            />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>商品名（部分一致）</span>
            <input
              value={nameInput}
              onChange={(e) => setNameInput(e.target.value)}
              placeholder="商品名"
              className={styles.input}
            />
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>カテゴリ</span>
            <select
              value={categoryCodeInput}
              onChange={(e) => setCategoryCodeInput(e.target.value)}
              className={styles.select}
            >
              <option value="">カテゴリ（すべて）</option>
              {categories.map((c) => (
                <option key={c.productCategoryId} value={c.productCategoryCd}>
                  {c.productCategoryName} ({c.productCategoryCd})
                </option>
              ))}
            </select>
          </label>

          <label className={styles.field}>
            <span className={styles.fieldLabel}>状態</span>
            <select
              value={activeInput}
              onChange={(e) => setActiveInput(e.target.value as "all" | "active" | "inactive")}
              className={styles.select}
            >
              <option value="all">状態（すべて）</option>
              <option value="active">有効</option>
              <option value="inactive">無効</option>
            </select>
          </label>
        </div>

        <div className={styles.searchActions}>
          <button
            type="button"
            onClick={() => {
              setSkuFilter(skuInput);
              setNameFilter(nameInput);
              setJanFilter(janInput);
              setCategoryCodeFilter(categoryCodeInput);
              setActiveFilter(activeInput);
              moveToPage(1);
            }}
            className={styles.searchButton}
          >
            検索
          </button>
        </div>
      </section>

      {loading && <p>読み込み中...</p>}
      {error && <div className={styles.errorBox}>取得に失敗しました: {error}</div>}

      {!loading && !error && (
        <>
          <section className={styles.tableWrapper}>
            <div className={styles.tableSummary}>
              <span>{total > 0 ? `${start} - ${end} / ${total} 件` : "0 件"}</span>
            </div>

            <div className={styles.tableContainer}>
              <table className={styles.table}>
                <thead className={styles.thead}>
                  <tr>
                    {["SKU", "JAN", "商品名", "カテゴリ", "売価", "原価", "状態", ""].map((h) => (
                      <th key={h} className={styles.th}>
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {products.map((p, index) => {
                    const rowClass = `${styles.row} ${index % 2 === 0 ? styles.rowEven : styles.rowOdd}`;
                    const statusClass = `${styles.statusChip} ${p.isActive ? styles.statusActive : styles.statusInactive}`;
                    const statusDotClass = `${styles.statusDot} ${p.isActive ? styles.statusDotActive : styles.statusDotInactive}`;

                    return (
                      <tr key={p.id} className={rowClass}>
                        <td className={`${styles.td} ${styles.tdSku}`}>{p.productCode}</td>
                        <td className={`${styles.td} ${styles.tdJan}`}>{p.janCode}</td>
                        <td className={`${styles.td} ${styles.tdProductName}`}>{p.productName}</td>
                        <td className={`${styles.td} ${styles.tdCategory}`}>
                          {p.categoryName ? `${p.categoryName} (${p.productCategoryCode})` : p.productCategoryCode}
                        </td>
                        <td className={`${styles.td} ${styles.tdPrice}`}>{formatYen(p.price)}</td>
                        <td className={`${styles.td} ${styles.tdCost}`}>{formatYen(p.cost)}</td>
                        <td className={styles.td}>
                          <span className={statusClass}>
                            <span className={statusDotClass} />
                            {p.isActive ? "有効" : "無効"}
                          </span>
                        </td>
                        <td className={styles.td}>
                          <button
                            type="button"
                            onClick={() => router.push(`/products/edit?id=${p.id}`)}
                            className={styles.editButton}
                          >
                            編集
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>

              {!hasData && <div className={styles.tableEmpty}>商品がありません。</div>}
            </div>
          </section>

          <div className={styles.pager}>
            <span className={styles.pagerText}>ページ {page} / {totalPages}</span>
            <div className={styles.pagerButtons}>
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => moveToPage(Math.max(1, page - 1))}
                className={styles.pagerButton}
              >
                前へ
              </button>

              <button
                type="button"
                disabled={page >= totalPages}
                onClick={() => moveToPage(Math.min(totalPages, page + 1))}
                className={styles.pagerButton}
              >
                次へ
              </button>
            </div>
          </div>
        </>
      )}
    </main>
  );
}
