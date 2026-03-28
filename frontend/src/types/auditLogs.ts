export type AuditLog = {
  auditLogId: string;
  userId: string | null;
  userName: string;
  action: string;
  entityName: string;
  entityId: string;
  oldValues: string | null;
  newValues: string | null;
  timestamp: string;
};
