# Retail Nexus

## 概要
Retail Nexusは、小売業における商品・在庫・販売データを一元管理し、業務の流れを可視化する業務管理プラットフォームです。

## 背景
小売業では、商品マスタ・在庫管理・販売データが分断されていることが多く、在庫差異や欠品、運用負荷の原因になります。  
Retail Nexusは、これらの情報を段階的に統合し、業務の流れを可視化できる状態を目指しています。

## 技術スタック
- Backend: ASP.NET Core / C#
- Frontend: Next.js / TypeScript
- Database: PostgreSQL
- Architecture: Clean Architecture

## ディレクトリ構成
- `backend`: API、アプリケーション層、ドメイン層、インフラ層
- `frontend`: UI、画面、API クライアント

## 現在の進捗
- 商品マスタ: 実装中
- 商品カテゴリマスタ: 実装済み
- 仕入先マスタ: 実装済み

## 今後の予定
- 店舗マスタ
- 在庫管理、在庫トランザクション
- 販売データ連携
- 棚卸機能
- ダッシュボード整備

## セットアップ
### Backend
- `backend` 配下で実行
- PostgreSQL を用意
- `RetailNexus.Api/appsettings.Development.json` を設定
- `dotnet ef database update`
- `dotnet run --project RetailNexus.Api`

### Frontend
- `frontend` 配下で実行
- `.env.local` を設定
- `npm install`
- `npm run dev`
