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
RetailNexus.Domain         → エンティティのみ（依存なし）
RetailNexus.Application    → ユースケース、リポジトリインターフェース
RetailNexus.Infrastructure → EF Core DbContext、リポジトリ実装、JWTサービス
RetailNexus.Api            → コントローラー、FluentValidationバリデーター、DI設定（Program.cs）
```

**主なパターン:**

- **リポジトリパターン**: インターフェースは `Application/Interfaces/`、実装は `Infrastructure/Repositories/`
- **EF Core設定**: エンティティごとのFluentAPI設定は `Infrastructure/Persistence/Configurations/`（データアノテーション不使用、エンティティをPOCOに保つ）
- **バリデーション**: FluentValidationバリデーターは `Api/Validators/` に配置
- **認証**: JWT Bearerトークン。`Infrastructure/Services/JwtService` が `UserId`・`Email`・`Role` クレームを含むトークンを生成

全エンティティは `CreatedAt`・`UpdatedAt` タイムスタンプと `IsActive` 論理削除フラグを持つ。物理削除は行わない。

### フロントエンド — Next.js App Router

```
src/
  app/                 → ルーティング専用
    (機能ルート)/        → products, suppliers, product-categories, areas, stores, store-types, login, dashboard
  components/
    layout/            → AppShell、AppHeader
    table/             → MasterTable（共有テーブルスタイル）
  lib/
    api/               → エンティティごとのHTTPクライアント（client.ts がJWT付与・エラー処理を共通化）
    validators/        → フロントエンドバリデーター（バックエンドと同一ルールで実装）
    utils/             → ユーティリティ関数
  services/            → authService（ログイン・トークン管理）
  types/               → 各エンティティのTypeScriptインターフェース
```

`tsconfig.json` に `@/*` → `src/*` のパスエイリアスを設定済み。importは `@/lib/api/client` のように記述する。

**主なパターン:**

- デフォルトはサーバーコンポーネント。インタラクティブなコンポーネント（フォーム等）にのみ `"use client"` を付与
- JWTアクセストークンは `localStorage` の `"accessToken"` に保存。`lib/api/client.ts` が全リクエストに自動付与
- `/auth/login` 以外の全APIエンドポイントは `Authorization: Bearer <token>` ヘッダーが必要
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
