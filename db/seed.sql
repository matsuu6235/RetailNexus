-- RetailNexus シードデータ
-- 実行前にマイグレーション適用済みであること

DO $$
DECLARE
    admin_id UUID;
    -- Area IDs
    area_hokkaido UUID; area_tohoku UUID; area_kanto UUID; area_hokuriku UUID; area_chubu UUID;
    area_kansai UUID; area_chugoku UUID; area_shikoku UUID; area_kyushu UUID; area_okinawa UUID;
    -- StoreType IDs
    st_chokuei UUID; st_fc UUID; st_ogata UUID; st_kogata UUID; st_souko UUID;
    st_ec UUID; st_honbu UUID; st_outlet UUID; st_showroom UUID; st_logistics UUID;
    -- ProductCategory IDs
    cat_fd UUID; cat_bv UUID; cat_cn UUID; cat_dg UUID; cat_st UUID;
    cat_cl UUID; cat_el UUID; cat_cs UUID; cat_pt UUID; cat_sp UUID;
    -- Supplier IDs
    sup_ids UUID[];
    -- temp
    i INT;
    j INT;
    area_ids UUID[];
    st_ids UUID[];
    store_names TEXT[];
    store_areas INT[];
    store_types INT[];
    product_name TEXT;
    jan_prefix TEXT;
