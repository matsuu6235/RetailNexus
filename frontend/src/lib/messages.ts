// ========== バリデーションメッセージ ==========
export const validation = {
  required: (field: string) => `${field}は必須です。`,
  maxLength: (field: string, max: number) => `${field}は${max}文字以内で入力してください。`,
  minLength: (field: string, min: number) => `${field}は${min}文字以上で入力してください。`,
  exactLength: (field: string, len: number) => `${field}は${len}桁で入力してください。`,
  digitsOnly: (field: string) => `${field}は数字のみ入力できます。`,
  minValue: (field: string, min: number) => `${field}は${min}以上で入力してください。`,
  greaterThan: (field: string, min: number) =>
    `${field}は${min}以上の整数で入力してください。`,
  emailFormat: "メールアドレスの形式が正しくありません。",
  phoneFormat: (field: string) =>
    `${field}は数字とハイフン（-）のみ入力できます。`,
  alphaRange: (field: string, min: number, max: number) =>
    `${field}は英字${min}〜${max}文字で入力してください。`,
  sameStore: "依頼元と依頼先は異なる店舗を選択してください。",
  duplicateProduct: "同一商品が複数行に含まれています。数量を変更してください。",
  listMinCount: (entity: string) => `${entity}を1行以上入力してください。`,
} as const;

// ========== API/HTTPエラー ==========
export const httpError = {
  sessionExpired:
    "ログインセッションが切れました。再度ログインしてください。",
  forbidden: "アクセス権限がありません。",
  serverError:
    "サーバーエラーが発生しました。しばらく時間をおいて再度お試しください。",
  unknownError: (status: number) => `エラーが発生しました。(${status})`,
  loginFailed: "ログインIDまたはパスワードが正しくありません。",
} as const;

// ========== 操作フォールバック ==========
export const fallback = {
  fetchFailed: (entity: string) => `${entity}の取得に失敗しました。`,
  listFetchFailed: (entity: string) => `${entity}一覧の取得に失敗しました。`,
  createFailed: (entity: string) => `${entity}の作成に失敗しました。`,
  updateFailed: (entity: string) => `${entity}の更新に失敗しました。`,
  saveFailed: "保存に失敗しました。",
  activationFailed: "状態の変更に失敗しました。",
  reorderFailed: "表示順の保存に失敗しました。",
  operationFailed: "操作に失敗しました。",
  masterFetchFailed: "マスタデータの取得に失敗しました。",
  loginFailed: "ログインに失敗しました。",
  searchFetchFailed: "検索条件の取得に失敗しました。",
  messageSendFailed: "メッセージの送信に失敗しました。",
  messageFetchFailed: "メッセージの取得に失敗しました。",
} as const;
