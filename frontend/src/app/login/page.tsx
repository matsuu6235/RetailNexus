"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { login } from "@/services/authService";
import { fallback } from "@/lib/messages";
import styles from "./page.module.css";

export default function LoginPage() {
  const router = useRouter();

  const [loginId, setLoginId] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await login({ loginId, password });
      router.push("/products");
    } catch (err) {
      const message = err instanceof Error ? err.message : fallback.loginFailed;
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>Retail Nexus ログイン</h1>

      <form onSubmit={onSubmit} className={styles.form}>
        <label className={styles.field}>
          <span>ログインID</span>
          <input
            value={loginId}
            onChange={(e) => setLoginId(e.target.value)}
            autoComplete="username"
            required
            className={styles.input}
          />
        </label>

        <label className={styles.field}>
          <span>パスワード</span>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
            required
            className={styles.input}
          />
        </label>

        {error && <p className={styles.error}>{error}</p>}

        <button type="submit" disabled={loading} className={styles.submitButton}>
          {loading ? "ログイン中..." : "ログイン"}
        </button>
      </form>
    </main>
  );
}
