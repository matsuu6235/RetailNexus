import type { Product } from "../../types/products";
import styles from "./ProductTable.module.css";

function formatYen(value: number) {
  return new Intl.NumberFormat("ja-JP", {
    style: "currency",
    currency: "JPY",
  }).format(value);
}

type ProductTableProps = {
  products: Product[];
  total: number;
  page: number;
  pageSize: number;
};

export function ProductTable({ products, total, page, pageSize }: ProductTableProps) {
  const hasData = products.length > 0;
  const start = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, total);

  return (
    <section className={styles.wrapper}>
      <div className={styles.summary}>
        <span>{total > 0 ? `${start} - ${end} / ${total} 件` : "0 件"}</span>
      </div>

      <div className={styles.tableContainer}>
        <table className={styles.table}>
          <thead className={styles.thead}>
            <tr>
              {["SKU", "JAN", "商品名", "カテゴリ", "売価", "原価", "状態"].map((h) => (
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
                    {p.categoryName ? `${p.categoryName} (${p.categoryCode})` : p.categoryCode}
                  </td>
                  <td className={`${styles.td} ${styles.tdPrice}`}>{formatYen(p.price)}</td>
                  <td className={`${styles.td} ${styles.tdCost}`}>{formatYen(p.cost)}</td>
                  <td className={styles.td}>
                    <span className={statusClass}>
                      <span className={statusDotClass} />
                      {p.isActive ? "有効" : "無効"}
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>

        {!hasData && <div className={styles.empty}>商品がありません。</div>}
      </div>
    </section>
  );
}