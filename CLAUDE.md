# CLAUDE.md

このファイルは、リポジトリ内のコードを操作する際にClaude Code（claude.ai/code）へのガイダンスを提供します。

## プロジェクト概要

**Retail Nexus** は、商品・在庫・販売データを一元管理するための小売業務管理プラットフォームです。

- **バックエンド**: C# / ASP.NET Core 8.0（.NET 8 SDK）
- **フロントエンド**: TypeScript / Next.js 15 + React 19
- **データベース**: PostgreSQL 17（localhost:5432、DB名: `retailnexus`）

## コマンド

### バックエンド（`backend/` ディレクトリで実行）

```bash
dotnet build                                                                                    # ソリューションのビルド
dotnet run --project RetailNexus.Api                                                            # API起動（http://localhost:5150）
dotnet test                                                                                     # テスト実行（xUnit + FluentAssertions + Moq）
dotnet ef migrations add <マイグレーション名> --project RetailNexus.Infrastructure --startup-project RetailNexus.Api  # マイグレーション作成
dotnet ef database update --project RetailNexus.Infrastructure --startup-project RetailNexus.Api                     # マイグレーション適用
```

> **注意**: `dotnet ef migrations add` 実行前に API を停止すること。起動中はDLLがロックされビルドが失敗する。

起動中は `http://localhost:5150/swagger` でSwagger UIが利用可能。

### フロントエンド（`frontend/` ディレクトリで実行）

```bash
npm install       # 依存関係のインストール
npm run dev       # 開発サーバー起動（http://localhost:3000）
npm run build     # 本番ビルド
npm run lint      # リント実行
```

フロントエンドは `.env.local` の `NEXT_PUBLIC_API_BASE_URL`（デフォルト: `http://localhost:5150`）でAPIと接続。

## アーキテクチャ

### バックエンド — クリーンアーキテクチャ

```
RetailNexus.Domain         → エンティティ（Entities/）+ Enum（Enums/）（依存なし）
RetailNexus.Application    → ユースケース、リポジトリインターフェース
RetailNexus.Infrastructure → EF Core DbContext、リポジトリ実装、JWTサービス
RetailNexus.Api            → コントローラー、FluentValidationバリデーター、認可属性、DI設定（Program.cs）
RetailNexus.Tests          → ユニットテスト（xUnit + FluentAssertions + Moq）
```

**主なパターン:**

- **リポジトリパターン**: インターフェースは `Application/Interfaces/`、実装は `Infrastructure/Repositories/`
- **EF Core設定**: エンティティごとのFluentAPI設定は `Infrastructure/Persistence/Configurations/`（データアノテーション不使用、エンティティをPOCOに保つ）
- **バリデーション**: FluentValidationバリデーターは `Api/Validators/` に配置
- **認証**: JWT Bearerトークン。`Infrastructure/Security/JwtService` が `UserId`・`Email`・`Role`・`Permission` クレームを含むトークンを生成
- **認可**: カスタム属性 `[RequirePermission("products.create")]` で権限コード単位の認可制御。`Api/Authorization/` に実装
- **パスワード**: BCrypt.Net-Next でハッシュ化。平文保存は行わない

全エンティティは `CreatedAt`・`CreatedBy`・`UpdatedAt`・`UpdatedBy` タイムスタンプ/監査カラムと `IsActive` 論理削除フラグを持つ。物理削除は行わない。明細エンティティ（PurchaseOrderDetail, StoreRequestDetail）も同様に監査カラムを持つ。

### 認可の仕組み

**��限モデル**: `User ──< UserRole >── Role ──< RolePermission >── Permission`

- 権限の粒度は「画面 × 操作（view / create / edit / delete / approve）」
- `*.view` / `*.create` / `*.edit` は各CRUD操作に対応
- `*.delete` は論理削除（IsActive切り替え）に対応。編集権限とは分離されている
- `*.approve` は承認操作に対応（発注・発送依頼の承認/差戻し）
- 論理削除は専用エンドポイント `PUT /api/{entity}/{id}/activation` で操作

**バックエンド:**
- JWTトークンのクレームにユーザーの全権限コードを含める
- `[RequirePermission("products.create")]` でコントローラーのアクション単位で認可
- `PermissionPolicyProvider` がポリシーを動的生成、`PermissionAuthorizationHandler` がJWTクレームから権限を検証

