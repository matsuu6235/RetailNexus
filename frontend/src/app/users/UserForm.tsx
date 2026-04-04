"use client";

import { useEffect, useState } from "react";
import { validation, fallback } from "@/lib/messages";
import { createUser, getUserById, updateUser, resetPassword, changeUserActivation } from "@/lib/api/users";
import { getRoles } from "@/lib/api/roles";
import type { Role } from "@/types/roles";
import { useActivation } from "@/lib/hooks/useActivation";
import styles from "@/components/modal/FormModal.module.css";

type UserFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

type FormData = {
  loginId: string;
  userName: string;
  email: string;
  password: string;
  roleIds: string[];
};

type FieldErrors = {
  loginId?: string;
  userName?: string;
  email?: string;
  password?: string;
};

function validate(form: FormData, mode: "create" | "edit"): FieldErrors {
  const errors: FieldErrors = {};

  if (!form.loginId.trim()) {
    errors.loginId = "ログインIDは必須です。";
  } else if (form.loginId.trim().length > 50) {
    errors.loginId = "ログインIDは50文字以内で入力してください。";
  }

  if (!form.userName.trim()) {
    errors.userName = "ユーザー名は必須です。";
  } else if (form.userName.trim().length > 100) {
    errors.userName = "ユーザー名は100文字以内で入力してください。";
  }

  if (form.email.trim() && form.email.trim().length > 255) {
    errors.email = "メールアドレスは255文字以内で入力してください。";
  }

  if (mode === "create") {
    if (!form.password) {
      errors.password = validation.required("パスワード");
    } else if (form.password.length < 8) {
      errors.password = validation.minLength("パスワード", 8);
    }
  }

  return errors;
}

