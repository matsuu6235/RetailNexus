# Retail Nexus

![CI](https://github.com/matthew113725/RetailNexus/actions/workflows/ci.yml/badge.svg)

小売業務の基幹データ（商品・仕入先・店舗・在庫・販売）を一元管理する業務管理プラットフォームです。
実務レベルのアーキテクチャ設計と実装を目的として開発しています。

## 技術スタック

| レイヤー | 技術 |
|---|---|
| バックエンド | ASP.NET Core 8.0 / C# |
| フロントエンド | Next.js 15 / React 19 / TypeScript |
| データベース | PostgreSQL 17 |
| ORM | Entity Framework Core 8 |
| バリデーション | FluentValidation 11 |
| 認証・認可 | JWT Bearer トークン / BCrypt.Net-Next / カスタム権限属性 |
| テスト | xUnit / FluentAssertions / Moq |

## アーキテクチャ

### バックエンド — クリーンアーキテクチャ

```
RetailNexus.Domain         依存なし。エンティティ定義のみ
RetailNexus.Application    ユースケース・リポジトリインターフェース
RetailNexus.Infrastructure EF Core実装・リポジトリ実装・JWTサービス
RetailNexus.Api            コントローラー・FluentValidation・認可属性・DI設定
RetailNexus.Tests          ユニットテスト（xUnit + FluentAssertions + Moq）
```

**設計上の判断:**

- **リポジトリパターン** — ドメイン層をORM実装から切り離し、テスタビリティを確保
- **FluentValidation** — バリデーションロジックをコントローラーから分離し、ルールを集約管理
- **EF Core Fluent API** — データアノテーションを使わずエンティティをPOCOに保つ
- **論理削除** — 全エンティティに `IsActive` フラグを持ち、物理削除を行わない
- **共通カラム** — 全ユーザー操作テーブルの末尾を `IsActive → UpdatedAt → UpdatedBy → CreatedAt → CreatedBy` で統一
- **権限ベースの認可** — `[RequirePermission("products.create")]` 属性でアクション単位の権限制御。ロールへの権限割り当てはDB管理で、コード変更なしに運用画面から変更可能
- **論理削除の権限分離** — 編集（`*.edit`）と論理削除（`*.delete`）を分離。論理削除は専用エンドポイント `PUT {id}/activation` で操作
- **監査ログ** — `SaveChangesAsync` オーバーライドで全エンティティの変更を自動記録。PasswordHash等の機密情報は除外
- **パスワード** — BCryptでハッシュ化。平文保存・平文ログ記録は行わない

### フロントエンド — Next.js App Router

```
src/
  app/               ルーティング専用（各機能の page.tsx）
  components/        共通レイアウト（権限ベースのサイドバー）・テーブル・モーダル
  lib/api/           エンティティごとのAPIクライアント（fetch + JWT自動付与）
  lib/validators/    フロントエンドバリデーター（バックエンドと同一ルールで実装）
  services/          認証サービス（ログイン・トークン・権限管理）
  types/             APIレスポンス型定義
```

**設計上の判断:**

- **バリデーターの分離** — バックエンド同様、バリデーションロジックをページコンポーネントから分離
- **リアルタイムバリデーション** — `onChange` のたびに該当フィールドのみ検証し、UXを向上
- **サーバーエラーのフィールドマッピング** — APIが返す `{"FieldName": ["message"]}` 形式をパースし、ユーザーへわかりやすく表示
- **権限ベースのUI制御** — サイドバーメニュー・編集モーダル内の有効状態変更を権限でフィルタリング
- **401/403エラーハンドリング** — セッション切れ・権限不足を日本語メッセージで表示

## 実装済み機能

| 機能 | 内容 |
|---|---|
| 認証 | ログイン・JWT発行（ロール・権限クレーム含む）・保護ルート |
| 認可 | ロール×権限モデル・カスタム認可属性 `[RequirePermission]`・33権限コード |
| 商品マスタ | 一覧・新規作成・編集・論理削除（カテゴリ連携） |
| 商品カテゴリマスタ | 一覧・新規作成・編集・論理削除・表示順ドラッグ変更 |
| 仕入先マスタ | 一覧・新規作成・編集・論理削除 |
| エリアマスタ | 一覧・新規作成・編集・論理削除 |
| 店舗マスタ | 一覧・新規作成・編集・論理削除（エリア・店舗種別連携） |
| 店舗種別マスタ | 一覧・新規作成・編集・論理削除 |
| ユーザー管理 | 一覧・新規作成・編集・論理削除・パスワードリセット・ロール割り当て |
| ロール管理 | 一覧・新規作成・編集・論理削除・カテゴリ別権限チェックボックス |
| 監査ログ | 全エンティティの変更を自動記録・閲覧画面（フィルター・ページネーション・モーダル詳細表示） |
| テスト | ドメインエンティティ・ログインハンドラー・JWTサービス・バリデーター |

## 今後の予定

- 発注・仕入管理
- 在庫管理・店舗間移動・発注点管理
- 販売・返品・ロス
- 棚卸・レポート・CSV出力
- ダッシュボード（KPI可視化）

## セットアップ

### Docker（推奨）

Docker Desktop がインストールされていれば、1コマンドで全環境が起動します。

```bash
docker compose up --build
# API:       http://localhost:5150/swagger
# フロント:  http://localhost:3000
# DB:        localhost:5432 (postgres/postgres)
```

停止: `docker compose down`（データ保持）
全削除: `docker compose down -v`（DBデータも削除）

### ローカル開発（Docker なし）

#### 前提

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 17

#### バックエンド

```bash
cd backend
# appsettings.Development.json の ConnectionStrings.Default を編集
dotnet ef database update --project RetailNexus.Infrastructure --startup-project RetailNexus.Api
dotnet run --project RetailNexus.Api
# → http://localhost:5150/swagger
```

#### フロントエンド

```bash
cd frontend
cp .env.local.example .env.local   # NEXT_PUBLIC_API_BASE_URL=http://localhost:5150
npm install
npm run dev
# → http://localhost:3000
```

#### テスト

```bash
cd backend
dotnet test
```
