"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import styles from "./AppShell.module.css";
import { getLoggedInUserName, logout } from "../../services/authService";

type AppShellProps = {
  children: React.ReactNode;
};

export default function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const router = useRouter();
  const [userName, setUserName] = useState("");

  useEffect(() => {
    setUserName(getLoggedInUserName());
  }, [pathname]);

  const isLoginPage = pathname === "/login";
  const isProductMasterActive = pathname.startsWith("/products");
  const isSupplierMasterActive = pathname.startsWith("/suppliers");
  const isProductCategoryMasterActive = pathname.startsWith("/product-categories");
  const isAreaMasterActive = pathname.startsWith("/areas");
  const isStoreMasterActive = pathname.startsWith("/stores");
  const isStoreTypeMasterActive = pathname.startsWith("/store-types");

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  if (isLoginPage) {
    return <>{children}</>;
  }

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <Link href="/products" className={styles.logo}>
            Retail Nexus
          </Link>

          <div className={styles.headerRight}>
            <span className={styles.userName}>
              {userName || "未ログインユーザー"}
            </span>
            <button
              type="button"
              onClick={handleLogout}
              className={styles.logoutButton}
            >
              ログアウト
            </button>
          </div>
        </div>
      </header>

      <div className={styles.body}>
        <aside className={styles.sidebar}>
          <nav className={styles.nav}>
            <Link href="/products" className={`${styles.navItem} ${isProductMasterActive ? styles.navItemActive : ""}`}>
              商品マスタ
            </Link>
            <Link href="/suppliers" className={`${styles.navItem} ${isSupplierMasterActive ? styles.navItemActive : ""}`}>
              仕入先マスタ
            </Link>
            <Link href="/product-categories" className={`${styles.navItem} ${isProductCategoryMasterActive ? styles.navItemActive : ""}`}>
              商品カテゴリマスタ
            </Link>
            <Link href="/areas" className={`${styles.navItem} ${isAreaMasterActive ? styles.navItemActive : ""}`}>
              エリアマスタ
            </Link>
            <Link href="/stores" className={`${styles.navItem} ${isStoreMasterActive ? styles.navItemActive : ""}`}>
              店舗マスタ
            </Link>
            <Link href="/store-types" className={`${styles.navItem} ${isStoreTypeMasterActive ? styles.navItemActive : ""}`}>
              店舗種別マスタ
            </Link>
          </nav>
        </aside>

        <div className={styles.content}>{children}</div>
      </div>
    </div>
  );
}