"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import styles from "./AppShell.module.css";
import { getLoggedInUserName, getPermissions, logout } from "@/services/authService";

type NavItem = {
  label: string;
  href: string;
  permission: string;
};

const navItems: NavItem[] = [
  { label: "商品マスタ", href: "/products", permission: "products.view" },
  { label: "仕入先マスタ", href: "/suppliers", permission: "suppliers.view" },
  { label: "商品カテゴリマスタ", href: "/product-categories", permission: "product-categories.view" },
  { label: "エリアマスタ", href: "/areas", permission: "areas.view" },
  { label: "店舗マスタ", href: "/stores", permission: "stores.view" },
  { label: "店舗種別マスタ", href: "/store-types", permission: "store-types.view" },
  { label: "在庫管理", href: "/inventories", permission: "inventory.view" },
  { label: "在庫変動履歴", href: "/inventory-transactions", permission: "inventory.view" },
  { label: "発注管理", href: "/purchase-orders", permission: "purchases.view" },
  { label: "発送依頼", href: "/store-requests", permission: "store-requests.view" },
  { label: "ユーザー管理", href: "/users", permission: "users.view" },
  { label: "ロール管理", href: "/roles", permission: "roles.view" },
  { label: "監査ログ", href: "/audit-logs", permission: "auditlog.view" },
];

type AppShellProps = {
  children: React.ReactNode;
};

export default function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const router = useRouter();
  const [userName, setUserName] = useState("");
  const [permissions, setPermissions] = useState<string[]>([]);

  useEffect(() => {
    setUserName(getLoggedInUserName());
    setPermissions(getPermissions());
  }, [pathname]);

  const isLoginPage = pathname === "/login";

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  if (isLoginPage) {
    return <>{children}</>;
  }

  const visibleNavItems = navItems.filter((item) => permissions.includes(item.permission));

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
            {visibleNavItems.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={`${styles.navItem} ${pathname.startsWith(item.href) ? styles.navItemActive : ""}`}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </aside>

        <div className={styles.content}>{children}</div>
      </div>
    </div>
  );
}
