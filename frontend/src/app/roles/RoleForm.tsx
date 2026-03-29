"use client";

import { useEffect, useState } from "react";
import { createRole, getRoleById, updateRole, getPermissions, changeRoleActivation } from "@/lib/api/roles";
import type { Permission } from "@/types/roles";
import { hasPermission } from "@/services/authService";
import styles from "@/components/modal/FormModal.module.css";

type RoleFormProps = {
  mode: "create" | "edit";
  editId?: string;
  onSave: () => void;
  onCancel: () => void;
};

type FormData = {
  roleName: string;
  description: string;
  permissionIds: string[];
};

type FieldErrors = {
  roleName?: string;
};

function validate(form: FormData): FieldErrors {
  const errors: FieldErrors = {};
  if (!form.roleName.trim()) {
    errors.roleName = "ロール名は必須です。";
  } else if (form.roleName.trim().length > 50) {
    errors.roleName = "ロール名は50文字以内で入力してください。";
  }
  return errors;
}

type GroupedPermissions = {
  category: string;
  permissions: Permission[];
};

const categoryOrder = [
  "商品管理",
  "仕入先管理",
  "商品カテゴリ管理",
  "エリア管理",
  "店舗管理",
  "店舗種別管理",
  "発注管理",
  "発送依頼",
  "ユーザー管理",
  "ロール管理",
  "監査ログ",
  "ダッシュボード",
];

function groupByCategory(permissions: Permission[]): GroupedPermissions[] {
  const map = new Map<string, Permission[]>();
  for (const p of permissions) {
    const list = map.get(p.category) ?? [];
    list.push(p);
    map.set(p.category, list);
  }
  return Array.from(map.entries())
    .map(([category, permissions]) => ({ category, permissions }))
    .sort((a, b) => {
      const ai = categoryOrder.indexOf(a.category);
      const bi = categoryOrder.indexOf(b.category);
      return (ai === -1 ? 999 : ai) - (bi === -1 ? 999 : bi);
    });
}

