import styles from "./Pager.module.css";

type PagerProps = {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

function getPageNumbers(page: number, totalPages: number): (number | "...")[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  const pages: (number | "...")[] = [1];

  if (page <= 4) {
    pages.push(2, 3, 4, 5, "...", totalPages);
  } else if (page >= totalPages - 3) {
    pages.push("...", totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages);
  } else {
    pages.push("...", page - 1, page, page + 1, "...", totalPages);
  }

  return pages;
}

export default function Pager({ page, totalPages, onPageChange }: PagerProps) {
  if (totalPages <= 1) return null;

  const pageNumbers = getPageNumbers(page, totalPages);

  return (
    <div className={styles.pager}>
      <span className={styles.pagerText}>ページ {page} / {totalPages}</span>
      <div className={styles.pagerButtons}>
        <button
          type="button"
          disabled={page <= 1}
          onClick={() => onPageChange(1)}
          className={styles.pagerButton}
        >
          &laquo;
        </button>
        <button
          type="button"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
          className={styles.pagerButton}
        >
          &lsaquo;
        </button>

        {pageNumbers.map((p, i) =>
          p === "..." ? (
            <span key={`ellipsis-${i}`} className={styles.ellipsis}>...</span>
          ) : (
            <button
              key={p}
              type="button"
              onClick={() => onPageChange(p)}
              className={`${styles.pagerButton} ${p === page ? styles.pagerButtonActive : ""}`}
            >
              {p}
            </button>
          )
        )}

        <button
          type="button"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
          className={styles.pagerButton}
        >
          &rsaquo;
        </button>
        <button
          type="button"
          disabled={page >= totalPages}
          onClick={() => onPageChange(totalPages)}
          className={styles.pagerButton}
        >
          &raquo;
        </button>
      </div>
    </div>
  );
}