export default function UserForm({ mode, editId, onSave, onCancel }: UserFormProps) {
  const [form, setForm] = useState<FormData>({
    loginId: "",
    userName: "",
    email: "",
    password: "",
    roleIds: [],
  });
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  const [fetchedIsActive, setFetchedIsActive] = useState(true);
  const activation = useActivation({ permissionCode: "users.delete", initialIsActive: fetchedIsActive, changeFn: changeUserActivation, editId });

  // パスワードリセット用
  const [newPassword, setNewPassword] = useState("");
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [resettingPassword, setResettingPassword] = useState(false);
  const [passwordResetSuccess, setPasswordResetSuccess] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const allRoles = await getRoles();
        if (!cancelled) setRoles(allRoles.filter((r) => r.isActive));

        if (mode === "edit" && editId) {
          const user = await getUserById(editId);
          if (!cancelled) {
            setForm({
              loginId: user.loginId,
              userName: user.userName,
              email: user.email ?? "",
              password: "",
              roleIds: user.roles.map((r) => r.roleId),
            });
            setFetchedIsActive(user.isActive);
          }
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : fallback.fetchFailed("データ"));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

  const handleChange = (field: keyof FormData, value: string | boolean | string[]) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validate(updatedForm, mode);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof FieldErrors] }));
  };

  const toggleRole = (roleId: string) => {
    const next = form.roleIds.includes(roleId)
      ? form.roleIds.filter((id) => id !== roleId)
      : [...form.roleIds, roleId];
    handleChange("roleIds", next);
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const errors = validate(form, mode);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;

    try {
      setSubmitting(true);

      if (mode === "create") {
        await createUser({
          loginId: form.loginId.trim(),
          userName: form.userName.trim(),
          email: form.email.trim() || null,
          password: form.password,
          roleIds: form.roleIds,
        });
      } else {
        await updateUser(editId!, {
          loginId: form.loginId.trim(),
          userName: form.userName.trim(),
          email: form.email.trim() || null,
          roleIds: form.roleIds,
        });
      }

      onSave();
    } catch (err) {
      setError(err instanceof Error ? err.message : fallback.saveFailed);
    } finally {
      setSubmitting(false);
    }
  };

  const onResetPassword = async () => {
    setPasswordError(null);
    setPasswordResetSuccess(false);

    if (!newPassword) {
      setPasswordError(validation.required("パスワード"));
      return;
    }
    if (newPassword.length < 8) {
      setPasswordError(validation.minLength("パスワード", 8));
      return;
    }

    try {
      setResettingPassword(true);
      await resetPassword(editId!, { newPassword });
      setNewPassword("");
      setPasswordResetSuccess(true);
    } catch (err) {
      setPasswordError(err instanceof Error ? err.message : "パスワードリセットに失敗しました。");
    } finally {
      setResettingPassword(false);
    }
  };

  if (loading) return <p>読み込み中...</p>;

  return (
    <form onSubmit={onSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>ログインID *</span>
        <input value={form.loginId} onChange={(e) => handleChange("loginId", e.target.value)} className={styles.input} />
        {fieldErrors.loginId && <small className={styles.errorText}>{fieldErrors.loginId}</small>}
      </label>

      <label className={styles.field}>
        <span>ユーザー名 *</span>
        <input value={form.userName} onChange={(e) => handleChange("userName", e.target.value)} className={styles.input} />
        {fieldErrors.userName && <small className={styles.errorText}>{fieldErrors.userName}</small>}
      </label>

      <label className={styles.field}>
        <span>メールアドレス</span>
        <input value={form.email} onChange={(e) => handleChange("email", e.target.value)} className={styles.input} />
        {fieldErrors.email && <small className={styles.errorText}>{fieldErrors.email}</small>}
      </label>

      {mode === "create" && (
        <label className={styles.field}>
          <span>パスワード *</span>
          <input type="password" value={form.password} onChange={(e) => handleChange("password", e.target.value)} className={styles.input} />
          <small className={styles.hint}>8文字以上で入力してください。</small>
          {fieldErrors.password && <small className={styles.errorText}>{fieldErrors.password}</small>}
        </label>
      )}

      <fieldset className={styles.field} style={{ border: "none", padding: 0, margin: 0 }}>
        <span>ロール</span>
        <div style={{ display: "flex", flexWrap: "wrap", gap: "8px", marginTop: "4px" }}>
          {roles.map((role) => (
            <label key={role.roleId} className={styles.checkboxField}>
              <input
                type="checkbox"
                checked={form.roleIds.includes(role.roleId)}
                onChange={() => toggleRole(role.roleId)}
              />
              <span>{role.roleName}</span>
            </label>
          ))}
        </div>
      </fieldset>

      {(error || activation.error) && <div className={styles.errorBox}>{error || activation.error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton}>
          キャンセル
        </button>
        <button type="submit" disabled={submitting} className={styles.submitButton}>
          {mode === "create" ? (submitting ? "作成中..." : "作成") : (submitting ? "更新中..." : "更新")}
        </button>
      </div>

      {mode === "edit" && (
        <fieldset className={styles.field} style={{ border: "1px solid #e2e8f0", borderRadius: "8px", padding: "12px", marginTop: "8px" }}>
          <legend style={{ fontSize: "13px", fontWeight: 600, color: "#0f172a", padding: "0 4px" }}>パスワードリセット</legend>
          <div style={{ display: "flex", gap: "8px", alignItems: "flex-start" }}>
            <div style={{ flex: 1 }}>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => { setNewPassword(e.target.value); setPasswordError(null); setPasswordResetSuccess(false); }}
                placeholder="新しいパスワード（8文字以上）"
                className={styles.input}
                style={{ width: "100%" }}
              />
              {passwordError && <small className={styles.errorText}>{passwordError}</small>}
              {passwordResetSuccess && <small style={{ color: "#16a34a", fontSize: "12px" }}>パスワードをリセットしました。</small>}
            </div>
            <button
              type="button"
              onClick={onResetPassword}
              disabled={resettingPassword}
              className={styles.submitButton}
              style={{ whiteSpace: "nowrap" }}
            >
              {resettingPassword ? "リセット中..." : "リセット"}
            </button>
          </div>
        </fieldset>
      )}

      {mode === "edit" && activation.canDelete && (
        <fieldset className={styles.field} style={{ border: "1px solid #e2e8f0", borderRadius: "8px", padding: "12px", marginTop: "8px" }}>
          <legend style={{ fontSize: "13px", fontWeight: 600, color: "#0f172a", padding: "0 4px" }}>有効状態の変更</legend>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
            <span style={{ fontSize: "13px" }}>
              現在の状態: <strong>{activation.currentIsActive ? "有効" : "無効"}</strong>
            </span>
            <button
              type="button"
              onClick={activation.toggle}
              disabled={activation.changingActivation}
              className={styles.submitButton}
              style={activation.currentIsActive ? { backgroundColor: "#dc2626" } : {}}
            >
              {activation.changingActivation ? "変更中..." : activation.currentIsActive ? "無効化する" : "有効化する"}
            </button>
          </div>
        </fieldset>
      )}
    </form>
  );
}