export default function RoleForm({ mode, editId, onSave, onCancel }: RoleFormProps) {
  const [form, setForm] = useState<FormData>({
    roleName: "",
    description: "",
    permissionIds: [],
  });
  const [allPermissions, setAllPermissions] = useState<Permission[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [canDelete, setCanDelete] = useState(false);
  const [currentIsActive, setCurrentIsActive] = useState(true);
  const [changingActivation, setChangingActivation] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const perms = await getPermissions();
        if (!cancelled) setAllPermissions(perms);

        if (mode === "edit" && editId) {
          const role = await getRoleById(editId);
          if (!cancelled) {
            setForm({
              roleName: role.roleName,
              description: role.description ?? "",
              permissionIds: role.permissions.map((p) => p.permissionId),
            });
            setCurrentIsActive(role.isActive);
          }
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : "データの取得に失敗しました。");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [mode, editId]);

  useEffect(() => {
    setCanDelete(hasPermission("roles.delete"));
  }, []);

  const handleChange = (field: keyof FormData, value: string | boolean | string[]) => {
    const updatedForm = { ...form, [field]: value };
    setForm(updatedForm);
    const errors = validate(updatedForm);
    setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof FieldErrors] }));
  };

  const togglePermission = (permId: string) => {
    const next = form.permissionIds.includes(permId)
      ? form.permissionIds.filter((id) => id !== permId)
      : [...form.permissionIds, permId];
    handleChange("permissionIds", next);
  };

  const toggleCategory = (category: string) => {
    const categoryPermIds = allPermissions.filter((p) => p.category === category).map((p) => p.permissionId);
    const allSelected = categoryPermIds.every((id) => form.permissionIds.includes(id));

    let next: string[];
    if (allSelected) {
      next = form.permissionIds.filter((id) => !categoryPermIds.includes(id));
    } else {
      next = [...new Set([...form.permissionIds, ...categoryPermIds])];
    }
    handleChange("permissionIds", next);
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const errors = validate(form);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;

    try {
      setSubmitting(true);

      const body = {
        roleName: form.roleName.trim(),
        description: form.description.trim() || null,
        permissionIds: form.permissionIds,
      };

      if (mode === "create") {
        await createRole(body);
      } else {
        await updateRole(editId!, body);
      }

      onSave();
    } catch (err) {
      setError(err instanceof Error ? err.message : "保存に失敗しました。");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <p>読み込み中...</p>;

  const grouped = groupByCategory(allPermissions);

  return (
    <form onSubmit={onSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>ロール名 *</span>
        <input value={form.roleName} onChange={(e) => handleChange("roleName", e.target.value)} className={styles.input} />
        {fieldErrors.roleName && <small className={styles.errorText}>{fieldErrors.roleName}</small>}
      </label>

      <label className={styles.field}>
        <span>説明</span>
        <input value={form.description} onChange={(e) => handleChange("description", e.target.value)} className={styles.input} />
      </label>

      <fieldset className={styles.permissionSection}>
        <span className={styles.permissionSectionTitle}>権限設定</span>
        {grouped.map((group) => {
          const categoryPermIds = group.permissions.map((p) => p.permissionId);
          const allSelected = categoryPermIds.every((id) => form.permissionIds.includes(id));
          const someSelected = categoryPermIds.some((id) => form.permissionIds.includes(id));

          return (
            <div key={group.category} className={styles.permissionGroup}>
              <label className={styles.permissionGroupHeader} onClick={(e) => { e.preventDefault(); toggleCategory(group.category); }}>
                <input
                  type="checkbox"
                  className={styles.categoryCheckbox}
                  checked={allSelected}
                  ref={(el) => { if (el) el.indeterminate = someSelected && !allSelected; }}
                  onChange={() => toggleCategory(group.category)}
                />
                <span className={styles.permissionGroupTitle}>{group.category}</span>
              </label>
              <div className={styles.permissionItems}>
                {group.permissions.map((perm) => (
                  <label key={perm.permissionId} className={styles.permissionItem}>
                    <input
                      type="checkbox"
                      className={styles.permissionCheckbox}
                      checked={form.permissionIds.includes(perm.permissionId)}
                      onChange={() => togglePermission(perm.permissionId)}
                    />
                    <span>{perm.permissionName}</span>
                  </label>
                ))}
              </div>
            </div>
          );
        })}
      </fieldset>

      {error && <div className={styles.errorBox}>{error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton}>
          キャンセル
        </button>
        <button type="submit" disabled={submitting} className={styles.submitButton}>
          {mode === "create" ? (submitting ? "作成中..." : "作成") : (submitting ? "更新中..." : "更新")}
        </button>
      </div>

      {mode === "edit" && canDelete && (
        <fieldset className={styles.field} style={{ border: "1px solid #e2e8f0", borderRadius: "8px", padding: "12px", marginTop: "8px" }}>
          <legend style={{ fontSize: "13px", fontWeight: 600, color: "#0f172a", padding: "0 4px" }}>有効状態の変更</legend>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
            <span style={{ fontSize: "13px" }}>
              現在の状態: <strong>{currentIsActive ? "有効" : "無効"}</strong>
            </span>
            <button
              type="button"
              onClick={async () => {
                try {
                  setChangingActivation(true);
                  await changeRoleActivation(editId!, !currentIsActive);
                  setCurrentIsActive(!currentIsActive);
                } catch (err) {
                  setError(err instanceof Error ? err.message : "状態の変更に失敗しました。");
                } finally {
                  setChangingActivation(false);
                }
              }}
              disabled={changingActivation}
              className={styles.submitButton}
              style={currentIsActive ? { backgroundColor: "#dc2626" } : {}}
            >
              {changingActivation ? "変更中..." : currentIsActive ? "無効化する" : "有効化する"}
            </button>
          </div>
        </fieldset>
      )}
    </form>
  );
}