**フロントエンド:**
- ログイン時にロール・権限一覧を localStorage に保存
- `hasPermission("products.create")` ユーティリティ関数で UI の出し分け
- `AppShell` のサイドバーメニューを権限でフィルタリング
- 編集モーダル内の「有効状態の変更」セクションは `*.delete` 権限時のみ表示

### 発注・発送依頼（ヘッダ+明細パターン）

**エンティティ構成**: ヘッダ（PurchaseOrder / StoreRequest）+ 明細（PurchaseOrderDetail / StoreRequestDetail）

**承認フロー**: Draft → AwaitingApproval → Approved → 後続ステータス遷移
- 承認申請: `PUT {id}/submit`（`*.edit` 権限）
- 承認: `PUT {id}/approve`（`*.approve` 権限）— 承認者ID・日時を記録
- 差戻し: `PUT {id}/reject`（`*.approve` 権限）— Draft に戻す

**明細の個別更新方式**: 更新時、フロントからの明細リストで `DetailId` が null なら INSERT（`DbSet.Add` で明示的に Added）、値ありなら UPDATE、送信されなかった既存行は DELETE

**自動採番**: PO-000001（発注）、SR-000001（発送依頼）

**Enum**: `Domain/Enums/` に配置（`PurchaseOrderStatus`, `StoreRequestStatus`）

### フロントエンド — Next.js App Router

```
src/
  app/                 → ルーティング専用
    (機能ルート)/        → products, suppliers, product-categories, areas, stores, store-types, users, roles, login, dashboard, purchase-orders, store-requests
  components/
    layout/            → AppShell（権限ベースのサイドバー）
    table/             → MasterTable（共有テーブルスタイル）
    modal/             → Modal、FormModal（共有モーダルスタイル）
  lib/
    api/               → エンティティごとのHTTPクライアント（client.ts がJWT付与・エラー処理を共通化）
    validators/        → フロントエンドバリデーター（バックエンドと同一ルールで実装）
    utils/             → ユーティリティ関数
  services/            → authService（ログイン・トークン・権限管理）
  types/               → 各エンティティのTypeScriptインターフェース
```

`tsconfig.json` に `@/*` → `src/*` のパスエイリアスを設定済み。importは `@/lib/api/client` のように記述する。

**主なパターン:**

- デフォルトはサーバーコンポーネント。インタラクティブなコンポーネント（フォーム等）にのみ `"use client"` を付与
- JWTアクセストークンは `localStorage` の `"accessToken"` に保存。`lib/api/client.ts` が全リクエストに自動付与
- `/auth/login` 以外の全APIエンドポイントは `Authorization: Bearer <token>` ヘッダーが必要
- `client.ts` は 401（セッション切れ）・403（権限不足）を日本語メッセージで返す
- **リアルタイムバリデーション**: `handleChange` 内でバリデーター関数を呼び出し、変更したフィールドのみエラーを更新する
- **サーバーエラー表示**: `client.ts` がAPIの `{"FieldName": ["message"]}` 形式をパースし、メッセージ文字列のみを `Error` としてスローする

### バリデーションパターン（フォームページ共通）

```typescript
// handleChange でリアルタイムバリデーション
const handleChange = (field: keyof XxxRequest, value: string | boolean) => {
  const updatedForm = { ...form, [field]: value };
  setForm(updatedForm);
  const errors = validateXxx(updatedForm);
  setFieldErrors((prev) => ({ ...prev, [field]: errors[field as keyof XxxFieldErrors] }));
};

// 送信時はバリデーター関数で全フィールド検証
const errors = validateXxx(form);
if (Object.keys(errors).length > 0) { setFieldErrors(errors); return; }
```

`products/new` は `price`・`cost` が `number` 型のため、`handleChange` 内で数値変換が必要。

### コードルール

- エリアコード・店舗種別コード・店舗コード・商品カテゴリコードは**数字のみ**（バックエンド・フロントエンド双方でバリデーション）
- JANコードは**13桁の数字のみ**（任意入力）
- コードフィールドのバリデーション順序: 必須チェック → 桁数 → 数字のみ → 重複チェック（非同期）
- エンティティの `Update()` メソッドは `IsActive` を含まない。論理削除は `SetActivation()` で分離
- コントローラーの `UpdateRequest` レコードは `IsActive` を含まない。論理削除は `PUT {id}/activation` + `*.delete` 権限で制御
- 発注・発送依頼の明細で同一商品の重複行は許可しない（バックエンド・フロントエンド双方でバリデーション）
- 発注・発送依頼の日付フィールド（発注日・希望到着日等）は過去日付を選択不可（フロントエンドの `min` 属性で制御）
