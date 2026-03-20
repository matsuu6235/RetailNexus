"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import styles from "./AppHeader.module.css";
import { getLoggedInUserName, logout } from "../../services/authService";

export default function AppHeader() {
const router = useRouter();
const pathname = usePathname();
const [userName, setUserName] = useState("");

useEffect(() => {
  setUserName(getLoggedInUserName());
}, [pathname]);

if (pathname === "/login") {
  return null;
}

const handleLogout = () => {
  logout();
  router.push("/login");
};

return (
  <header className={styles.header}>
    <div className={styles.inner}>
      <Link href="/products" className={styles.logo}>
        Retail Nexus
      </Link>

      <div className={styles.right}>
        <span className={styles.userName}>
          {userName || "未ログインユーザー"}
        </span>
        <button type="button" onClick={handleLogout} className={styles.logoutButton}>
          ログアウト
        </button>
      </div>
    </div>
  </header>
);
}