BEGIN
    -- =====================================================
    -- 管理者ユーザー取得/作成
    -- =====================================================
    SELECT user_id INTO admin_id FROM users WHERE login_id = 'admin';
    IF admin_id IS NULL THEN
        admin_id := gen_random_uuid();
        INSERT INTO users (user_id, login_id, user_name, email, password_hash, is_active, created_at, updated_at)
        VALUES (admin_id, 'admin', '管理者', 'admin@example.com', 'password123', true, now(), now());
    END IF;

    -- =====================================================
    -- エリア（10地区）
    -- =====================================================
    area_hokkaido := gen_random_uuid(); area_tohoku := gen_random_uuid(); area_kanto := gen_random_uuid();
    area_hokuriku := gen_random_uuid(); area_chubu := gen_random_uuid(); area_kansai := gen_random_uuid();
    area_chugoku := gen_random_uuid(); area_shikoku := gen_random_uuid(); area_kyushu := gen_random_uuid();
    area_okinawa := gen_random_uuid();

    INSERT INTO areas (area_id, area_cd, area_name, display_order, is_active, created_by, updated_by, created_at, updated_at) VALUES
        (area_hokkaido, '01', '北海道',  1, true, admin_id, admin_id, now(), now()),
        (area_tohoku,   '02', '東北',    2, true, admin_id, admin_id, now(), now()),
        (area_kanto,    '03', '関東',    3, true, admin_id, admin_id, now(), now()),
        (area_hokuriku, '04', '北陸',    4, true, admin_id, admin_id, now(), now()),
        (area_chubu,    '05', '中部',    5, true, admin_id, admin_id, now(), now()),
        (area_kansai,   '06', '関西',    6, true, admin_id, admin_id, now(), now()),
        (area_chugoku,  '07', '中国',    7, true, admin_id, admin_id, now(), now()),
        (area_shikoku,  '08', '四国',    8, true, admin_id, admin_id, now(), now()),
        (area_kyushu,   '09', '九州',    9, true, admin_id, admin_id, now(), now()),
        (area_okinawa,  '10', '沖縄',   10, true, admin_id, admin_id, now(), now());

    area_ids := ARRAY[area_hokkaido, area_tohoku, area_kanto, area_hokuriku, area_chubu,
                      area_kansai, area_chugoku, area_shikoku, area_kyushu, area_okinawa];

    -- =====================================================
    -- 店舗種別（10種）
    -- =====================================================
    st_chokuei := gen_random_uuid(); st_fc := gen_random_uuid(); st_ogata := gen_random_uuid();
    st_kogata := gen_random_uuid(); st_souko := gen_random_uuid(); st_ec := gen_random_uuid();
    st_honbu := gen_random_uuid(); st_outlet := gen_random_uuid(); st_showroom := gen_random_uuid();
    st_logistics := gen_random_uuid();

    INSERT INTO store_types (store_type_id, store_type_cd, store_type_name, display_order, is_active, created_by, updated_by, created_at, updated_at) VALUES
        (st_chokuei,  '01', '直営店',       1, true, admin_id, admin_id, now(), now()),
        (st_fc,       '02', 'FC店',         2, true, admin_id, admin_id, now(), now()),
        (st_ogata,    '03', '大型店',       3, true, admin_id, admin_id, now(), now()),
        (st_kogata,   '04', '小型店',       4, true, admin_id, admin_id, now(), now()),
        (st_souko,    '05', '倉庫',         5, true, admin_id, admin_id, now(), now()),
        (st_ec,       '06', 'EC拠点',       6, true, admin_id, admin_id, now(), now()),
        (st_honbu,    '07', '本部',         7, true, admin_id, admin_id, now(), now()),
        (st_outlet,   '08', 'アウトレット', 8, true, admin_id, admin_id, now(), now()),
        (st_showroom, '09', 'ショールーム', 9, true, admin_id, admin_id, now(), now()),
        (st_logistics,'10', '物流センター',10, true, admin_id, admin_id, now(), now());

    st_ids := ARRAY[st_chokuei, st_fc, st_ogata, st_kogata, st_souko,
                    st_ec, st_honbu, st_outlet, st_showroom, st_logistics];

    -- =====================================================
    -- 商品カテゴリ（10分類）
    -- =====================================================
    cat_fd := gen_random_uuid(); cat_bv := gen_random_uuid(); cat_cn := gen_random_uuid();
    cat_dg := gen_random_uuid(); cat_st := gen_random_uuid(); cat_cl := gen_random_uuid();
    cat_el := gen_random_uuid(); cat_cs := gen_random_uuid(); cat_pt := gen_random_uuid();
    cat_sp := gen_random_uuid();

    INSERT INTO product_categories (product_category_id, product_category_cd, category_abbreviation, product_category_name, display_order, is_active, created_by, updated_by, created_at, updated_at) VALUES
        (cat_fd, '01', 'FD', '食品',         1, true, admin_id, admin_id, now(), now()),
        (cat_bv, '02', 'BV', '飲料',         2, true, admin_id, admin_id, now(), now()),
        (cat_cn, '03', 'CN', '菓子',         3, true, admin_id, admin_id, now(), now()),
        (cat_dg, '04', 'DG', '日用品',       4, true, admin_id, admin_id, now(), now()),
        (cat_st, '05', 'ST', '文房具',       5, true, admin_id, admin_id, now(), now()),
        (cat_cl, '06', 'CL', '衣料品',       6, true, admin_id, admin_id, now(), now()),
        (cat_el, '07', 'EL', '家電',         7, true, admin_id, admin_id, now(), now()),
        (cat_cs, '08', 'CS', '化粧品',       8, true, admin_id, admin_id, now(), now()),
        (cat_pt, '09', 'PT', 'ペット用品',   9, true, admin_id, admin_id, now(), now()),
        (cat_sp, '10', 'SP', 'スポーツ用品',10, true, admin_id, admin_id, now(), now());

    -- =====================================================
    -- 仕入先（10社）
    -- =====================================================
    FOR i IN 1..10 LOOP
        sup_ids[i] := gen_random_uuid();
    END LOOP;

    INSERT INTO suppliers (supplier_id, supplier_code, supplier_name, phone_number, email, is_active, created_by, updated_by, created_at, updated_at) VALUES
        (sup_ids[1],  '00001', '山田食品工業株式会社',     '03-3456-7890', 'info@yamada-foods.co.jp',    true, admin_id, admin_id, now(), now()),
        (sup_ids[2],  '00002', '東京飲料株式会社',         '03-5678-1234', 'sales@tokyo-beverage.co.jp', true, admin_id, admin_id, now(), now()),
        (sup_ids[3],  '00003', '大阪菓子製造株式会社',     '06-1234-5678', 'order@osaka-kashi.co.jp',    true, admin_id, admin_id, now(), now()),
        (sup_ids[4],  '00004', '日本日用品株式会社',       '045-234-5678', 'info@nihon-nichiyou.co.jp',  true, admin_id, admin_id, now(), now()),
        (sup_ids[5],  '00005', 'サクラ文具株式会社',       '048-345-6789', 'sales@sakura-bungu.co.jp',   true, admin_id, admin_id, now(), now()),
        (sup_ids[6],  '00006', 'ファッションリンク株式会社','052-456-7890', 'info@fashion-link.co.jp',    true, admin_id, admin_id, now(), now()),
        (sup_ids[7],  '00007', 'テクノライフ株式会社',     '06-5678-9012', 'support@technolife.co.jp',   true, admin_id, admin_id, now(), now()),
        (sup_ids[8],  '00008', 'ビューティーワールド株式会社','03-6789-0123','order@beauty-world.co.jp',  true, admin_id, admin_id, now(), now()),
        (sup_ids[9],  '00009', 'ペットファミリー株式会社', '044-789-0123', 'info@pet-family.co.jp',      true, admin_id, admin_id, now(), now()),
        (sup_ids[10], '00010', 'スポーツプラネット株式会社','078-890-1234', 'sales@sports-planet.co.jp',  true, admin_id, admin_id, now(), now());

    -- =====================================================
    -- 店舗（50店舗）
    -- =====================================================
    INSERT INTO stores (store_id, store_cd, store_name, area_id, store_type_id, is_active, created_by, updated_by, created_at, updated_at) VALUES
        -- 北海道（3店）
        (gen_random_uuid(), '000001', '札幌中央店',     area_hokkaido, st_chokuei, true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000002', '旭川店',         area_hokkaido, st_fc,      true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000003', '函館店',         area_hokkaido, st_kogata,  true, admin_id, admin_id, now(), now()),
        -- 東北（4店）
        (gen_random_uuid(), '000004', '仙台駅前店',     area_tohoku, st_chokuei, true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000005', '盛岡店',         area_tohoku, st_fc,      true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000006', '秋田店',         area_tohoku, st_kogata,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000007', '郡山店',         area_tohoku, st_fc,      true, admin_id, admin_id, now(), now()),
        -- 関東（10店）
        (gen_random_uuid(), '000008', '東京本部',       area_kanto, st_honbu,    true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000009', '新宿店',         area_kanto, st_ogata,    true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000010', '渋谷店',         area_kanto, st_chokuei,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000011', '池袋店',         area_kanto, st_chokuei,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000012', '横浜店',         area_kanto, st_ogata,    true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000013', 'さいたま店',     area_kanto, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000014', '千葉店',         area_kanto, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000015', '立川店',         area_kanto, st_kogata,   true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000016', '関東EC倉庫',     area_kanto, st_ec,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000017', '関東物流センター', area_kanto, st_logistics, true, admin_id, admin_id, now(), now()),
        -- 北陸（3店）
        (gen_random_uuid(), '000018', '金沢店',         area_hokuriku, st_chokuei, true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000019', '新潟店',         area_hokuriku, st_fc,      true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000020', '富山店',         area_hokuriku, st_kogata,  true, admin_id, admin_id, now(), now()),
        -- 中部（6店）
        (gen_random_uuid(), '000021', '名古屋栄店',     area_chubu, st_ogata,    true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000022', '名古屋駅前店',   area_chubu, st_chokuei,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000023', '静岡店',         area_chubu, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000024', '浜松店',         area_chubu, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000025', '岐阜店',         area_chubu, st_kogata,   true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000026', '中部倉庫',       area_chubu, st_souko,    true, admin_id, admin_id, now(), now()),
        -- 関西（8店）
        (gen_random_uuid(), '000027', '大阪梅田店',     area_kansai, st_ogata,    true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000028', '大阪なんば店',   area_kansai, st_chokuei,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000029', '京都店',         area_kansai, st_chokuei,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000030', '神戸店',         area_kansai, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000031', '奈良店',         area_kansai, st_kogata,   true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000032', '関西アウトレット', area_kansai, st_outlet,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000033', '関西EC倉庫',     area_kansai, st_ec,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000034', '関西物流センター', area_kansai, st_logistics, true, admin_id, admin_id, now(), now()),
        -- 中国（4店）
        (gen_random_uuid(), '000035', '広島店',         area_chugoku, st_chokuei, true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000036', '岡山店',         area_chugoku, st_fc,      true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000037', '山口店',         area_chugoku, st_kogata,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000038', '中国ショールーム', area_chugoku, st_showroom, true, admin_id, admin_id, now(), now()),
        -- 四国（3店）
        (gen_random_uuid(), '000039', '高松店',         area_shikoku, st_chokuei, true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000040', '松山店',         area_shikoku, st_fc,      true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000041', '高知店',         area_shikoku, st_kogata,  true, admin_id, admin_id, now(), now()),
        -- 九州（6店）
        (gen_random_uuid(), '000042', '福岡天神店',     area_kyushu, st_ogata,    true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000043', '福岡博多店',     area_kyushu, st_chokuei,  true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000044', '北九州店',       area_kyushu, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000045', '熊本店',         area_kyushu, st_fc,       true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000046', '鹿児島店',       area_kyushu, st_kogata,   true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000047', '九州倉庫',       area_kyushu, st_souko,    true, admin_id, admin_id, now(), now()),
        -- 沖縄（3店）
        (gen_random_uuid(), '000048', '那覇店',         area_okinawa, st_chokuei, true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000049', '北谷店',         area_okinawa, st_fc,      true, admin_id, admin_id, now(), now()),
        (gen_random_uuid(), '000050', '沖縄アウトレット', area_okinawa, st_outlet, true, admin_id, admin_id, now(), now());

    -- =====================================================
    -- 商品（1000品）- カテゴリ別に100品ずつ
    -- =====================================================

    -- 食品（FD-000001 ~ FD-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'こしひかり ' || CEIL(i::float / 20) || 'kg'
            WHEN 2  THEN 'あきたこまち ' || CEIL(i::float / 20) || 'kg'
            WHEN 3  THEN '讃岐うどん ' || (i * 10) || 'g'
            WHEN 4  THEN '信州そば ' || (i * 10) || 'g'
            WHEN 5  THEN 'スパゲッティ ' || (i * 5) || 'g'
            WHEN 6  THEN '醤油 ' || (i * 10) || 'ml'
            WHEN 7  THEN '味噌 ' || (i * 5) || 'g'
            WHEN 8  THEN '食塩 ' || (i * 5) || 'g'
            WHEN 9  THEN '砂糖 ' || (i * 10) || 'g'
            WHEN 10 THEN 'オリーブオイル ' || (i * 5) || 'ml'
            WHEN 11 THEN 'カレールウ ' || (i * 3) || '皿分'
            WHEN 12 THEN 'シチュールウ ' || (i * 3) || '皿分'
            WHEN 13 THEN 'ツナ缶 ' || CEIL(i::float / 20) || '個入'
            WHEN 14 THEN 'サバ缶 ' || CEIL(i::float / 20) || '個入'
            WHEN 15 THEN 'トマト缶 ' || (i * 10) || 'g'
            WHEN 16 THEN 'レトルトカレー ' || (i * 5) || 'g'
            WHEN 17 THEN 'インスタントラーメン ' || CEIL(i::float / 20) || '食入'
            WHEN 18 THEN 'パン粉 ' || (i * 10) || 'g'
            WHEN 19 THEN '小麦粉 ' || (i * 10) || 'g'
            WHEN 20 THEN 'ホットケーキミックス ' || (i * 10) || 'g'
        END;
        jan_prefix := '490' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'FD-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '01',
                (100 + (i * 7) % 900)::decimal, (50 + (i * 5) % 500)::decimal, true, now(), now());
    END LOOP;

    -- 飲料（BV-000001 ~ BV-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN '緑茶 ' || (i * 5 + 300) || 'ml'
            WHEN 2  THEN 'ほうじ茶 ' || (i * 5 + 300) || 'ml'
            WHEN 3  THEN '烏龍茶 ' || (i * 5 + 300) || 'ml'
            WHEN 4  THEN '麦茶 ' || (i * 10 + 500) || 'ml'
            WHEN 5  THEN 'ミネラルウォーター ' || (i * 5 + 300) || 'ml'
            WHEN 6  THEN '炭酸水 ' || (i * 5 + 300) || 'ml'
            WHEN 7  THEN 'コーラ ' || (i * 5 + 300) || 'ml'
            WHEN 8  THEN 'オレンジジュース ' || (i * 10) || 'ml'
            WHEN 9  THEN 'りんごジュース ' || (i * 10) || 'ml'
            WHEN 10 THEN '野菜ジュース ' || (i * 5 + 100) || 'ml'
            WHEN 11 THEN 'トマトジュース ' || (i * 5 + 100) || 'ml'
            WHEN 12 THEN 'スポーツドリンク ' || (i * 5 + 300) || 'ml'
            WHEN 13 THEN 'エナジードリンク ' || (i * 3 + 200) || 'ml'
            WHEN 14 THEN '缶コーヒー ブラック ' || (i * 3 + 150) || 'ml'
            WHEN 15 THEN '缶コーヒー 微糖 ' || (i * 3 + 150) || 'ml'
            WHEN 16 THEN 'カフェラテ ' || (i * 3 + 200) || 'ml'
            WHEN 17 THEN '豆乳 ' || (i * 5 + 200) || 'ml'
            WHEN 18 THEN '牛乳 ' || (i * 10 + 500) || 'ml'
            WHEN 19 THEN 'ヨーグルトドリンク ' || (i * 3 + 100) || 'ml'
            WHEN 20 THEN 'レモンティー ' || (i * 5 + 300) || 'ml'
        END;
        jan_prefix := '491' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'BV-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '02',
                (80 + (i * 3) % 400)::decimal, (40 + (i * 2) % 200)::decimal, true, now(), now());
    END LOOP;

    -- 菓子（CN-000001 ~ CN-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'チョコレート ' || (i * 5 + 30) || 'g'
            WHEN 2  THEN 'ミルクチョコレート ' || (i * 3 + 30) || 'g'
            WHEN 3  THEN 'ポテトチップス ' || (i * 3 + 50) || 'g'
            WHEN 4  THEN 'せんべい ' || CEIL(i::float / 10) || '枚入'
            WHEN 5  THEN 'クッキー ' || CEIL(i::float / 10) || '枚入'
            WHEN 6  THEN 'ビスケット ' || (i * 3 + 50) || 'g'
            WHEN 7  THEN 'グミ ' || (i * 2 + 30) || 'g'
            WHEN 8  THEN 'キャンディー ' || (i * 2 + 50) || 'g'
            WHEN 9  THEN 'ガム ' || CEIL(i::float / 10) || '粒入'
            WHEN 10 THEN 'プリン ' || CEIL(i::float / 10) || '個入'
            WHEN 11 THEN 'ゼリー ' || CEIL(i::float / 10) || '個入'
            WHEN 12 THEN 'アイスクリーム ' || (i * 5 + 50) || 'ml'
            WHEN 13 THEN 'ドーナツ ' || CEIL(i::float / 10) || '個入'
            WHEN 14 THEN 'ケーキ ' || CEIL(i::float / 20) || '号'
            WHEN 15 THEN 'まんじゅう ' || CEIL(i::float / 10) || '個入'
            WHEN 16 THEN 'ようかん ' || (i * 5 + 100) || 'g'
            WHEN 17 THEN 'どら焼き ' || CEIL(i::float / 10) || '個入'
            WHEN 18 THEN 'おかき ' || (i * 3 + 50) || 'g'
            WHEN 19 THEN 'ナッツミックス ' || (i * 3 + 30) || 'g'
            WHEN 20 THEN 'ドライフルーツ ' || (i * 3 + 30) || 'g'
        END;
        jan_prefix := '492' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'CN-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '03',
                (100 + (i * 5) % 500)::decimal, (50 + (i * 3) % 250)::decimal, true, now(), now());
    END LOOP;

    -- 日用品（DG-000001 ~ DG-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'ティッシュペーパー ' || CEIL(i::float / 10) || '箱入'
            WHEN 2  THEN 'トイレットペーパー ' || CEIL(i::float / 5) || 'ロール'
            WHEN 3  THEN 'キッチンペーパー ' || CEIL(i::float / 10) || 'ロール'
            WHEN 4  THEN '食器用洗剤 ' || (i * 10 + 200) || 'ml'
            WHEN 5  THEN '洗濯洗剤 ' || (i * 10 + 500) || 'ml'
            WHEN 6  THEN '柔軟剤 ' || (i * 10 + 400) || 'ml'
            WHEN 7  THEN 'ハンドソープ ' || (i * 5 + 200) || 'ml'
            WHEN 8  THEN 'ボディソープ ' || (i * 10 + 300) || 'ml'
            WHEN 9  THEN 'シャンプー ' || (i * 10 + 300) || 'ml'
            WHEN 10 THEN 'コンディショナー ' || (i * 10 + 300) || 'ml'
            WHEN 11 THEN '歯磨き粉 ' || (i * 5 + 50) || 'g'
            WHEN 12 THEN '歯ブラシ ' || CEIL(i::float / 20) || '本入'
            WHEN 13 THEN 'ゴミ袋 ' || (i + 10) || 'L ' || CEIL(i::float / 5) || '枚入'
            WHEN 14 THEN 'ラップ ' || (i * 5 + 10) || 'm'
            WHEN 15 THEN 'アルミホイル ' || (i * 3 + 5) || 'm'
            WHEN 16 THEN 'スポンジ ' || CEIL(i::float / 10) || '個入'
            WHEN 17 THEN '除菌スプレー ' || (i * 10 + 200) || 'ml'
            WHEN 18 THEN 'ウェットティッシュ ' || (i * 3 + 10) || '枚入'
            WHEN 19 THEN '綿棒 ' || (i * 10 + 50) || '本入'
            WHEN 20 THEN '絆創膏 ' || (i * 3 + 10) || '枚入'
        END;
        jan_prefix := '493' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'DG-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '04',
                (100 + (i * 8) % 800)::decimal, (50 + (i * 4) % 400)::decimal, true, now(), now());
    END LOOP;

    -- 文房具（ST-000001 ~ ST-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'ボールペン 黒 ' || (i * 0.1 + 0.3)::numeric(3,1) || 'mm'
            WHEN 2  THEN 'ボールペン 赤 ' || (i * 0.1 + 0.3)::numeric(3,1) || 'mm'
            WHEN 3  THEN 'シャープペンシル ' || (i * 0.1 + 0.3)::numeric(3,1) || 'mm'
            WHEN 4  THEN '消しゴム ' || CEIL(i::float / 20) || '個入'
            WHEN 5  THEN 'ノート A4 ' || (i * 5 + 20) || 'ページ'
            WHEN 6  THEN 'ノート B5 ' || (i * 5 + 20) || 'ページ'
            WHEN 7  THEN 'ファイル A4 ' || CEIL(i::float / 10) || '冊入'
            WHEN 8  THEN 'クリアファイル ' || CEIL(i::float / 5) || '枚入'
            WHEN 9  THEN 'ハサミ ' || (i + 10) || 'cm'
            WHEN 10 THEN 'カッター ' || CEIL(i::float / 20) || '本入'
            WHEN 11 THEN 'のり スティック ' || (i * 2 + 5) || 'g'
            WHEN 12 THEN 'セロテープ ' || (i * 3 + 10) || 'm'
            WHEN 13 THEN 'マスキングテープ ' || (i * 2 + 5) || 'm'
            WHEN 14 THEN '蛍光ペンセット ' || CEIL(i::float / 20) || '色入'
            WHEN 15 THEN '修正テープ ' || (i * 2 + 3) || 'm'
            WHEN 16 THEN 'ふせん ' || (i * 10 + 50) || '枚入'
            WHEN 17 THEN 'ホッチキス No.' || (i % 10 + 1)
            WHEN 18 THEN '定規 ' || (i + 10) || 'cm'
            WHEN 19 THEN 'コンパス 標準型 No.' || (i % 5 + 1)
            WHEN 20 THEN '色鉛筆 ' || (i + 5) || '色セット'
        END;
        jan_prefix := '494' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'ST-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '05',
                (50 + (i * 10) % 1000)::decimal, (25 + (i * 5) % 500)::decimal, true, now(), now());
    END LOOP;

    -- 衣料品（CL-000001 ~ CL-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'Tシャツ 無地 ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 2  THEN 'Tシャツ ボーダー ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 3  THEN 'ポロシャツ ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 4  THEN 'ワイシャツ 白 ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 5  THEN 'デニムパンツ ' || (i + 24) || 'インチ'
            WHEN 6  THEN 'チノパンツ ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 7  THEN 'スウェットパーカー ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 8  THEN 'ダウンジャケット ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 9  THEN 'カーディガン ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 10 THEN 'ニットセーター ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 11 THEN 'ショートパンツ ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 12 THEN 'スカート ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 13 THEN 'ワンピース ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 14 THEN '靴下 ' || CEIL(i::float / 10) || '足セット'
            WHEN 15 THEN 'インナーシャツ ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 16 THEN 'マフラー ウール No.' || (i % 10 + 1)
            WHEN 17 THEN '手袋 ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
            WHEN 18 THEN 'ベルト ' || (i + 70) || 'cm'
            WHEN 19 THEN 'キャップ フリーサイズ No.' || (i % 10 + 1)
            WHEN 20 THEN 'トートバッグ ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
        END;
        jan_prefix := '495' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'CL-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '06',
                (500 + (i * 30) % 5000)::decimal, (250 + (i * 15) % 2500)::decimal, true, now(), now());
    END LOOP;

    -- 家電（EL-000001 ~ EL-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'LED電球 ' || (i * 5 + 20) || 'W相当'
            WHEN 2  THEN 'デスクライト モデル' || (i % 10 + 1)
            WHEN 3  THEN '電気ケトル ' || (i * 0.2 + 0.6)::numeric(3,1) || 'L'
            WHEN 4  THEN 'トースター ' || (i % 4 + 1) || '枚焼き'
            WHEN 5  THEN '電子レンジ ' || (i + 15) || 'L'
            WHEN 6  THEN '炊飯器 ' || (i % 5 + 3) || '合炊き'
            WHEN 7  THEN 'ドライヤー ' || (i * 50 + 800) || 'W'
            WHEN 8  THEN '電動歯ブラシ モデル' || (i % 10 + 1)
            WHEN 9  THEN 'シェーバー モデル' || (i % 10 + 1)
            WHEN 10 THEN '加湿器 ' || (i + 5) || '畳用'
            WHEN 11 THEN '扇風機 ' || (i + 20) || 'cm'
            WHEN 12 THEN 'モバイルバッテリー ' || (i * 1000 + 3000) || 'mAh'
            WHEN 13 THEN 'USBケーブル Type-C ' || (i * 0.5 + 0.5)::numeric(3,1) || 'm'
            WHEN 14 THEN 'ワイヤレスイヤホン モデル' || (i % 10 + 1)
            WHEN 15 THEN 'Bluetoothスピーカー モデル' || (i % 10 + 1)
            WHEN 16 THEN 'マウス ワイヤレス モデル' || (i % 10 + 1)
            WHEN 17 THEN 'キーボード モデル' || (i % 10 + 1)
            WHEN 18 THEN 'USBハブ ' || (i % 4 + 4) || 'ポート'
            WHEN 19 THEN 'SDカード ' || POWER(2, (i % 5 + 3))::int || 'GB'
            WHEN 20 THEN '電源タップ ' || (i % 6 + 3) || '口'
        END;
        jan_prefix := '496' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'EL-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '07',
                (500 + (i * 50) % 10000)::decimal, (250 + (i * 25) % 5000)::decimal, true, now(), now());
    END LOOP;

    -- 化粧品（CS-000001 ~ CS-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN '化粧水 しっとり ' || (i * 10 + 100) || 'ml'
            WHEN 2  THEN '化粧水 さっぱり ' || (i * 10 + 100) || 'ml'
            WHEN 3  THEN '乳液 ' || (i * 5 + 50) || 'ml'
            WHEN 4  THEN '美容液 ' || (i * 3 + 20) || 'ml'
            WHEN 5  THEN 'クレンジングオイル ' || (i * 10 + 100) || 'ml'
            WHEN 6  THEN '洗顔フォーム ' || (i * 5 + 50) || 'g'
            WHEN 7  THEN 'フェイスクリーム ' || (i * 3 + 20) || 'g'
            WHEN 8  THEN '日焼け止め SPF' || (i % 3 * 15 + 20) || ' ' || (i * 3 + 20) || 'ml'
            WHEN 9  THEN 'リップクリーム No.' || (i % 10 + 1)
            WHEN 10 THEN 'ハンドクリーム ' || (i * 5 + 30) || 'g'
            WHEN 11 THEN 'フェイスマスク ' || CEIL(i::float / 10) || '枚入'
            WHEN 12 THEN 'アイクリーム ' || (i * 2 + 10) || 'g'
            WHEN 13 THEN 'ファンデーション No.' || (i % 10 + 1)
            WHEN 14 THEN '口紅 カラー' || LPAD((i % 20 + 1)::text, 2, '0')
            WHEN 15 THEN 'マスカラ No.' || (i % 10 + 1)
            WHEN 16 THEN 'アイシャドウ ' || (i % 4 + 4) || '色パレット'
            WHEN 17 THEN 'チーク カラー' || LPAD((i % 10 + 1)::text, 2, '0')
            WHEN 18 THEN 'ネイルカラー No.' || (i % 20 + 1)
            WHEN 19 THEN 'ボディクリーム ' || (i * 10 + 100) || 'g'
            WHEN 20 THEN 'ヘアオイル ' || (i * 5 + 30) || 'ml'
        END;
        jan_prefix := '497' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'CS-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '08',
                (300 + (i * 20) % 3000)::decimal, (150 + (i * 10) % 1500)::decimal, true, now(), now());
    END LOOP;

    -- ペット用品（PT-000001 ~ PT-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'ドッグフード ' || (i * 0.5 + 1)::numeric(3,1) || 'kg'
            WHEN 2  THEN 'キャットフード ' || (i * 0.3 + 0.5)::numeric(3,1) || 'kg'
            WHEN 3  THEN '犬用おやつ ジャーキー ' || (i * 5 + 30) || 'g'
            WHEN 4  THEN '猫用おやつ ' || (i * 3 + 10) || 'g'
            WHEN 5  THEN '猫砂 ' || (i + 3) || 'L'
            WHEN 6  THEN 'ペットシーツ ' || (i * 5 + 20) || '枚入'
            WHEN 7  THEN '犬用リード ' || (i * 0.5 + 1)::numeric(3,1) || 'm'
            WHEN 8  THEN '犬用首輪 ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
            WHEN 9  THEN 'ペット用食器 ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
            WHEN 10 THEN 'キャットタワー ' || (i + 80) || 'cm'
            WHEN 11 THEN 'ペット用ベッド ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
            WHEN 12 THEN '犬用シャンプー ' || (i * 10 + 200) || 'ml'
            WHEN 13 THEN '猫用ブラシ No.' || (i % 5 + 1)
            WHEN 14 THEN '犬用おもちゃ ボール No.' || (i % 10 + 1)
            WHEN 15 THEN '猫用おもちゃ ねずみ No.' || (i % 10 + 1)
            WHEN 16 THEN 'ペットキャリー ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
            WHEN 17 THEN '犬用ウェア ' || CASE (i % 4) WHEN 0 THEN 'XS' WHEN 1 THEN 'S' WHEN 2 THEN 'M' ELSE 'L' END
            WHEN 18 THEN 'ペット用消臭スプレー ' || (i * 10 + 200) || 'ml'
            WHEN 19 THEN '小動物用フード ' || (i * 100 + 300) || 'g'
            WHEN 20 THEN '水槽用フィルター No.' || (i % 5 + 1)
        END;
        jan_prefix := '498' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'PT-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '09',
                (200 + (i * 15) % 3000)::decimal, (100 + (i * 8) % 1500)::decimal, true, now(), now());
    END LOOP;

    -- スポーツ用品（SP-000001 ~ SP-000100）
    FOR i IN 1..100 LOOP
        product_name := CASE ((i - 1) % 20) + 1
            WHEN 1  THEN 'ランニングシューズ ' || (i + 22) || 'cm'
            WHEN 2  THEN 'ウォーキングシューズ ' || (i + 22) || 'cm'
            WHEN 3  THEN 'スポーツタオル ' || (i * 10 + 30) || 'cm'
            WHEN 4  THEN 'ヨガマット ' || (i + 3) || 'mm厚'
            WHEN 5  THEN 'ダンベル ' || (i * 0.5 + 1)::numeric(3,1) || 'kg'
            WHEN 6  THEN 'トレーニングウェア上 ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 7  THEN 'トレーニングウェア下 ' || CASE (i % 4) WHEN 0 THEN 'S' WHEN 1 THEN 'M' WHEN 2 THEN 'L' ELSE 'XL' END
            WHEN 8  THEN 'スポーツボトル ' || (i * 100 + 300) || 'ml'
            WHEN 9  THEN 'プロテイン ' || (i * 100 + 500) || 'g'
            WHEN 10 THEN 'サッカーボール ' || (i % 3 + 3) || '号'
            WHEN 11 THEN 'バスケットボール ' || (i % 3 + 5) || '号'
            WHEN 12 THEN 'テニスラケット モデル' || (i % 10 + 1)
            WHEN 13 THEN 'バドミントンラケット モデル' || (i % 10 + 1)
            WHEN 14 THEN '縄跳び ' || (i * 0.5 + 2)::numeric(3,1) || 'm'
            WHEN 15 THEN 'スイムゴーグル No.' || (i % 10 + 1)
            WHEN 16 THEN 'スイムキャップ ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
            WHEN 17 THEN 'ゴルフボール ' || CEIL(i::float / 10) || 'ダース'
            WHEN 18 THEN 'テニスボール ' || CEIL(i::float / 10) || '個入'
            WHEN 19 THEN 'リストバンド No.' || (i % 10 + 1)
            WHEN 20 THEN 'スポーツバッグ ' || CASE (i % 3) WHEN 0 THEN 'S' WHEN 1 THEN 'M' ELSE 'L' END
        END;
        jan_prefix := '499' || LPAD(i::text, 10, '0');
        INSERT INTO products (product_id, product_code, jan_code, product_name, product_category_code, price, cost, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'SP-' || LPAD(i::text, 6, '0'), SUBSTRING(jan_prefix, 1, 13), product_name, '10',
                (500 + (i * 30) % 8000)::decimal, (250 + (i * 15) % 4000)::decimal, true, now(), now());
    END LOOP;

    RAISE NOTICE 'シードデータの投入が完了しました';
END $$;
