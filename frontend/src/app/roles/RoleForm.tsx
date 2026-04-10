"use client";

import { useState } from "react";
import { fallback } from "@/lib/messages";
import { createRole, getRoleById, updateRole, getPermissions, changeRoleActivation } from "@/lib/api/roles";
import type { Permission } from "@/types/roles";
import { useMasterForm, type MasterFormProps } from "@/lib/hooks/useMasterForm";
import ActivationFieldset from "@/components/form/ActivationFieldset";
import styles from "@/components/modal/FormModal.module.css";

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

export default function RoleForm({ mode, editId, onSave, onCancel }: MasterFormProps) {
  const [allPermissions, setAllPermissions] = useState<Permission[]>([]);

  const { form, loading, submitting, error, fieldErrors, activation, handleChange, handleSubmit } =
    useMasterForm<FormData, FieldErrors>({
      mode,
      editId,
      initialForm: { roleName: "", description: "", permissionIds: [] },
      entityName: "ロール",
      validator: validate,
      load: async (id) => {
        const perms = await getPermissions();
        setAllPermissions(perms);

        if (!id) return undefined;
        const role = await getRoleById(id);
        return {
          form: {
            roleName: role.roleName,
            description: role.description ?? "",
            permissionIds: role.permissions.map((p) => p.permissionId),
          },
          isActive: role.isActive,
        };
      },
      save: async (f) => {
        const body = {
          roleName: f.roleName.trim(),
          description: f.description.trim() || null,
          permissionIds: f.permissionIds,
        };
        if (mode === "create") await createRole(body);
        else await updateRole(editId!, body);
      },
      onSave,
      activation: { permissionCode: "roles.delete", changeFn: changeRoleActivation },
    });

  const togglePermission = (permId: string) => {
    const next = form.permissionIds.includes(permId)
      ? form.permissionIds.filter((id) => id !== permId)
      : [...form.permissionIds, permId];
    handleChange("permissionIds", next as FormData["permissionIds"]);
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
    handleChange("permissionIds", next as FormData["permissionIds"]);
  };

  if (loading) return <p>読み込み中...</p>;

  const grouped = groupByCategory(allPermissions);

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <label className={styles.field}>
        <span>ロール名 *</span>
        <input value={form.roleName} onChange={(e) => handleChange("roleName", e.target.value as string)} className={styles.input} />
        {fieldErrors.roleName && <small className={styles.errorText}>{fieldErrors.roleName}</small>}
      </label>

      <label className={styles.field}>
        <span>説明</span>
        <input value={form.description} onChange={(e) => handleChange("description", e.target.value as string)} className={styles.input} />
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

      {(error || activation.error) && <div className={styles.errorBox}>{error || activation.error}</div>}

      <div className={styles.actions}>
        <button type="button" onClick={onCancel} className={styles.cancelButton}>
          キャンセル
        </button>
        <button type="submit" disabled={submitting} className={styles.submitButton}>
          {mode === "create" ? (submitting ? "作成中..." : "作成") : (submitting ? "更新中..." : "更新")}
        </button>
      </div>

      {mode === "edit" && activation.canDelete && (
        <ActivationFieldset currentIsActive={activation.currentIsActive} changingActivation={activation.changingActivation} toggle={activation.toggle} />
      )}
    </form>
  );
}
