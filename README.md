# Retail Nexus
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
| 認証 | JWT Bearer トークン |

## アーキテクチャ

### バックエンド — クリーンアーキテクチャ

```
RetailNexus.Domain         依存なし。エンティティ定義のみ
RetailNexus.Application    ユースケース・リポジトリインターフェース
RetailNexus.Infrastructure EF Core実装・リポジトリ実装・JWTサービス
RetailNexus.Api            コントローラー・FluentValidation・DI設定
```

**設計上の判断:**

- **リポジトリパターン** — ドメイン層をORM実装から切り離し、テスタビリティを確保
- **FluentValidation** — バリデーションロジックをコントローラーから分離し、ルールを集約管理
- **EF Core Fluent API** — データアノテーションを使わずエンティティをPOCOに保つ
- **論理削除** — 全エンティティに `IsActive` フラグを持ち、物理削除を行わない
- **タイムスタンプ** — 全エンティティに `CreatedAt` / `UpdatedAt` を付与

### フロントエンド — Next.js App Router

```
src/app/
  lib/api/         エンティティごとのAPIクライアント（fetch + JWT自動付与）
  lib/validators/  フロントエンドバリデーター（バックエンドと同一ルールで実装）
  types/           APIレスポンス型定義
  components/      共通レイアウト・機能別コンポーネント
  (各機能)/        page.tsx（新規作成・編集・一覧）
```

**設計上の判断:**

- **バリデーターの分離** — バックエンド同様、バリデーションロジックをページコンポーネントから分離
- **リアルタイムバリデーション** — `onChange` のたびに該当フィールドのみ検証し、UXを向上
- **サーバーエラーのフィールドマッピング** — APIが返す `{"FieldName": ["message"]}` 形式をパースし、ユーザーへわかりやすく表示

## 実装済み機能

| 機能 | 内容 |
|---|---|
| 認証 | ログイン・JWT発行・保護ルート |
| 商品マスタ | 一覧・新規作成（カテゴリ連携） |
| 商品カテゴリマスタ | 一覧・新規作成・編集・表示順ドラッグ変更 |
| 仕入先マスタ | 一覧・新規作成・編集 |
| エリアマスタ | 一覧・新規作成・編集 |
| 店舗マスタ | 一覧・新規作成・編集（エリア・店舗種別連携） |
| 店舗種別マスタ | 一覧・新規作成・編集 |

## 今後の予定

- 商品マスタの編集・削除
- 在庫管理・入出庫トランザクション
- 棚卸機能
- 販売データ連携
- ダッシュボード（KPI可視化）

## セットアップ

### 前提

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 17

### バックエンド

```bash
cd backend
# appsettings.Development.json の ConnectionStrings.Default を編集
dotnet ef database update --project RetailNexus.Infrastructure --startup-project RetailNexus.Api
dotnet run --project RetailNexus.Api
# → http://localhost:5150/swagger
```

### フロントエンド

```bash
cd frontend
cp .env.local.example .env.local   # NEXT_PUBLIC_API_BASE_URL=http://localhost:5150
npm install
npm run dev
# → http://localhost:3000
```
cal` を設定
- `npm install`
- `npm run dev`
